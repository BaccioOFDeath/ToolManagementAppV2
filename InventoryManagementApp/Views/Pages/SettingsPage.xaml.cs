using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using InventoryManagementApp.ViewModels;
using TextBox = System.Windows.Controls.TextBox;

namespace InventoryManagementApp.Views.Pages
{
    public partial class SettingsPage : Page
    {
        private SettingsViewModel? _settingsViewModel;
        private bool _themeDesignerTabAdded;
        private bool _themeDesignerTabRetryQueued;
        private bool _sensitiveFieldSyncQueued;
        private Task? _initializeSettingsTask;
        private CancellationTokenSource? _initializeSettingsCts;
        private int _initializeSettingsVersion;

        public SettingsPage()
        {
            InitializeComponent();
            Loaded += SettingsPage_Loaded;
            Unloaded += SettingsPage_Unloaded;
            DataContextChanged += SettingsPage_DataContextChanged;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            AddThemeDesignerTab();
            AttachViewModel(DataContext as SettingsViewModel);
            QueueSensitiveFieldSync(_settingsViewModel);
            StartSettingsInitialization();
        }

        private void SettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            CancelSettingsInitialization();
            AttachViewModel(null);
        }

        private void SettingsPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AttachViewModel(e.NewValue as SettingsViewModel);
            QueueSensitiveFieldSync(_settingsViewModel);
            StartSettingsInitialization();
        }

        private void AddThemeDesignerTab()
        {
            if (_themeDesignerTabAdded)
                return;

            var tabControl = FindVisualChild<TabControl>(this);
            if (tabControl == null)
            {
                QueueThemeDesignerTabRetry();
                return;
            }

            _themeDesignerTabRetryQueued = false;
            var tab = new TabItem
            {
                Header = "06 Themes",
                Style = TryFindResource("DesktopSectionListTabItem") as Style,
                Content = new ThemeDesignerControl()
            };

            tabControl.Items.Insert(System.Math.Min(5, tabControl.Items.Count), tab);
            RenumberTabs(tabControl);
            _themeDesignerTabAdded = true;
        }

        private void QueueThemeDesignerTabRetry()
        {
            if (_themeDesignerTabRetryQueued)
                return;

            _themeDesignerTabRetryQueued = true;
            Dispatcher.BeginInvoke(AddThemeDesignerTab, DispatcherPriority.Loaded);
        }

        private static void RenumberTabs(TabControl tabControl)
        {
            for (var i = 0; i < tabControl.Items.Count; i++)
            {
                if (tabControl.Items[i] is not TabItem item)
                    continue;

                var header = item.Header?.ToString();
                if (string.IsNullOrWhiteSpace(header) || header.Length < 4 || !char.IsDigit(header[0]) || !char.IsDigit(header[1]))
                    continue;

                item.Header = $"{i + 1:00}{header[2..]}";
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typed)
                    return typed;

                var nested = FindVisualChild<T>(child);
                if (nested != null)
                    return nested;
            }

            return null;
        }

        private void AttachViewModel(SettingsViewModel? viewModel)
        {
            if (ReferenceEquals(_settingsViewModel, viewModel))
            {
                return;
            }

            if (_settingsViewModel != null)
            {
                _settingsViewModel.PropertyChanged -= SettingsViewModel_PropertyChanged;
            }

            CancelSettingsInitialization();
            _settingsViewModel = viewModel;
            _sensitiveFieldSyncQueued = false;
            if (_settingsViewModel != null)
            {
                _settingsViewModel.PropertyChanged += SettingsViewModel_PropertyChanged;
            }
        }

        private void CancelSettingsInitialization()
        {
            _initializeSettingsVersion++;
            _initializeSettingsCts?.Cancel();
            _initializeSettingsCts?.Dispose();
            _initializeSettingsCts = null;
            _initializeSettingsTask = null;
        }

        private void SettingsViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(SettingsViewModel.SmtpPassword) or nameof(SettingsViewModel.SmsApiKey)
                && sender is SettingsViewModel sourceViewModel)
            {
                QueueSensitiveFieldSync(sourceViewModel);
            }
        }

        private void QueueSensitiveFieldSync(SettingsViewModel? sourceViewModel)
        {
            if (sourceViewModel == null || _sensitiveFieldSyncQueued || !ReferenceEquals(_settingsViewModel, sourceViewModel))
            {
                return;
            }

            _sensitiveFieldSyncQueued = true;
            Dispatcher.BeginInvoke(new Action(() => SyncSensitiveFieldsFromViewModel(sourceViewModel)), DispatcherPriority.Background);
        }

        private void SyncSensitiveFieldsFromViewModel(SettingsViewModel sourceViewModel)
        {
            _sensitiveFieldSyncQueued = false;

            if (!ReferenceEquals(_settingsViewModel, sourceViewModel))
            {
                return;
            }

            if (SmtpPasswordBox.Password != sourceViewModel.SmtpPassword)
            {
                SmtpPasswordBox.Password = sourceViewModel.SmtpPassword;
            }

            if (SmsApiKeyBox.Password != sourceViewModel.SmsApiKey)
            {
                SmsApiKeyBox.Password = sourceViewModel.SmsApiKey;
            }
        }

        private void StartSettingsInitialization()
        {
            if (_settingsViewModel == null || _initializeSettingsTask != null)
            {
                return;
            }

            _initializeSettingsCts?.Dispose();
            _initializeSettingsCts = new CancellationTokenSource();
            var version = ++_initializeSettingsVersion;
            _initializeSettingsTask = InitializeSettingsAsync(_settingsViewModel, _initializeSettingsCts.Token, version);
        }

        private async Task InitializeSettingsAsync(SettingsViewModel viewModel, CancellationToken cancellationToken, int version)
        {
            try
            {
                await Dispatcher.Yield(DispatcherPriority.Background);
                cancellationToken.ThrowIfCancellationRequested();
                if (!ReferenceEquals(_settingsViewModel, viewModel) || version != _initializeSettingsVersion)
                {
                    return;
                }

                await viewModel.InitializeAsync().ConfigureAwait(true);
                cancellationToken.ThrowIfCancellationRequested();
                if (!ReferenceEquals(_settingsViewModel, viewModel) || version != _initializeSettingsVersion)
                {
                    return;
                }

                QueueSensitiveFieldSync(viewModel);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Navigation away from Settings or a DataContext swap owns this cancellation path.
            }
            catch (Exception ex)
            {
                _initializeSettingsTask = null;

                if (!ReferenceEquals(_settingsViewModel, viewModel) || version != _initializeSettingsVersion)
                {
                    return;
                }

                MessageBox.Show(
                    $"Failed to load settings: {ex.Message}",
                    "Settings",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SmtpPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_settingsViewModel != null && _settingsViewModel.SmtpPassword != SmtpPasswordBox.Password)
            {
                _settingsViewModel.SmtpPassword = SmtpPasswordBox.Password;
            }
        }

        private void SmsApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_settingsViewModel != null && _settingsViewModel.SmsApiKey != SmsApiKeyBox.Password)
            {
                _settingsViewModel.SmsApiKey = SmsApiKeyBox.Password;
            }
        }

        void PasswordIterationsBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            var proposed = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            e.Handled = !int.TryParse(proposed, out var value) || value <= 0;
        }

        void AutoLogoutMinutesBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            var proposed = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            e.Handled = !int.TryParse(proposed, out var value) || value < 0;
        }
    }
}

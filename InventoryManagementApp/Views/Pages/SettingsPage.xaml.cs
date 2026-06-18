using System.Windows.Controls;
using System.Windows.Input;
using InventoryManagementApp.ViewModels;
using TextBox = System.Windows.Controls.TextBox;

namespace InventoryManagementApp.Views.Pages
{
    public partial class SettingsPage : Page
    {
        private SettingsViewModel? _settingsViewModel;

        public SettingsPage()
        {
            InitializeComponent();
            Loaded += SettingsPage_Loaded;
            Unloaded += SettingsPage_Unloaded;
            DataContextChanged += SettingsPage_DataContextChanged;
        }

        private void SettingsPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            AttachViewModel(DataContext as SettingsViewModel);
            SyncSensitiveFieldsFromViewModel();
        }

        private void SettingsPage_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            AttachViewModel(null);
        }

        private void SettingsPage_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            AttachViewModel(e.NewValue as SettingsViewModel);
            SyncSensitiveFieldsFromViewModel();
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

            _settingsViewModel = viewModel;
            if (_settingsViewModel != null)
            {
                _settingsViewModel.PropertyChanged += SettingsViewModel_PropertyChanged;
            }
        }

        private void SettingsViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(SettingsViewModel.SmtpPassword) or nameof(SettingsViewModel.SmsApiKey))
            {
                Dispatcher.Invoke(SyncSensitiveFieldsFromViewModel);
            }
        }

        private void SyncSensitiveFieldsFromViewModel()
        {
            if (_settingsViewModel == null)
            {
                return;
            }

            if (SmtpPasswordBox.Password != _settingsViewModel.SmtpPassword)
            {
                SmtpPasswordBox.Password = _settingsViewModel.SmtpPassword;
            }

            if (SmsApiKeyBox.Password != _settingsViewModel.SmsApiKey)
            {
                SmsApiKeyBox.Password = _settingsViewModel.SmsApiKey;
            }
        }

        private void SmtpPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_settingsViewModel != null && _settingsViewModel.SmtpPassword != SmtpPasswordBox.Password)
            {
                _settingsViewModel.SmtpPassword = SmtpPasswordBox.Password;
            }
        }

        private void SmsApiKeyBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
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

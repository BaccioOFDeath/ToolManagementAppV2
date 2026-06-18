using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TextBox = System.Windows.Controls.TextBox;

namespace InventoryManagementApp.Views.Pages
{
    public partial class SettingsPage : Page
    {
        private bool _themeDesignerTabAdded;

        public SettingsPage()
        {
            InitializeComponent();
            Loaded += SettingsPage_Loaded;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            AddThemeDesignerTab();

            // Set initial password value if view model has one
            if (DataContext is ViewModels.SettingsViewModel viewModel && !string.IsNullOrEmpty(viewModel.SmtpPassword))
            {
                SmtpPasswordBox.Password = viewModel.SmtpPassword;
            }
            if (DataContext is ViewModels.SettingsViewModel smsViewModel && !string.IsNullOrEmpty(smsViewModel.SmsApiKey))
            {
                SmsApiKeyBox.Password = smsViewModel.SmsApiKey;
            }
        }

        private void AddThemeDesignerTab()
        {
            if (_themeDesignerTabAdded)
                return;

            var tabControl = FindVisualChild<TabControl>(this);
            if (tabControl == null)
                return;

            var tab = new TabItem
            {
                Header = "06 Themes",
                Style = TryFindResource("DesktopSectionListTabItem") as Style,
                Content = new ThemeDesignerControl()
            };

            var insertIndex = 0;
            while (insertIndex < tabControl.Items.Count && tabControl.Items[insertIndex] is TabItem item && item.Header?.ToString()?.StartsWith("0", System.StringComparison.Ordinal) == true)
            {
                insertIndex++;
            }

            tabControl.Items.Insert(System.Math.Min(5, tabControl.Items.Count), tab);
            RenumberTabs(tabControl);
            _themeDesignerTabAdded = true;
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

        private void SmtpPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.SettingsViewModel viewModel)
            {
                viewModel.SmtpPassword = SmtpPasswordBox.Password;
            }
        }

        private void SmsApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.SettingsViewModel viewModel)
            {
                viewModel.SmsApiKey = SmsApiKeyBox.Password;
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

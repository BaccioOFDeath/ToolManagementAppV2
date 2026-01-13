using System.Windows.Controls;
using System.Windows.Input;
using TextBox = System.Windows.Controls.TextBox;

namespace InventoryManagementApp.Views.Pages
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            Loaded += SettingsPage_Loaded;
        }

        private void SettingsPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
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

        private void SmtpPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ViewModels.SettingsViewModel viewModel)
            {
                viewModel.SmtpPassword = SmtpPasswordBox.Password;
            }
        }

        private void SmsApiKeyBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
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

// PasswordPromptWindow.xaml.cs – Updated to allow multiple attempts without closing the window
using System.Windows;
using System.Windows.Input;

namespace ToolManagementAppV2
{
    public partial class PasswordPromptWindow : Window
    {
        public string EnteredPassword { get; private set; }
        public bool IsPasswordResetRequested { get; private set; } = false;
        private int _attemptCount = 0;
        private const int MaxAttempts = 2;  // After 2 failed attempts, show the reset option.
        // Delegate to validate the entered password.
        public Func<string, bool> ValidatePassword { get; set; }
        public Models.User SelectedUser { get; set; }

        public PasswordPromptWindow()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            string pwd = PasswordBox.Password;
            if (ValidatePassword != null && ValidatePassword(pwd))
            {
                EnteredPassword = pwd;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                ShowError("Incorrect password. Please try again.");
                // The window remains open for another attempt.
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public void ShowError(string message)
        {
            _attemptCount++;
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
            // Show the "Forgot your password?" link after MaxAttempts.
            ForgotPasswordTextBlock.Visibility = _attemptCount >= MaxAttempts ? Visibility.Visible : Visibility.Collapsed;
            PasswordBox.Clear();
        }

        private void ForgotPasswordTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectedUser != null && SelectedUser.IsAdmin)
            {
                if (MessageBox.Show("You have entered the wrong password multiple times. Would you like to reset your password to the default value? You can change it after login.",
                    "Reset Password", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // Mark that a reset was requested.
                    IsPasswordResetRequested = true;
                    // Reset the attempt counter and clear any error messages.
                    _attemptCount = 0;
                    ErrorTextBlock.Visibility = Visibility.Collapsed;
                    ForgotPasswordTextBlock.Visibility = Visibility.Collapsed;
                    PasswordBox.Clear();
                }
            }
            else
            {
                MessageBox.Show("Password recovery is only available for admin users.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}

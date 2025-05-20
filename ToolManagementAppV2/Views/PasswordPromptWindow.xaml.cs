using System;
using System.Windows;
using System.Windows.Input;

namespace ToolManagementAppV2
{
    public partial class PasswordPromptWindow : Window
    {
        public string EnteredPassword { get; private set; }
        public bool IsPasswordResetRequested { get; private set; } = false;
        private int _attemptCount = 0;
        private const int MaxAttempts = 2;
        public Func<string, bool> ValidatePassword { get; set; }
        public Models.User SelectedUser { get; set; }

        public PasswordPromptWindow()
        {
            InitializeComponent();

            // Once the Window is loaded, update the prompt
            Loaded += (s, e) =>
            {
                if (SelectedUser != null)
                    PromptTextBlock.Text = $"{SelectedUser.UserName}, please enter your password:";
                PasswordBox.Focus();
            };
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            string pwd = PasswordBox.Password;
            if (ValidatePassword != null && ValidatePassword(pwd))
            {
                EnteredPassword = pwd;
                DialogResult = true;
                Close();
            }
            else
            {
                ShowError("Incorrect password. Please try again.");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public void ShowError(string message)
        {
            _attemptCount++;
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
            ForgotPasswordTextBlock.Visibility = _attemptCount >= MaxAttempts
                ? Visibility.Visible
                : Visibility.Collapsed;
            PasswordBox.Clear();
        }

        private void ForgotPasswordTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectedUser?.IsAdmin == true)
            {
                if (MessageBox.Show(
                        "You have entered the wrong password multiple times. Would you like to reset your password to the default value? You can change it after login.",
                        "Reset Password",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    ) == MessageBoxResult.Yes)
                {
                    IsPasswordResetRequested = true;
                    _attemptCount = 0;
                    ErrorTextBlock.Visibility = Visibility.Collapsed;
                    ForgotPasswordTextBlock.Visibility = Visibility.Collapsed;
                    PasswordBox.Clear();
                }
            }
            else
            {
                MessageBox.Show(
                    "Password recovery is only available for admin users.",
                    "Not Allowed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }
    }
}

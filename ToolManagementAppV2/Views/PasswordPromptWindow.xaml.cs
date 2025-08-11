// Views/PasswordPromptWindow.xaml.cs
using System;
using System.Windows;
using System.Windows.Input;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Views
{
    public partial class PasswordPromptWindow : Window
    {
        private const int MaxAttempts = 2;
        private int _attemptCount;

        public string EnteredPassword { get; private set; } = string.Empty;
        public bool IsPasswordResetRequested { get; private set; }
        public Func<string, bool> ValidatePassword { get; set; } = _ => true;
        public User? SelectedUser { get; set; }

        public PasswordPromptWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (SelectedUser != null)
                PromptTextBlock.Text = $"{SelectedUser.UserName}, please enter your password:";
            else
                PromptTextBlock.Text = "Please enter your password:";

            PasswordBox.Focus();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            var pwd = PasswordBox.Password;
            if (ValidatePassword?.Invoke(pwd) == true)
            {
                EnteredPassword = pwd;
                DialogResult = true;
                return;
            }

            ShowError("Incorrect password. Please try again.");
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ShowError(string message)
        {
            _attemptCount++;
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;

            ForgotPasswordTextBlock.Visibility = _attemptCount >= MaxAttempts
                ? Visibility.Visible
                : Visibility.Collapsed;

            PasswordBox.Clear();
            PasswordBox.Focus();
        }

        private void ForgotPasswordTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectedUser?.IsAdmin != true)
            {
                System.Windows.MessageBox.Show(
                    "Password recovery is only available for admin users.",
                    "Not Allowed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            var result = System.Windows.MessageBox.Show(
                "You have entered the wrong password multiple times. Reset to default and change it after login?",
                "Reset Password",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes) return;

            IsPasswordResetRequested = true;
            DialogResult = true;
        }
    }
}

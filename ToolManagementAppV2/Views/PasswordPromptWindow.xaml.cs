using System.Windows;
using System.Windows.Input;
using ToolManagementAppV2.Models;

namespace ToolManagementAppV2
{
    public partial class PasswordPromptWindow : Window
    {
        const int MaxAttempts = 2;
        int _attemptCount;

        public string EnteredPassword { get; private set; }
        public bool IsPasswordResetRequested { get; private set; }
        public Func<string, bool> ValidatePassword { get; set; }
        public User SelectedUser { get; set; }

        public PasswordPromptWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (SelectedUser != null)
                PromptTextBlock.Text = $"{SelectedUser.UserName}, please enter your password:";
            PasswordBox.Focus();
        }

        void OK_Click(object sender, RoutedEventArgs e)
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

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        void ShowError(string message)
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

        void ForgotPasswordTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectedUser?.IsAdmin != true)
            {
                MessageBox.Show(
                    "Password recovery is only available for admin users.",
                    "Not Allowed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            var result = MessageBox.Show(
                "You have entered the wrong password multiple times. Reset to default and change it after login?",
                "Reset Password",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes) return;

            IsPasswordResetRequested = true;
            _attemptCount = 0;
            ErrorTextBlock.Visibility = Visibility.Collapsed;
            ForgotPasswordTextBlock.Visibility = Visibility.Collapsed;
            PasswordBox.Clear();
            PasswordBox.Focus();
        }
    }
}

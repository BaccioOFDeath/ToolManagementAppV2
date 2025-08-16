// Views/PasswordPromptWindow.xaml.cs
using System;
using System.Windows;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views
{
    public partial class PasswordPromptWindow : Window
    {
        private const int MaxAttempts = 2;
        private int _attemptCount;
        private readonly IDialogService _dialogService;

        public PasswordPromptViewModel VM => (PasswordPromptViewModel)DataContext;
        public string EnteredPassword => VM.EnteredPassword;
        public bool IsPasswordResetRequested
        {
            get => VM.IsPasswordResetRequested;
            set => VM.IsPasswordResetRequested = value;
        }
        public Func<string, bool> ValidatePassword
        {
            get => VM.ValidatePassword;
            set => VM.ValidatePassword = value;
        }
        public User? SelectedUser
        {
            get => VM.SelectedUser;
            set => VM.SelectedUser = value;
        }

        public PasswordPromptWindow(IDialogService dialogService)
        {
            InitializeComponent();
            _dialogService = dialogService;
            DataContext = new PasswordPromptViewModel(
                _dialogService,
                () => { DialogResult = true; },
                () => { DialogResult = false; },
                ShowError);
            this.DisposeDataContextOnUnload();
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

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            VM.EnteredPassword = PasswordBox.Password;
        }

        private void ShowError(string message)
        {
            _attemptCount++;
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;

            ForgotPasswordButton.Visibility = _attemptCount >= MaxAttempts
                ? Visibility.Visible
                : Visibility.Collapsed;

            PasswordBox.Clear();
            PasswordBox.Focus();
        }
    }
}

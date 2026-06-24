// Views/PasswordPromptWindow.xaml.cs
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
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
            Activated += OnActivated;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (SelectedUser != null)
                PromptTextBlock.Text = $"{SelectedUser.UserName}, please enter your password:";
            else
                PromptTextBlock.Text = "Please enter your password:";

            FocusPasswordBox();
        }

        private void OnActivated(object? sender, EventArgs e)
        {
            FocusPasswordBox();
        }

        private void FocusPasswordBox()
        {
            PasswordBox.Dispatcher.BeginInvoke(() =>
            {
                PasswordBox.Focus();
                Keyboard.Focus(PasswordBox);
            }, DispatcherPriority.Input);
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

            var showResetRecovery = _attemptCount >= MaxAttempts;
            ResetRecoveryPanel.Visibility = showResetRecovery
                ? Visibility.Visible
                : Visibility.Collapsed;
            ForgotPasswordButton.Visibility = showResetRecovery
                ? Visibility.Visible
                : Visibility.Collapsed;

            PasswordBox.Clear();
            FocusPasswordBox();
        }
    }
}

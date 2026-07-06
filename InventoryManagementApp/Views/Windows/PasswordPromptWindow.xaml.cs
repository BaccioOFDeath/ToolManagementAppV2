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
        private DispatcherOperation? _pendingFocusOperation;
        private bool _isUnloaded;

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
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _isUnloaded = false;

            if (SelectedUser != null)
                PromptTextBlock.Text = $"{SelectedUser.UserName}, please enter your password:";
            else
                PromptTextBlock.Text = "Please enter your password:";

            FocusPasswordBox(selectAll: true);
        }

        private void OnActivated(object? sender, EventArgs e)
        {
            FocusPasswordBox();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _isUnloaded = true;
            AbortPendingFocus();
        }

        private void FocusPasswordBox(bool selectAll = false)
        {
            if (_isUnloaded || !IsLoaded)
                return;

            AbortPendingFocus();
            _pendingFocusOperation = PasswordBox.Dispatcher.BeginInvoke(() =>
            {
                _pendingFocusOperation = null;

                if (_isUnloaded || !IsLoaded)
                    return;

                PasswordBox.Focus();
                Keyboard.Focus(PasswordBox);

                if (selectAll)
                    PasswordBox.SelectAll();
            }, DispatcherPriority.Input);
        }

        private void AbortPendingFocus()
        {
            if (_pendingFocusOperation is { Status: DispatcherOperationStatus.Pending })
                _pendingFocusOperation.Abort();

            _pendingFocusOperation = null;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            VM.EnteredPassword = PasswordBox.Password;
            VM.ClearPasswordFeedback();

            if (!string.IsNullOrEmpty(PasswordBox.Password))
                ErrorTextBlock.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && VM.OkCommand.CanExecute(null))
            {
                VM.OkCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape && VM.CancelCommand.CanExecute(null))
            {
                VM.CancelCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void ShowError(string message)
        {
            _attemptCount++;
            VM.RegisterFailedAttempt(_attemptCount, MaxAttempts);
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

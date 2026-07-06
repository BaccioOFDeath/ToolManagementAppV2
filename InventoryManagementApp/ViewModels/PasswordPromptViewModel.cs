using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.ViewModels
{
    public class PasswordPromptViewModel : ObservableObject
    {
        private string _enteredPassword = string.Empty;
        private bool _isPasswordResetRequested;
        private bool _isResetInProgress;
        private User? _selectedUser;
        private string _statusMessage = "Ready to unlock.";
        private string _failureSummary = "No failed attempts.";

        public string EnteredPassword
        {
            get => _enteredPassword;
            set
            {
                if (SetProperty(ref _enteredPassword, value ?? string.Empty))
                    OkCommand.NotifyCanExecuteChanged();
            }
        }

        public bool IsPasswordResetRequested
        {
            get => _isPasswordResetRequested;
            set => SetProperty(ref _isPasswordResetRequested, value);
        }

        public bool IsResetInProgress
        {
            get => _isResetInProgress;
            private set
            {
                if (SetProperty(ref _isResetInProgress, value))
                {
                    OkCommand.NotifyCanExecuteChanged();
                    ResetPasswordCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public string FailureSummary
        {
            get => _failureSummary;
            private set => SetProperty(ref _failureSummary, value);
        }

        public Func<string, bool> ValidatePassword { get; set; } = _ => true;

        public User? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public IRelayCommand OkCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public IAsyncRelayCommand ResetPasswordCommand { get; }

        private readonly Action _onSuccess;
        private readonly Action _onCancel;
        private readonly Action<string> _showError;
        readonly IDialogService _dialogService;

        public PasswordPromptViewModel(IDialogService dialogService, Action onSuccess, Action onCancel, Action<string> showError)
        {
            _dialogService = dialogService;
            _onSuccess = onSuccess;
            _onCancel = onCancel;
            _showError = showError;

            OkCommand = new RelayCommand(OnOk, CanSubmitPassword);
            CancelCommand = new RelayCommand(() => _onCancel());
            ResetPasswordCommand = new AsyncRelayCommand(OnResetPasswordAsync, CanRequestReset);
        }

        public void RegisterFailedAttempt(int attemptCount, int resetThreshold)
        {
            var safeAttemptCount = Math.Max(0, attemptCount);
            var safeResetThreshold = Math.Max(1, resetThreshold);
            FailureSummary = $"Failed attempts: {safeAttemptCount} of {safeResetThreshold}.";
            StatusMessage = safeAttemptCount >= safeResetThreshold
                ? "Reset assistance is available for admin users."
                : "Password was not accepted. Try again.";
        }

        public void ClearPasswordFeedback()
        {
            if (!string.IsNullOrEmpty(EnteredPassword))
                StatusMessage = "Ready to unlock. Press Enter or choose Unlock.";
        }

        private bool CanSubmitPassword()
        {
            return !IsResetInProgress && !string.IsNullOrWhiteSpace(EnteredPassword);
        }

        private bool CanRequestReset()
        {
            return !IsResetInProgress;
        }

        private void OnOk()
        {
            if (!CanSubmitPassword())
                return;

            var pwd = EnteredPassword ?? string.Empty;
            if (ValidatePassword?.Invoke(pwd) == true)
            {
                StatusMessage = "Password accepted.";
                _onSuccess();
                return;
            }

            _showError("Incorrect password. Please try again.");
        }

        async Task OnResetPasswordAsync()
        {
            if (IsResetInProgress)
                return;

            IsResetInProgress = true;
            StatusMessage = "Preparing reset confirmation...";

            try
            {
                if (SelectedUser?.IsAdmin != true)
                {
                    StatusMessage = "Password recovery is only available for admin users.";
                    await _dialogService.ShowInfoAsync(
                        "Password recovery is only available for admin users.",
                        "Not Allowed");
                    return;
                }

                if (!await _dialogService.ShowConfirmationAsync(
                    "You have entered the wrong password multiple times. Reset to default and change it after login?",
                    "Reset Password"))
                {
                    StatusMessage = "Password reset was canceled.";
                    return;
                }

                IsPasswordResetRequested = true;
                StatusMessage = "Password reset requested.";
                _onSuccess();
            }
            finally
            {
                IsResetInProgress = false;
            }
        }
    }
}

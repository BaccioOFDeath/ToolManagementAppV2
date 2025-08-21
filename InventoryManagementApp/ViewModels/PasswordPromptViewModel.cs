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
        public string EnteredPassword { get; set; } = string.Empty;
        public bool IsPasswordResetRequested { get; set; }
        public Func<string, bool> ValidatePassword { get; set; } = _ => true;
        public User? SelectedUser { get; set; }

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

            OkCommand = new RelayCommand(OnOk);
            CancelCommand = new RelayCommand(() => _onCancel());
            ResetPasswordCommand = new AsyncRelayCommand(OnResetPasswordAsync);
        }

        private void OnOk()
        {
            var pwd = EnteredPassword ?? string.Empty;
            if (ValidatePassword?.Invoke(pwd) == true)
            {
                _onSuccess();
                return;
            }

            _showError("Incorrect password. Please try again.");
        }

        async Task OnResetPasswordAsync()
        {
            if (SelectedUser?.IsAdmin != true)
            {
                await _dialogService.ShowInfoAsync(
                    "Password recovery is only available for admin users.",
                    "Not Allowed");
                return;
            }

            if (!await _dialogService.ShowConfirmationAsync(
                "You have entered the wrong password multiple times. Reset to default and change it after login?",
                "Reset Password"))
                return;

            IsPasswordResetRequested = true;
            _onSuccess();
        }
    }
}


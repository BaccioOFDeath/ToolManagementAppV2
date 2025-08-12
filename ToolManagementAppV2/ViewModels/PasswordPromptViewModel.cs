using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.ViewModels
{
    public class PasswordPromptViewModel : ObservableObject
    {
        public string EnteredPassword { get; set; } = string.Empty;
        public bool IsPasswordResetRequested { get; set; }
        public Func<string, bool> ValidatePassword { get; set; } = _ => true;
        public User? SelectedUser { get; set; }

        public IRelayCommand OkCommand { get; }
        public IRelayCommand CancelCommand { get; }

        private readonly Action _onSuccess;
        private readonly Action _onCancel;
        private readonly Action<string> _showError;

        public PasswordPromptViewModel(Action onSuccess, Action onCancel, Action<string> showError)
        {
            _onSuccess = onSuccess;
            _onCancel = onCancel;
            _showError = showError;

            OkCommand = new RelayCommand(OnOk);
            CancelCommand = new RelayCommand(() => _onCancel());
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
    }
}


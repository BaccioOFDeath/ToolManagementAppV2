using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.ViewModels
{
    public class PasswordPromptViewModel : ObservableObject
    {
        public string EnteredPassword { get; private set; } = string.Empty;
        public bool IsPasswordResetRequested { get; set; }
        public Func<string, bool> ValidatePassword { get; set; } = _ => true;
        public User? SelectedUser { get; set; }

        public IRelayCommand<string> OkCommand { get; }
        public IRelayCommand CancelCommand { get; }

        private readonly Action _onSuccess;
        private readonly Action _onCancel;
        private readonly Action<string> _showError;

        public PasswordPromptViewModel(Action onSuccess, Action onCancel, Action<string> showError)
        {
            _onSuccess = onSuccess;
            _onCancel = onCancel;
            _showError = showError;

            OkCommand = new RelayCommand<string>(OnOk);
            CancelCommand = new RelayCommand(() => _onCancel());
        }

        private void OnOk(string? password)
        {
            var pwd = password ?? string.Empty;
            if (ValidatePassword?.Invoke(pwd) == true)
            {
                EnteredPassword = pwd;
                _onSuccess();
                return;
            }

            _showError("Incorrect password. Please try again.");
        }
    }
}


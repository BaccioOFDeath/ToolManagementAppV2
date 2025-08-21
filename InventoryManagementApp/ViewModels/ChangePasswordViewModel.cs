using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using InventoryManagementApp.Utilities.Helpers;

namespace InventoryManagementApp.ViewModels
{
    public class ChangePasswordViewModel : ObservableObject
    {
        string _newPassword = string.Empty;
        public string NewPassword
        {
            get => _newPassword;
            set
            {
                if (SetProperty(ref _newPassword, value))
                    ValidationMessage = string.Empty;
            }
        }

        string _confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                    ValidationMessage = string.Empty;
            }
        }

        string _validationMessage = string.Empty;
        public string ValidationMessage
        {
            get => _validationMessage;
            private set => SetProperty(ref _validationMessage, value);
        }

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ChangePasswordViewModel(Action onSave, Action onCancel)
        {
            SaveCommand = new RelayCommand(() =>
            {
                if (!PasswordValidator.IsValid(NewPassword, out var error))
                {
                    ValidationMessage = error!;
                    return;
                }

                if (NewPassword != ConfirmPassword)
                {
                    ValidationMessage = "Passwords do not match.";
                    return;
                }

                ValidationMessage = string.Empty;
                onSave();
            });
            CancelCommand = new RelayCommand(onCancel);
        }
    }
}

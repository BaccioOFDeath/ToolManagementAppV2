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
                {
                    ValidationMessage = string.Empty;
                    NotifyPasswordEntryStateChanged();
                }
            }
        }

        string _confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                {
                    ValidationMessage = string.Empty;
                    NotifyPasswordEntryStateChanged();
                }
            }
        }

        string _validationMessage = string.Empty;
        public string ValidationMessage
        {
            get => _validationMessage;
            private set
            {
                if (SetProperty(ref _validationMessage, value))
                {
                    OnPropertyChanged(nameof(HasValidationMessage));
                    OnPropertyChanged(nameof(PasswordReadinessSummary));
                }
            }
        }

        public bool HasValidationMessage => !string.IsNullOrWhiteSpace(ValidationMessage);

        public bool CanAttemptSave =>
            !string.IsNullOrWhiteSpace(NewPassword) && !string.IsNullOrWhiteSpace(ConfirmPassword);

        public string PasswordReadinessSummary
        {
            get
            {
                if (HasValidationMessage)
                    return ValidationMessage;

                if (string.IsNullOrWhiteSpace(NewPassword) && string.IsNullOrWhiteSpace(ConfirmPassword))
                    return "Enter and confirm the new password to enable Save Password.";

                if (string.IsNullOrWhiteSpace(NewPassword))
                    return "Enter the new password before saving.";

                if (string.IsNullOrWhiteSpace(ConfirmPassword))
                    return "Confirm the new password before saving.";

                return "Ready to validate and save the new password.";
            }
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
            }, () => CanAttemptSave);
            CancelCommand = new RelayCommand(onCancel);
        }

        private void NotifyPasswordEntryStateChanged()
        {
            OnPropertyChanged(nameof(CanAttemptSave));
            OnPropertyChanged(nameof(PasswordReadinessSummary));
            SaveCommand.NotifyCanExecuteChanged();
        }
    }
}
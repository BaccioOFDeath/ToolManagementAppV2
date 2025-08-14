using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace ToolManagementAppV2.ViewModels
{
    public class ChangePasswordViewModel : ObservableObject
    {
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ChangePasswordViewModel(Action onSave, Action onCancel)
        {
            SaveCommand = new RelayCommand(() =>
            {
                if (!string.IsNullOrWhiteSpace(NewPassword) && NewPassword == ConfirmPassword)
                    onSave();
            });
            CancelCommand = new RelayCommand(onCancel);
        }
    }
}

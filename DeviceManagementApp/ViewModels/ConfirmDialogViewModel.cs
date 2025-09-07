using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace DeviceManagementApp.ViewModels
{
    public class ConfirmDialogViewModel : ObservableObject
    {
        public string Message { get; }
        public IRelayCommand OkCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ConfirmDialogViewModel(string message, Action<bool?> close)
        {
            Message = message;
            OkCommand = new RelayCommand(() => close(true));
            CancelCommand = new RelayCommand(() => close(false));
        }
    }
}

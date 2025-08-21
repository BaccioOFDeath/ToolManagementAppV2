using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace InventoryManagementApp.ViewModels
{
    public class InfoDialogViewModel : ObservableObject
    {
        public string Message { get; }
        public IRelayCommand OkCommand { get; }

        public InfoDialogViewModel(string message, Action close)
        {
            Message = message;
            OkCommand = new RelayCommand(close);
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace InventoryManagementApp.ViewModels
{
    public class PrintPreviewViewModel : ObservableObject
    {
        public IRelayCommand PageSetupCommand { get; }
        public IRelayCommand PrintCommand { get; }
        public IRelayCommand CloseCommand { get; }

        public PrintPreviewViewModel(Action onPageSetup, Action onPrint, Action onClose)
        {
            PageSetupCommand = new RelayCommand(onPageSetup);
            PrintCommand = new RelayCommand(onPrint);
            CloseCommand = new RelayCommand(onClose);
        }
    }
}


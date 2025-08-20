// ViewModels/ToolDetailsViewModel.cs
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ToolManagementAppV2.ViewModels
{
    public class ToolDetailsViewModel : ObservableObject
    {
        public ItemModel ItemModel { get; }

        public IRelayCommand CloseCommand { get; }

        public ToolDetailsViewModel(ItemModel tool, Action onClose)
        {
            ItemModel = tool;
            CloseCommand = new RelayCommand(onClose);
        }
    }
}

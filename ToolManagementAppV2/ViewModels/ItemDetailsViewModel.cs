// ViewModels/ItemDetailsViewModel.cs
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ToolManagementAppV2.ViewModels
{
    public class ItemDetailsViewModel : ObservableObject
    {
        public ItemModel ItemModel { get; }

        public IRelayCommand CloseCommand { get; }

        public ItemDetailsViewModel(ItemModel item, Action onClose)
        {
            ItemModel = item;
            CloseCommand = new RelayCommand(onClose);
        }
    }
}

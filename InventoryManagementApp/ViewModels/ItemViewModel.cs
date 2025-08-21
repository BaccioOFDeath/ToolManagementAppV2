using CommunityToolkit.Mvvm.ComponentModel;

namespace InventoryManagementApp.ViewModels
{
    internal class ItemViewModel : ObservableObject
    {
        private ItemModel _item;
        public ItemModel ItemModel
        {
            get => _item;
            set => SetProperty(ref _item, value);
        }

        public ItemViewModel(ItemModel item)
        {
            _item = item;
        }

        public string DisplayName => $"{_item.ItemNumber} - {_item.NameDescription}";
    }
}

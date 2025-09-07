using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceManagementApp.Models
{
    public class ItemDetailOption : ObservableObject
    {
        public ItemDetailOption(ItemDetailField field, bool isVisible)
        {
            Field = field;
            _isVisible = isVisible;
        }

        public ItemDetailField Field { get; }

        bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;

namespace ToolManagementAppV2.ViewModels
{
    internal class ToolViewModel : ObservableObject
    {
        private ItemModel _tool;
        public ItemModel ItemModel
        {
            get => _tool;
            set => SetProperty(ref _tool, value);
        }

        public ToolViewModel(ItemModel tool)
        {
            _tool = tool;
        }

        public string DisplayName => $"{_tool.ItemNumber} - {_tool.NameDescription}";
    }
}

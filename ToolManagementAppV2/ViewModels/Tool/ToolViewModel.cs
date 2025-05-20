using CommunityToolkit.Mvvm.ComponentModel;

namespace ToolManagementAppV2.ViewModels.Tool
{
    internal class ToolViewModel : ObservableObject
    {
        private ToolModel _tool;
        public ToolModel Tool
        {
            get => _tool;
            set => SetProperty(ref _tool, value);
        }

        public ToolViewModel(ToolModel tool)
        {
            _tool = tool;
        }

        public string DisplayName => $"{_tool.ToolNumber} - {_tool.NameDescription}";
    }
}

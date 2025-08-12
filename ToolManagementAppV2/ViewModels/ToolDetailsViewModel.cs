// ViewModels/ToolDetailsViewModel.cs
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ToolManagementAppV2.ViewModels
{
    public class ToolDetailsViewModel : ObservableObject
    {
        public ToolModel Tool { get; }

        public IRelayCommand CloseCommand { get; }

        public ToolDetailsViewModel(ToolModel tool, Action onClose)
        {
            Tool = tool;
            CloseCommand = new RelayCommand(onClose);
        }
    }
}

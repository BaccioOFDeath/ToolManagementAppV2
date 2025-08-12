// ViewModels/ToolEditViewModel.cs
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ToolManagementAppV2.ViewModels
{
    public class ToolEditViewModel : ObservableObject
    {
        public ToolModel Tool { get; }

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ToolEditViewModel(ToolModel tool, Action onSave, Action onCancel)
        {
            Tool = tool;
            SaveCommand = new RelayCommand(onSave);
            CancelCommand = new RelayCommand(onCancel);
        }
    }
}

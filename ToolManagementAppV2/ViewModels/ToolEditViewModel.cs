// ViewModels/ToolEditViewModel.cs
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.ViewModels
{
    public class ToolEditViewModel : ObservableObject
    {
        private readonly IFileDialogService _fileDialog;

        public ToolModel Tool { get; }

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public IRelayCommand BrowseImageCommand { get; }
        public IRelayCommand RemoveImageCommand { get; }

        public ToolEditViewModel(ToolModel tool, Action onSave, Action onCancel, IFileDialogService fileDialog)
        {
            Tool = tool;
            _fileDialog = fileDialog;

            SaveCommand = new RelayCommand(onSave);
            CancelCommand = new RelayCommand(onCancel);

            BrowseImageCommand = new RelayCommand(BrowseImage);
            RemoveImageCommand = new RelayCommand(RemoveImage);
        }

        void BrowseImage()
        {
            var path = _fileDialog.OpenFile("Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                Tool.ToolImagePath = path;
            }
        }

        void RemoveImage()
        {
            Tool.ToolImagePath = string.Empty;
        }
    }
}

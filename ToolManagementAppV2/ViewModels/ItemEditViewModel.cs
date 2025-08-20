// ViewModels/ItemEditViewModel.cs
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.ViewModels
{
    public class ItemEditViewModel : ObservableObject
    {
        /// <summary>
        /// Service used to display file dialogs for selecting item images.
        /// </summary>
        private readonly IFileDialogService _fileDialog;

        public ItemModel ItemModel { get; }

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        /// <summary>
        /// Opens a file dialog to select an image and updates <see cref="ItemModel.ImagePath"/>.
        /// </summary>
        public IRelayCommand BrowseImageCommand { get; }

        /// <summary>
        /// Clears the current <see cref="ItemModel.ImagePath"/> and removes the preview.
        /// </summary>
        public IRelayCommand RemoveImageCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemEditViewModel"/> class.
        /// </summary>
        /// <param name="item">The item being edited.</param>
        /// <param name="onSave">Action invoked to persist the item changes.</param>
        /// <param name="onCancel">Action invoked when editing is canceled.</param>
        /// <param name="fileDialog">Service used for browsing image files.</param>
        public ItemEditViewModel(ItemModel item, Action onSave, Action onCancel, IFileDialogService fileDialog)
        {
            ItemModel = item;
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
                ItemModel.ImagePath = path;
            }
        }

        void RemoveImage()
        {
            ItemModel.ImagePath = string.Empty;
        }
    }
}

// ViewModels/ItemEditViewModel.cs
using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;

namespace InventoryManagementApp.ViewModels
{
    public class ItemEditViewModel : ObservableObject
    {
        /// <summary>
        /// Service used to display file dialogs for selecting item images.
        /// </summary>
        private readonly IFileDialogService _fileDialog;
        private readonly IDeviceService _deviceService;

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
        public ObservableCollection<Device> Devices { get; } = new();

        public ItemEditViewModel(ItemModel item, Action onSave, Action onCancel, IFileDialogService fileDialog, IDeviceService deviceService)
        {
            ItemModel = item;
            _fileDialog = fileDialog;
            _deviceService = deviceService;

            SaveCommand = new RelayCommand(onSave);
            CancelCommand = new RelayCommand(onCancel);

            BrowseImageCommand = new RelayCommand(BrowseImage);
            RemoveImageCommand = new RelayCommand(RemoveImage);
            _ = LoadDevicesAsync();
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

        async Task LoadDevicesAsync()
        {
            try
            {
                var devices = await _deviceService.GetDevicesAsync();
                Devices.Clear();
                Devices.Add(new Device { Ip = string.Empty, Hostname = "(None)" });
                foreach (var d in devices)
                    Devices.Add(d);
            }
            catch { }
        }
    }
}

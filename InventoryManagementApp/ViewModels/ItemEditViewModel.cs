// ViewModels/ItemEditViewModel.cs
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Utilities.Helpers;
using InventoryManagementApp.Views.Windows;

namespace InventoryManagementApp.ViewModels
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
        /// Creates a transparent PNG copy by removing a plain light or corner-matched background.
        /// </summary>
        public IRelayCommand RemoveImageBackgroundCommand { get; }

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
            RemoveImageBackgroundCommand = new RelayCommand(RemoveImageBackground);
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

        void RemoveImageBackground()
        {
            var sourcePath = ResolveImagePath(ItemModel.ImagePath);
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                return;

            var outputPath = BuildBackgroundRemovedImagePath(sourcePath);
            var dialog = new ImageBackgroundRemovalWindow(sourcePath, outputPath);
            var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
            if (owner != null)
                dialog.Owner = owner;

            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.SavedImagePath))
                ItemModel.ImagePath = AppAssetHelper.ToAppRelativePath(dialog.SavedImagePath);
        }

        static string? ResolveImagePath(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return null;

            return AppAssetHelper.ResolveAssetPath(imagePath) ?? Path.GetFullPath(imagePath);
        }

        string BuildBackgroundRemovedImagePath(string sourcePath)
        {
            var targetDirectory = AppAssetHelper.EnsureAssetFolder(AppAssetHelper.ItemImagesFolder);
            var seed = string.IsNullOrWhiteSpace(ItemModel.ItemNumber)
                ? Path.GetFileNameWithoutExtension(sourcePath)
                : ItemModel.ItemNumber;
            var fileName = $"{AppAssetHelper.SanitizeFileName(seed)}-transparent.png";
            return Path.Combine(targetDirectory, fileName);
        }

        static void SaveBackgroundRemovedPng(string sourcePath, string outputPath)
        {
            var source = LoadBgra32(sourcePath);
            var transparent = ImageBackgroundRemovalWindow.CreateBackgroundRemovedBitmap(source, new Rect(0, 0, source.PixelWidth, source.PixelHeight), 72);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(transparent));

            using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            encoder.Save(output);
        }

        static BitmapSource LoadBgra32(string sourcePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(sourcePath, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();

            BitmapSource source = bitmap.Format == PixelFormats.Bgra32
                ? bitmap
                : new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
            source.Freeze();
            return source;
        }

    }
}

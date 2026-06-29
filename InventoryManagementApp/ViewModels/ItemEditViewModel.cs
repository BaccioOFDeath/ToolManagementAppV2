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
            SaveBackgroundRemovedPng(sourcePath, outputPath);
            ItemModel.ImagePath = AppAssetHelper.ToAppRelativePath(outputPath);
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
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(sourcePath, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();

            BitmapSource source = bitmap.Format == PixelFormats.Bgra32
                ? bitmap
                : new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);

            var width = source.PixelWidth;
            var height = source.PixelHeight;
            var stride = width * 4;
            var pixels = new byte[height * stride];
            source.CopyPixels(pixels, stride, 0);

            var background = EstimateBackgroundColor(pixels, width, height, stride);
            for (var offset = 0; offset < pixels.Length; offset += 4)
            {
                var blue = pixels[offset];
                var green = pixels[offset + 1];
                var red = pixels[offset + 2];
                if (IsBackgroundPixel(red, green, blue, background))
                    pixels[offset + 3] = 0;
            }

            var transparent = BitmapSource.Create(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null, pixels, stride);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(transparent));

            using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            encoder.Save(output);
        }

        static Color EstimateBackgroundColor(byte[] pixels, int width, int height, int stride)
        {
            var samplePoints = new[]
            {
                (X: 0, Y: 0),
                (X: Math.Max(0, width - 1), Y: 0),
                (X: 0, Y: Math.Max(0, height - 1)),
                (X: Math.Max(0, width - 1), Y: Math.Max(0, height - 1))
            };

            var red = 0;
            var green = 0;
            var blue = 0;
            foreach (var point in samplePoints)
            {
                var offset = point.Y * stride + point.X * 4;
                blue += pixels[offset];
                green += pixels[offset + 1];
                red += pixels[offset + 2];
            }

            return Color.FromRgb((byte)(red / samplePoints.Length), (byte)(green / samplePoints.Length), (byte)(blue / samplePoints.Length));
        }

        static bool IsBackgroundPixel(byte red, byte green, byte blue, Color background)
        {
            var distance = Math.Abs(red - background.R) + Math.Abs(green - background.G) + Math.Abs(blue - background.B);
            var isNearCornerBackground = distance <= 72;
            var isPlainLightBackground = red >= 238 && green >= 238 && blue >= 238 && Math.Abs(red - green) <= 12 && Math.Abs(red - blue) <= 12;
            return isNearCornerBackground || isPlainLightBackground;
        }

    }
}

using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Utilities.Helpers;
using InventoryManagementApp.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemEditViewModelImageBackgroundTests
    {
        [Fact]
        public void ItemEditViewModel_ExposesImageBackgroundRemovalCommand()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ItemEditViewModel.cs");

            Assert.Contains("public IRelayCommand RemoveImageBackgroundCommand { get; }", source, StringComparison.Ordinal);
            Assert.Contains("RemoveImageBackgroundCommand = new RelayCommand(RemoveImageBackground);", source, StringComparison.Ordinal);
            Assert.Contains("void RemoveImageBackground()", source, StringComparison.Ordinal);
            Assert.Contains("AppAssetHelper.EnsureAssetFolder(AppAssetHelper.ItemImagesFolder)", source, StringComparison.Ordinal);
            Assert.Contains("new ImageBackgroundRemovalWindow(sourcePath, outputPath)", source, StringComparison.Ordinal);
            Assert.Contains("dialog.ShowDialog() == true", source, StringComparison.Ordinal);
            Assert.Contains("ItemModel.ImagePath = AppAssetHelper.ToAppRelativePath(dialog.SavedImagePath);", source, StringComparison.Ordinal);
            Assert.Contains("ItemModel.PropertyChanged += ItemModel_PropertyChanged;", source, StringComparison.Ordinal);
            Assert.Contains("if (e.PropertyName == nameof(ItemModel.ImagePath))", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(ItemModel));", source, StringComparison.Ordinal);
            Assert.Contains("transparent-{DateTime.Now:yyyyMMddHHmmssfff}.png", source, StringComparison.Ordinal);
            Assert.Contains("PixelFormats.Bgra32", source, StringComparison.Ordinal);
        }

        [Fact]
        public void BrowseImageCommand_CopiesResizedImageToItemAssets()
        {
            RunSta(() =>
            {
                var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempDir);
                var source = Path.Combine(tempDir, "external-source.png");

                try
                {
                    CreateTestImage(source, 1800, 900);

                    var item = new ItemModel { ItemNumber = "T-Manual Image" };
                    var viewModel = new ItemEditViewModel(item, () => { }, () => { }, new FakeFileDialogService(source));

                    viewModel.BrowseImageCommand.Execute(null);

                    Assert.StartsWith(Path.Combine(AppAssetHelper.AssetsDirectoryName, AppAssetHelper.ItemImagesFolder), item.ImagePath);
                    Assert.EndsWith(".jpg", item.ImagePath, StringComparison.OrdinalIgnoreCase);
                    Assert.Contains("T-Manual Image", item.ImagePath, StringComparison.Ordinal);
                    Assert.NotEqual(source, item.ImagePath);

                    var copiedPath = AppAssetHelper.ResolveAssetPath(item.ImagePath);
                    Assert.NotNull(copiedPath);
                    Assert.True(File.Exists(copiedPath));

                    var copied = LoadBitmap(copiedPath!);
                    Assert.True(copied.PixelWidth <= 1024);
                    Assert.True(copied.PixelHeight <= 1024);
                    Assert.Equal(1024, copied.PixelWidth);
                    Assert.Equal(512, copied.PixelHeight);
                }
                finally
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            });
        }

        [Fact]
        public void ItemDetailsViewModel_ReraisesItemModelWhenImagePathChanges()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ItemDetailsViewModel.cs");

            Assert.Contains("ItemModel.PropertyChanged += ItemModel_PropertyChanged;", source, StringComparison.Ordinal);
            Assert.Contains("void ItemModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)", source, StringComparison.Ordinal);
            Assert.Contains("if (e.PropertyName == nameof(ItemModel.ImagePath))", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(ItemModel));", source, StringComparison.Ordinal);
            Assert.Contains("await InvokeOnUiThreadAsync(() =>", source, StringComparison.Ordinal);
            Assert.Contains("CopyItem(ItemModel, refreshed);", source, StringComparison.Ordinal);
            Assert.Contains("RefreshState();", source, StringComparison.Ordinal);
            Assert.Contains("ItemModel.PropertyChanged -= ItemModel_PropertyChanged;", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeService_SnapshotsWindowsBeforeRefreshingLayouts()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "Services", "ThemeService.cs");

            Assert.Contains("foreach (Window window in app.Windows.Cast<Window>().ToList())", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemService_DoesNotFailSaveWhenUiRefreshNotificationFails()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");

            Assert.Contains("try", source, StringComparison.Ordinal);
            Assert.Contains("WeakReferenceMessenger.Default.Send(new DomainDataChangedMessage(scope, entityId));", source, StringComparison.Ordinal);
            Assert.Contains("catch (InvalidOperationException)", source, StringComparison.Ordinal);
            Assert.Contains("Persistence has already completed", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ImageBackgroundRemovalWindow_ProvidesClipPreviewAndSaveControls()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ImageBackgroundRemovalWindow.xaml");
            var codeBehind = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ImageBackgroundRemovalWindow.xaml.cs");

            Assert.Contains("x:Name=\"ClipCanvas\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"PreviewImage\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"ThresholdSlider\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<DockPanel LastChildFill=\"True\" Margin=\"0,4,0,0\">", xaml, StringComparison.Ordinal);
            Assert.True(
                xaml.IndexOf("DockPanel.Dock=\"Right\" Text=\"{Binding ElementName=ThresholdSlider", StringComparison.Ordinal) <
                xaml.IndexOf("x:Name=\"ThresholdSlider\"", StringComparison.Ordinal),
                "The threshold value label must be docked before the slider so the slider fills the remaining width.");
            Assert.Contains("Height=\"32\" VerticalAlignment=\"Center\" ValueChanged=\"ThresholdSlider_ValueChanged\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Rotate Left\" Click=\"RotateLeft_Click\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Rotate Right\" Click=\"RotateRight_Click\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Save Clipped Image", xaml, StringComparison.Ordinal);
            Assert.Contains("void RotateSource(double angle)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("static BitmapSource RotateBitmap(BitmapSource source, double angle)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("new RotateTransform(angle)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("MoveThumb_DragDelta", codeBehind, StringComparison.Ordinal);
            Assert.Contains("CreateBackgroundRemovedBitmap", codeBehind, StringComparison.Ordinal);
            Assert.Contains("pixels[offset + 3] = 0;", codeBehind, StringComparison.Ordinal);
        }

        private static string ReadRepositoryFile(params string[] path)
            => File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", Path.Combine(path))));

        private static void RunSta(Action action)
        {
            Exception? threadException = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
                finally
                {
                    WpfTestHelper.ShutdownApplication();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (threadException != null)
                throw threadException;
        }

        private static void CreateTestImage(string path, int width, int height)
        {
            var pixels = new byte[width * height * 4];
            for (var i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 255;
                pixels[i + 1] = 128;
                pixels[i + 2] = 0;
                pixels[i + 3] = 255;
            }

            var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using var stream = File.Create(path);
            encoder.Save(stream);
        }

        private static BitmapImage LoadBitmap(string path)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private sealed class FakeFileDialogService(string path) : IFileDialogService
        {
            public string? OpenFile(string filter, string? initialDirectory = null) => path;
            public string? SaveFile(string filter, string? initialDirectory = null) => null;
            public string? BrowseFolder(string? initialDirectory = null) => null;
        }
    }
}

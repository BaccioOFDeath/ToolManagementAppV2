using System;
using System.IO;
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
            Assert.Contains("PixelFormats.Bgra32", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ImageBackgroundRemovalWindow_ProvidesClipPreviewAndSaveControls()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ImageBackgroundRemovalWindow.xaml");
            var codeBehind = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ImageBackgroundRemovalWindow.xaml.cs");

            Assert.Contains("x:Name=\"ClipCanvas\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"PreviewImage\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"ThresholdSlider\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Save Clipped Image", xaml, StringComparison.Ordinal);
            Assert.Contains("MoveThumb_DragDelta", codeBehind, StringComparison.Ordinal);
            Assert.Contains("CreateBackgroundRemovedBitmap", codeBehind, StringComparison.Ordinal);
            Assert.Contains("pixels[offset + 3] = 0;", codeBehind, StringComparison.Ordinal);
        }

        private static string ReadRepositoryFile(params string[] path)
            => File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", Path.Combine(path))));
    }
}

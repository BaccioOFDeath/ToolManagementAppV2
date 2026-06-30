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
            Assert.Contains("ItemModel.PropertyChanged += ItemModel_PropertyChanged;", source, StringComparison.Ordinal);
            Assert.Contains("if (e.PropertyName == nameof(ItemModel.ImagePath))", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(ItemModel));", source, StringComparison.Ordinal);
            Assert.Contains("transparent-{DateTime.Now:yyyyMMddHHmmssfff}.png", source, StringComparison.Ordinal);
            Assert.Contains("PixelFormats.Bgra32", source, StringComparison.Ordinal);
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
            Assert.Contains("Save Clipped Image", xaml, StringComparison.Ordinal);
            Assert.Contains("MoveThumb_DragDelta", codeBehind, StringComparison.Ordinal);
            Assert.Contains("CreateBackgroundRemovedBitmap", codeBehind, StringComparison.Ordinal);
            Assert.Contains("pixels[offset + 3] = 0;", codeBehind, StringComparison.Ordinal);
        }

        private static string ReadRepositoryFile(params string[] path)
            => File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", Path.Combine(path))));
    }
}

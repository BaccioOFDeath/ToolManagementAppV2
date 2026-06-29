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
            Assert.Contains("SaveBackgroundRemovedPng(sourcePath, outputPath);", source, StringComparison.Ordinal);
            Assert.Contains("ItemModel.ImagePath = AppAssetHelper.ToAppRelativePath(outputPath);", source, StringComparison.Ordinal);
            Assert.Contains("PixelFormats.Bgra32", source, StringComparison.Ordinal);
            Assert.Contains("pixels[offset + 3] = 0;", source, StringComparison.Ordinal);
        }

        private static string ReadRepositoryFile(params string[] path)
            => File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", Path.Combine(path))));
    }
}

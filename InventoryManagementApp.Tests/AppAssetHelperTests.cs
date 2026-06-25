using System;
using System.IO;
using InventoryManagementApp.Utilities.Helpers;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class AppAssetHelperTests
    {
        [Fact]
        public void CopyImageToAssetFolder_CreatesFolderAndReturnsRelativeAssetPath()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            var source = Path.Combine(tempDir, "company logo.png");
            File.WriteAllBytes(source, [1, 2, 3, 4]);

            try
            {
                var relativePath = AppAssetHelper.CopyImageToAssetFolder(source, AppAssetHelper.CompanyLogoFolder);
                var fullPath = AppAssetHelper.ResolveAssetPath(relativePath);

                Assert.StartsWith(Path.Combine(AppAssetHelper.AssetsDirectoryName, AppAssetHelper.CompanyLogoFolder), relativePath);
                Assert.NotNull(fullPath);
                Assert.True(File.Exists(fullPath));
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public void CopyImageToAssetFolder_RejectsNonImageFiles()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            var source = Path.Combine(tempDir, "notes.txt");
            File.WriteAllText(source, "not an image");

            try
            {
                Assert.Throws<InvalidOperationException>(() =>
                    AppAssetHelper.CopyImageToAssetFolder(source, AppAssetHelper.CompanyLogoFolder));
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}

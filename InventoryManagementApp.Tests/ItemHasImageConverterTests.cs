using System;
using System.Globalization;
using System.IO;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Utilities.Converters;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemHasImageConverterTests
    {
        [Fact]
        public void Convert_ItemWithExistingImage_ReturnsTrue()
        {
            var imagePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");
            File.WriteAllBytes(imagePath, [0x89, 0x50, 0x4e, 0x47]);
            try
            {
                var item = new ItemModel { ImagePath = imagePath };
                var result = new ItemHasImageConverter().Convert(item, typeof(bool), null!, CultureInfo.InvariantCulture);

                Assert.True(Assert.IsType<bool>(result));
            }
            finally
            {
                File.Delete(imagePath);
            }
        }

        [Fact]
        public void Convert_ItemUsingNumberedImageFallback_ReturnsTrue()
        {
            var itemNumber = $"hover-{Guid.NewGuid():N}";
            var imageDirectory = Path.Combine(AppContext.BaseDirectory, "Assets", "ItemImages");
            var imagePath = Path.Combine(imageDirectory, itemNumber + ".jpg");
            Directory.CreateDirectory(imageDirectory);
            File.WriteAllBytes(imagePath, [0xff, 0xd8, 0xff, 0xd9]);
            try
            {
                var item = new ItemModel { ItemNumber = itemNumber };
                var result = new ItemHasImageConverter().Convert(item, typeof(bool), null!, CultureInfo.InvariantCulture);

                Assert.True(Assert.IsType<bool>(result));
            }
            finally
            {
                File.Delete(imagePath);
            }
        }

        [Fact]
        public void Convert_ItemWithoutImage_ReturnsFalse()
        {
            var item = new ItemModel
            {
                ImagePath = Path.Combine("missing", $"{Guid.NewGuid():N}.png"),
                ItemNumber = $"missing-{Guid.NewGuid():N}"
            };

            var result = new ItemHasImageConverter().Convert(item, typeof(bool), null!, CultureInfo.InvariantCulture);

            Assert.False(Assert.IsType<bool>(result));
        }
    }
}

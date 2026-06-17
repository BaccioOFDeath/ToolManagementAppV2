using System;
using System.Globalization;
using System.IO;
using InventoryManagementApp.Utilities.Converters;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ExistingFilePathToBoolConverterTests
    {
        [Fact]
        public void Convert_RelativeExistingPath_ReturnsTrue()
        {
            var baseDir = AppContext.BaseDirectory;
            var tempDir = Path.Combine(baseDir, Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var filePath = Path.Combine(tempDir, "file.txt");
                File.WriteAllText(filePath, "test");
                var relative = Path.GetRelativePath(baseDir, filePath);
                var converter = new ExistingFilePathToBoolConverter();
                var result = converter.Convert(relative, typeof(bool), null, CultureInfo.InvariantCulture);
                Assert.True(result is bool b && b);
            }
            finally
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }

        [Fact]
        public void Convert_AbsoluteExistingPath_ReturnsTrue()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
            File.WriteAllText(tempFile, "test");
            try
            {
                var converter = new ExistingFilePathToBoolConverter();
                var result = converter.Convert(tempFile, typeof(bool), null, CultureInfo.InvariantCulture);
                Assert.True(result is bool b && b);
            }
            finally
            {
                try { File.Delete(tempFile); } catch { }
            }
        }

        [Fact]
        public void Convert_RelativeMissingPath_ReturnsFalse()
        {
            var relative = Path.Combine("nonexistent", "file.txt");
            var converter = new ExistingFilePathToBoolConverter();
            var result = converter.Convert(relative, typeof(bool), null, CultureInfo.InvariantCulture);
            Assert.False(result is bool b && b);
        }
    }
}


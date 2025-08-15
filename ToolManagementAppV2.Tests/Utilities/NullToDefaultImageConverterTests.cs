using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using ToolManagementAppV2.Utilities.Converters;
using Xunit;

namespace ToolManagementAppV2.Tests.Utilities
{
    public class NullToDefaultImageConverterTests
    {
        private static readonly byte[] PngBytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIW2NgYGD4DwABBAEAj4SR7QAAAABJRU5ErkJggg==");

        [Fact]
        public void Convert_CachesLoadedImages()
        {
            var converter = new NullToDefaultImageConverter();
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(baseDir, "cache_test.png");
            File.WriteAllBytes(path, PngBytes);
            try
            {
                var first = (BitmapImage)converter.Convert(path, typeof(BitmapImage), null, CultureInfo.InvariantCulture);
                var second = (BitmapImage)converter.Convert(path, typeof(BitmapImage), null, CultureInfo.InvariantCulture);
                Assert.Same(first, second);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [Fact]
        public void Convert_EvictsOldestEntries_WhenMaxExceeded()
        {
            var converter = new NullToDefaultImageConverter();
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var field = typeof(NullToDefaultImageConverter).GetField("MaxCacheEntries", BindingFlags.NonPublic | BindingFlags.Static);
            var max = (int)field!.GetValue(null)!;

            var firstPath = Path.Combine(baseDir, "evict0.png");
            File.WriteAllBytes(firstPath, PngBytes);
            var first = (BitmapImage)converter.Convert(firstPath, typeof(BitmapImage), null, CultureInfo.InvariantCulture);

            for (int i = 1; i <= max; i++)
            {
                var p = Path.Combine(baseDir, $"evict{i}.png");
                File.WriteAllBytes(p, PngBytes);
                converter.Convert(p, typeof(BitmapImage), null, CultureInfo.InvariantCulture);
            }

            var again = (BitmapImage)converter.Convert(firstPath, typeof(BitmapImage), null, CultureInfo.InvariantCulture);
            Assert.NotSame(first, again);

            for (int i = 0; i <= max; i++)
            {
                var p = Path.Combine(baseDir, $"evict{i}.png");
                if (File.Exists(p))
                    File.Delete(p);
            }
        }
    }
}

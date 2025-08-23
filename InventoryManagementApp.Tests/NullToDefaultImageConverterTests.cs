using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using InventoryManagementApp.Utilities.Converters;
using InventoryManagementApp.Utilities.Helpers;
using System.Collections.Concurrent;
using System.Reflection;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class NullToDefaultImageConverterTests
    {
        [Fact]
        public void Convert_InvalidPath_UsesDefaultImage()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var converter = new NullToDefaultImageConverter();
                    Assert.Null(PathHelper.GetAbsolutePath("../nonexistent.png", false));
                    var defaultImage = Assert.IsType<BitmapImage>(converter.Convert(null, typeof(BitmapImage), "user", CultureInfo.InvariantCulture));
                    var result = Assert.IsType<BitmapImage>(converter.Convert("../nonexistent.png", typeof(BitmapImage), "user", CultureInfo.InvariantCulture));
                    Assert.Same(defaultImage, result);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void Convert_InvalidPath_IsCached()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var converter = new NullToDefaultImageConverter();
                    var field = typeof(NullToDefaultImageConverter).GetField("_invalidPaths", BindingFlags.NonPublic | BindingFlags.Static);
                    var cache = (ConcurrentDictionary<string, byte>)field!.GetValue(null)!;
                    cache.Clear();
                    var path = "../nonexistent.png";
                    Assert.Null(PathHelper.GetAbsolutePath(path, false));
                    converter.Convert(path, typeof(BitmapImage), "user", CultureInfo.InvariantCulture);
                    Assert.True(cache.ContainsKey(path));
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }
    }
}

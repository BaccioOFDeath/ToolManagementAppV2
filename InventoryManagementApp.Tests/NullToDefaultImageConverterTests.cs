using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using InventoryManagementApp.Utilities.Converters;
using InventoryManagementApp.Utilities.Helpers;
using System.Reflection;
using System.IO;
using Microsoft.Extensions.Caching.Memory;
using InventoryManagementApp.Models.Domain;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class NullToDefaultImageConverterTests
    {
        [Fact]
        public void Convert_InvalidPath_ReturnsEmptyImage()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var converter = new NullToDefaultImageConverter();
                    Assert.Null(PathHelper.GetAbsolutePath("../nonexistent.png", false));
                    var result = Assert.IsType<BitmapImage>(converter.Convert("../nonexistent.png", typeof(BitmapImage), "user", CultureInfo.InvariantCulture));
                    Assert.Null(result.UriSource);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    WpfTestHelper.ShutdownApplication();
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
                    var cache = (MemoryCache)field!.GetValue(null)!;
                    cache.Compact(1.0);
                    var path = "../nonexistent.png";
                    Assert.Null(PathHelper.GetAbsolutePath(path, false));
                    converter.Convert(path, typeof(BitmapImage), "user", CultureInfo.InvariantCulture);
                    Assert.True(cache.TryGetValue(path, out _));
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    WpfTestHelper.ShutdownApplication();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void Convert_CacheEvictsLeastRecentlyUsed()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempDir);
                try
                {
                    var converter = new NullToDefaultImageConverter();
                    var cacheField = typeof(NullToDefaultImageConverter).GetField("_imageCache", BindingFlags.NonPublic | BindingFlags.Static);
                    var cache = (MemoryCache)cacheField!.GetValue(null)!;
                    cache.Compact(1.0);

                    var pngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAAAAAA6fptVAAAAC0lEQVQI12NgYAAAAAMAAWgmWQ0AAAAASUVORK5CYII=");
                    var firstPath = Path.Combine(tempDir, "0.png");
                    File.WriteAllBytes(firstPath, pngBytes);
                    var firstImage = Assert.IsType<BitmapImage>(converter.Convert(firstPath, typeof(BitmapImage), "user", CultureInfo.InvariantCulture));

                    for (int i = 1; i <= 100; i++)
                    {
                        var path = Path.Combine(tempDir, $"{i}.png");
                        File.WriteAllBytes(path, pngBytes);
                        converter.Convert(path, typeof(BitmapImage), "user", CultureInfo.InvariantCulture);
                    }

                    var secondImage = Assert.IsType<BitmapImage>(converter.Convert(firstPath, typeof(BitmapImage), "user", CultureInfo.InvariantCulture));
                    Assert.NotSame(firstImage, secondImage);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                    WpfTestHelper.ShutdownApplication();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void Convert_InvalidPathCacheEvictsLeastRecentlyUsed()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var converter = new NullToDefaultImageConverter();
                    var cacheField = typeof(NullToDefaultImageConverter).GetField("_invalidPaths", BindingFlags.NonPublic | BindingFlags.Static);
                    var cache = (MemoryCache)cacheField!.GetValue(null)!;
                    cache.Compact(1.0);

                    var firstPath = "../nonexistent0.png";
                    Assert.Null(PathHelper.GetAbsolutePath(firstPath, false));
                    converter.Convert(firstPath, typeof(BitmapImage), "user", CultureInfo.InvariantCulture);

                    for (int i = 1; i <= 100; i++)
                    {
                        var path = $"../nonexistent{i}.png";
                        Assert.Null(PathHelper.GetAbsolutePath(path, false));
                        converter.Convert(path, typeof(BitmapImage), "user", CultureInfo.InvariantCulture);
                    }

                    Assert.True(cache.Count <= 100);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    WpfTestHelper.ShutdownApplication();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void Convert_FileChangedAtSamePath_ReloadsImageInsteadOfServingStaleCache()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempDir);
                try
                {
                    ClearImageCaches();
                    var path = Path.Combine(tempDir, "item.png");
                    var pngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAAAAAA6fptVAAAAC0lEQVQI12NgYAAAAAMAAWgmWQ0AAAAASUVORK5CYII=");
                    File.WriteAllBytes(path, pngBytes);
                    var converter = new NullToDefaultImageConverter();
                    var first = Assert.IsType<BitmapImage>(converter.Convert(path, typeof(BitmapImage), "item", CultureInfo.InvariantCulture));

                    File.WriteAllBytes(path, pngBytes);
                    File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddSeconds(2));
                    var second = Assert.IsType<BitmapImage>(converter.Convert(path, typeof(BitmapImage), "item", CultureInfo.InvariantCulture));

                    Assert.NotSame(first, second);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                    WpfTestHelper.ShutdownApplication();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void Convert_ItemWithoutImagePath_FallsBackToItemNumberImage()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    ClearImageCaches();
                    EnsureItemImageCandidate("T401.jpeg");
                    var converter = new NullToDefaultImageConverter();
                    var item = new ItemModel
                    {
                        ItemNumber = "T401",
                        ImagePath = string.Empty
                    };

                    var result = Assert.IsType<BitmapImage>(converter.Convert(item, typeof(BitmapImage), "item", CultureInfo.InvariantCulture));

                    Assert.NotNull(result.UriSource);
                    Assert.EndsWith("Assets/ItemImages/T401.jpeg", result.UriSource.LocalPath.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    WpfTestHelper.ShutdownApplication();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void Convert_ReservationWithoutImagePath_FallsBackToItemNumberImage()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    ClearImageCaches();
                    EnsureItemImageCandidate("T401.jpeg");
                    var converter = new NullToDefaultImageConverter();
                    var reservation = new Reservation
                    {
                        ItemNumber = "T401",
                        ImagePath = string.Empty
                    };

                    var result = Assert.IsType<BitmapImage>(converter.Convert(reservation, typeof(BitmapImage), "item", CultureInfo.InvariantCulture));

                    Assert.NotNull(result.UriSource);
                    Assert.EndsWith("Assets/ItemImages/T401.jpeg", result.UriSource.LocalPath.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    WpfTestHelper.ShutdownApplication();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        static void EnsureItemImageCandidate(string fileName)
        {
            var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Assets", "ItemImages", fileName));
            var targetDir = Path.Combine(AppContext.BaseDirectory, "Assets", "ItemImages");
            Directory.CreateDirectory(targetDir);
            File.Copy(source, Path.Combine(targetDir, fileName), overwrite: true);
        }

        static void ClearImageCaches()
        {
            var imageCacheField = typeof(NullToDefaultImageConverter).GetField("_imageCache", BindingFlags.NonPublic | BindingFlags.Static);
            var invalidPathsField = typeof(NullToDefaultImageConverter).GetField("_invalidPaths", BindingFlags.NonPublic | BindingFlags.Static);
            ((MemoryCache)imageCacheField!.GetValue(null)!).Compact(1.0);
            ((MemoryCache)invalidPathsField!.GetValue(null)!).Compact(1.0);
        }
    }
}

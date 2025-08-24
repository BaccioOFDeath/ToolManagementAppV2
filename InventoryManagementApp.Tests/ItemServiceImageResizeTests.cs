using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using InventoryManagementApp.Services.Items;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemServiceImageResizeTests
    {
        [Fact]
        public void CopyFileAsync_ResizesImage()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    Directory.CreateDirectory(tempDir);
                    var source = Path.Combine(tempDir, "src.png");
                    var dest = Path.Combine(tempDir, "dest.jpg");
                    CreateTestImage(source);
                    var service = new TestItemService();
                    service.InvokeCopyFileAsync(source, dest, 96, 96).GetAwaiter().GetResult();
                    Assert.True(File.Exists(dest));
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.UriSource = new Uri(dest);
                    img.EndInit();
                    Assert.Equal(96, img.PixelWidth);
                    Assert.Equal(48, img.PixelHeight);
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

        static void CreateTestImage(string path)
        {
            const int width = 200;
            const int height = 100;
            var pixels = new byte[width * height * 4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 255;     // B
                pixels[i + 1] = 0;   // G
                pixels[i + 2] = 0;   // R
                pixels[i + 3] = 255; // A
            }
            var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var fs = File.Create(path);
            encoder.Save(fs);
        }

        class TestItemService : ItemService
        {
            public TestItemService() : base(null!, null!) { }

            public Task InvokeCopyFileAsync(string src, string dest, int maxW, int maxH)
                => CopyFileAsync(src, dest, maxW, maxH, CancellationToken.None);
        }
    }
}

using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class WindowBackgroundOverlayTests
    {
        [Fact]
        public void AppLoadedWindowHook_AppliesSharedBackgroundOverlayToPopOutWindows()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "App.xaml.cs");

            Assert.Contains("EventManager.RegisterClassHandler(typeof(Window)", source, StringComparison.Ordinal);
            Assert.Contains("ApplyBackgroundOverlay(window);", source, StringComparison.Ordinal);
            Assert.Contains("ThemeAppBackgroundOverlayBrush", source, StringComparison.Ordinal);
            Assert.Contains("BackgroundBrush", source, StringComparison.Ordinal);
            Assert.Contains("HasThemedWindowOverlay(window)", source, StringComparison.Ordinal);
        }

        static string ReadRepositoryFile(params string[] relativePathParts)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "InventoryManagementApp.sln")))
                directory = directory.Parent;

            Assert.NotNull(directory);
            var path = Path.Combine(directory!.FullName, Path.Combine(relativePathParts));
            Assert.True(File.Exists(path), $"Expected repository file at {path}");
            return File.ReadAllText(path);
        }
    }
}

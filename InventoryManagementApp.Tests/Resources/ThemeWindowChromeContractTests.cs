using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Resources
{
    public class ThemeWindowChromeContractTests
    {
        [Fact]
        public void App_LoadsThemeWindowChromeAfterSharedHierarchyResources()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "App.xaml");

            var hierarchyIndex = xaml.IndexOf("Resources/PolishedVisualHierarchy.xaml", StringComparison.Ordinal);
            var windowChromeIndex = xaml.IndexOf("Resources/Theme.WindowChrome.xaml", StringComparison.Ordinal);

            Assert.True(hierarchyIndex >= 0, "App.xaml should load the shared polished visual hierarchy resources.");
            Assert.True(windowChromeIndex > hierarchyIndex, "Window chrome resources depend on shared hierarchy styles and should load after them.");
        }

        [Fact]
        public void ThemeWindowChrome_UsesAdminThemeTokensForWholeWindowFrames()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.WindowChrome.xaml");

            Assert.Contains("ThemedWindowRoot", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemedWindowOverlay", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemedWindowHeader", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemedWindowPane", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemedDocumentCanvasFrame", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemedWindowFooter", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BackgroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeAppBackgroundOverlayBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeShellHeaderBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDialogSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeCardCornerRadius}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemePanelCornerRadius}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeInputCornerRadius}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeSurfaceShadow}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeRaisedShadow}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlShadow}", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintPreviewWindow_ConsumesThemedWindowChromeStyles()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml");

            Assert.Contains("Style=\"{StaticResource ThemedWindowRoot}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource ThemedWindowOverlay}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource ThemedWindowHeader}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource ThemedWindowPane}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource ThemedDocumentCanvasFrame}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource ThemedWindowFooter}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("FontFamily=\"{DynamicResource ThemeFontFamily}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("BorderThickness=\"{DynamicResource ThemeControlBorderThickness}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CornerRadius=\"{DynamicResource ThemePanelCornerRadius}\"", xaml, StringComparison.Ordinal);
        }

        private static string ReadRepositoryFile(params string[] relativePathParts)
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

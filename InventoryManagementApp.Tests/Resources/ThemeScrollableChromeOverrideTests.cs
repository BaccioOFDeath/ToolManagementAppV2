using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Resources
{
    public class ThemeScrollableChromeOverrideTests
    {
        [Fact]
        public void FullCustomizationOverrides_ThemeListMenuAndScrollChrome()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.FullCustomizationOverrides.xaml");

            Assert.Contains("TargetType=\"ListBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ListView\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ListBoxItem\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ListViewItem\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ContextMenu\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"MenuItem\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Separator\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ScrollBar\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"{x:Type primitives:Thumb}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void FullCustomizationOverrides_ListMenuAndScrollChromeUseAdminThemeTokens()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.FullCustomizationOverrides.xaml");

            Assert.Contains("xmlns:primitives=\"clr-namespace:System.Windows.Controls.Primitives;assembly=PresentationFramework\"", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource SurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource GlassSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDialogSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource TransparentSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource AccentBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BorderBrushAlt}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeSubtleBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderlessThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{x:Null}", xaml, StringComparison.Ordinal);
            Assert.Contains("{x:Null}", xaml, StringComparison.Ordinal);
            Assert.Contains("{x:Null}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDisabledOpacity}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlMinHeight}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeFontFamily}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemHoverBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemSelectedBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeGridLineBrush}", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void FullCustomizationOverrides_ListMenuAndScrollChromeLoadAfterCommonControls()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.FullCustomizationOverrides.xaml");

            var statusBarIndex = xaml.IndexOf("TargetType=\"StatusBar\"", StringComparison.Ordinal);
            var listBoxIndex = xaml.IndexOf("TargetType=\"ListBox\"", StringComparison.Ordinal);
            var contextMenuIndex = xaml.IndexOf("TargetType=\"ContextMenu\"", StringComparison.Ordinal);
            var scrollBarIndex = xaml.IndexOf("TargetType=\"ScrollBar\"", StringComparison.Ordinal);

            Assert.True(statusBarIndex >= 0, "Common control overrides should remain before this extension block.");
            Assert.True(listBoxIndex > statusBarIndex, "List chrome should extend the final override layer after common controls.");
            Assert.True(contextMenuIndex > listBoxIndex, "Menu chrome should follow list chrome in the final override layer.");
            Assert.True(scrollBarIndex > contextMenuIndex, "Scrollbar chrome should be part of the late final override block.");
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

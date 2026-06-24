using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Resources
{
    public class DesktopThemeResourceContractTests
    {
        [Fact]
        public void DesktopShell_UsesAdminThemeTokensForCommonChrome()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "DesktopShell.xaml");

            Assert.Contains("{DynamicResource ThemeBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeButtonCornerRadius}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlMinHeight}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBodyFontSize}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDisabledOpacity}", xaml, StringComparison.Ordinal);
            Assert.Contains("{x:Null}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDataGridRowHeight}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDataGridHeaderHeight}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeGridLineBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource CardPadding}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ControlPadding}", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void DesktopPageShell_UsesAdminThemeTokensForSectionRailsAndActionStrips()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "DesktopPageShellResources.xaml");

            Assert.Contains("{DynamicResource ThemeBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemePanelCornerRadius}", xaml, StringComparison.Ordinal);
            Assert.Contains("{x:Null}", xaml, StringComparison.Ordinal);
            Assert.Contains("{x:Null}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlMinHeight}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBodyFontSize}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource CardPadding}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeSubtleBorderThickness}", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void DesktopPageShell_SectionRailTabsScrollWhenPaddingConsumesHeight()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "DesktopPageShellResources.xaml");

            Assert.Contains("<ScrollViewer VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalScrollBarVisibility=\"Disabled\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("IsItemsHost=\"True\"", xaml, StringComparison.Ordinal);
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

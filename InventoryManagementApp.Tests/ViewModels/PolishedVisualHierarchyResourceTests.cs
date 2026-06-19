using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class PolishedVisualHierarchyResourceTests
    {
        [Fact]
        public void PolishedVisualHierarchy_DefinesStableActionAndFooterSizing()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "PolishedVisualHierarchy.xaml");

            Assert.Contains("RaisedSurfaceShadow", xaml, StringComparison.Ordinal);
            Assert.Contains("DesktopStatusFooter", xaml, StringComparison.Ordinal);
            Assert.Contains("AdminHandoffCard", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeControlMinHeight", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalContentAlignment\" Value=\"Center\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VerticalContentAlignment\" Value=\"Center\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PolishedVisualHierarchy_PreservesExistingWorkbenchStyleContracts()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "PolishedVisualHierarchy.xaml");

            Assert.Contains("ToolbarCard", xaml, StringComparison.Ordinal);
            Assert.Contains("PrimaryButton", xaml, StringComparison.Ordinal);
            Assert.Contains("GhostButton", xaml, StringComparison.Ordinal);
            Assert.Contains("DesktopSummaryCard", xaml, StringComparison.Ordinal);
            Assert.Contains("DesktopPaneHeader", xaml, StringComparison.Ordinal);
            Assert.Contains("DesktopPaneSubheader", xaml, StringComparison.Ordinal);
            Assert.Contains("DesktopSectionActionStrip", xaml, StringComparison.Ordinal);
            Assert.Contains("DesktopNoteCard", xaml, StringComparison.Ordinal);
            Assert.Contains("DataRunLogCard", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeResources_DefineAdminControlledDensityTokens()
        {
            var customization = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.Customization.xaml");
            var hierarchy = ReadRepositoryFile("InventoryManagementApp", "Resources", "PolishedVisualHierarchy.xaml");

            Assert.Contains("ThemeFontScale", customization, StringComparison.Ordinal);
            Assert.Contains("ThemeBodyFontSize", customization, StringComparison.Ordinal);
            Assert.Contains("ThemeControlMinHeight", customization, StringComparison.Ordinal);
            Assert.Contains("ThemeDataGridRowHeight", customization, StringComparison.Ordinal);
            Assert.Contains("ThemeDataGridHeaderHeight", customization, StringComparison.Ordinal);
            Assert.Contains("NavigationSurfaceBrush", customization, StringComparison.Ordinal);
            Assert.Contains("NavigationSurfaceBrush", hierarchy, StringComparison.Ordinal);
            Assert.Contains("ThemeDataGridRowHeight", hierarchy, StringComparison.Ordinal);
            Assert.Contains("ThemeDataGridHeaderHeight", hierarchy, StringComparison.Ordinal);
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
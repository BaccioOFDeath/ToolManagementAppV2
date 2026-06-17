using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class KitManagementPageXamlTests
    {
        [Fact]
        public void KitManagementPage_UsesWorkbenchSummariesAndCommands()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "KitManagementPage.xaml");

            Assert.Contains("Kit Workbench", xaml, StringComparison.Ordinal);
            Assert.Contains("KitResultsSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("KitItemsSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedKitSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedKitAvailabilitySummary", xaml, StringComparison.Ordinal);
            Assert.Contains("AddKitCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CheckAvailabilityCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintSelectedKitCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintKitListCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("KitRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("KitItemRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("DataGridRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void KitManagementPage_HasStyledEmptyStatesForDirectoryAndMembership()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "KitManagementPage.xaml");

            Assert.Contains("No kits match this filter", xaml, StringComparison.Ordinal);
            Assert.Contains("FilteredKits.Count", xaml, StringComparison.Ordinal);
            Assert.Contains("No items assigned to this kit", xaml, StringComparison.Ordinal);
            Assert.Contains("KitItems.Count", xaml, StringComparison.Ordinal);
            Assert.Contains("DesktopNoteCard", xaml, StringComparison.Ordinal);
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

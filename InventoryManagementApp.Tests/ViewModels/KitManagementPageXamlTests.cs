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

            Assert.Contains("Text=\"Kits\"", xaml, StringComparison.Ordinal);
            Assert.Contains("KitResultsSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("KitItemsSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedKitSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedKitHandoffSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedKitPrintSummary", xaml, StringComparison.Ordinal);
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

            Assert.Contains("KitEmptyStateTitle", xaml, StringComparison.Ordinal);
            Assert.Contains("KitEmptyStateMessage", xaml, StringComparison.Ordinal);
            Assert.Contains("IsKitDirectoryEmptyVisible", xaml, StringComparison.Ordinal);
            Assert.Contains("KitItemsEmptyStateTitle", xaml, StringComparison.Ordinal);
            Assert.Contains("KitItemsEmptyStateMessage", xaml, StringComparison.Ordinal);
            Assert.Contains("IsKitItemsEmptyVisible", xaml, StringComparison.Ordinal);
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

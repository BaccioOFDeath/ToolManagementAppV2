using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class CategoriesPageXamlTests
    {
        [Fact]
        public void CategoriesPage_UsesWorkbenchSummariesAndCommands()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml");

            Assert.Contains("Category Workbench", xaml, StringComparison.Ordinal);
            Assert.Contains("CategoryResultsSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("CategoryFilterSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("CategorySetupSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedCategoryTitle", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedCategoryNextAction", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedCategoryChecklist", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedCategoryHandoff", xaml, StringComparison.Ordinal);
            Assert.Contains("AddCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("SaveCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("DeleteCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("RefreshCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ClearSearchCommand", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoriesPage_PreservesDirectoryHooksAndStyledEmptyState()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml");

            Assert.Contains("CategoryStatCard", xaml, StringComparison.Ordinal);
            Assert.Contains("CategoryDetailCard", xaml, StringComparison.Ordinal);
            Assert.Contains("DesktopPaneHeader", xaml, StringComparison.Ordinal);
            Assert.Contains("DesktopNoteCard", xaml, StringComparison.Ordinal);
            Assert.Contains("CategoryEmptyStateTitle", xaml, StringComparison.Ordinal);
            Assert.Contains("CategoryEmptyStateMessage", xaml, StringComparison.Ordinal);
            Assert.Contains("IsCategoryEmptyStateVisible", xaml, StringComparison.Ordinal);
            Assert.Contains("CategoryRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("CategoryRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenCategoryDetail_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("CopyCategory_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintSelectedCategory_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintCategories_Click", xaml, StringComparison.Ordinal);
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

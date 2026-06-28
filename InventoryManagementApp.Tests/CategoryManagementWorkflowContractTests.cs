using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CategoryManagementWorkflowContractTests
    {
        [Fact]
        public void CategoryLoadFailuresClearStaleRowsSelectionAndEditState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CategoryManagementViewModel.cs");

            AssertContainsAll(
                source,
                "ClearCategoryStateAfterLoadFailure();",
                "Categories could not be loaded. Category rows were cleared until reload succeeds.",
                "private void ClearCategoryStateAfterLoadFailure()",
                "Categories.Clear();",
                "FilteredCategories.Clear();",
                "SelectedCategory = null;",
                "CategoryName = \"\";",
                "RaiseDirectoryProperties();",
                "_saveCommand = new AsyncCommand(SaveAsync, () => SelectedCategory != null && !string.IsNullOrWhiteSpace(CategoryName));",
                "_deleteCommand = new AsyncCommand(DeleteAsync, () => SelectedCategory != null);",
                "if (_schemaInitialized)",
                "private void ShowCategoryLoadFailureDialogOnce()",
                "if (_loadFailureDialogShown) return;",
                "_loadFailureDialogShown = false;",
                "WpfMessageBox.Show(\"Categories could not be loaded. Category rows were cleared until reload succeeds. Please retry or check the application log.");
            Assert.DoesNotContain("Categories could not be loaded. Review logs or retry refresh.\";\n                WpfMessageBox.Show(\"Categories could not be loaded. Please retry", NormalizeNewlines(source), StringComparison.Ordinal);
        }

        [Fact]
        public void CategoryMutationFailuresRefreshOrClearVisibleRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CategoryManagementViewModel.cs");

            AssertContainsAll(
                source,
                "private async Task RefreshCategoryDirectoryAfterMutationFailureAsync(int? preferredSelectedId, string refreshedStatusMessage, string clearedStatusMessage)",
                "var list = await _service.GetCategoriesForInventoryAsync(SelectedInventoryId);",
                "Categories.Clear();",
                "ApplyFilter(preferredSelectedId);",
                "StatusMessage = refreshedStatusMessage;",
                "_logger.LogWarning(refreshEx, \"Failed to refresh categories after a category mutation failure for inventory {InventoryId}\", SelectedInventoryId);",
                "ClearCategoryStateAfterLoadFailure();",
                "StatusMessage = clearedStatusMessage;",
                "int? createdCategoryId = null;",
                "createdCategoryId = id;",
                "Category rows were refreshed after '{name}' failed to finish creating.",
                "Category rows were cleared after '{name}' failed to finish creating and recovery reload failed.",
                "Category rows were refreshed after category #{id} could not be renamed.",
                "Category rows were refreshed after category #{id} failed to finish saving.",
                "Category rows were refreshed after '{category.Name}' could not be deleted.",
                "Category rows were refreshed after '{category.Name}' failed to finish deleting.",
                "Category rows were refreshed from the saved data where possible.");
            Assert.DoesNotContain("The category was not renamed. Refresh and try again.", source, StringComparison.Ordinal);
            Assert.DoesNotContain("The category was not deleted. Refresh and try again.", source, StringComparison.Ordinal);
        }

        private static void AssertContainsAll(string source, params string[] expectedSnippets)
        {
            foreach (var snippet in expectedSnippets)
            {
                Assert.Contains(snippet, source, StringComparison.Ordinal);
            }
        }

        private static string NormalizeNewlines(string source) => source.Replace("\r\n", "\n");

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }
    }
}

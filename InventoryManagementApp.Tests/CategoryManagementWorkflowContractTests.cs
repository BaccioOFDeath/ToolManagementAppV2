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

            Assert.Contains("ClearCategoryStateAfterLoadFailure();\n                StatusMessage = \"Categories could not be loaded. Category rows were cleared until reload succeeds.\";", source, StringComparison.Ordinal);
            Assert.Contains("private void ClearCategoryStateAfterLoadFailure()", source, StringComparison.Ordinal);
            Assert.Contains("Categories.Clear();\n            FilteredCategories.Clear();\n            SelectedCategory = null;\n            CategoryName = \"\";\n            RaiseDirectoryProperties();", source, StringComparison.Ordinal);
            Assert.Contains("_saveCommand = new AsyncCommand(SaveAsync, () => SelectedCategory != null && !string.IsNullOrWhiteSpace(CategoryName));", source, StringComparison.Ordinal);
            Assert.Contains("_deleteCommand = new AsyncCommand(DeleteAsync, () => SelectedCategory != null);", source, StringComparison.Ordinal);
            Assert.Contains("WpfMessageBox.Show(\"Categories could not be loaded. Category rows were cleared until reload succeeds. Please retry or check the application log.\"", source, StringComparison.Ordinal);
            Assert.DoesNotContain("StatusMessage = \"Categories could not be loaded. Review logs or retry refresh.\";\n                WpfMessageBox.Show(\"Categories could not be loaded. Please retry or check the application log.\"", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoryMutationFailuresRefreshOrClearVisibleRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CategoryManagementViewModel.cs");

            Assert.Contains("private async Task RefreshCategoryDirectoryAfterMutationFailureAsync(int? preferredSelectedId, string refreshedStatusMessage, string clearedStatusMessage)", source, StringComparison.Ordinal);
            Assert.Contains("var list = await _service.GetCategoriesForInventoryAsync(SelectedInventoryId);\n                Categories.Clear();\n                foreach (var c in list) Categories.Add(new CategoryItem { CategoryID = c.CategoryID, Name = c.Name });\n                ApplyFilter(preferredSelectedId);\n                StatusMessage = refreshedStatusMessage;", source, StringComparison.Ordinal);
            Assert.Contains("_logger.LogWarning(refreshEx, \"Failed to refresh categories after a category mutation failure for inventory {InventoryId}\", SelectedInventoryId);\n                ClearCategoryStateAfterLoadFailure();\n                StatusMessage = clearedStatusMessage;", source, StringComparison.Ordinal);
            Assert.Contains("int? createdCategoryId = null;", source, StringComparison.Ordinal);
            Assert.Contains("createdCategoryId = id;", source, StringComparison.Ordinal);
            Assert.Contains("await RefreshCategoryDirectoryAfterMutationFailureAsync(\n                    createdCategoryId,\n                    $\"Category rows were refreshed after '{name}' failed to finish creating.\",\n                    $\"Category rows were cleared after '{name}' failed to finish creating and recovery reload failed.\");", source, StringComparison.Ordinal);
            Assert.Contains("await RefreshCategoryDirectoryAfterMutationFailureAsync(\n                        id,\n                        $\"Category rows were refreshed after category #{id} could not be renamed.\",\n                        $\"Category rows were cleared after category #{id} could not be renamed and recovery reload failed.\");", source, StringComparison.Ordinal);
            Assert.Contains("await RefreshCategoryDirectoryAfterMutationFailureAsync(\n                    id,\n                    $\"Category rows were refreshed after category #{id} failed to finish saving.\",\n                    $\"Category rows were cleared after category #{id} failed to finish saving and recovery reload failed.\");", source, StringComparison.Ordinal);
            Assert.Contains("await RefreshCategoryDirectoryAfterMutationFailureAsync(\n                        category.CategoryID,\n                        $\"Category rows were refreshed after '{category.Name}' could not be deleted.\",\n                        $\"Category rows were cleared after '{category.Name}' could not be deleted and recovery reload failed.\");", source, StringComparison.Ordinal);
            Assert.Contains("await RefreshCategoryDirectoryAfterMutationFailureAsync(\n                    category.CategoryID,\n                    $\"Category rows were refreshed after '{category.Name}' failed to finish deleting.\",\n                    $\"Category rows were cleared after '{category.Name}' failed to finish deleting and recovery reload failed.\");", source, StringComparison.Ordinal);
            Assert.Contains("Category rows were refreshed from the saved data where possible.", source, StringComparison.Ordinal);
            Assert.DoesNotContain("StatusMessage = $\"Category '{name}' could not be created.\";\n                WpfMessageBox.Show($\"Category '{name}' could not be created. Please retry or check the application log.\"", source, StringComparison.Ordinal);
            Assert.DoesNotContain("StatusMessage = $\"Category #{id} could not be renamed.\";\n                    WpfMessageBox.Show(\"The category was not renamed. Refresh and try again.\"", source, StringComparison.Ordinal);
            Assert.DoesNotContain("StatusMessage = $\"Category '{category.Name}' could not be deleted.\";\n                    WpfMessageBox.Show(\"The category was not deleted. Refresh and try again.\"", source, StringComparison.Ordinal);
        }

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
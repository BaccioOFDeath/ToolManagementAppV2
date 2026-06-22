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

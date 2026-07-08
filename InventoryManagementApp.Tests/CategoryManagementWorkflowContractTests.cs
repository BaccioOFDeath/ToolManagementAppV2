using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CategoryManagementWorkflowContractTests
    {
        [Fact]
        public void CategoryLoadFailuresPreserveRowsWhenAvailableAndClearRowsDuringRecoveryFallback()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CategoryManagementViewModel.cs");

            AssertContainsAll(
                source,
                "_saveCommand = new AsyncCommand(SaveAsync, () => !IsCategoryInteractionBusy && SelectedCategory != null && !string.IsNullOrWhiteSpace(CategoryName));",
                "_deleteCommand = new AsyncCommand(DeleteAsync, () => !IsCategoryInteractionBusy && SelectedCategory != null);",
                "if (_schemaInitialized)",
                "await _service.EnsureInventoryAsync(SelectedInventoryId, \"Main\");",
                "StatusMessage = Categories.Count == 0",
                "? \"Categories could not be loaded. Retry refresh before creating or printing category rows.\"",
                ": \"Category refresh failed. Existing category rows were kept so current work can continue.\";",
                "RaiseDirectoryProperties();",
                "private void ShowCategoryLoadFailureDialogOnce()",
                "if (_loadFailureDialogShown) return;",
                "_loadFailureDialogShown = false;",
                "WpfMessageBox.Show(\"Categories could not be refreshed. Existing category rows were kept when available; retry refresh or check the application log.",
                "private void ClearCategoryStateAfterLoadFailure()",
                "Categories.Clear();",
                "FilteredCategories.Clear();",
                "SelectedCategory = null;",
                "CategoryName = \"\";",
                "RaiseDirectoryProperties();");
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
                "StatusMessage = IsCategoryFilterWindowCapped",
                "? $\"{refreshedStatusMessage} Showing the first {FilteredCategories.Count} matching rows.\"",
                ": refreshedStatusMessage;",
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

        [Fact]
        public void CategoriesPageOwnsSingleInitializationPath()
        {
            var pageSource = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml.cs");
            var mainViewModelSource = ReadRepoFile("InventoryManagementApp", "ViewModels", "MainViewModel.cs");
            var openCategoriesCommand = ExtractSourceBlock(
                mainViewModelSource,
                "OpenCategoriesCommand = new AsyncRelayCommand",
                "OpenSettingsCommand = new AsyncRelayCommand");

            AssertContainsAll(
                pageSource,
                "private Task? _initializeCategoriesTask;",
                "private CategoryManagementViewModel? _initializedViewModel;",
                "DataContextChanged += CategoriesPage_DataContextChanged;",
                "private async Task InitializeCategoriesOnceAsync(CategoryManagementViewModel vm)",
                "if (ReferenceEquals(_initializedViewModel, vm) && _initializeCategoriesTask is { IsCompleted: false })",
                "if (ReferenceEquals(_initializedViewModel, vm) && _initializeCategoriesTask is { IsCompletedSuccessfully: true })",
                "_initializedViewModel = vm;",
                "vm.SelectedInventoryId = _inventoryId;",
                "await Dispatcher.Yield(DispatcherPriority.Background);",
                "_initializeCategoriesTask = vm.InitializeAsync();",
                "await _initializeCategoriesTask;");

            Assert.DoesNotContain("await vm.InitializeAsync();", openCategoriesCommand, StringComparison.Ordinal);
        }

        private static void AssertContainsAll(string source, params string[] expectedSnippets)
        {
            foreach (var snippet in expectedSnippets)
            {
                Assert.Contains(snippet, source, StringComparison.Ordinal);
            }
        }

        private static string NormalizeNewlines(string source) => source.Replace("\r\n", "\n");

        private static string ExtractSourceBlock(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find source block start marker: {startMarker}");

            var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
            Assert.True(end > start, $"Could not find source block end marker: {endMarker}");

            return source[start..end];
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return NormalizeLineEndings(File.ReadAllText(candidate));

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }
        static string NormalizeLineEndings(string text)
            => text.Replace("\r\n", "\n");

    }
}

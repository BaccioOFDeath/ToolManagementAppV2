using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CategoryManagementFilterResponsivenessContractTests
    {
        [Fact]
        public void CategoryViewModel_BoundsFilteredGridWindowAndTracksOmittedMatches()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CategoryManagementViewModel.cs");

            Assert.Contains("private const int MaxVisibleFilteredCategoryRows = 500;", source, StringComparison.Ordinal);
            Assert.Contains("private int _matchedCategoryCount;", source, StringComparison.Ordinal);
            Assert.Contains("private int _omittedFilteredCategoryCount;", source, StringComparison.Ordinal);
            Assert.Contains("public int FullFilteredCategoryCount => _matchedCategoryCount;", source, StringComparison.Ordinal);
            Assert.Contains("public int FilteredCategoryOmittedCount => _omittedFilteredCategoryCount;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsCategoryFilterWindowCapped => FilteredCategoryOmittedCount > 0;", source, StringComparison.Ordinal);
            Assert.Contains("var visible = filtered.Take(MaxVisibleFilteredCategoryRows).ToList();", source, StringComparison.Ordinal);
            Assert.Contains("_omittedFilteredCategoryCount = Math.Max(0, filtered.Count - visible.Count);", source, StringComparison.Ordinal);
            Assert.Contains("ReplaceFilteredCategories(visible);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoryViewModel_UsesFullMatchCountsForAvailabilityEmptyAndPrintState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CategoryManagementViewModel.cs");

            Assert.Contains("public bool IsDirectoryPrintAvailable => !IsCategoryInteractionBusy && FullFilteredCategoryCount > 0;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsCategoryEmptyStateVisible => !IsCategoryInteractionBusy && FullFilteredCategoryCount == 0;", source, StringComparison.Ordinal);
            Assert.Contains("if (FullFilteredCategoryCount == 0) return \"Print is available after categories are loaded or the filter has matches.\";", source, StringComparison.Ordinal);
            Assert.Contains("var printableRows = Math.Min(FilteredCategories.Count, 250);", source, StringComparison.Ordinal);
            Assert.Contains("var omittedFromPrint = Math.Max(0, FullFilteredCategoryCount - printableRows);", source, StringComparison.Ordinal);
            Assert.Contains("Ready to print the first {printableRows} of {FullFilteredCategoryCount}", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoryViewModel_ExposesHonestGridWindowSummariesForLargeFilters()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CategoryManagementViewModel.cs");

            Assert.Contains("public string CategoryVisibleWindowSummary", source, StringComparison.Ordinal);
            Assert.Contains("All matching categories are visible in the grid.", source, StringComparison.Ordinal);
            Assert.Contains("Showing first {FilteredCategories.Count} of {FullFilteredCategoryCount} matching categories", source, StringComparison.Ordinal);
            Assert.Contains("held out of the grid for responsiveness", source, StringComparison.Ordinal);
            Assert.Contains("Showing the first {FilteredCategories.Count} matches.", source, StringComparison.Ordinal);
            Assert.Contains("Loaded {Categories.Count} categories. Showing the first {FilteredCategories.Count} matches so the grid stays responsive.", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoryViewModel_AvoidsUnnecessaryFilteredCollectionChurn()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CategoryManagementViewModel.cs");
            var helper = ExtractSourceBlock(source, "private void ReplaceFilteredCategories", "private async Task ClearSearchAsync");

            Assert.Contains("IReadOnlyList<CategoryItem> visibleCategories", helper, StringComparison.Ordinal);
            Assert.Contains("FilteredCategories.Count == visibleCategories.Count", helper, StringComparison.Ordinal);
            Assert.Contains("ReferenceEquals(FilteredCategories[i], visibleCategories[i])", helper, StringComparison.Ordinal);
            Assert.Contains("if (unchanged) return;", helper, StringComparison.Ordinal);
            Assert.Contains("FilteredCategories.Clear();", helper, StringComparison.Ordinal);
            Assert.Contains("FilteredCategories.Add(category);", helper, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoryViewModel_NotifiesAllDerivedWindowPropertiesWhenDirectoryChanges()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CategoryManagementViewModel.cs");
            var raiseBlock = ExtractSourceBlock(source, "private void RaiseDirectoryProperties", "private void RaiseSelectedCategoryProperties");

            Assert.Contains("OnPropertyChanged(nameof(CategoryVisibleWindowSummary));", raiseBlock, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(FullFilteredCategoryCount));", raiseBlock, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(FilteredCategoryOmittedCount));", raiseBlock, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(IsCategoryFilterWindowCapped));", raiseBlock, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CategoryPrintSummary));", raiseBlock, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(IsDirectoryPrintAvailable));", raiseBlock, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(IsCategoryEmptyStateVisible));", raiseBlock, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoryItem_RaisesDirectoryLabelWhenNameChanges()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CategoryManagementViewModel.cs");

            Assert.Contains("public string Name { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged(); OnPropertyChanged(nameof(DirectoryLabel)); } }", source, StringComparison.Ordinal);
            Assert.Contains("public string DirectoryLabel => $\"#{CategoryID} | {Name}\";", source, StringComparison.Ordinal);
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

        private static string ExtractSourceBlock(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find source block start marker: {startMarker}");

            var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
            Assert.True(end > start, $"Could not find source block end marker: {endMarker}");

            return source[start..end];
        }

        private static string NormalizeLineEndings(string text) => text.Replace("\r\n", "\n");
    }
}

using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class KitManagementFilterResponsivenessContractTests
    {
        [Fact]
        public void KitViewModel_BoundsFilteredGridWindowAndTracksOmittedMatches()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "KitManagementViewModel.cs");

            Assert.Contains("private const int MaxVisibleFilteredKitRows = 500;", source, StringComparison.Ordinal);
            Assert.Contains("private int _matchedKitCount;", source, StringComparison.Ordinal);
            Assert.Contains("private int _omittedFilteredKitCount;", source, StringComparison.Ordinal);
            Assert.Contains("public int FullFilteredKitCount => _matchedKitCount;", source, StringComparison.Ordinal);
            Assert.Contains("public int FilteredKitOmittedCount => _omittedFilteredKitCount;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsKitFilterWindowCapped => FilteredKitOmittedCount > 0;", source, StringComparison.Ordinal);
            Assert.Contains("var visible = matched.Take(MaxVisibleFilteredKitRows).ToList();", source, StringComparison.Ordinal);
            Assert.Contains("_omittedFilteredKitCount = Math.Max(0, matched.Count - visible.Count);", source, StringComparison.Ordinal);
            Assert.Contains("ReplaceFilteredKits(visible);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void KitViewModel_ExposesHonestVisibleWindowAndFilterSummaries()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "KitManagementViewModel.cs");

            Assert.Contains("public string KitVisibleWindowSummary", source, StringComparison.Ordinal);
            Assert.Contains("All matching kit rows are visible in the grid.", source, StringComparison.Ordinal);
            Assert.Contains("Showing first {FilteredKits.Count} of {FullFilteredKitCount} matching kit rows", source, StringComparison.Ordinal);
            Assert.Contains("held out of the grid for responsiveness", source, StringComparison.Ordinal);
            Assert.Contains("Showing the first {FilteredKits.Count} matches so the grid stays responsive.", source, StringComparison.Ordinal);
            Assert.Contains("{matched} matching kit", source, StringComparison.Ordinal);
            Assert.Contains("first {FilteredKits.Count} shown", source, StringComparison.Ordinal);
        }

        [Fact]
        public void KitViewModel_UsesFullMatchCountForPrintMessagingAndPreviewAccounting()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "KitManagementViewModel.cs");

            Assert.Contains("var printableRows = Math.Min(FilteredKits.Count, MaxDirectoryPrintRows);", source, StringComparison.Ordinal);
            Assert.Contains("var omittedFromPrint = Math.Max(0, FullFilteredKitCount - printableRows);", source, StringComparison.Ordinal);
            Assert.Contains("Ready to print the first {printableRows} of {FullFilteredKitCount} matching kit rows", source, StringComparison.Ordinal);
            Assert.Contains("var omittedCount = Math.Max(0, FullFilteredKitCount - printedKits.Count);", source, StringComparison.Ordinal);
            Assert.Contains("Matched {FullFilteredKitCount} | Grid window {visibleKits.Count} | Printed {printedKits.Count} | Omitted {omittedCount}", source, StringComparison.Ordinal);
            Assert.Contains("Large filtered directories print the first 250 matching rows to keep preview responsive.", source, StringComparison.Ordinal);
        }

        [Fact]
        public void KitViewModel_AvoidsUnnecessaryFilteredCollectionChurn()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "KitManagementViewModel.cs");
            var helper = ExtractSourceBlock(source, "private void ReplaceFilteredKits", "private void RefreshKitItemSummaries");

            Assert.Contains("IReadOnlyList<Kit> visibleKits", helper, StringComparison.Ordinal);
            Assert.Contains("FilteredKits.Count == visibleKits.Count", helper, StringComparison.Ordinal);
            Assert.Contains("ReferenceEquals(FilteredKits[i], visibleKits[i])", helper, StringComparison.Ordinal);
            Assert.Contains("if (unchanged) return;", helper, StringComparison.Ordinal);
            Assert.Contains("FilteredKits.Clear();", helper, StringComparison.Ordinal);
            Assert.Contains("FilteredKits.Add(kit);", helper, StringComparison.Ordinal);
        }

        [Fact]
        public void KitViewModel_ResetsAndNotifiesCappedDirectoryState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "KitManagementViewModel.cs");
            var failureBlock = ExtractSourceBlock(source, "private void ClearKitStateAfterLoadFailure", "private async Task LoadKitItemsAsync");
            var raiseBlock = ExtractSourceBlock(source, "private void RaiseDirectoryStateChanged", "private void RaiseKitItemStateChanged");

            Assert.Contains("_matchedKitCount = 0;", failureBlock, StringComparison.Ordinal);
            Assert.Contains("_omittedFilteredKitCount = 0;", failureBlock, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(FullFilteredKitCount));", raiseBlock, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(FilteredKitOmittedCount));", raiseBlock, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(IsKitFilterWindowCapped));", raiseBlock, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(KitVisibleWindowSummary));", raiseBlock, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(KitResultsSummary));", raiseBlock, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(KitFilterSummary));", raiseBlock, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(KitPrintSummary));", raiseBlock, StringComparison.Ordinal);
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
            Assert.True(end > start, $"Could not find source block end marker after {startMarker}: {endMarker}");

            return source[start..end];
        }

        private static string NormalizeLineEndings(string text) => text.Replace("\r\n", "\n");
    }
}

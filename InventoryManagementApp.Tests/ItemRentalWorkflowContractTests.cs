using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemRentalWorkflowContractTests
    {
        [Fact]
        public void ItemRentActionsRefreshFilteredRowsAndSelectionAfterSuccessfulRental()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ItemManagementViewModel.cs");

            Assert.Equal(2, CountOccurrences(source, "await ReloadItemsAfterRentalAsync(item.ItemID, cancellationToken);"));
            Assert.Contains("private Task ReloadItemsAfterRentalAsync(int itemId, CancellationToken cancellationToken)", source, StringComparison.Ordinal);
            Assert.Contains("return ReloadItemsAfterItemWorkflowAsync(itemId, cancellationToken);", source, StringComparison.Ordinal);
            Assert.Contains("private async Task ReloadItemsAfterItemWorkflowAsync(int itemId, CancellationToken cancellationToken)", source, StringComparison.Ordinal);
            Assert.Contains("await LoadItemsAsync(new ItemPage(1, PageSize));", source, StringComparison.Ordinal);
            Assert.Contains("await FilterItemsAsync();", source, StringComparison.Ordinal);
            Assert.Contains("SelectedItem = SearchResults.FirstOrDefault(t => t.ItemID == itemId)", source, StringComparison.Ordinal);
            Assert.Contains("?? Items.FirstOrDefault(t => t.ItemID == itemId)", source, StringComparison.Ordinal);
            Assert.Contains("?? CheckedOutItems.FirstOrDefault(t => t.ItemID == itemId)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemCheckoutToggleRefreshesAllItemCollectionsAfterSuccessfulToggle()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ItemManagementViewModel.cs");

            Assert.Contains("private Task ReloadItemsAfterCheckoutAsync(ItemModel item, CancellationToken cancellationToken)", source, StringComparison.Ordinal);
            Assert.Contains("return ReloadItemAfterCheckoutAsync(item, cancellationToken);", source, StringComparison.Ordinal);
            Assert.Contains("var refreshed = await _itemService.GetItemByIDAsync(itemId, cancellationToken).ConfigureAwait(false);", source, StringComparison.Ordinal);
            Assert.Contains("var existingRows = new[] { item }", source, StringComparison.Ordinal);
            Assert.Contains("ApplyItemState(row, refreshed);", source, StringComparison.Ordinal);
            Assert.Contains("var result = await _itemService.ToggleItemCheckOutStatusAsync(item.ItemID, cancellationToken).ConfigureAwait(false);", source, StringComparison.Ordinal);
            Assert.Contains("await ReloadItemsAfterCheckoutAsync(item, cancellationToken);", source, StringComparison.Ordinal);
            Assert.Contains("?? CheckedOutItems.FirstOrDefault(t => t.ItemID == itemId)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemCheckoutToggleFailureRefreshesAndExplainsTheConflict()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ItemManagementViewModel.cs");

            Assert.Contains("if (!result)", source, StringComparison.Ordinal);
            Assert.Contains("await ReloadItemsAfterCheckoutAsync(item, cancellationToken);", source, StringComparison.Ordinal);
            Assert.Contains("Check-out status could not be updated. The item may have been changed by another user; the list has been refreshed.", source, StringComparison.Ordinal);
            Assert.Contains("\"Check-out Status\"", source, StringComparison.Ordinal);
            Assert.DoesNotContain("if (!result) return;", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemWorkflowExceptionsRefreshListsAndExplainPossibleSavedState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ItemManagementViewModel.cs");

            Assert.Contains("private async Task RefreshItemsAfterWorkflowFailureAsync(int itemId, CancellationToken cancellationToken)", source, StringComparison.Ordinal);
            Assert.Contains("await ReloadItemsAfterItemWorkflowAsync(itemId, cancellationToken);", source, StringComparison.Ordinal);
            Assert.Contains("_logger.LogError(refreshEx, \"Failed to refresh items after workflow failure for item {ItemID}\", itemId);", source, StringComparison.Ordinal);
            Assert.Equal(3, CountOccurrences(source, "await RefreshItemsAfterWorkflowFailureAsync(item.ItemID, cancellationToken);"));
            Assert.Equal(2, CountOccurrences(source, "The item list has been refreshed in case the rental was saved before the failure."));
            Assert.Contains("Failed to update check-out status: {ex.Message} The item list has been refreshed in case the check-out status changed before the failure.", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowInfoAsync($\"Failed to rent {LabelProvider.Instance.ItemLabelSingular.ToLower()}: {ex.Message}\", \"Error\");", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowInfoAsync($\"Failed to update check-out status: {ex.Message}\", \"Error\");", source, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryLoadFailureShowsOperatorFeedback()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ItemManagementViewModel.cs");

            Assert.Contains("_logger.LogError(ex, \"Failed to open rental history for {ItemLabelSingular} {ItemID}\"", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync($\"Failed to load rental history: {ex.Message}\", \"Error\");", source, StringComparison.Ordinal);
        }

        private static int CountOccurrences(string source, string value)
        {
            var count = 0;
            var index = 0;

            while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
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

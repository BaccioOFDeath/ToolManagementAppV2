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
        public void IncrementalItemMutationsRefreshRowsAfterOperationFailures()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ItemsViewModel.cs");

            Assert.Contains("private async Task<bool> RefreshItemsAfterMutationFailureAsync(int? preferredItemId, CancellationToken cancellationToken)", source, StringComparison.Ordinal);
            Assert.Contains("var firstPage = await LoadPageAsync(1, cancellationToken).ConfigureAwait(false);", source, StringComparison.Ordinal);
            Assert.Contains("Items.ResetWith(firstPage);", source, StringComparison.Ordinal);
            Assert.Contains("SelectedItem = preferredItemId.HasValue", source, StringComparison.Ordinal);
            Assert.Contains("Items.FirstOrDefault(item => item.ItemID == preferredItemId.Value)", source, StringComparison.Ordinal);
            Assert.Contains("Items.Reset();", source, StringComparison.Ordinal);
            Assert.Contains("The item list has been refreshed in case saved state changed before the failure.", source, StringComparison.Ordinal);
            Assert.Contains("The item list could not be refreshed, so visible item rows were cleared until reload succeeds.", source, StringComparison.Ordinal);
            Assert.Contains("await RefreshItemsAfterMutationFailureAsync(updated.ItemID, ct).ConfigureAwait(false);", source, StringComparison.Ordinal);
            Assert.Contains("await RefreshItemsAfterMutationFailureAsync(item.ItemID > 0 ? item.ItemID : null, ct).ConfigureAwait(false);", source, StringComparison.Ordinal);
            Assert.Contains("await RefreshItemsAfterMutationFailureAsync(toRemove.FirstOrDefault()?.ItemID, ct).ConfigureAwait(false);", source, StringComparison.Ordinal);
            Assert.Contains("await RefreshItemsAfterMutationFailureAsync(edits.FirstOrDefault()?.ItemID, ct).ConfigureAwait(false);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Failed to save changes: {ex.Message}", source, StringComparison.Ordinal);
        }

        [Fact]
        public void IncrementalItemEditAndCreateDialogFailuresShowOperatorFeedback()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ItemsViewModel.cs");

            Assert.Contains("var selected = SelectedItem;", source, StringComparison.Ordinal);
            Assert.Contains("if (selected == null) return;", source, StringComparison.Ordinal);
            Assert.Contains("ItemID = selected.ItemID,", source, StringComparison.Ordinal);
            Assert.Contains("_logger.LogError(ex, \"Failed to open edit item dialog for item {ItemID}\", selected.ItemID);", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync($\"Failed to open edit item dialog: {ex.Message}\", \"Error\").ConfigureAwait(false);", source, StringComparison.Ordinal);
            Assert.Contains("_logger.LogError(ex, \"Failed to open new item dialog\");", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync($\"Failed to open new item dialog: {ex.Message}\", \"Error\").ConfigureAwait(false);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("catch\n            {\n                return;\n            }", source, StringComparison.Ordinal);
            Assert.DoesNotContain("ItemID = SelectedItem.ItemID,", source, StringComparison.Ordinal);
        }

        [Fact]
        public void IncrementalItemDetailsAndHistoryFailuresShowOperatorFeedback()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ItemsViewModel.cs");

            Assert.Contains("var item = SelectedItem;", source, StringComparison.Ordinal);
            Assert.Contains("_dialogService.ShowItemDetails(item);", source, StringComparison.Ordinal);
            Assert.Contains("_logger.LogError(ex, \"Failed to open item details for item {ItemID}\", item.ItemID);", source, StringComparison.Ordinal);
            Assert.Contains("_dialogService.ShowInfo($\"Failed to open item details: {ex.Message}\", \"Error\");", source, StringComparison.Ordinal);
            Assert.Contains("var history = await _rentalService.GetRentalHistoryForItemAsync(item.ItemID).ConfigureAwait(false);", source, StringComparison.Ordinal);
            Assert.Contains("_dialogService.ShowRentalHistory(item, history);", source, StringComparison.Ordinal);
            Assert.Contains("_logger.LogError(ex, \"Failed to open incremental rental history for item {ItemID}\", item.ItemID);", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync($\"Failed to load rental history: {ex.Message}\", \"Error\").ConfigureAwait(false);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("_dialogService.ShowItemDetails(SelectedItem);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("_dialogService.ShowRentalHistory(SelectedItem, history);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemLoadAndSearchFailuresClearStaleVisibleRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ItemManagementViewModel.cs");

            Assert.Contains("private void ClearItemStateAfterLoadFailure()", source, StringComparison.Ordinal);
            Assert.Contains("Items.Clear();", source, StringComparison.Ordinal);
            Assert.Contains("SearchResults.Clear();", source, StringComparison.Ordinal);
            Assert.Contains("CheckedOutItems.Clear();", source, StringComparison.Ordinal);
            Assert.Contains("Categories.ReplaceRange(new[] { \"All\" });", source, StringComparison.Ordinal);
            Assert.Contains("_selectedCategory = \"All\";", source, StringComparison.Ordinal);
            Assert.Contains("SelectedItem = null;", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(SearchResultsSummary));", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CheckedOutSummary));", source, StringComparison.Ordinal);
            Assert.Equal(2, CountOccurrences(source, "ClearItemStateAfterLoadFailure();"));
            Assert.Contains("_logger.LogError(ex, \"Failed to load item directory\");", source, StringComparison.Ordinal);
            Assert.Contains("_logger.LogError(ex, \"Failed to search item directory\");", source, StringComparison.Ordinal);
            Assert.Contains("Failed to load {LabelProvider.Instance.ItemLabelPlural.ToLower()}: {ex.Message} Visible item rows were cleared until reload succeeds.", source, StringComparison.Ordinal);
            Assert.Contains("Failed to search {LabelProvider.Instance.ItemLabelPlural.ToLower()}: {ex.Message} Visible item rows were cleared until reload succeeds.", source, StringComparison.Ordinal);
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

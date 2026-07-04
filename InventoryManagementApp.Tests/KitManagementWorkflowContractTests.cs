using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class KitManagementWorkflowContractTests
    {
        [Fact]
        public void KitLoadFailuresClearStaleRowsSelectionAndItems()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "KitManagementViewModel.cs");

            AssertContainsAll(
                source,
                "ClearKitStateAfterLoadFailure();",
                "await _dialogService.ShowErrorAsync(\"Error loading kits\", $\"{ex.Message} Kit rows were cleared until reload succeeds.\");",
                "private void ClearKitStateAfterLoadFailure()",
                "Kits.Clear();",
                "FilteredKits.Clear();",
                "SelectedKit = null;",
                "SelectedKitItem = null;",
                "KitItems.Clear();",
                "RefreshKitItemSummaries();",
                "OnPropertyChanged(nameof(KitResultsSummary));",
                "OnPropertyChanged(nameof(SelectedKitAvailabilitySummary));",
                "PrintKitListCommand.NotifyCanExecuteChanged();",
                "private bool CanEditOrDelete() => SelectedKit != null && !IsKitInteractionBusy && !IsLoadingKitItems;",
                "private bool CanEditOrRemoveKitItem() => SelectedKitItem != null && !IsKitItemInteractionBusy;");
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error loading kits\", ex.Message);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void KitItemLoadFailuresClearStaleMemberRowsAndSelection()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "KitManagementViewModel.cs");

            AssertContainsAll(
                source,
                "var selectedKitItemId = SelectedKitItem?.KitItemID;",
                "ClearKitItemsForReload();",
                "private void ClearKitItemsForReload()",
                "KitItems.Clear();",
                "SelectedKitItem = null;",
                "RefreshKitItemSummaries();",
                "await _dialogService.ShowErrorAsync(\"Error loading kit items\", $\"{ex.Message} Kit item rows were cleared until reload succeeds.\");");
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error loading kit items\", ex.Message);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void KitItemMutationFailuresRefreshOrClearMemberRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "KitManagementViewModel.cs");

            AssertContainsAll(
                source,
                "private async Task RefreshKitItemsAfterMutationFailureAsync(string title, string message)",
                "await ReloadKitItemsForRecoveryAsync(SelectedKit.KitID);",
                "Kit item rows were refreshed in case the membership list changed before the failure.",
                "ClearKitItemsForReload();",
                "Kit item rows were cleared because refresh also failed: {refreshEx.Message}",
                "private async Task ReloadKitItemsForRecoveryAsync(int kitID)");
            Assert.True(
                CountOccurrences(source, "await RefreshKitItemsAfterMutationFailureAsync(\"Error") >= 3,
                "Expected add, edit, and remove kit-item failure paths to refresh or clear member rows.");
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error adding item to kit\", ex.Message);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error updating kit item\", ex.Message);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error removing item from kit\", ex.Message);", source, StringComparison.Ordinal);
        }

        private static void AssertContainsAll(string source, params string[] expectedSnippets)
        {
            foreach (var snippet in expectedSnippets)
            {
                Assert.Contains(snippet, source, StringComparison.Ordinal);
            }
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

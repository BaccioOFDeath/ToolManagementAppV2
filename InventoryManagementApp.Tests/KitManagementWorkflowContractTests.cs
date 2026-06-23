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

            Assert.Contains("ClearKitStateAfterLoadFailure();\n                await _dialogService.ShowErrorAsync(\"Error loading kits\", $\"{ex.Message} Kit rows were cleared until reload succeeds.\");", source, StringComparison.Ordinal);
            Assert.Contains("private void ClearKitStateAfterLoadFailure()", source, StringComparison.Ordinal);
            Assert.Contains("Kits.Clear();\n            FilteredKits.Clear();\n            SelectedKit = null;\n            SelectedKitItem = null;\n            KitItems.Clear();", source, StringComparison.Ordinal);
            Assert.Contains("RefreshKitItemSummaries();\n            OnPropertyChanged(nameof(KitResultsSummary));\n            OnPropertyChanged(nameof(SelectedKitAvailabilitySummary));\n            PrintKitListCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
            Assert.Contains("private bool CanEditOrDelete() => SelectedKit != null;", source, StringComparison.Ordinal);
            Assert.Contains("private bool CanEditOrRemoveKitItem() => SelectedKitItem != null;", source, StringComparison.Ordinal);
            Assert.DoesNotContain("catch (Exception ex)\n            {\n                await _dialogService.ShowErrorAsync(\"Error loading kits\", ex.Message);\n            }", source, StringComparison.Ordinal);
        }

        [Fact]
        public void KitItemLoadFailuresClearStaleMemberRowsAndSelection()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "KitManagementViewModel.cs");

            Assert.Contains("var selectedKitItemId = SelectedKitItem?.KitItemID;\n            ClearKitItemsForReload();", source, StringComparison.Ordinal);
            Assert.Contains("private void ClearKitItemsForReload()", source, StringComparison.Ordinal);
            Assert.Contains("KitItems.Clear();\n            SelectedKitItem = null;\n            RefreshKitItemSummaries();", source, StringComparison.Ordinal);
            Assert.Contains("ClearKitItemsForReload();\n                await _dialogService.ShowErrorAsync(\"Error loading kit items\", $\"{ex.Message} Kit item rows were cleared until reload succeeds.\");", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error loading kit items\", ex.Message);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void KitItemMutationFailuresRefreshOrClearMemberRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "KitManagementViewModel.cs");

            Assert.Contains("private async Task RefreshKitItemsAfterMutationFailureAsync(string title, string message)", source, StringComparison.Ordinal);
            Assert.Contains("await ReloadKitItemsForRecoveryAsync(SelectedKit.KitID);", source, StringComparison.Ordinal);
            Assert.Contains("Kit item rows were refreshed in case the membership list changed before the failure.", source, StringComparison.Ordinal);
            Assert.Contains("ClearKitItemsForReload();\n                await _dialogService.ShowErrorAsync(title, $\"{message} Kit item rows were cleared because refresh also failed: {refreshEx.Message}\");", source, StringComparison.Ordinal);
            Assert.Contains("private async Task ReloadKitItemsForRecoveryAsync(int kitID)", source, StringComparison.Ordinal);
            Assert.Equal(3, CountOccurrences(source, "await RefreshKitItemsAfterMutationFailureAsync(\"Error"));
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error adding item to kit\", ex.Message);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error updating kit item\", ex.Message);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error removing item from kit\", ex.Message);", source, StringComparison.Ordinal);
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

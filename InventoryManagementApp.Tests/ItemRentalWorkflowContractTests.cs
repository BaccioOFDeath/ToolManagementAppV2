using System;
using System.IO;
using System.Linq;
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
            Assert.Contains("private async Task ReloadItemsAfterRentalAsync(int itemId, CancellationToken cancellationToken)", source, StringComparison.Ordinal);
            Assert.Contains("await LoadItemsAsync(new ItemPage(1, PageSize));", source, StringComparison.Ordinal);
            Assert.Contains("await FilterItemsAsync();", source, StringComparison.Ordinal);
            Assert.Contains("SelectedItem = SearchResults.FirstOrDefault(t => t.ItemID == itemId)", source, StringComparison.Ordinal);
            Assert.Contains("?? Items.FirstOrDefault(t => t.ItemID == itemId)", source, StringComparison.Ordinal);
            Assert.Contains("?? CheckedOutItems.FirstOrDefault(t => t.ItemID == itemId)", source, StringComparison.Ordinal);
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

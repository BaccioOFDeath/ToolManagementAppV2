using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class ItemWorkflowRefreshContractTests
    {
        [Fact]
        public void ItemWorkflowFailurePathsRefreshRowsBeforeShowingOperatorErrors()
        {
            var source = ReadRepositoryFile("InventoryManagementApp/ViewModels/ItemManagementViewModel.cs");

            Assert.Contains("private async Task RefreshItemsAfterWorkflowFailureAsync(int itemId, CancellationToken cancellationToken)", source);
            Assert.Contains("await ReloadItemsAfterItemWorkflowAsync(itemId, cancellationToken);", source);
            Assert.Contains("Failed to refresh items after workflow failure for item {ItemID}", source);
            Assert.Contains("SearchResults.FirstOrDefault(t => t.ItemID == itemId)", source);
            Assert.Contains("CheckedOutItems.FirstOrDefault(t => t.ItemID == itemId)", source);

            Assert.True(
                CountOccurrences(source, "await RefreshItemsAfterWorkflowFailureAsync(item.ItemID, cancellationToken);") >= 3,
                "Rent and check-out exception paths should refresh the item lists before notifying the operator.");
            Assert.Contains("The item list has been refreshed in case the rental was saved before the failure.", source);
            Assert.Contains("The item list has been refreshed in case the check-out status changed before the failure.", source);
        }

        private static string ReadRepositoryFile(string relativePath)
        {
            var root = FindRepositoryRoot();
            return File.ReadAllText(Path.Combine(root, relativePath));
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "InventoryManagementApp.sln")))
                    return directory.FullName;

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not find repository root containing InventoryManagementApp.sln.");
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
    }
}

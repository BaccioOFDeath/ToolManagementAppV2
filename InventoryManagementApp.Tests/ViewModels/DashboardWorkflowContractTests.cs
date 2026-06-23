using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class DashboardWorkflowContractTests
    {
        [Fact]
        public void DashboardLoadFailures_ClearPaneRowsAndSelectionState()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "DashboardViewModel.cs");

            Assert.Contains("ClearDashboardStatsAfterLoadFailure();", source, StringComparison.Ordinal);
            Assert.Contains("ClearRecentActivityAfterLoadFailure();", source, StringComparison.Ordinal);
            Assert.Contains("ClearCheckedOutItemsAfterLoadFailure();", source, StringComparison.Ordinal);
            Assert.Contains("ClearRentedItemsAfterLoadFailure();", source, StringComparison.Ordinal);
            Assert.Contains("ClearCommonlyUsedItemsAfterLoadFailure();", source, StringComparison.Ordinal);
            Assert.Contains("ClearIncompleteItemsAfterLoadFailure();", source, StringComparison.Ordinal);

            Assert.Contains("private void ClearDashboardStatsAfterLoadFailure()", source, StringComparison.Ordinal);
            Assert.Contains("StatCards.Clear();", source, StringComparison.Ordinal);
            Assert.Contains("RecentActivity.Clear();", source, StringComparison.Ordinal);
            Assert.Contains("SelectedActivity = null;", source, StringComparison.Ordinal);
            Assert.Contains("CheckedOutItems.Clear();", source, StringComparison.Ordinal);
            Assert.Contains("SelectedCheckedOutItem = null;", source, StringComparison.Ordinal);
            Assert.Contains("RentedItems.Clear();", source, StringComparison.Ordinal);
            Assert.Contains("SelectedRental = null;", source, StringComparison.Ordinal);
            Assert.Contains("CommonlyUsedItems.Clear();", source, StringComparison.Ordinal);
            Assert.Contains("SelectedCommonlyUsedItem = null;", source, StringComparison.Ordinal);
            Assert.Contains("IncompleteItems.Clear();", source, StringComparison.Ordinal);
            Assert.Contains("SelectedIncompleteItem = null;", source, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardLoadFailures_RefreshSummariesAndCommandState()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "DashboardViewModel.cs");

            Assert.Contains("ClearActivitySelectionIfMissing();", source, StringComparison.Ordinal);
            Assert.Contains("private void ClearActivitySelectionIfMissing()", source, StringComparison.Ordinal);
            Assert.Contains("RecentActivity.All(log => log.LogID != SelectedActivity.LogID)", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(OperationsSummary));", source, StringComparison.Ordinal);
            Assert.Contains("OpenActivityDestinationCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
            Assert.Contains("CheckInSelectedItemCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
            Assert.Contains("ReturnSelectedRentalCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
        }

        static string ReadRepositoryFile(params string[] relativePathParts)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "InventoryManagementApp.sln")))
                directory = directory.Parent;

            Assert.NotNull(directory);
            var path = Path.Combine(directory!.FullName, Path.Combine(relativePathParts));
            Assert.True(File.Exists(path), $"Expected repository file at {path}");
            return File.ReadAllText(path);
        }
    }
}

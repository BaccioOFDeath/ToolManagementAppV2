using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ManageRentalsSelectionContractTests
    {
        [Fact]
        public void RentalsReloadRestoresSelectionFromFreshFilteredRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ManageRentalsViewModel.cs");

            AssertContainsAll(
                source,
                "var selectedRentalId = SelectedRental?.RentalID;",
                "ApplyFilter(selectedRentalId);",
                "void ApplyFilter() => ApplyFilter(SelectedRental?.RentalID);",
                "void ApplyFilter(int? selectedRentalId)",
                "Rentals.ReplaceRange(filtered.ToList());",
                "RestoreSelectedRental(selectedRentalId);",
                "void RestoreSelectedRental(int? selectedRentalId)",
                "SelectedRental = Rentals.FirstOrDefault(r => r.RentalID == selectedRentalId.Value);");
        }

        [Fact]
        public void RentalsLoadFailuresClearStaleRowsAndDisableRentalActions()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ManageRentalsViewModel.cs");

            AssertContainsAll(
                source,
                "ClearRentalStateAfterLoadFailure();",
                "Failed to load rentals: {ex.Message} Rental rows were cleared until reload succeeds.",
                "void ClearRentalStateAfterLoadFailure()",
                "_allRentals.Clear();",
                "Rentals.Clear();",
                "ActiveRentals.Clear();",
                "SelectedRental = null;",
                "OnPropertyChanged(nameof(SearchSummary));",
                "OnPropertyChanged(nameof(CheckedOutSummary));",
                "OnPropertyChanged(nameof(SelectedRequestHolderLine));",
                "OnPropertyChanged(nameof(SelectedRequestNextAction));");
            Assert.DoesNotContain("await _dialogService.ShowInfoAsync($\"Failed to load rentals: {ex.Message}\", \"Error\");", source, StringComparison.Ordinal);
        }

        [Fact]
        public void MissingSelectionAfterFilterClearsRentalActions()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ManageRentalsViewModel.cs");

            AssertContainsAll(
                source,
                "if (!selectedRentalId.HasValue)",
                "if (SelectedRental != null && !Rentals.Contains(SelectedRental))",
                "SelectedRental = null;",
                "bool CanReturnSelectedRental() => SelectedRental != null && IsRentalActive(SelectedRental);",
                "bool CanPlaceRequestForSelectedRental() => SelectedRental != null && IsRentalActive(SelectedRental);");
        }

        [Fact]
        public void RequestStatusUpdatesRefreshOpenRequestQueue()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ManageRentalsViewModel.cs");

            AssertContainsAll(
                source,
                "var updated = await _reservationService.ConfirmReservationAsync(requestId);",
                "var updated = await _reservationService.CancelReservationAsync(request.ReservationID);",
                "The selected request could not be confirmed. It may have been removed or changed by another user. The open request queue has been refreshed.",
                "The selected request could not be cancelled. It may have been removed or changed by another user. The open request queue has been refreshed.",
                "Failed to confirm request: {ex.Message} The open request queue has been refreshed in case the request status changed before the failure.",
                "Failed to cancel request: {ex.Message} The open request queue has been refreshed in case the request status changed before the failure.");
            Assert.True(
                CountOccurrences(source, "await LoadPendingRequestsAsync();") >= 6,
                "Expected request status, placement, and rental recovery paths to refresh the open request queue.");
        }

        [Fact]
        public void OpenRequestRefreshFailuresAreContainedBeforeStatusErrorDialogs()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ManageRentalsViewModel.cs");

            AssertContainsAll(
                source,
                "async Task LoadPendingRequestsAsync(bool notifyOnFailure = false)",
                "_logger.LogError(ex, \"Failed to load open reservations for rentals page\");",
                "PendingRequests.Clear();",
                "SelectedRequest = null;",
                "OnPropertyChanged(nameof(RequestSummary));",
                "Failed to confirm request: {ex.Message}",
                "Failed to cancel request: {ex.Message}");
            Assert.DoesNotContain("await LoadPendingRequestsAsync(notifyOnFailure: true);\n                await _dialogService.ShowInfoAsync($\"Failed to confirm request", NormalizeNewlines(source), StringComparison.Ordinal);
            Assert.DoesNotContain("await LoadPendingRequestsAsync(notifyOnFailure: true);\n                await _dialogService.ShowInfoAsync($\"Failed to cancel request", NormalizeNewlines(source), StringComparison.Ordinal);
        }

        [Fact]
        public void RequestPlacementAndQueueLoadFailuresRefreshAndExplainState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ManageRentalsViewModel.cs");

            AssertContainsAll(
                source,
                "await LoadPendingRequestsAsync(notifyOnFailure: true);",
                "async Task LoadPendingRequestsAsync(bool notifyOnFailure = false)",
                "PendingRequests.Clear();",
                "SelectedRequest = null;",
                "if (notifyOnFailure)",
                "Failed to load open requests: {ex.Message} The open request queue has been cleared until it can be refreshed.",
                "Failed to place request: {ex.Message} The open request queue has been refreshed in case the request was saved before the failure.");
        }

        [Fact]
        public void RentalOperationFailuresRefreshDeskAndExplainState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ManageRentalsViewModel.cs");

            AssertContainsAll(
                source,
                "async Task RefreshRentalDeskAfterOperationFailureAsync(int rentalId)",
                "_allRentals = await _rentalService.GetAllRentalsAsync();",
                "await LoadPendingRequestsAsync();",
                "RefreshActiveRentals();",
                "ApplyFilter(rentalId);",
                "var rentalToExtend = SelectedRental;",
                "await _rentalService.ExtendRentalAsync(rentalToExtend.RentalID, newDueDate);",
                "_logger.LogError(ex, \"Failed to extend rental {RentalID}\", rentalToExtend.RentalID);",
                "_logger.LogError(ex, \"Failed to delete rental {RentalID}\", rentalToDelete.RentalID);",
                "The rental desk has been refreshed so current rental actions match the latest saved state.");
            Assert.True(
                CountOccurrences(source, "await RefreshRentalDeskAfterOperationFailureAsync(") >= 3,
                "Expected check-in, extend, and delete failure paths to refresh the rental desk.");
            Assert.DoesNotContain("_logger.LogError(ex, \"Failed to extend rental {RentalID}\", SelectedRental.RentalID);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("_logger.LogError(ex, \"Failed to delete rental {RentalID}\", rentalToDelete?.RentalID);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalGridRightClickSelectionUsesSharedSafeTreeTraversal()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml.cs");
            var helper = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "GridContextMenuSelection.cs");

            AssertContainsAll(
                source,
                "var row = GridContextMenuSelection.SelectRow(sender, e);",
                "GridContextMenuSelection.FindAncestor<System.Windows.Controls.DataGrid>(focusedElement)");
            Assert.DoesNotContain("VisualTreeHelper.GetParent", source, StringComparison.Ordinal);
            Assert.DoesNotContain("private static DependencyObject? GetParent", source, StringComparison.Ordinal);
            AssertContainsAll(
                helper,
                "return TryGetVisualParent(current)",
                "?? TryGetLogicalParent(current)",
                "?? TryGetFrameworkParent(current);",
                "FrameworkElement element => element.Parent",
                "FrameworkContentElement contentElement => contentElement.Parent");
            Assert.True(
                CountOccurrences(helper, "ex is InvalidOperationException") >= 2,
                "Expected guarded visual and logical tree traversal to keep invalid WPF parent states non-fatal.");
            Assert.DoesNotContain("return LogicalTreeHelper.GetParent(current);", helper, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalDetailsOpenDedicatedJobWindowInsteadOfInfoNotice()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ManageRentalsViewModel.cs");
            var window = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalJobDetailsWindow.xaml");

            AssertContainsAll(
                source,
                "var window = new RentalJobDetailsWindow(this);",
                "window.ShowDialog();",
                "BuildRentalDetailsText(SelectedRental)");
            Assert.DoesNotContain("_dialogService.ShowInfo(details.ToString(), $\"Rental Details - {rental.ItemNumber}\");", source, StringComparison.Ordinal);
            AssertContainsAll(
                window,
                "Rental Job",
                "SelectedRental",
                "NullToDefaultImageConverter",
                "CheckInCommand",
                "ExtendCommand",
                "PlaceRequestCommand",
                "PrintRentalCommand",
                "OpenHistoryCommand");
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

        private static string NormalizeNewlines(string source) => source.Replace("\r\n", "\n");

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

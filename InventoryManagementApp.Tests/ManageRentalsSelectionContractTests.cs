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

            Assert.Contains("var selectedRentalId = SelectedRental?.RentalID;", source, StringComparison.Ordinal);
            Assert.Contains("ApplyFilter(selectedRentalId);", source, StringComparison.Ordinal);
            Assert.Contains("void ApplyFilter() => ApplyFilter(SelectedRental?.RentalID);", source, StringComparison.Ordinal);
            Assert.Contains("void ApplyFilter(int? selectedRentalId)", source, StringComparison.Ordinal);
            Assert.Contains("Rentals.ReplaceRange(filtered.ToList());", source, StringComparison.Ordinal);
            Assert.Contains("RestoreSelectedRental(selectedRentalId);", source, StringComparison.Ordinal);
            Assert.Contains("void RestoreSelectedRental(int? selectedRentalId)", source, StringComparison.Ordinal);
            Assert.Contains("SelectedRental = Rentals.FirstOrDefault(r => r.RentalID == selectedRentalId.Value);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalsLoadFailuresClearStaleRowsAndDisableRentalActions()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ManageRentalsViewModel.cs");

            Assert.Contains("ClearRentalStateAfterLoadFailure();\n                await _dialogService.ShowInfoAsync($\"Failed to load rentals: {ex.Message} Rental rows were cleared until reload succeeds.\", \"Error\");", source, StringComparison.Ordinal);
            Assert.Contains("void ClearRentalStateAfterLoadFailure()", source, StringComparison.Ordinal);
            Assert.Contains("_allRentals.Clear();\n            Rentals.Clear();\n            ActiveRentals.Clear();\n            SelectedRental = null;", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(SearchSummary));\n            OnPropertyChanged(nameof(CheckedOutSummary));", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(SelectedRequestHolderLine));\n            OnPropertyChanged(nameof(SelectedRequestNextAction));", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowInfoAsync($\"Failed to load rentals: {ex.Message}\", \"Error\");", source, StringComparison.Ordinal);
        }

        [Fact]
        public void MissingSelectionAfterFilterClearsRentalActions()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ManageRentalsViewModel.cs");

            Assert.Contains("if (!selectedRentalId.HasValue)", source, StringComparison.Ordinal);
            Assert.Contains("if (SelectedRental != null && !Rentals.Contains(SelectedRental))", source, StringComparison.Ordinal);
            Assert.Contains("SelectedRental = null;", source, StringComparison.Ordinal);
            Assert.Contains("bool CanReturnSelectedRental() => SelectedRental != null && IsRentalActive(SelectedRental);", source, StringComparison.Ordinal);
            Assert.Contains("bool CanPlaceRequestForSelectedRental() => SelectedRental != null && IsRentalActive(SelectedRental);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void RequestStatusUpdatesRefreshOpenRequestQueue()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ManageRentalsViewModel.cs");

            Assert.Contains("var updated = await _reservationService.ConfirmReservationAsync(requestId);", source, StringComparison.Ordinal);
            Assert.Contains("var updated = await _reservationService.CancelReservationAsync(request.ReservationID);", source, StringComparison.Ordinal);
            Assert.Equal(8, CountOccurrences(source, "await LoadPendingRequestsAsync();"));
            Assert.Contains("The selected request could not be confirmed. It may have been removed or changed by another user. The open request queue has been refreshed.", source, StringComparison.Ordinal);
            Assert.Contains("The selected request could not be cancelled. It may have been removed or changed by another user. The open request queue has been refreshed.", source, StringComparison.Ordinal);
            Assert.Contains("await LoadPendingRequestsAsync();\n                await _dialogService.ShowInfoAsync($\"Failed to confirm request: {ex.Message} The open request queue has been refreshed in case the request status changed before the failure.\", \"Error\");", source, StringComparison.Ordinal);
            Assert.Contains("await LoadPendingRequestsAsync();\n                await _dialogService.ShowInfoAsync($\"Failed to cancel request: {ex.Message} The open request queue has been refreshed in case the request status changed before the failure.\", \"Error\");", source, StringComparison.Ordinal);
        }

        [Fact]
        public void OpenRequestRefreshFailuresAreContainedBeforeStatusErrorDialogs()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ManageRentalsViewModel.cs");

            Assert.Contains("async Task LoadPendingRequestsAsync(bool notifyOnFailure = false)", source, StringComparison.Ordinal);
            Assert.Contains("catch (Exception ex)\n            {\n                _logger.LogError(ex, \"Failed to load open reservations for rentals page\");", source, StringComparison.Ordinal);
            Assert.Contains("PendingRequests.Clear();\n                SelectedRequest = null;", source, StringComparison.Ordinal);
            Assert.Contains("finally\n            {\n                OnPropertyChanged(nameof(RequestSummary));\n            }", source, StringComparison.Ordinal);
            Assert.Contains("await LoadPendingRequestsAsync();\n                await _dialogService.ShowInfoAsync($\"Failed to confirm request: {ex.Message}", source, StringComparison.Ordinal);
            Assert.Contains("await LoadPendingRequestsAsync();\n                await _dialogService.ShowInfoAsync($\"Failed to cancel request: {ex.Message}", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await LoadPendingRequestsAsync(notifyOnFailure: true);\n                await _dialogService.ShowInfoAsync($\"Failed to confirm request", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await LoadPendingRequestsAsync(notifyOnFailure: true);\n                await _dialogService.ShowInfoAsync($\"Failed to cancel request", source, StringComparison.Ordinal);
        }

        [Fact]
        public void RequestPlacementAndQueueLoadFailuresRefreshAndExplainState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ManageRentalsViewModel.cs");

            Assert.Contains("await LoadPendingRequestsAsync(notifyOnFailure: true);", source, StringComparison.Ordinal);
            Assert.Contains("async Task LoadPendingRequestsAsync(bool notifyOnFailure = false)", source, StringComparison.Ordinal);
            Assert.Contains("PendingRequests.Clear();\n                SelectedRequest = null;", source, StringComparison.Ordinal);
            Assert.Contains("if (notifyOnFailure)", source, StringComparison.Ordinal);
            Assert.Contains("Failed to load open requests: {ex.Message} The open request queue has been cleared until it can be refreshed.", source, StringComparison.Ordinal);
            Assert.Contains("await LoadPendingRequestsAsync();\n                await _dialogService.ShowInfoAsync($\"Failed to place request: {ex.Message} The open request queue has been refreshed in case the request was saved before the failure.\", \"Error\");", source, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalOperationFailuresRefreshDeskAndExplainState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ManageRentalsViewModel.cs");

            Assert.Contains("async Task RefreshRentalDeskAfterOperationFailureAsync(int rentalId)", source, StringComparison.Ordinal);
            Assert.Contains("_allRentals = await _rentalService.GetAllRentalsAsync();", source, StringComparison.Ordinal);
            Assert.Contains("await LoadPendingRequestsAsync();", source, StringComparison.Ordinal);
            Assert.Contains("RefreshActiveRentals();", source, StringComparison.Ordinal);
            Assert.Contains("ApplyFilter(rentalId);", source, StringComparison.Ordinal);
            Assert.Equal(3, CountOccurrences(source, "await RefreshRentalDeskAfterOperationFailureAsync("));
            Assert.Contains("var rentalToExtend = SelectedRental;", source, StringComparison.Ordinal);
            Assert.Contains("await _rentalService.ExtendRentalAsync(rentalToExtend.RentalID, newDueDate);", source, StringComparison.Ordinal);
            Assert.Contains("_logger.LogError(ex, \"Failed to extend rental {RentalID}\", rentalToExtend.RentalID);", source, StringComparison.Ordinal);
            Assert.Contains("_logger.LogError(ex, \"Failed to delete rental {RentalID}\", rentalToDelete.RentalID);", source, StringComparison.Ordinal);
            Assert.Equal(3, CountOccurrences(source, "The rental desk has been refreshed so current rental actions match the latest saved state."));
            Assert.DoesNotContain("_logger.LogError(ex, \"Failed to extend rental {RentalID}\", SelectedRental.RentalID);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("_logger.LogError(ex, \"Failed to delete rental {RentalID}\", rentalToDelete?.RentalID);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalGridRightClickSelectionUsesSharedSafeTreeTraversal()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml.cs");
            var helper = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "GridContextMenuSelection.cs");

            Assert.Contains("var row = GridContextMenuSelection.SelectRow(sender, e);", source, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.FindAncestor<System.Windows.Controls.DataGrid>(focusedElement)", source, StringComparison.Ordinal);
            Assert.DoesNotContain("VisualTreeHelper.GetParent", source, StringComparison.Ordinal);
            Assert.DoesNotContain("private static DependencyObject? GetParent", source, StringComparison.Ordinal);
            Assert.Contains("return TryGetVisualParent(current)", helper, StringComparison.Ordinal);
            Assert.Contains("?? TryGetLogicalParent(current)", helper, StringComparison.Ordinal);
            Assert.Contains("?? TryGetFrameworkParent(current);", helper, StringComparison.Ordinal);
            Assert.Equal(2, CountOccurrences(helper, "catch (InvalidOperationException)"));
            Assert.Contains("FrameworkElement element => element.Parent", helper, StringComparison.Ordinal);
            Assert.Contains("FrameworkContentElement contentElement => contentElement.Parent", helper, StringComparison.Ordinal);
            Assert.DoesNotContain("return LogicalTreeHelper.GetParent(current);", helper, StringComparison.Ordinal);
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

using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReservationWorkflowContractTests
    {
        [Fact]
        public void ReservationLoadFailuresClearStaleVisibleRowsAndExplainState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ReservationManagementViewModel.cs");

            AssertContainsAll(
                source,
                "Reservations.Clear();",
                "FilteredReservations.Clear();",
                "SelectedReservation = null;",
                "OnPropertyChanged(nameof(ReservationResultsSummary));",
                "await _dialogService.ShowErrorAsync(\"Error loading reservations\", $\"{ex.Message} The reservation list has been cleared until reload succeeds.\");");
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error loading reservations\", ex.Message);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationOperationFailuresRefreshVisibleRowsAndExplainState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ReservationManagementViewModel.cs");

            AssertContainsAll(
                source,
                "private async Task<bool> RefreshReservationsAfterOperationFailureAsync(int? preferredReservationId = null)",
                "var reservations = await _reservationService.GetAllReservationsAsync();",
                "ApplyFilter(preferredReservationId);",
                "Reservations.Clear();",
                "FilteredReservations.Clear();",
                "SelectedReservation = null;",
                "AppendReservationRefreshMessage(ex.Message, refreshed)",
                "newReservation.ReservationID > 0 ? newReservation.ReservationID : null",
                "RefreshReservationsAfterOperationFailureAsync(clone.ReservationID)",
                "RefreshReservationsAfterOperationFailureAsync(reservation.ReservationID)",
                "RefreshReservationsAfterOperationFailureAsync(reservationId)",
                "The reservation list has been refreshed in case saved state changed before the failure.",
                "The reservation list could not be refreshed, so visible reservation rows were cleared until reload succeeds.");
            Assert.True(
                CountOccurrences(source, "var refreshed = await RefreshReservationsAfterOperationFailureAsync(") >= 6,
                "Expected all reservation mutation failure paths to refresh or clear visible rows.");
        }

        [Fact]
        public void ConfirmAndFulfillPreserveReservationIdForFailureRefresh()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ReservationManagementViewModel.cs");

            AssertContainsAll(
                source,
                "var reservationId = SelectedReservation.ReservationID;",
                "await _reservationService.ConfirmReservationAsync(reservationId);",
                "await _reservationService.FulfillReservationAsync(reservationId, rentalId);",
                "var refreshed = await RefreshReservationsAfterOperationFailureAsync(reservationId);",
                "await _dialogService.ShowErrorAsync(\"Error confirming reservation\", AppendReservationRefreshMessage(ex.Message, refreshed));",
                "await _dialogService.ShowErrorAsync(\"Error fulfilling reservation\", AppendReservationRefreshMessage(ex.Message, refreshed));");
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error confirming reservation\", ex.Message);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error fulfilling reservation\", ex.Message);", source, StringComparison.Ordinal);
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
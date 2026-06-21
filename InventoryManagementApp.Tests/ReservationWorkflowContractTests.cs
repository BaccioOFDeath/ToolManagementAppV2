using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReservationWorkflowContractTests
    {
        [Fact]
        public void ReservationOperationFailuresRefreshVisibleRowsAndExplainState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ReservationManagementViewModel.cs");

            Assert.Contains("private async Task<bool> RefreshReservationsAfterOperationFailureAsync(int? preferredReservationId = null)", source, StringComparison.Ordinal);
            Assert.Contains("var reservations = await _reservationService.GetAllReservationsAsync();", source, StringComparison.Ordinal);
            Assert.Contains("ApplyFilter(preferredReservationId);", source, StringComparison.Ordinal);
            Assert.Contains("Reservations.Clear();\n                FilteredReservations.Clear();\n                SelectedReservation = null;", source, StringComparison.Ordinal);
            Assert.Contains("AppendReservationRefreshMessage(ex.Message, refreshed)", source, StringComparison.Ordinal);
            Assert.Equal(6, CountOccurrences(source, "var refreshed = await RefreshReservationsAfterOperationFailureAsync("));
            Assert.Contains("newReservation.ReservationID > 0 ? newReservation.ReservationID : null", source, StringComparison.Ordinal);
            Assert.Contains("RefreshReservationsAfterOperationFailureAsync(clone.ReservationID)", source, StringComparison.Ordinal);
            Assert.Contains("RefreshReservationsAfterOperationFailureAsync(reservation.ReservationID)", source, StringComparison.Ordinal);
            Assert.Contains("RefreshReservationsAfterOperationFailureAsync(reservationId)", source, StringComparison.Ordinal);
            Assert.Contains("The reservation list has been refreshed in case saved state changed before the failure.", source, StringComparison.Ordinal);
            Assert.Contains("The reservation list could not be refreshed, so visible reservation rows were cleared until reload succeeds.", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ConfirmAndFulfillPreserveReservationIdForFailureRefresh()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ReservationManagementViewModel.cs");

            Assert.Contains("var reservationId = SelectedReservation.ReservationID;\n            try\n            {\n                await _reservationService.ConfirmReservationAsync(reservationId);", source, StringComparison.Ordinal);
            Assert.Contains("var reservationId = SelectedReservation.ReservationID;\n                try\n                {\n                    await _reservationService.FulfillReservationAsync(reservationId, rentalId);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("catch (Exception ex)\n            {\n                await _dialogService.ShowErrorAsync(\"Error confirming reservation\", ex.Message);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("catch (Exception ex)\n                {\n                    await _dialogService.ShowErrorAsync(\"Error fulfilling reservation\", ex.Message);", source, StringComparison.Ordinal);
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

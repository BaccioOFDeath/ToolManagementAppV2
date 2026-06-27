using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReservationServiceWriteGuardContractTests
    {
        [Fact]
        public void ReservationWritesThrowWhenNoRowsAreAffected()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Reservations", "ReservationService.cs");

            AssertWriteGuard(
                source,
                "public async Task<bool> UpdateReservationAsync",
                "public async Task<bool> ConfirmReservationAsync",
                "var updatedRows = cmd.ExecuteNonQuery();",
                "EnsureReservationWriteSucceeded(updatedRows);");
            AssertWriteGuard(
                source,
                "public async Task<bool> ConfirmReservationAsync",
                "public async Task<bool> CancelReservationAsync",
                "var confirmedRows = cmd.ExecuteNonQuery();",
                "EnsureReservationWriteSucceeded(confirmedRows);");
            AssertWriteGuard(
                source,
                "public async Task<bool> CancelReservationAsync",
                "public async Task<bool> FulfillReservationAsync",
                "var cancelledRows = cmd.ExecuteNonQuery();",
                "EnsureReservationWriteSucceeded(cancelledRows);");
            AssertWriteGuard(
                source,
                "public async Task<bool> FulfillReservationAsync",
                "public async Task<bool> DeleteReservationAsync",
                "var fulfilledRows = cmd.ExecuteNonQuery();",
                "EnsureReservationWriteSucceeded(fulfilledRows);");
            AssertWriteGuard(
                source,
                "public async Task<bool> DeleteReservationAsync",
                "public async Task<bool> CheckAvailabilityAsync",
                "var deletedRows = cmd.ExecuteNonQuery();",
                "EnsureReservationWriteSucceeded(deletedRows);");

            Assert.Contains("private static void EnsureReservationWriteSucceeded(int affectedRows)", source, StringComparison.Ordinal);
            Assert.Contains("if (affectedRows == 0)", source, StringComparison.Ordinal);
            Assert.Contains("throw new InvalidOperationException(\"Reservation not found.\");", source, StringComparison.Ordinal);
        }

        private static void AssertWriteGuard(
            string source,
            string startMarker,
            string endMarker,
            string executeSnippet,
            string guardSnippet)
        {
            var method = ExtractMethod(source, startMarker, endMarker);

            Assert.Contains(executeSnippet, method, StringComparison.Ordinal);
            Assert.Contains(guardSnippet, method, StringComparison.Ordinal);
            Assert.Contains("return true;", method, StringComparison.Ordinal);
            Assert.DoesNotContain("return cmd.ExecuteNonQuery() > 0;", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf(executeSnippet, StringComparison.Ordinal) < method.IndexOf(guardSnippet, StringComparison.Ordinal),
                $"Expected {startMarker} to check affected rows after executing the write.");
            Assert.True(
                method.IndexOf(guardSnippet, StringComparison.Ordinal) < method.IndexOf("return true;", StringComparison.Ordinal),
                $"Expected {startMarker} to fail stale writes before reporting success.");
        }

        private static string ExtractMethod(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find method start marker: {startMarker}");

            var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
            Assert.True(end > start, $"Could not find method end marker: {endMarker}");

            return source[start..end];
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

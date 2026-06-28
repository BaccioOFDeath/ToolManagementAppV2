using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReservationServiceQueryGuardContractTests
    {
        [Fact]
        public void ReservationReadModelsRequireExistingItemAndCustomerRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Reservations", "ReservationService.cs");

            AssertContainsAll(
                source,
                "FROM Reservations r\n                    JOIN Items i ON r.ItemID = i.ItemID\n                    JOIN Customers c ON r.CustomerID = c.CustomerID",
                "public async Task<List<Reservation>> GetAllReservationsAsync()",
                "public async Task<List<Reservation>> GetActiveReservationsAsync()",
                "public async Task<List<Reservation>> GetReservationsByItemAsync(int itemID)",
                "public async Task<List<Reservation>> GetReservationsByCustomerAsync(int customerID)",
                "public async Task<List<Reservation>> GetUpcomingReservationsAsync(int days = 7)",
                "public async Task<Reservation?> GetReservationByIdAsync(int reservationID)");
            Assert.DoesNotContain(
                "FROM Reservations r\n                    LEFT JOIN Items i ON r.ItemID = i.ItemID",
                source,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                "LEFT JOIN Customers c ON r.CustomerID = c.CustomerID",
                source,
                StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationHistoryQueriesValidateParentRowsBeforeExecutingHistoryQueries()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Reservations", "ReservationService.cs");

            AssertContainsAll(
                source,
                "private static void EnsureReservationItemExists(SqliteConnection conn, int itemID)",
                "SELECT COUNT(*) FROM Items WHERE ItemID = @ID",
                "throw new InvalidOperationException(\"Item not found.\");",
                "private static void EnsureReservationCustomerExists(SqliteConnection conn, int customerID)",
                "SELECT COUNT(*) FROM Customers WHERE CustomerID = @ID",
                "throw new InvalidOperationException(\"Customer not found.\");");

            var itemMethod = ExtractMethod(
                source,
                "public async Task<List<Reservation>> GetReservationsByItemAsync(int itemID)",
                "public async Task<List<Reservation>> GetReservationsByCustomerAsync(int customerID)");
            AssertContainsAll(
                itemMethod,
                "if (itemID < 1)",
                "throw new ArgumentOutOfRangeException(nameof(itemID), \"Item ID must be greater than 0.\");",
                "using var conn = _databaseService.CreateConnection();",
                "EnsureReservationItemExists(conn, itemID);",
                "WHERE r.ItemID = @ItemID");
            Assert.True(
                itemMethod.IndexOf("EnsureReservationItemExists(conn, itemID);", StringComparison.Ordinal) <
                itemMethod.IndexOf("var sql = @\"", StringComparison.Ordinal),
                "Expected item reservation history to confirm the item row exists before building/executing the history query.");

            var customerMethod = ExtractMethod(
                source,
                "public async Task<List<Reservation>> GetReservationsByCustomerAsync(int customerID)",
                "public async Task<List<Reservation>> GetUpcomingReservationsAsync");
            AssertContainsAll(
                customerMethod,
                "if (customerID < 1)",
                "throw new ArgumentOutOfRangeException(nameof(customerID), \"Customer ID must be greater than 0.\");",
                "using var conn = _databaseService.CreateConnection();",
                "EnsureReservationCustomerExists(conn, customerID);",
                "WHERE r.CustomerID = @CustomerID");
            Assert.True(
                customerMethod.IndexOf("EnsureReservationCustomerExists(conn, customerID);", StringComparison.Ordinal) <
                customerMethod.IndexOf("var sql = @\"", StringComparison.Ordinal),
                "Expected customer reservation history to confirm the customer row exists before building/executing the history query.");
        }

        [Fact]
        public void ReservationAvailabilityValidatesItemRowBeforeAvailabilityQuery()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Reservations", "ReservationService.cs");

            var availabilityMethod = ExtractMethod(
                source,
                "public async Task<bool> CheckAvailabilityAsync(int itemID, DateTime startDate, DateTime endDate, int quantity)",
                "private Reservation MapReservation");

            AssertContainsAll(
                availabilityMethod,
                "if (itemID < 1)",
                "throw new ArgumentOutOfRangeException(nameof(itemID), \"Item ID must be greater than 0.\");",
                "using var conn = _databaseService.CreateConnection();",
                "EnsureReservationItemExists(conn, itemID);",
                "SELECT i.AvailableQuantity",
                "WHERE i.ItemID = @ItemID");
            Assert.True(
                availabilityMethod.IndexOf("EnsureReservationItemExists(conn, itemID);", StringComparison.Ordinal) <
                availabilityMethod.IndexOf("var sql = @\"", StringComparison.Ordinal),
                "Expected reservation availability checks to confirm the item row exists before building/executing the availability query.");
        }

        [Fact]
        public void ReservationAvailabilityCountsOnlyVisibleCustomerBackedHolds()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Reservations", "ReservationService.cs");

            var availabilityMethod = ExtractMethod(
                source,
                "public async Task<bool> CheckAvailabilityAsync(int itemID, DateTime startDate, DateTime endDate, int quantity)",
                "private Reservation MapReservation");

            AssertContainsAll(
                availabilityMethod,
                "LEFT JOIN Reservations r ON i.ItemID = r.ItemID",
                "AND r.Status IN ('Pending', 'Confirmed')",
                "AND EXISTS (SELECT 1 FROM Customers c WHERE c.CustomerID = r.CustomerID)",
                "COALESCE(SUM(r.Quantity), 0) as ReservedQuantity");
        }

        private static void AssertContainsAll(string source, params string[] expectedSnippets)
        {
            foreach (var snippet in expectedSnippets)
            {
                Assert.Contains(snippet, source, StringComparison.Ordinal);
            }
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
                    return NormalizeLineEndings(File.ReadAllText(candidate));

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }

        private static string NormalizeLineEndings(string text) =>
            text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
    }
}

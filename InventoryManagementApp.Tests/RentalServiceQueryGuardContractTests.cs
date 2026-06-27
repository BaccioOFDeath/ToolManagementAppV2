using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RentalServiceQueryGuardContractTests
    {
        [Fact]
        public void RentalHistoryQueriesValidatePositiveIdentifiersBeforeSqlWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Rentals", "RentalService.cs");

            AssertContainsAll(
                source,
                "public async Task<List<Rental>> GetRentalHistoryForItemAsync(int itemID)",
                "if (itemID < 1)",
                "throw new ArgumentOutOfRangeException(nameof(itemID), \"Item ID must be greater than 0.\");",
                "public async Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID)",
                "if (customerID < 1)",
                "throw new ArgumentOutOfRangeException(nameof(customerID), \"Customer ID must be greater than 0.\");");

            Assert.True(
                source.IndexOf("throw new ArgumentOutOfRangeException(nameof(itemID)", StringComparison.Ordinal) <
                source.IndexOf("WHERE r.ItemID = @ItemID", StringComparison.Ordinal),
                "Expected item rental history to validate the item id before building/executing SQL.");
            Assert.True(
                source.IndexOf("throw new ArgumentOutOfRangeException(nameof(customerID)", StringComparison.Ordinal) <
                source.IndexOf("WHERE r.CustomerID = @CustomerID", StringComparison.Ordinal),
                "Expected customer rental history to validate the customer id before building/executing SQL.");
        }

        [Fact]
        public void RentalHistoryQueriesValidateParentRowsBeforePreparingHistoryQueries()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Rentals", "RentalService.cs");

            AssertContainsAll(
                source,
                "private static async Task EnsureItemExistsAsync(SqliteConnection conn, int itemID)",
                "SELECT COUNT(*) FROM Items WHERE ItemID=@ItemID",
                "throw new InvalidOperationException(\"Item not found.\");",
                "private static async Task EnsureCustomerExistsAsync(SqliteConnection conn, int customerID)",
                "SELECT COUNT(*) FROM Customers WHERE CustomerID=@CustomerID",
                "throw new InvalidOperationException(\"Customer not found.\");");

            var itemMethod = ExtractMethod(
                source,
                "public async Task<List<Rental>> GetRentalHistoryForItemAsync(int itemID)",
                "public async Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID)");
            AssertContainsAll(
                itemMethod,
                "using var conn = _dbService.CreateConnection();",
                "await EnsureItemExistsAsync(conn, itemID).ConfigureAwait(false);",
                "const string sql = BaseSelect + @\" WHERE r.ItemID = @ItemID ORDER BY r.RentalDate DESC\";",
                "var p = new[] { new SqliteParameter(\"@ItemID\", itemID) };");
            Assert.True(
                itemMethod.IndexOf("await EnsureItemExistsAsync(conn, itemID).ConfigureAwait(false);", StringComparison.Ordinal) <
                itemMethod.IndexOf("const string sql = BaseSelect", StringComparison.Ordinal),
                "Expected item rental history to confirm the item row exists before preparing the history query.");
            Assert.True(
                itemMethod.IndexOf("await EnsureItemExistsAsync(conn, itemID).ConfigureAwait(false);", StringComparison.Ordinal) <
                itemMethod.IndexOf("new SqliteParameter(\"@ItemID\", itemID)", StringComparison.Ordinal),
                "Expected item rental history to confirm the item row exists before preparing query parameters.");

            var customerMethod = ExtractMethod(
                source,
                "public async Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID)",
                "public async Task<List<ItemRentalFrequency>> GetRentalFrequencyAsync");
            AssertContainsAll(
                customerMethod,
                "using var conn = _dbService.CreateConnection();",
                "await EnsureCustomerExistsAsync(conn, customerID).ConfigureAwait(false);",
                "const string sql = BaseSelect + @\" WHERE r.CustomerID = @CustomerID ORDER BY r.RentalDate DESC\";",
                "var p = new[] { new SqliteParameter(\"@CustomerID\", customerID) };");
            Assert.True(
                customerMethod.IndexOf("await EnsureCustomerExistsAsync(conn, customerID).ConfigureAwait(false);", StringComparison.Ordinal) <
                customerMethod.IndexOf("const string sql = BaseSelect", StringComparison.Ordinal),
                "Expected customer rental history to confirm the customer row exists before preparing the history query.");
            Assert.True(
                customerMethod.IndexOf("await EnsureCustomerExistsAsync(conn, customerID).ConfigureAwait(false);", StringComparison.Ordinal) <
                customerMethod.IndexOf("new SqliteParameter(\"@CustomerID\", customerID)", StringComparison.Ordinal),
                "Expected customer rental history to confirm the customer row exists before preparing query parameters.");
        }

        [Fact]
        public void RentalFrequencyValidatesPositiveLimitBeforeSqlWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Rentals", "RentalService.cs");

            AssertContainsAll(
                source,
                "public async Task<List<ItemRentalFrequency>> GetRentalFrequencyAsync(int topN = 10)",
                "if (topN < 1)",
                "throw new ArgumentOutOfRangeException(nameof(topN), \"Top rental frequency count must be greater than 0.\");",
                "LIMIT @TopN");

            Assert.True(
                source.IndexOf("throw new ArgumentOutOfRangeException(nameof(topN)", StringComparison.Ordinal) <
                source.IndexOf("LIMIT @TopN", StringComparison.Ordinal),
                "Expected rental frequency to validate the limit before building/executing SQL.");
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

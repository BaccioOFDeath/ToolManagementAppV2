using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RentalServiceReferenceGuardContractTests
    {
        [Fact]
        public void RentItemGuardsItemAndCustomerReferencesBeforeInsert()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Rentals", "RentalService.cs");

            AssertContainsAll(
                source,
                "var avail = await GetAvailableQuantityForExistingItemAsync(conn, tx, itemID);",
                "await EnsureCustomerExistsAsync(conn, tx, customerID);",
                "await SqliteHelper.ExecuteNonQueryAsync(conn, tx,",
                "INSERT INTO Rentals (ItemID, CustomerID, RentalDate, DueDate, Status)",
                "throw new InvalidOperationException(\"Item not found.\");",
                "throw new InvalidOperationException(\"Customer not found.\");");
            Assert.True(
                source.IndexOf("GetAvailableQuantityForExistingItemAsync(conn, tx, itemID)", StringComparison.Ordinal) <
                source.IndexOf("INSERT INTO Rentals (ItemID, CustomerID, RentalDate, DueDate, Status)", StringComparison.Ordinal),
                "Expected item existence validation before rental insert.");
            Assert.True(
                source.IndexOf("EnsureCustomerExistsAsync(conn, tx, customerID)", StringComparison.Ordinal) <
                source.IndexOf("INSERT INTO Rentals (ItemID, CustomerID, RentalDate, DueDate, Status)", StringComparison.Ordinal),
                "Expected customer existence validation before rental insert.");
            Assert.DoesNotContain("Convert.ToInt32(await availCmd.ExecuteScalarAsync() ?? 0)", source, StringComparison.Ordinal);
        }

        private static void AssertContainsAll(string source, params string[] expectedSnippets)
        {
            foreach (var snippet in expectedSnippets)
            {
                Assert.Contains(snippet, source, StringComparison.Ordinal);
            }
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

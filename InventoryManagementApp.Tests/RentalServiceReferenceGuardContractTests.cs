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

        [Fact]
        public void ReturnItemGuardsActiveRentalWriteBeforeInventorySync()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Rentals", "RentalService.cs");

            AssertContainsAll(
                source,
                "SELECT ItemID FROM Rentals WHERE RentalID=@RentalID AND Status='Rented'",
                "var returnedRows = await SqliteHelper.ExecuteNonQueryAsync(conn, tx,",
                "UPDATE Rentals SET ReturnDate=@ReturnDate,Status='Returned' WHERE RentalID=@RentalID AND Status='Rented'",
                "if (returnedRows == 0)",
                "throw new InvalidOperationException(\"Rental not found or already returned.\");",
                "await _itemService.UpdateItemQuantitiesAsync(itemID, 1, false, conn, tx);");
            Assert.True(
                source.IndexOf("UPDATE Rentals SET ReturnDate=@ReturnDate,Status='Returned' WHERE RentalID=@RentalID AND Status='Rented'", StringComparison.Ordinal) <
                source.IndexOf("await _itemService.UpdateItemQuantitiesAsync(itemID, 1, false, conn, tx);", StringComparison.Ordinal),
                "Expected rental return persistence to prove the active rental row was updated before inventory sync.");
            Assert.True(
                source.IndexOf("if (returnedRows == 0)", StringComparison.Ordinal) <
                source.IndexOf("await _itemService.UpdateItemQuantitiesAsync(itemID, 1, false, conn, tx);", StringComparison.Ordinal),
                "Expected stale return writes to fail before returning item quantity to stock.");
        }

        [Fact]
        public void DeleteRentalGuardsDeleteWriteBeforeInventorySync()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Rentals", "RentalService.cs");

            AssertContainsAll(
                source,
                "public async Task DeleteRentalAsync(int rentalID)",
                "SELECT ItemID, Status, ReturnDate FROM Rentals WHERE RentalID=@RentalID",
                "var deletedRows = await deleteCmd.ExecuteNonQueryAsync();",
                "if (deletedRows == 0)",
                "throw new InvalidOperationException(\"Rental not found.\");",
                "await _itemService.UpdateItemQuantitiesAsync(itemID, 1, false, conn, tx);");

            var deleteMethodIndex = source.IndexOf("public async Task DeleteRentalAsync(int rentalID)", StringComparison.Ordinal);
            var deletedRowsIndex = source.IndexOf("var deletedRows = await deleteCmd.ExecuteNonQueryAsync();", deleteMethodIndex, StringComparison.Ordinal);
            var staleGuardIndex = source.IndexOf("if (deletedRows == 0)", deleteMethodIndex, StringComparison.Ordinal);
            var deleteInventorySyncIndex = source.LastIndexOf("await _itemService.UpdateItemQuantitiesAsync(itemID, 1, false, conn, tx);", StringComparison.Ordinal);

            Assert.True(
                deletedRowsIndex < deleteInventorySyncIndex,
                "Expected rental delete persistence to prove the row was removed before inventory sync.");
            Assert.True(
                staleGuardIndex < deleteInventorySyncIndex,
                "Expected stale rental delete writes to fail before returning item quantity to stock.");
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

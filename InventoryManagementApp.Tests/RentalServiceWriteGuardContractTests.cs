using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RentalServiceWriteGuardContractTests
    {
        [Fact]
        public void RentalWritesThrowWhenNoRowsAreAffected()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Rentals", "RentalService.cs");

            var returnMethod = ExtractMethod(
                source,
                "public async Task ReturnItemAsync(int rentalID, DateTime returnDate)",
                "public async Task ExtendRentalAsync");
            AssertContainsAll(
                returnMethod,
                "var returnedRows = await SqliteHelper.ExecuteNonQueryAsync(conn, tx,",
                "if (returnedRows == 0)",
                "throw new InvalidOperationException(\"Rental not found or already returned.\");",
                "await _itemService.UpdateItemQuantitiesAsync(itemID, 1, false, conn, tx);");
            Assert.True(
                returnMethod.IndexOf("var returnedRows = await SqliteHelper.ExecuteNonQueryAsync(conn, tx,", StringComparison.Ordinal) <
                returnMethod.IndexOf("if (returnedRows == 0)", StringComparison.Ordinal),
                "Expected return writes to inspect affected rows after executing the update.");
            Assert.True(
                returnMethod.IndexOf("if (returnedRows == 0)", StringComparison.Ordinal) <
                returnMethod.IndexOf("await _itemService.UpdateItemQuantitiesAsync(itemID, 1, false, conn, tx);", StringComparison.Ordinal),
                "Expected stale return writes to fail before inventory quantity sync.");

            var extendMethod = ExtractMethod(
                source,
                "public async Task ExtendRentalAsync",
                "public async Task DeleteRentalAsync");
            AssertContainsAll(
                extendMethod,
                "if (await updateCmd.ExecuteNonQueryAsync() == 0)",
                "throw new InvalidOperationException(\"Unable to extend rental. Rental not found or already returned.\");");

            var deleteMethod = ExtractMethod(
                source,
                "public async Task DeleteRentalAsync",
                "public async Task<int> CountActiveRentalsAsync");
            AssertContainsAll(
                deleteMethod,
                "var deletedRows = await deleteCmd.ExecuteNonQueryAsync();",
                "if (deletedRows == 0)",
                "throw new InvalidOperationException(\"Rental not found.\");",
                "await _itemService.UpdateItemQuantitiesAsync(itemID, 1, false, conn, tx);");
            Assert.True(
                deleteMethod.IndexOf("var deletedRows = await deleteCmd.ExecuteNonQueryAsync();", StringComparison.Ordinal) <
                deleteMethod.IndexOf("if (deletedRows == 0)", StringComparison.Ordinal),
                "Expected delete writes to inspect affected rows after executing the delete.");
            Assert.True(
                deleteMethod.IndexOf("if (deletedRows == 0)", StringComparison.Ordinal) <
                deleteMethod.IndexOf("await _itemService.UpdateItemQuantitiesAsync(itemID, 1, false, conn, tx);", StringComparison.Ordinal),
                "Expected stale delete writes to fail before inventory quantity sync.");
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

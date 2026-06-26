using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemRepositoryBulkSaveContractTests
    {
        [Fact]
        public void BulkItemSaveChecksAffectedRowsAndFailsForStaleItems()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Data", "ItemRepository.cs");
            var method = ExtractMethod(
                source,
                "public async Task SaveChangesAsync(IEnumerable<Item> changes, CancellationToken ct)",
                "public async Task<int> InsertAsync(Item item, CancellationToken ct)");

            Assert.Contains("var rows = await conn.ExecuteAsync(new CommandDefinition(sql, new", method, StringComparison.Ordinal);
            Assert.Contains("}, tx, cancellationToken: ct)).ConfigureAwait(false);", method, StringComparison.Ordinal);
            Assert.Contains("if (rows == 0)", method, StringComparison.Ordinal);
            Assert.Contains("throw new InvalidOperationException($\"Failed to save item {item.ItemID}.\");", method, StringComparison.Ordinal);
            Assert.DoesNotContain("await conn.ExecuteAsync(sql, new", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("var rows = await conn.ExecuteAsync", StringComparison.Ordinal) <
                method.IndexOf("if (rows == 0)", StringComparison.Ordinal),
                "Bulk item saves should inspect the affected-row count immediately after each row update.");
            Assert.True(
                method.IndexOf("if (rows == 0)", StringComparison.Ordinal) <
                method.IndexOf("tx.Commit();", StringComparison.Ordinal),
                "Bulk item saves should fail before committing when a stale item row is encountered.");
        }

        [Fact]
        public void ItemRepositoryRejectsInvalidItemIdsBeforeConnectionWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Data", "ItemRepository.cs");

            AssertInvalidIdGuardBeforeConnection(
                source,
                "public async Task<Item?> GetByIdAsync(int id, CancellationToken ct)",
                "public async Task SaveChangesAsync",
                "if (id < 1)",
                "throw new ArgumentOutOfRangeException(nameof(id), \"Item ID must be greater than 0.\");");
            AssertInvalidIdGuardBeforeConnection(
                source,
                "public async Task UpdateAsync(Item item, CancellationToken ct)",
                "public async Task DeleteAsync",
                "if (item.ItemID < 1)",
                "throw new ArgumentOutOfRangeException(nameof(item), \"Item ID must be greater than 0.\");");
            AssertInvalidIdGuardBeforeConnection(
                source,
                "public async Task DeleteAsync(int itemID, CancellationToken ct)",
                "public async Task<bool> ToggleCheckOutStatusAsync",
                "if (itemID < 1)",
                "throw new ArgumentOutOfRangeException(nameof(itemID), \"Item ID must be greater than 0.\");");
            AssertInvalidIdGuardBeforeConnection(
                source,
                "public async Task<bool> ToggleCheckOutStatusAsync(int itemID",
                "public async Task<List<Item>> GetItemsCheckedOutByAsync",
                "if (itemID < 1)",
                "throw new ArgumentOutOfRangeException(nameof(itemID), \"Item ID must be greater than 0.\");");
            AssertInvalidIdGuardBeforeConnection(
                source,
                "public async Task UpdateItemImageAsync(int itemID",
                "public async Task<List<Item>> GetMostCommonlyUsedItemsAsync",
                "if (itemID < 1)",
                "throw new ArgumentOutOfRangeException(nameof(itemID), \"Item ID must be greater than 0.\");");
        }

        [Fact]
        public void UpdateItemRejectsNullBeforeCancellationAndSqlWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Data", "ItemRepository.cs");
            var method = ExtractMethod(
                source,
                "public async Task UpdateAsync(Item item, CancellationToken ct)",
                "public async Task DeleteAsync");

            Assert.Contains("if (item is null)", method, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentNullException(nameof(item));", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("if (item is null)", StringComparison.Ordinal) < method.IndexOf("ct.ThrowIfCancellationRequested();", StringComparison.Ordinal),
                "Null item updates should fail before cancellation and SQL work can dereference the item.");
        }

        [Fact]
        public void MostCommonlyUsedItemsRejectsInvalidLimitsBeforeSqlWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Data", "ItemRepository.cs");
            var method = ExtractMethod(
                source,
                "public async Task<List<Item>> GetMostCommonlyUsedItemsAsync(int limit, CancellationToken ct)",
                "public async Task<List<Item>> GetIncompleteItemsAsync(CancellationToken ct)");

            Assert.Contains("ct.ThrowIfCancellationRequested();", method, StringComparison.Ordinal);
            Assert.Contains("if (limit < 1)", method, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentOutOfRangeException(nameof(limit), \"Limit must be positive.\");", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("if (limit < 1)", StringComparison.Ordinal) < method.IndexOf("var sql =", StringComparison.Ordinal),
                "The invalid limit guard should run before most-common item SQL work starts.");
            Assert.True(
                method.IndexOf("if (limit < 1)", StringComparison.Ordinal) < method.IndexOf("await using var conn", StringComparison.Ordinal),
                "The invalid limit guard should run before opening a database connection.");
        }

        [Fact]
        public void MostCommonlyUsedItemsStillOrdersByCheckoutCountAndAppliesTheRequestedLimit()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Data", "ItemRepository.cs");
            var method = ExtractMethod(
                source,
                "public async Task<List<Item>> GetMostCommonlyUsedItemsAsync(int limit, CancellationToken ct)",
                "public async Task<List<Item>> GetIncompleteItemsAsync(CancellationToken ct)");

            Assert.Contains("WHERE CheckoutCount > 0", method, StringComparison.Ordinal);
            Assert.Contains("ORDER BY CheckoutCount DESC", method, StringComparison.Ordinal);
            Assert.Contains("LIMIT @Limit", method, StringComparison.Ordinal);
            Assert.Contains("new { Limit = limit }", method, StringComparison.Ordinal);
        }

        private static void AssertInvalidIdGuardBeforeConnection(string source, string startMarker, string endMarker, string guardSnippet, string exceptionSnippet)
        {
            var method = ExtractMethod(source, startMarker, endMarker);

            Assert.Contains("ct.ThrowIfCancellationRequested();", method, StringComparison.Ordinal);
            Assert.Contains(guardSnippet, method, StringComparison.Ordinal);
            Assert.Contains(exceptionSnippet, method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf(guardSnippet, StringComparison.Ordinal) < method.IndexOf("await using var conn", StringComparison.Ordinal),
                $"Expected {startMarker} to reject invalid item IDs before opening a database connection.");
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
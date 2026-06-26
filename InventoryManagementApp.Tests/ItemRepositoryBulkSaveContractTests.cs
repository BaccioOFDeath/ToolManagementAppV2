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

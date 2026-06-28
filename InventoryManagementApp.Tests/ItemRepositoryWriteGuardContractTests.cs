using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemRepositoryWriteGuardContractTests
    {
        [Fact]
        public void ItemRepositoryWritesThrowWhenNoRowsAreAffected()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Data", "ItemRepository.cs");

            var saveChangesMethod = ExtractMethod(
                source,
                "public async Task SaveChangesAsync(IEnumerable<Item> changes, CancellationToken ct)",
                "public async Task<int> InsertAsync(Item item, CancellationToken ct)");
            AssertContainsAll(
                saveChangesMethod,
                "var rows = await conn.ExecuteAsync(new CommandDefinition(sql, new",
                "if (rows == 0)",
                "throw new InvalidOperationException($\"Failed to save item {item.ItemID}.\");");
            Assert.True(
                saveChangesMethod.IndexOf("var rows = await conn.ExecuteAsync(new CommandDefinition(sql, new", StringComparison.Ordinal) <
                saveChangesMethod.IndexOf("if (rows == 0)", StringComparison.Ordinal),
                "Expected bulk item saves to inspect affected rows after executing each update.");

            var updateMethod = ExtractMethod(
                source,
                "public async Task UpdateAsync(Item item, CancellationToken ct)",
                "public async Task DeleteAsync(int itemID, CancellationToken ct)");
            AssertContainsAll(
                updateMethod,
                "var rows = await conn.ExecuteAsync(new CommandDefinition(sql, new",
                "if (rows == 0)",
                "throw new InvalidOperationException($\"Failed to update item {item.ItemID}.\");");
            Assert.True(
                updateMethod.IndexOf("var rows = await conn.ExecuteAsync(new CommandDefinition(sql, new", StringComparison.Ordinal) <
                updateMethod.IndexOf("if (rows == 0)", StringComparison.Ordinal),
                "Expected item updates to inspect affected rows after executing the update.");

            var deleteMethod = ExtractMethod(
                source,
                "public async Task DeleteAsync(int itemID, CancellationToken ct)",
                "public async Task<bool> ToggleCheckOutStatusAsync");
            AssertContainsAll(
                deleteMethod,
                "var rows = await conn.ExecuteAsync(new CommandDefinition(\"DELETE FROM Items WHERE ItemID=@ID\"",
                "if (rows == 0)",
                "throw new InvalidOperationException($\"Failed to delete item {itemID}.\");");
            Assert.True(
                deleteMethod.IndexOf("var rows = await conn.ExecuteAsync", StringComparison.Ordinal) <
                deleteMethod.IndexOf("if (rows == 0)", StringComparison.Ordinal),
                "Expected item deletes to inspect affected rows after executing the delete.");

            var toggleMethod = ExtractMethod(
                source,
                "public async Task<bool> ToggleCheckOutStatusAsync",
                "public async Task<List<Item>> GetItemsCheckedOutByAsync");
            AssertContainsAll(
                toggleMethod,
                "var rows = await conn.ExecuteAsync(new CommandDefinition(@\"UPDATE Items SET",
                "if (rows == 0)",
                "throw new InvalidOperationException(\"Check-out status update failed.\");");
            Assert.True(
                toggleMethod.IndexOf("var rows = await conn.ExecuteAsync", StringComparison.Ordinal) <
                toggleMethod.IndexOf("if (rows == 0)", StringComparison.Ordinal),
                "Expected checkout toggles to inspect affected rows after executing the status update.");
            Assert.True(
                toggleMethod.IndexOf("if (rows == 0)", StringComparison.Ordinal) <
                toggleMethod.IndexOf("return true;", StringComparison.Ordinal),
                "Expected checkout toggles to fail stale writes before reporting success.");

            var imageMethod = ExtractMethod(
                source,
                "public async Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken ct)",
                "public async Task<List<Item>> GetMostCommonlyUsedItemsAsync");
            AssertContainsAll(
                imageMethod,
                "var rows = await conn.ExecuteAsync(new CommandDefinition(\"UPDATE Items SET ImagePath=@Img WHERE ItemID=@ID\"",
                "if (rows == 0)",
                "throw new InvalidOperationException($\"Failed to update image for item {itemID}.\");");
            Assert.True(
                imageMethod.IndexOf("var rows = await conn.ExecuteAsync", StringComparison.Ordinal) <
                imageMethod.IndexOf("if (rows == 0)", StringComparison.Ordinal),
                "Expected item image writes to inspect affected rows after executing the update.");
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

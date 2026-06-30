using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemServiceImportInsertGuardContractTests
    {
        [Fact]
        public void ItemImportInsertChecksAffectedRowsBeforeReadingGeneratedId()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var method = ExtractMethod(
                source,
                "protected virtual async Task<int> InsertItemAsync",
                "private async Task ExportItemsToCsvInternalAsync");

            Assert.Contains("const string insertSql", method, StringComparison.Ordinal);
            Assert.DoesNotContain("SELECT last_insert_rowid();\";", method[..method.IndexOf("var parameters = new[]", StringComparison.Ordinal)], StringComparison.Ordinal);
            Assert.Contains("var insertedRows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);", method, StringComparison.Ordinal);
            Assert.Contains("EnsureItemImportCreateSucceeded(insertedRows);", method, StringComparison.Ordinal);
            Assert.Contains("using var idCommand = new SqliteCommand(\"SELECT last_insert_rowid();\", conn, transaction);", method, StringComparison.Ordinal);
            Assert.Contains("if (id < 1)", method, StringComparison.Ordinal);
            Assert.Contains("throw new InvalidOperationException(\"Unable to import item.\");", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("var insertedRows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);", StringComparison.Ordinal) <
                method.IndexOf("EnsureItemImportCreateSucceeded(insertedRows);", StringComparison.Ordinal),
                "Item import should capture affected rows before checking the insert result.");
            Assert.True(
                method.IndexOf("EnsureItemImportCreateSucceeded(insertedRows);", StringComparison.Ordinal) <
                method.IndexOf("using var idCommand = new SqliteCommand(\"SELECT last_insert_rowid();\", conn, transaction);", StringComparison.Ordinal),
                "Failed item imports should stop before reading a generated id.");
            Assert.True(
                method.IndexOf("if (id < 1)", StringComparison.Ordinal) >
                method.IndexOf("var id = Convert.ToInt32(await idCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));", StringComparison.Ordinal),
                "Item import should reject invalid generated ids before returning success.");
        }

        [Fact]
        public void CsvAndGenericItemImportsAssignGuardedGeneratedIds()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var csvImportMethod = ExtractMethod(
                source,
                "private async Task<List<int>> ImportItemsFromCsvInternalAsync",
                "protected virtual async Task<int> InsertItemAsync");
            var genericImportMethod = ExtractMethod(
                source,
                "public async Task<List<int>> ImportItemsAsync",
                "private static string GenerateNextImportedItemNumber");

            Assert.Contains("item.ItemID = await InsertItemAsync(conn, transaction, item, cancellationToken).ConfigureAwait(false);", csvImportMethod, StringComparison.Ordinal);
            Assert.Contains("item.ItemID = await InsertItemAsync(conn, transaction, item, cancellationToken).ConfigureAwait(false);", genericImportMethod, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemImportCreateGuardUsesImportFailureMessage()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");

            Assert.Contains("static void EnsureItemImportCreateSucceeded(int affectedRows)", source, StringComparison.Ordinal);
            Assert.Contains("if (affectedRows == 0)", source, StringComparison.Ordinal);
            Assert.Contains("throw new InvalidOperationException(\"Unable to import item.\");", source, StringComparison.Ordinal);
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

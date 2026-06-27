using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemServiceImportTransactionTests
    {
        [Fact]
        public async Task ImportItemsAsync_RollsBackBatch_WhenInsertFails()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

            try
            {
                using var database = new DatabaseService(dbPath, NullLogger<DatabaseService>.Instance);
                var service = new FailingImportItemService(database, Mock.Of<IItemRepository>());
                var importer = new StubItemImporter(new[]
                {
                    CreateItem("T9001", "First imported item"),
                    CreateItem("T9002", "Second imported item")
                });

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.ImportItemsAsync("unused.csv", importer, CancellationToken.None));

                using var conn = database.CreateConnection();
                using var command = conn.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Items WHERE ItemNumber IN ('T9001', 'T9002')";

                var count = Convert.ToInt32(await command.ExecuteScalarAsync(CancellationToken.None));
                Assert.Equal(0, count);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void CsvImportRejectsOutOfRangeQuantitiesBeforeInsert()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var method = ExtractMethod(
                source,
                "private async Task<List<int>> ImportItemsFromCsvInternalAsync",
                "protected virtual async Task<int> InsertItemAsync");

            Assert.Contains("var parsedQuantity = TryParseInt(quantity);", method, StringComparison.Ordinal);
            Assert.Contains("if (!skip && (parsedQuantity < 0 || parsedQuantity > MaxQuantityOnHand))", method, StringComparison.Ordinal);
            Assert.Contains("_logger.LogWarning(\"Skipping row {Row}: AvailableQuantity {Quantity} is outside the allowed range.\", row, parsedQuantity);", method, StringComparison.Ordinal);
            Assert.Contains("invalidRows.Add(row);", method, StringComparison.Ordinal);
            Assert.Contains("QuantityOnHand = parsedQuantity", method, StringComparison.Ordinal);
            Assert.DoesNotContain("QuantityOnHand = TryParseInt(quantity)", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("var parsedQuantity = TryParseInt(quantity);", StringComparison.Ordinal) <
                method.IndexOf("var item = new ItemModel", StringComparison.Ordinal),
                "CSV import should parse quantity before building the item row.");
            Assert.True(
                method.IndexOf("if (!skip && (parsedQuantity < 0 || parsedQuantity > MaxQuantityOnHand))", StringComparison.Ordinal) <
                method.IndexOf("var item = new ItemModel", StringComparison.Ordinal),
                "CSV import should reject out-of-range quantities before building the item row.");
            Assert.True(
                method.IndexOf("if (!skip && (parsedQuantity < 0 || parsedQuantity > MaxQuantityOnHand))", StringComparison.Ordinal) <
                method.IndexOf("await InsertItemAsync", StringComparison.Ordinal),
                "CSV import should reject out-of-range quantities before direct insert work.");
        }

        private static ItemModel CreateItem(string itemNumber, string name) => new()
        {
            ItemNumber = itemNumber,
            Name = name,
            Location = "Import shelf",
            Brand = "Import brand",
            PartNumber = itemNumber,
            Supplier = "Import supplier",
            QuantityOnHand = 1
        };

        private sealed class FailingImportItemService : ItemService
        {
            public FailingImportItemService(DatabaseService dbService, IItemRepository repository)
                : base(dbService, repository, logger: NullLogger<ItemService>.Instance)
            {
            }

            protected override Task<int> InsertItemAsync(SqliteConnection conn, SqliteTransaction? transaction, ItemModel item, CancellationToken cancellationToken)
            {
                if (item.ItemNumber == "T9002")
                    throw new InvalidOperationException("Forced import failure.");

                return base.InsertItemAsync(conn, transaction, item, cancellationToken);
            }
        }

        private sealed class StubItemImporter : IDataImporter<ItemModel>
        {
            private readonly IEnumerable<ItemModel> _items;

            public StubItemImporter(IEnumerable<ItemModel> items)
            {
                _items = items;
            }

            public string FileExtension => ".csv";

            public string FileFilter => "CSV Files|*.csv";

            public string FormatName => "Test CSV";

            public Task<(IEnumerable<ItemModel> Data, List<int> SkippedRows)> ImportAsync(string filePath, CancellationToken cancellationToken = default)
            {
                return Task.FromResult((_items, new List<int>()));
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

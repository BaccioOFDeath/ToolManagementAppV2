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
    }
}

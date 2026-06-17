using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Data.Sqlite;
using InventoryManagementApp.Data;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Models.ImportExport;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using Xunit;

public class ItemServiceCsvImportTests
{
    [Fact]
    public async Task ImportItemsFromCsv_UsesBoundedMemoryForLargeFiles()
    {
        var csvPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        try
        {
            await using (var writer = new StreamWriter(csvPath))
            {
                await writer.WriteLineAsync("ItemNumber,NameDescription,Location,Brand,PartNumber,Supplier,PurchasedDate,Notes,AvailableQuantity,IsPowered");
                for (int i = 0; i < 10000; i++)
                    await writer.WriteLineAsync($"NUM{i},Name{i},Loc,Brand,Part,Supplier,2020-01-01,Note,1,0");
                await writer.FlushAsync();
            }

            await using (var db = new DatabaseService(dbPath))
            {
                new MigrationRunner(db).Migrate();
                var repository = new ItemRepository(new SqliteConnectionFactory(db.ConnectionString));
                var service = new ItemService(db, repository);
                var map = new Dictionary<string, string> { ["ItemNumber"] = "ItemNumber" };

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                var before = GC.GetTotalMemory(true);
                var invalid = await service.ImportItemsFromCsvAsync(csvPath, map, CancellationToken.None);
                var after = GC.GetTotalMemory(true);

                Assert.Empty(invalid);
                Assert.True(after - before < 80_000_000);
            }
        }
        finally
        {
            File.Delete(csvPath);
            File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ImportItemsFromCsv_PopulatesNames()
    {
        var csvPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        try
        {
            await File.WriteAllTextAsync(csvPath, "ItemNumber,NameDescription\nNUM1,ItemName");

            await using (var db = new DatabaseService(dbPath))
            {
                new MigrationRunner(db).Migrate();
                var repository = new ItemRepository(new SqliteConnectionFactory(db.ConnectionString));
                var service = new ItemService(db, repository);

                var map = new Dictionary<string, string>
                {
                    ["ItemNumber"] = "ItemNumber",
                    [nameof(ItemImportDto.Name)] = "NameDescription"
                };

                var invalid = await service.ImportItemsFromCsvAsync(csvPath, map, CancellationToken.None);
                Assert.Empty(invalid);

                using var conn = db.CreateConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT NameDescription FROM Items WHERE ItemNumber='NUM1'";
                var name = cmd.ExecuteScalar()?.ToString();
                Assert.Equal("ItemName", name);
            }
        }
        finally
        {
            File.Delete(csvPath);
            File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ImportItemsFromCsv_PopulatesKeywords()
    {
        var csvPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        try
        {
            await File.WriteAllTextAsync(csvPath, "ItemNumber,Keywords\nNUM1,tag1 tag2");

            await using (var db = new DatabaseService(dbPath))
            {
                new MigrationRunner(db).Migrate();
                var repository = new ItemRepository(new SqliteConnectionFactory(db.ConnectionString));
                var service = new ItemService(db, repository);

                var map = new Dictionary<string, string>
                {
                    ["ItemNumber"] = "ItemNumber",
                    [nameof(ItemImportDto.Keywords)] = "Keywords"
                };

                var invalid = await service.ImportItemsFromCsvAsync(csvPath, map, CancellationToken.None);
                Assert.Empty(invalid);

                using var conn = db.CreateConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Keywords FROM Items WHERE ItemNumber='NUM1'";
                var keywords = cmd.ExecuteScalar()?.ToString();
                Assert.Equal("tag1 tag2", keywords);
            }
        }
        finally
        {
            File.Delete(csvPath);
            File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ImportItemsFromCsv_DefaultsMissingColumns()
    {
        var csvPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        try
        {
            await File.WriteAllTextAsync(csvPath, "ItemNumber\nNUM1");

            await using (var db = new DatabaseService(dbPath))
            {
                new MigrationRunner(db).Migrate();
                var repository = new ItemRepository(new SqliteConnectionFactory(db.ConnectionString));
                var service = new ItemService(db, repository);

                var map = new Dictionary<string, string>
                {
                    ["ItemNumber"] = "ItemNumber"
                };

                var invalid = await service.ImportItemsFromCsvAsync(csvPath, map, CancellationToken.None);
                Assert.Empty(invalid);

                using var conn = db.CreateConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT NameDescription, Location FROM Items WHERE ItemNumber='NUM1'";
                using var reader = cmd.ExecuteReader();
                Assert.True(reader.Read());
                Assert.Equal(string.Empty, reader.GetString(0));
                Assert.Equal(string.Empty, reader.GetString(1));
            }
        }
        finally
        {
            File.Delete(csvPath);
            File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ImportItemsFromCsv_SkipsRowsWithMissingMappedFields()
    {
        var csvPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        try
        {
            await File.WriteAllTextAsync(csvPath, "ItemNumber\nNUM1");

            await using (var db = new DatabaseService(dbPath))
            {
                new MigrationRunner(db).Migrate();
                var repository = new ItemRepository(new SqliteConnectionFactory(db.ConnectionString));
                var service = new ItemService(db, repository);

                var map = new Dictionary<string, string>
                {
                    ["ItemNumber"] = "ItemNumber",
                    [nameof(ItemImportDto.Name)] = "NameDescription"
                };

                var invalid = await service.ImportItemsFromCsvAsync(csvPath, map, CancellationToken.None);
                Assert.Contains(2, invalid);

                using var conn = db.CreateConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Items";
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                Assert.Equal(0, count);
            }
        }
        finally
        {
            File.Delete(csvPath);
            File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ImportItemsFromCsv_SkipsDuplicateItemNumbers()
    {
        var csvPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        try
        {
            await File.WriteAllTextAsync(csvPath, "ItemNumber,NameDescription\nNUM1,Existing\nNUM1,Duplicate");

            await using (var db = new DatabaseService(dbPath))
            {
                new MigrationRunner(db).Migrate();
                var repository = new ItemRepository(new SqliteConnectionFactory(db.ConnectionString));
                var service = new ItemService(db, repository);

                var map = new Dictionary<string, string>
                {
                    ["ItemNumber"] = "ItemNumber",
                    [nameof(ItemImportDto.Name)] = "NameDescription"
                };

                var invalid = await service.ImportItemsFromCsvAsync(csvPath, map, CancellationToken.None);
                Assert.Contains(3, invalid);

                using var conn = db.CreateConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Items WHERE ItemNumber='NUM1'";
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                Assert.Equal(1, count);
            }
        }
        finally
        {
            File.Delete(csvPath);
            File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ImportItemsFromCsv_RollsBackInsertedRowsWhenInsertFails()
    {
        var csvPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        try
        {
            await File.WriteAllTextAsync(csvPath, "ItemNumber,NameDescription\nNUM1,First\nNUM2,Second");

            await using (var db = new DatabaseService(dbPath))
            {
                new MigrationRunner(db).Migrate();
                var repository = new ItemRepository(new SqliteConnectionFactory(db.ConnectionString));
                var service = new FailingInsertItemService(db, repository, failOnInsertAttempt: 2);

                var map = new Dictionary<string, string>
                {
                    ["ItemNumber"] = "ItemNumber",
                    [nameof(ItemImportDto.Name)] = "NameDescription"
                };

                await Assert.ThrowsAsync<InvalidOperationException>(() => service.ImportItemsFromCsvAsync(csvPath, map, CancellationToken.None));

                using var conn = db.CreateConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Items";
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                Assert.Equal(0, count);
            }
        }
        finally
        {
            File.Delete(csvPath);
            File.Delete(dbPath);
        }
    }

    private sealed class FailingInsertItemService : ItemService
    {
        private readonly int _failOnInsertAttempt;
        private int _insertAttempts;

        public FailingInsertItemService(DatabaseService dbService, IItemRepository repository, int failOnInsertAttempt)
            : base(dbService, repository)
        {
            _failOnInsertAttempt = failOnInsertAttempt;
        }

        protected override Task<int> InsertItemAsync(SqliteConnection conn, SqliteTransaction? transaction, ItemModel item, CancellationToken cancellationToken)
        {
            _insertAttempts++;
            if (_insertAttempts == _failOnInsertAttempt)
                throw new InvalidOperationException("Simulated insert failure.");

            return base.InsertItemAsync(conn, transaction, item, cancellationToken);
        }
    }
}

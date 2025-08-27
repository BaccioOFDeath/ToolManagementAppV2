using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
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
        await using var writer = new StreamWriter(csvPath);
        await writer.WriteLineAsync("ItemNumber,NameDescription,Location,Brand,PartNumber,Supplier,PurchasedDate,Notes,AvailableQuantity,IsPowered");
        for (int i = 0; i < 10000; i++)
            await writer.WriteLineAsync($"NUM{i},Name{i},Loc,Brand,Part,Supplier,2020-01-01,Note,1,0");
        await writer.FlushAsync();

        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
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

        File.Delete(csvPath);
        File.Delete(dbPath);
    }

    [Fact]
    public async Task ImportItemsFromCsv_PopulatesNames()
    {
        var csvPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        await File.WriteAllTextAsync(csvPath, "ItemNumber,NameDescription\nNUM1,ItemName");

        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
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

        File.Delete(csvPath);
        File.Delete(dbPath);
    }

    [Fact]
    public async Task ImportItemsFromCsv_PopulatesKeywords()
    {
        var csvPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        await File.WriteAllTextAsync(csvPath, "ItemNumber,Keywords\nNUM1,tag1 tag2");

        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
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

        File.Delete(csvPath);
        File.Delete(dbPath);
    }
}

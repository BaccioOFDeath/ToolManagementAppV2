using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using InventoryManagementApp.Data;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Models.ImportExport;
using Microsoft.Data.Sqlite;
using Xunit;

public class ItemServiceCsvImportTests
{
    private sealed class DummyItemRepository : IItemRepository
    {
        public IAsyncEnumerable<ItemModel> GetPageAsync(ItemFilter filter, ItemPage page, CancellationToken ct) => AsyncEnumerable.Empty<ItemModel>();
        public Task<int> CountAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(0);
        public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct) => Task.CompletedTask;
    }

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
        var service = new ItemService(db, new DummyItemRepository());
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
    public async Task ImportItemsFromCsvAsync_PersistsKeywords()
    {
        var csvPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        await File.WriteAllTextAsync(csvPath, "ItemNumber,Keywords\nNUM1,\"drill hammer\"");

        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var service = new ItemService(db, new DummyItemRepository());
        var map = new Dictionary<string, string>
        {
            ["ItemNumber"] = "ItemNumber",
            [nameof(ItemImportDto.Keywords)] = "Keywords"
        };

        var invalid = await service.ImportItemsFromCsvAsync(csvPath, map, CancellationToken.None);
        Assert.Empty(invalid);

        using var conn = db.CreateConnection();
        var result = await SqliteHelper.ExecuteScalarAsync(conn,
            "SELECT Keywords FROM Items WHERE ItemNumber=@num",
            new[] { new SqliteParameter("@num", "NUM1") },
            CancellationToken.None);

        Assert.Equal("drill hammer", result?.ToString());

        File.Delete(csvPath);
        File.Delete(dbPath);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models.ImportExport;
using InventoryManagementApp.Utilities.IO;
using Xunit;

public class CsvHelperUtilTests
{
    [Fact]
    public void LoadItemsFromCsv_PopulatesName()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        File.WriteAllText(path, "ItemNumber,NameDescription\nNUM1,ItemName");
        var map = new Dictionary<string, string>
        {
            ["ItemNumber"] = "ItemNumber",
            [nameof(ItemImportDto.Name)] = "NameDescription"
        };
        var items = CsvHelperUtil.LoadItemsFromCsv(path, map, out var invalid);
        Assert.Single(items);
        Assert.Equal("ItemName", items[0].Name);
        Assert.Empty(invalid);
        File.Delete(path);
    }

    [Fact]
    public void LoadItemsFromCsv_PopulatesKeywords()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        File.WriteAllText(path, "ItemNumber,Keywords\nNUM1,tag1 tag2");
        var map = new Dictionary<string, string>
        {
            ["ItemNumber"] = "ItemNumber",
            [nameof(ItemImportDto.Keywords)] = "Keywords"
        };
        var items = CsvHelperUtil.LoadItemsFromCsv(path, map, out var invalid);
        Assert.Single(items);
        Assert.Equal("tag1 tag2", items[0].Keywords);
        Assert.Empty(invalid);
        File.Delete(path);
    }

    [Fact]
    public async Task StreamItemsFromCsvAsync_PopulatesName()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        await File.WriteAllTextAsync(path, "ItemNumber,NameDescription\nNUM1,ItemName");
        var map = new Dictionary<string, string>
        {
            ["ItemNumber"] = "ItemNumber",
            [nameof(ItemImportDto.Name)] = "NameDescription"
        };
        var invalid = new List<int>();
        var items = new List<InventoryManagementApp.Models.Domain.ItemModel>();
        await foreach (var item in CsvHelperUtil.StreamItemsFromCsvAsync(path, map, invalid)
            .WithCancellation(CancellationToken.None))
            items.Add(item);
        Assert.Single(items);
        Assert.Equal("ItemName", items[0].Name);
        Assert.Empty(invalid);
        File.Delete(path);
    }

    [Fact]
    public async Task StreamItemsFromCsvAsync_PopulatesKeywords()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        await File.WriteAllTextAsync(path, "ItemNumber,Keywords\nNUM1,tag1 tag2");
        var map = new Dictionary<string, string>
        {
            ["ItemNumber"] = "ItemNumber",
            [nameof(ItemImportDto.Keywords)] = "Keywords"
        };
        var invalid = new List<int>();
        var items = new List<InventoryManagementApp.Models.Domain.ItemModel>();
        await foreach (var item in CsvHelperUtil.StreamItemsFromCsvAsync(path, map, invalid)
            .WithCancellation(CancellationToken.None))
            items.Add(item);
        Assert.Single(items);
        Assert.Equal("tag1 tag2", items[0].Keywords);
        Assert.Empty(invalid);
        File.Delete(path);
    }
}

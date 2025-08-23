using System;
using System.Collections.Generic;
using System.IO;
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
    public async Task LoadItemsFromCsvAsync_PopulatesName()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        await File.WriteAllTextAsync(path, "ItemNumber,NameDescription\nNUM1,ItemName");
        var map = new Dictionary<string, string>
        {
            ["ItemNumber"] = "ItemNumber",
            [nameof(ItemImportDto.Name)] = "NameDescription"
        };
        var (items, invalid) = await CsvHelperUtil.LoadItemsFromCsvAsync(path, map);
        Assert.Single(items);
        Assert.Equal("ItemName", items[0].Name);
        Assert.Empty(invalid);
        File.Delete(path);
    }
}

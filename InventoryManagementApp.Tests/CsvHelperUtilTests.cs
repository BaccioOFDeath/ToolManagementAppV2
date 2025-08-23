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
    public async Task LoadItemsFromCsv_PreservesKeywords()
    {
        var csvPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        await File.WriteAllTextAsync(csvPath, "ItemNumber,Keywords\nA1,\"tag1 tag2\"");

        var map = new Dictionary<string, string>
        {
            ["ItemNumber"] = "ItemNumber",
            [nameof(ItemImportDto.Keywords)] = "Keywords"
        };

        var items = CsvHelperUtil.LoadItemsFromCsv(csvPath, map, out var invalid);
        Assert.Empty(invalid);
        Assert.Single(items);
        Assert.Equal("tag1 tag2", items[0].Keywords);

        var (itemsAsync, invalidAsync) = await CsvHelperUtil.LoadItemsFromCsvAsync(csvPath, map, CancellationToken.None);
        Assert.Empty(invalidAsync);
        Assert.Single(itemsAsync);
        Assert.Equal("tag1 tag2", itemsAsync[0].Keywords);

        File.Delete(csvPath);
    }
}

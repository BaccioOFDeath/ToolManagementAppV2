using System.Collections.Generic;
using System.IO;
using System;
using InventoryManagementApp.Utilities.IO;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Models.Domain;
using Xunit;
using System.Threading;

public class CsvImportTests
{
    [Fact]
    public void LoadItemsFromCsv_SkipsRowsMissingItemNumber()
    {
        var csv = string.Join('\n',
            "ItemNumber,NameDescription,AvailableQuantity",
            ",Hammer,5",
            "T1,Screwdriver,2");
        var path = Path.GetTempFileName();
        File.WriteAllText(path, csv);

        var map = new Dictionary<string, string>
        {
            { "ItemNumber", "ItemNumber" },
            { "NameDescription", "NameDescription" },
            { "AvailableQuantity", "AvailableQuantity" }
        };

        var items = CsvHelperUtil.LoadItemsFromCsv(path, map, out var invalid);

        Assert.Single(items);
        Assert.Contains(2, invalid);
    }

    [Fact]
    public void LoadItemsFromCsv_HandlesQuotedHeaders()
    {
        var csv = string.Join('\n',
            "\"ItemNumber\",\"NameDescription\",\"AvailableQuantity\"",
            "T1,Hammer,5");
        var path = Path.GetTempFileName();
        File.WriteAllText(path, csv);

        var map = new Dictionary<string, string>
        {
            { "ItemNumber", "ItemNumber" },
            { "NameDescription", "NameDescription" },
            { "AvailableQuantity", "AvailableQuantity" }
        };

        var items = CsvHelperUtil.LoadItemsFromCsv(path, map, out var invalid);

        Assert.Single(items);
        Assert.Empty(invalid);
        Assert.Equal("T1", items[0].ItemNumber);
    }

    [Fact]
    public void LoadItemsFromCsv_MissingRequiredMapping_Throws()
    {
        var csv = string.Join('\n',
            "ItemNumber,NameDescription",
            "T1,Hammer");
        var path = Path.GetTempFileName();
        File.WriteAllText(path, csv);

        var map = new Dictionary<string, string>
        {
            { "NameDescription", "NameDescription" }
        };

        Assert.Throws<ArgumentException>(() => CsvHelperUtil.LoadItemsFromCsv(path, map, out _));
    }

    [Fact]
    public async Task LoadItemsFromCsvAsync_RespectsCancellation()
    {
        var csv = string.Join('\n',
            "ItemNumber,NameDescription",
            "T1,Hammer");
        var path = Path.GetTempFileName();
        File.WriteAllText(path, csv);

        var map = new Dictionary<string, string>
        {
            {"ItemNumber", "ItemNumber"},
            {"NameDescription", "NameDescription"}
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => CsvHelperUtil.LoadItemsFromCsvAsync(path, map, cts.Token));

        if (File.Exists(path)) File.Delete(path);
    }

    [Fact]
    public async System.Threading.Tasks.Task ImportItemsFromCsvAsync_SkipsInvalidRows()
    {
        var dbPath = Path.GetTempFileName();
        var csvPath = Path.GetTempFileName();
        try
        {
            var csv = string.Join('\n',
                "ItemNumber,NameDescription",
                ",Hammer",
                "T1,Screwdriver");
            File.WriteAllText(csvPath, csv);
            var db = new DatabaseService(dbPath);
            var service = new ItemService(db);
            var map = new Dictionary<string, string>
            {
                {"ItemNumber", "ItemNumber"},
                {"NameDescription", "NameDescription"}
            };

            var invalid = await service.ImportItemsFromCsvAsync(csvPath, map, CancellationToken.None);

            Assert.Single(invalid);
            Assert.Contains(2, invalid);
            Assert.Single(service.GetAllItems());
        }
        finally
        {
            if (File.Exists(csvPath)) File.Delete(csvPath);
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task ImportItemsFromCsvAsync_PerformanceWithDuplicates()
    {
        var dbPath = Path.GetTempFileName();
        var csvPath = Path.GetTempFileName();
        try
        {
            var db = new DatabaseService(dbPath);
            var service = new ItemService(db);

            for (int i = 0; i < 100; i++)
                service.AddItem(new ItemModel { ItemNumber = $"E{i}", NameDescription = $"Existing {i}" });

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("ItemNumber,NameDescription");
            for (int i = 0; i < 100; i++)
                sb.AppendLine($"E{i},Dup {i}");
            for (int i = 0; i < 900; i++)
                sb.AppendLine($"N{i},New {i}");
            File.WriteAllText(csvPath, sb.ToString());

            var map = new Dictionary<string, string>
            {
                {"ItemNumber", "ItemNumber"},
                {"NameDescription", "NameDescription"}
            };

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var invalid = await service.ImportItemsFromCsvAsync(csvPath, map, CancellationToken.None);
            sw.Stop();

            Assert.True(sw.ElapsedMilliseconds < 5000, $"Import took {sw.ElapsedMilliseconds}ms");
            Assert.Empty(invalid);
            Assert.Equal(1000, service.GetAllItems().Count);
        }
        finally
        {
            if (File.Exists(csvPath)) File.Delete(csvPath);
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task ExportItemsToCsvAsync_WritesExpectedFile()
    {
        var dbPath = Path.GetTempFileName();
        var csvPath = Path.GetTempFileName();
        try
        {
            using var db = new DatabaseService(dbPath);
            var service = new ItemService(db);
            service.AddItem(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 5, IsPowered = true });

            await service.ExportItemsToCsvAsync(csvPath);

            var lines = await File.ReadAllLinesAsync(csvPath);
            Assert.True(lines.Length > 1);
            Assert.Equal("ItemNumber,NameDescription,Location,Brand,PartNumber,Supplier,PurchasedDate,Notes,AvailableQuantity,IsPowered", lines[0]);
            Assert.Contains("T1", lines[1]);
        }
        finally
        {
            if (File.Exists(csvPath)) File.Delete(csvPath);
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }
}


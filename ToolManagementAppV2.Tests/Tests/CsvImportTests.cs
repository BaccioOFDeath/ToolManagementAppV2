using System.Collections.Generic;
using System.IO;
using System;
using ToolManagementAppV2.Utilities.IO;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Models.Domain;
using Xunit;

public class CsvImportTests
{
    [Fact]
    public void LoadToolsFromCsv_SkipsRowsMissingToolNumber()
    {
        var csv = string.Join('\n',
            "ToolNumber,NameDescription,AvailableQuantity",
            ",Hammer,5",
            "T1,Screwdriver,2");
        var path = Path.GetTempFileName();
        File.WriteAllText(path, csv);

        var map = new Dictionary<string, string>
        {
            { "ToolNumber", "ToolNumber" },
            { "NameDescription", "NameDescription" },
            { "AvailableQuantity", "AvailableQuantity" }
        };

        var tools = CsvHelperUtil.LoadToolsFromCsv(path, map, out var invalid);

        Assert.Single(tools);
        Assert.Contains(2, invalid);
    }

    [Fact]
    public void LoadToolsFromCsv_HandlesQuotedHeaders()
    {
        var csv = string.Join('\n',
            "\"ToolNumber\",\"NameDescription\",\"AvailableQuantity\"",
            "T1,Hammer,5");
        var path = Path.GetTempFileName();
        File.WriteAllText(path, csv);

        var map = new Dictionary<string, string>
        {
            { "ToolNumber", "ToolNumber" },
            { "NameDescription", "NameDescription" },
            { "AvailableQuantity", "AvailableQuantity" }
        };

        var tools = CsvHelperUtil.LoadToolsFromCsv(path, map, out var invalid);

        Assert.Single(tools);
        Assert.Empty(invalid);
        Assert.Equal("T1", tools[0].ToolNumber);
    }

    [Fact]
    public void LoadToolsFromCsv_MissingRequiredMapping_Throws()
    {
        var csv = string.Join('\n',
            "ToolNumber,NameDescription",
            "T1,Hammer");
        var path = Path.GetTempFileName();
        File.WriteAllText(path, csv);

        var map = new Dictionary<string, string>
        {
            { "NameDescription", "NameDescription" }
        };

        Assert.Throws<ArgumentException>(() => CsvHelperUtil.LoadToolsFromCsv(path, map, out _));
    }

    [Fact]
    public async System.Threading.Tasks.Task ImportToolsFromCsvAsync_SkipsInvalidRows()
    {
        var dbPath = Path.GetTempFileName();
        var csvPath = Path.GetTempFileName();
        try
        {
            var csv = string.Join('\n',
                "ToolNumber,NameDescription",
                ",Hammer",
                "T1,Screwdriver");
            File.WriteAllText(csvPath, csv);
            var db = new DatabaseService(dbPath);
            var service = new ToolService(db);
            var map = new Dictionary<string, string>
            {
                {"ToolNumber", "ToolNumber"},
                {"NameDescription", "NameDescription"}
            };

            var invalid = await service.ImportToolsFromCsvAsync(csvPath, map);

            Assert.Single(invalid);
            Assert.Contains(2, invalid);
            Assert.Single(service.GetAllTools());
        }
        finally
        {
            if (File.Exists(csvPath)) File.Delete(csvPath);
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task ImportToolsFromCsvAsync_PerformanceWithDuplicates()
    {
        var dbPath = Path.GetTempFileName();
        var csvPath = Path.GetTempFileName();
        try
        {
            var db = new DatabaseService(dbPath);
            var service = new ToolService(db);

            for (int i = 0; i < 100; i++)
                service.AddTool(new ToolModel { ToolNumber = $"E{i}", NameDescription = $"Existing {i}" });

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("ToolNumber,NameDescription");
            for (int i = 0; i < 100; i++)
                sb.AppendLine($"E{i},Dup {i}");
            for (int i = 0; i < 900; i++)
                sb.AppendLine($"N{i},New {i}");
            File.WriteAllText(csvPath, sb.ToString());

            var map = new Dictionary<string, string>
            {
                {"ToolNumber", "ToolNumber"},
                {"NameDescription", "NameDescription"}
            };

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var invalid = await service.ImportToolsFromCsvAsync(csvPath, map);
            sw.Stop();

            Assert.True(sw.ElapsedMilliseconds < 5000, $"Import took {sw.ElapsedMilliseconds}ms");
            Assert.Empty(invalid);
            Assert.Equal(1000, service.GetAllTools().Count);
        }
        finally
        {
            if (File.Exists(csvPath)) File.Delete(csvPath);
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task ExportToolsToCsvAsync_WritesExpectedFile()
    {
        var dbPath = Path.GetTempFileName();
        var csvPath = Path.GetTempFileName();
        try
        {
            using var db = new DatabaseService(dbPath);
            var service = new ToolService(db);
            service.AddTool(new ToolModel { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 5, IsPowerTool = true });

            await service.ExportToolsToCsvAsync(csvPath);

            var lines = await File.ReadAllLinesAsync(csvPath);
            Assert.True(lines.Length > 1);
            Assert.Equal("ToolNumber,NameDescription,Location,Brand,PartNumber,Supplier,PurchasedDate,Notes,AvailableQuantity,IsPowerTool", lines[0]);
            Assert.Contains("T1", lines[1]);
        }
        finally
        {
            if (File.Exists(csvPath)) File.Delete(csvPath);
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }
}


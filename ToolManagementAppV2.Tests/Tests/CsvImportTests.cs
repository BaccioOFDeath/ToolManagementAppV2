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
}


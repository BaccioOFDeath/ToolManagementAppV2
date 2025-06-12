using System.Collections.Generic;
using System.IO;
using ToolManagementAppV2.Utilities.IO;
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
}


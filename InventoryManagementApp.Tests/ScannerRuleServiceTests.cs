using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagementApp.Models;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Devices;
using Xunit;

public class ScannerRuleServiceTests
{
    static DatabaseService CreateDb(string path)
        => new DatabaseService(path);

    [Fact]
    public async Task AddRule_Persists()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        using var db = CreateDb(dbPath);
        var service = new ScannerRuleService(db);
        var rule = new ScannerFileRule { DeviceId = "dev1", SourcePath = Path.GetTempPath(), DestinationPath = Path.GetTempPath(), Pattern = "*.txt" };
        var id = await service.AddRuleAsync(rule);
        var rules = await service.GetRulesAsync("dev1");
        Assert.Single(rules);
        Assert.Equal(id, rules.First().Id);
    }

    [Fact]
    public async Task CreatedFile_TriggersCopy()
    {
        var source = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var dest = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(source);
        Directory.CreateDirectory(dest);
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        using var db = CreateDb(dbPath);
        var service = new ScannerRuleService(db);
        var rule = new ScannerFileRule { DeviceId = "dev1", SourcePath = source, DestinationPath = dest, Pattern = "*.txt" };
        await service.AddRuleAsync(rule);
        var file = Path.Combine(source, "test.txt");
        await File.WriteAllTextAsync(file, "hello");
        var copied = false;
        for (int i = 0; i < 20; i++)
        {
            await Task.Delay(100);
            if (File.Exists(Path.Combine(dest, "test.txt")))
            {
                copied = true;
                break;
            }
        }
        Assert.True(copied);
    }
}

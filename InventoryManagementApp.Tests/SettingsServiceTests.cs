using System;
using System.IO;
using System.Threading.Tasks;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Settings;
using Xunit;

public class SettingsServiceTests
{
    [Fact]
    public async Task GetSettingAsync_ReturnsNull_WhenKeyIsNull()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var service = new SettingsService(db);

        var value = await service.GetSettingAsync(null);

        Assert.Null(value);
    }
}

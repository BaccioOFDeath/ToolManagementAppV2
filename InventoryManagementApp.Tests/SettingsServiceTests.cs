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

    [Fact]
    public async Task SaveAndGetThemeAsync_RoundTrip()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var service = new SettingsService(db);

        await service.SaveThemeAsync("Dark");
        var value = await service.GetThemeAsync();

        Assert.Equal("Dark", value);
    }

    [Fact]
    public async Task GetAutoLogoutMinutesAsync_ReturnsDefaultOfOne()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var service = new SettingsService(db);

        var value = await service.GetAutoLogoutMinutesAsync();

        Assert.Equal(1, value);
    }

    [Fact]
    public async Task SaveAndGetItemCardSizeAsync_RoundTrip()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var service = new SettingsService(db);

        await service.SaveItemCardSizeAsync(1.2);
        var value = await service.GetItemCardSizeAsync();

        Assert.Equal(1.2, value);
    }
}

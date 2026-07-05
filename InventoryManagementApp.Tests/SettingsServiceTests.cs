using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
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
    public async Task GetSettingAsync_ReturnsNull_WhenKeyIsWhitespace()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var service = new SettingsService(db);

        var value = await service.GetSettingAsync("   ");

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
    public async Task SaveAndGetSettingAsync_NormalizesKeysForRoundTripAndAllSettings()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var service = new SettingsService(db);

        await service.SaveSettingAsync("  Theme  ", "Dark");

        Assert.Equal("Dark", await service.GetSettingAsync("Theme"));
        Assert.Equal("Dark", await service.GetSettingAsync("  Theme  "));

        var settings = await service.GetAllSettingsAsync();
        Assert.True(settings.ContainsKey("Theme"));
        Assert.False(settings.ContainsKey("  Theme  "));
    }

    [Fact]
    public async Task GetAutoLogoutMinutesAsync_ReturnsDefaultOfFifteen()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var service = new SettingsService(db);

        var value = await service.GetAutoLogoutMinutesAsync();

        Assert.Equal(15, value);
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

    [Fact]
    public async Task SaveAndGetItemLabelsAsync_NormalizesWhitespaceAndDefaultsBlankLabels()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var service = new SettingsService(db);

        await service.SaveItemLabelSingularAsync("  Asset  ");
        await service.SaveItemLabelPluralAsync("  Assets  ");

        Assert.Equal("Asset", await service.GetItemLabelSingularAsync());
        Assert.Equal("Assets", await service.GetItemLabelPluralAsync());

        await service.SaveItemLabelSingularAsync("   ");
        await service.SaveItemLabelPluralAsync("\t");

        Assert.Equal("Item", await service.GetItemLabelSingularAsync());
        Assert.Equal("Items", await service.GetItemLabelPluralAsync());
    }

    [Fact]
    public async Task SaveItemDetailVisibilityAsync_CanonicalizesMissingFieldsAndEventPayload()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var service = new SettingsService(db);
        var firstField = Enum.GetValues<ItemDetailField>().First();
        IDictionary<ItemDetailField, bool>? eventPayload = null;
        service.ItemDetailVisibilityChanged += (_, visibility) => eventPayload = visibility;

        await service.SaveItemDetailVisibilityAsync(new Dictionary<ItemDetailField, bool>
        {
            [firstField] = false
        });

        var savedVisibility = await service.GetItemDetailVisibilityAsync();
        var allFields = Enum.GetValues<ItemDetailField>();
        Assert.All(allFields, field => Assert.True(savedVisibility.ContainsKey(field)));
        Assert.False(savedVisibility[firstField]);
        Assert.All(allFields.Where(field => field != firstField), field => Assert.True(savedVisibility[field]));

        Assert.NotNull(eventPayload);
        Assert.All(allFields, field => Assert.True(eventPayload!.ContainsKey(field)));
        Assert.False(eventPayload![firstField]);
        Assert.All(allFields.Where(field => field != firstField), field => Assert.True(eventPayload![field]));
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Settings;
using Xunit;

public class SettingsServiceAuthorizationTests
{
    private sealed class NonAdminAuthorizationService : IAuthorizationService
    {
        public bool IsAdmin => false;
        public void EnsureAdmin() => throw new UnauthorizedAccessException();
    }

    [Fact]
    public async Task NonAdmin_LoadsDefaultWithoutSaving()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var auth = new NonAdminAuthorizationService();
        var settings = new SettingsService(db, auth);

        var value = await settings.GetShowItemImageAsync();

        Assert.True(value);
        Assert.Null(await settings.GetSettingAsync("ShowItemImage"));
    }
}


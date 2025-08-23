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

        var dict = await settings.GetItemDetailVisibilityAsync();

        Assert.All(dict.Values, Assert.True);
        Assert.Null(await settings.GetSettingAsync("ItemDetailVisibility"));
    }
}


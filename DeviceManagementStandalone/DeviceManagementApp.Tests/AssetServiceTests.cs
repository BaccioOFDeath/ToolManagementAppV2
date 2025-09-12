using System;
using System.Threading.Tasks;
using DeviceManagementApp.Models;
using DeviceManagementApp.Services;
using Xunit;

public class AssetServiceTests
{
    [Fact]
    public async Task AddUpdateAndDeleteAsset()
    {
        using var db = new DatabaseService(":memory:");
        var service = new AssetService(db);

        var asset = new Asset { Name = "Laptop", SerialNumber = "ABC123" };
        await service.AddOrUpdateAssetAsync(asset);
        Assert.True(asset.AssetId > 0);

        var loaded = await service.GetAssetAsync(asset.AssetId);
        Assert.NotNull(loaded);
        Assert.Equal("Laptop", loaded!.Name);

        asset.Name = "Desktop";
        await service.AddOrUpdateAssetAsync(asset);
        loaded = await service.GetAssetAsync(asset.AssetId);
        Assert.Equal("Desktop", loaded!.Name);

        await service.DeleteAssetAsync(asset.AssetId);
        loaded = await service.GetAssetAsync(asset.AssetId);
        Assert.Null(loaded);
    }
}

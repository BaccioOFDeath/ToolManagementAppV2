using System;
using System.Threading.Tasks;
using DeviceManagementApp.Models;
using DeviceManagementApp.Services;
using Xunit;

public class AssetAssignmentServiceTests
{
    [Fact]
    public async Task AssignAndReturnAsset()
    {
        using var db = new DatabaseService(":memory:");
        var assetService = new AssetService(db);
        var assignmentService = new AssetAssignmentService(db);

        var asset = new Asset { Name = "Phone" };
        await assetService.AddOrUpdateAssetAsync(asset);

        var assignment = new AssetAssignment
        {
            AssetId = asset.AssetId,
            UserId = 1,
            DepartmentId = 2,
            AssignedDate = DateTime.UtcNow
        };
        await assignmentService.AssignAsync(assignment);

        var current = await assignmentService.GetCurrentAssignmentAsync(asset.AssetId);
        Assert.NotNull(current);
        Assert.Equal(1, current!.UserId);
        Assert.Equal(2, current.DepartmentId);

        var loaded = await assetService.GetAssetAsync(asset.AssetId);
        Assert.NotNull(loaded);
        Assert.Equal(1, loaded!.AssignedUserId);

        await assignmentService.ReturnAsync(asset.AssetId);
        current = await assignmentService.GetCurrentAssignmentAsync(asset.AssetId);
        Assert.Null(current);
        loaded = await assetService.GetAssetAsync(asset.AssetId);
        Assert.NotNull(loaded);
        Assert.Null(loaded!.AssignedUserId);
    }
}

using System.Linq;
using System.Threading.Tasks;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Devices;
using Xunit;

public class ScannerGroupServiceTests
{
    [Fact]
    public async Task CreateGroupAndAssignDevice()
    {
        using var db = new DatabaseService(":memory:");
        var service = new ScannerGroupService(db);

        var groupId = await service.CreateGroupAsync("Test Group");
        var groups = await service.GetGroupsAsync();
        Assert.Single(groups);
        Assert.Equal("Test Group", groups.First().Name);

        await service.AssignDeviceToGroupAsync("10.0.0.1", groupId);
        var assigned = await service.GetDeviceGroupIdAsync("10.0.0.1");
        Assert.Equal(groupId, assigned);
    }
}


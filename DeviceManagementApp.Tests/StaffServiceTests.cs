using System.Linq;
using System.Threading.Tasks;
using DeviceManagementApp.Models;
using DeviceManagementApp.Services;
using Xunit;

public class StaffServiceTests
{
    [Fact]
    public async Task AddUpdateDeleteStaff()
    {
        using var db = new DatabaseService(":memory:");
        var service = new StaffService(db);

        var staff = new Staff { Name = "John" };
        var id = await service.AddStaffAsync(staff);
        Assert.True(id > 0);

        var staffList = await service.GetStaffAsync();
        Assert.Single(staffList);
        var stored = staffList.First();
        Assert.Equal("John", stored.Name);

        stored.Name = "Johnny";
        await service.UpdateStaffAsync(stored);
        var updated = (await service.GetStaffAsync()).First();
        Assert.Equal("Johnny", updated.Name);

        await service.DeleteStaffAsync(updated.StaffId);
        var listAfterDelete = await service.GetStaffAsync();
        Assert.Empty(listAfterDelete);
    }
}

using System;
using System.Threading.Tasks;
using DeviceManagementApp.Models;
using DeviceManagementApp.Services;
using Xunit;

public class DeviceAssignmentServiceTests
{
    [Fact]
    public async Task AssignAndReturnDevice()
    {
        using var db = new DatabaseService(":memory:");
        var service = new DeviceAssignmentService(db);

        var assignment = new DeviceAssignment
        {
            DeviceIp = "10.0.0.1",
            UserId = 42,
            DepartmentId = 7,
            AssignedDate = DateTime.UtcNow
        };

        await service.AssignDeviceAsync(assignment);
        var current = await service.GetCurrentAssignmentAsync("10.0.0.1");
        Assert.NotNull(current);
        Assert.Equal(42, current!.UserId);

        await service.ReturnDeviceAsync("10.0.0.1");
        current = await service.GetCurrentAssignmentAsync("10.0.0.1");
        Assert.Null(current);
    }
}

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
        var assignmentService = new DeviceAssignmentService(db);
        var deviceService = new DeviceService(db);

        await deviceService.AddOrUpdateDeviceAsync(new Device { Ip = "1.2.3.4" });

        var assignment = new DeviceAssignment
        {
            DeviceIp = "1.2.3.4",
            UserId = 1,
            DepartmentId = 2,
            AssignedDate = DateTime.UtcNow
        };
        await assignmentService.AssignAsync(assignment);

        var current = await assignmentService.GetCurrentAssignmentAsync("1.2.3.4");
        Assert.NotNull(current);
        Assert.Equal(1, current!.UserId);
        Assert.Equal(2, current.DepartmentId);

        var device = await deviceService.GetDeviceAsync("1.2.3.4", null);
        Assert.NotNull(device);
        Assert.Equal(1, device!.AssignedUserId);
        Assert.Equal(2, device.DepartmentId);

        await assignmentService.ReturnAsync("1.2.3.4");
        current = await assignmentService.GetCurrentAssignmentAsync("1.2.3.4");
        Assert.Null(current);
        device = await deviceService.GetDeviceAsync("1.2.3.4", null);
        Assert.NotNull(device);
        Assert.Null(device!.AssignedUserId);
    }
}

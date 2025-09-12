using System;
using System.IO;
using System.Threading.Tasks;
using DeviceManagementApp.Models;
using DeviceManagementApp.Services;
using Xunit;

public class DeviceServiceTests
{
    [Fact]
    public async Task AddDevice_Persists()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var service = new DeviceService(db);
        var device = new Device
        {
            Ip = "1.2.3.4",
            Port = 21,
            Hostname = "test",
            Protocol = DeviceProtocol.Ftp,
            Username = "u",
            Password = "p",
            Domain = "d",
            AssignedUserId = 1,
            DepartmentId = 2,
            Cpu = "i5",
            MemoryGb = 8,
            StorageGb = 256,
            OperatingSystem = "Windows"
        };
        await service.AddOrUpdateDeviceAsync(device);
        var fetched = await service.GetDeviceAsync("1.2.3.4", 21);
        Assert.NotNull(fetched);
        Assert.Equal("test", fetched!.Hostname);
        Assert.Equal(1, fetched.AssignedUserId);
        Assert.Equal(2, fetched.DepartmentId);
        Assert.Equal("i5", fetched.Cpu);
        Assert.Equal(8, fetched.MemoryGb);
        Assert.Equal(256, fetched.StorageGb);
        Assert.Equal("Windows", fetched.OperatingSystem);
        var all = await service.GetDevicesAsync();
        Assert.Single(all);
        await service.DeleteDeviceAsync("1.2.3.4", 21);
        Assert.Empty(await service.GetDevicesAsync());
    }
}

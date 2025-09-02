using System;
using System.IO;
using System.Threading.Tasks;
using InventoryManagementApp.Models;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Devices;
using Xunit;

public class DeviceServiceTests
{
    [Fact]
    public async Task AddDevice_Persists()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var service = new DeviceService(db);
        var device = new Device { Ip = "1.2.3.4", Hostname = "test", Protocol = DeviceProtocol.Ftp, Username = "u", Password = "p", Domain = "d" };
        await service.AddOrUpdateDeviceAsync(device);
        var fetched = await service.GetDeviceAsync("1.2.3.4");
        Assert.NotNull(fetched);
        Assert.Equal("test", fetched!.Hostname);
        var all = await service.GetDevicesAsync();
        Assert.Single(all);
        await service.DeleteDeviceAsync("1.2.3.4");
        Assert.Empty(await service.GetDevicesAsync());
    }
}

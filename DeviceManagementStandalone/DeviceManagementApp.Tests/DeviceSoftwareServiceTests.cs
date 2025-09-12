using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeviceManagementApp.Models;
using DeviceManagementApp.Services;
using Xunit;

public class DeviceSoftwareServiceTests
{
    [Fact]
    public async Task SoftwareCrud_Works()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var deviceService = new DeviceService(db);
        await deviceService.AddOrUpdateDeviceAsync(new Device { Ip = "1.2.3.4", Port = 21 });
        var service = new DeviceSoftwareService(db);
        var sw = new DeviceSoftware { DeviceIp = "1.2.3.4", DevicePort = 21, Name = "Tool", Version = "1.0" };
        await service.AddOrUpdateAsync(sw);
        var all = (await service.GetSoftwareAsync("1.2.3.4", 21)).ToList();
        Assert.Single(all);
        Assert.Equal("1.0", all[0].Version);
        sw.Version = "2.0";
        await service.AddOrUpdateAsync(sw);
        all = (await service.GetSoftwareAsync("1.2.3.4", 21)).ToList();
        Assert.Single(all);
        Assert.Equal("2.0", all[0].Version);
        await service.DeleteAsync("1.2.3.4", 21, "Tool");
        Assert.Empty(await service.GetSoftwareAsync("1.2.3.4", 21));
    }
}

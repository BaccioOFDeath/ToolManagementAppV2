using System;
using System.IO;
using System.Threading.Tasks;
using DeviceManagementApp.Models;
using DeviceManagementApp.Services;
using Xunit;
using System.Threading;
using InventoryManagementApp.Data;
using InventoryManagementApp.Models.Domain;
using System.Linq;

public class DeviceServiceTests
{
    [Fact]
    public async Task AddDevice_Persists()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var service = new DeviceService(db);
        var device = new Device { Ip = "1.2.3.4", Port = 21, Hostname = "test", Protocol = DeviceProtocol.Ftp, Username = "u", Password = "p", Domain = "d" };
        await service.AddOrUpdateDeviceAsync(device);
        var fetched = await service.GetDeviceAsync("1.2.3.4", 21);
        Assert.NotNull(fetched);
        Assert.Equal("test", fetched!.Hostname);
        var all = await service.GetDevicesAsync();
        Assert.Single(all);
        await service.DeleteDeviceAsync("1.2.3.4", 21);
        Assert.Empty(await service.GetDevicesAsync());
    }

    [Fact]
    public async Task GetDevices_ReturnsItemNameWhenLinked()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var deviceService = new DeviceService(db);
        var repo = new ItemRepository(new SqliteConnectionFactory(db.ConnectionString));
        var item = new ItemModel { ItemNumber = "I1", Name = "Hammer" };
        var itemId = await repo.InsertAsync(item, CancellationToken.None);
        await deviceService.AddOrUpdateDeviceAsync(new Device { Ip = "9.9.9.9", ItemId = itemId });
        var devices = (await deviceService.GetDevicesAsync()).ToList();
        Assert.Single(devices);
        Assert.Equal("Hammer", devices[0].ItemName);
    }
}

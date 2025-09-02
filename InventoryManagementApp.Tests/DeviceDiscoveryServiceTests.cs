using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models;
using InventoryManagementApp.Services.Devices;
using Microsoft.Extensions.Configuration;
using Xunit;

public class DeviceDiscoveryServiceTests
{
    [Fact]
    public async Task DiscoverDevices_FindsProtocolsAndMarksStatus()
    {
        var configDict = new Dictionary<string, string?>
        {
            ["DeviceDiscovery:Subnets:0"] = "192.168.0.0/29",
            ["DeviceDiscovery:FtpPort"] = "2121"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        Task<bool> PortChecker(string ip, int port, CancellationToken _)
        {
            if (ip == "192.168.0.1" && port == 445) return Task.FromResult(true);
            if (ip == "192.168.0.2" && port == 2121) return Task.FromResult(true);
            return Task.FromResult(false);
        }

        var service = new DeviceDiscoveryService(configuration, null, PortChecker);

        var devices = (await service.DiscoverDevicesAsync()).ToList();

        Assert.Equal(6, devices.Count);

        var smb = devices.First(d => d.Ip == "192.168.0.1");
        Assert.True(smb.IsOnline);
        Assert.Contains(DeviceProtocol.Smb, smb.Protocols);

        var ftp = devices.First(d => d.Ip == "192.168.0.2");
        Assert.True(ftp.IsOnline);
        Assert.Contains(DeviceProtocol.Ftp, ftp.Protocols);

        var offline = devices.First(d => d.Ip == "192.168.0.3");
        Assert.False(offline.IsOnline);
        Assert.Empty(offline.Protocols);
    }

    [Fact]
    public async Task DiscoverDevices_NoSubnetsConfigured_ReturnsEmpty()
    {
        var configuration = new ConfigurationBuilder().Build();
        var service = new DeviceDiscoveryService(configuration);

        Assert.False(service.HasConfiguredSubnets);
        var devices = await service.DiscoverDevicesAsync();

        Assert.Empty(devices);
    }
}

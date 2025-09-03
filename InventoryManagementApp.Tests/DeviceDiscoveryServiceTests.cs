using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models;
using InventoryManagementApp.Services.Devices;
using Microsoft.Extensions.Configuration;
using Xunit;

public class DeviceDiscoveryServiceTests
{
    [Fact]
    public void Constructor_UsesConfiguredMaxConcurrentScans()
    {
        var configDict = new Dictionary<string, string?>
        {
            ["DeviceDiscovery:MaxConcurrentScans"] = "42"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        var service = new DeviceDiscoveryService(configuration, null, null, () => new List<string>());

        var field = typeof(DeviceDiscoveryService).GetField("_maxConcurrentScans", BindingFlags.NonPublic | BindingFlags.Instance);
        var value = Assert.IsType<int>(field!.GetValue(service));
        Assert.Equal(42, value);
    }

    [Fact]
    public void Constructor_DefaultsMaxConcurrentScansTo128()
    {
        var configuration = new ConfigurationBuilder().Build();

        var service = new DeviceDiscoveryService(configuration, null, null, () => new List<string>());

        var field = typeof(DeviceDiscoveryService).GetField("_maxConcurrentScans", BindingFlags.NonPublic | BindingFlags.Instance);
        var value = Assert.IsType<int>(field!.GetValue(service));
        Assert.Equal(128, value);
    }

    [Fact]
    public async Task DiscoverDevices_FindsProtocolsAndMarksStatus()
    {
        var configDict = new Dictionary<string, string?>
        {
            ["DeviceDiscovery:Subnets:0"] = "192.168.0.0/29",
            ["DeviceDiscovery:FtpPorts:0"] = "21",
            ["DeviceDiscovery:FtpPorts:1"] = "2121"
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
    public async Task DiscoverDevices_FindsFtpOnDifferentPorts()
    {
        var configDict = new Dictionary<string, string?>
        {
            ["DeviceDiscovery:Subnets:0"] = "192.168.0.0/29",
            ["DeviceDiscovery:FtpPorts:0"] = "21",
            ["DeviceDiscovery:FtpPorts:1"] = "2121"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        Task<bool> PortChecker(string ip, int port, CancellationToken _)
        {
            if (ip == "192.168.0.2" && port == 21) return Task.FromResult(true);
            if (ip == "192.168.0.3" && port == 2121) return Task.FromResult(true);
            return Task.FromResult(false);
        }

        var service = new DeviceDiscoveryService(configuration, null, PortChecker);

        var devices = (await service.DiscoverDevicesAsync()).ToList();

        var ftp1 = devices.First(d => d.Ip == "192.168.0.2");
        Assert.Contains(DeviceProtocol.Ftp, ftp1.Protocols);

        var ftp2 = devices.First(d => d.Ip == "192.168.0.3");
        Assert.Contains(DeviceProtocol.Ftp, ftp2.Protocols);
    }

    [Fact]
    public async Task DiscoverDevices_FindsAdditionalPorts()
    {
        var configDict = new Dictionary<string, string?>
        {
            ["DeviceDiscovery:Subnets:0"] = "192.168.0.0/29",
            ["DeviceDiscovery:AdditionalPorts:5555"] = "Adb",
            ["DeviceDiscovery:AdditionalPorts:80"] = "Http",
            ["DeviceDiscovery:AdditionalPorts:8080"] = "Http"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        Task<bool> PortChecker(string ip, int port, CancellationToken _)
        {
            if (ip == "192.168.0.2" && port == 5555) return Task.FromResult(true);
            if (ip == "192.168.0.3" && port == 80) return Task.FromResult(true);
            if (ip == "192.168.0.4" && port == 8080) return Task.FromResult(true);
            return Task.FromResult(false);
        }

        var service = new DeviceDiscoveryService(configuration, null, PortChecker);

        var devices = (await service.DiscoverDevicesAsync()).ToList();

        var adb = devices.First(d => d.Ip == "192.168.0.2");
        Assert.Contains(DeviceProtocol.Adb, adb.Protocols);

        var http1 = devices.First(d => d.Ip == "192.168.0.3");
        Assert.Contains(DeviceProtocol.Http, http1.Protocols);

        var http2 = devices.First(d => d.Ip == "192.168.0.4");
        Assert.Contains(DeviceProtocol.Http, http2.Protocols);
    }

    [Fact]
    public async Task DiscoverDevices_UsesAutoDetectedSubnets()
    {
        var configDict = new Dictionary<string, string?>
        {
            ["DeviceDiscovery:FtpPorts:0"] = "21",
            ["DeviceDiscovery:FtpPorts:1"] = "2121"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        IList<string> detected = new List<string> { "192.168.0.0/29" };

        Task<bool> PortChecker(string ip, int port, CancellationToken _)
        {
            if (ip == "192.168.0.1" && port == 445) return Task.FromResult(true);
            if (ip == "192.168.0.2" && port == 21) return Task.FromResult(true);
            return Task.FromResult(false);
        }

        var service = new DeviceDiscoveryService(configuration, null, PortChecker, () => detected);

        Assert.True(service.HasConfiguredSubnets);
        var devices = await service.DiscoverDevicesAsync();

        Assert.Equal(6, devices.Count);
        Assert.Contains(devices, d => d.Ip == "192.168.0.1" && d.Protocols.Contains(DeviceProtocol.Smb));
    }

    [Fact]
    public async Task DiscoverDevices_NoConfigOrDetectedSubnets_ReturnsEmpty()
    {
        var configDict = new Dictionary<string, string?>
        {
            ["DeviceDiscovery:FtpPorts:0"] = "21",
            ["DeviceDiscovery:FtpPorts:1"] = "2121"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();
        var service = new DeviceDiscoveryService(configuration, null, null, () => new List<string>());

        Assert.False(service.HasConfiguredSubnets);
        var devices = await service.DiscoverDevicesAsync();

        Assert.Empty(devices);
    }

    [Fact]
    public async Task DiscoverDevices_PingResponsiveHostWithoutPorts_ReturnsUnknown()
    {
        var configDict = new Dictionary<string, string?>
        {
            ["DeviceDiscovery:Subnets:0"] = "127.0.0.1"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        Task<bool> PortChecker(string ip, int port, CancellationToken _) => Task.FromResult(false);

        var service = new DeviceDiscoveryService(configuration, null, PortChecker);

        var devices = await service.DiscoverDevicesAsync();

        var device = Assert.Single(devices);
        Assert.True(device.IsOnline);
        Assert.Contains(DeviceProtocol.Unknown, device.Protocols);
    }

    [Fact]
    public async Task ScanIpAsync_CancelsRemainingFtpChecksAfterFirstSuccess()
    {
        var configDict = new Dictionary<string, string?>
        {
            ["DeviceDiscovery:Subnets:0"] = "192.168.0.1",
            ["DeviceDiscovery:FtpPorts:0"] = "21",
            ["DeviceDiscovery:FtpPorts:1"] = "2121"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        var secondCancelled = false;
        Task<bool> PortChecker(string ip, int port, CancellationToken token)
        {
            if (port == 21)
                return Task.FromResult(true);
            if (port == 2121)
            {
                var tcs = new TaskCompletionSource<bool>();
                token.Register(() => { secondCancelled = true; tcs.TrySetCanceled(token); });
                return tcs.Task;
            }
            return Task.FromResult(false);
        }

        var service = new DeviceDiscoveryService(configuration, null, PortChecker);

        var devices = await service.DiscoverDevicesAsync();

        var device = Assert.Single(devices);
        Assert.Contains(DeviceProtocol.Ftp, device.Protocols);
        Assert.True(secondCancelled);
    }
}

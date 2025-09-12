using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Models;
using DeviceManagementApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

public class DeviceDiscoveryServiceTests
{
    private sealed class ListLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Logs { get; } = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) where TState : notnull
            => Logs.Add((logLevel, formatter(state, exception)));

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }

    [Fact]
    public void ExpandAddressPattern_ValidPrefix_ExpandsCorrectly()
    {
        var configuration = new ConfigurationBuilder().Build();
        var logger = new ListLogger<DeviceDiscoveryService>();
        var service = new DeviceDiscoveryService(configuration, logger, null, () => new List<string>());

        var method = typeof(DeviceDiscoveryService).GetMethod("ExpandAddressPattern", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var result = ((IEnumerable<string>)method.Invoke(service, new object[] { "192.168.1.0/30" })).ToList();

        Assert.Equal(new[] { "192.168.1.1", "192.168.1.2" }, result);
        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("192.168.1.0/33")]
    [InlineData("192.168.1.0/-1")]
    public void ExpandAddressPattern_InvalidPrefix_SkipsAndLogs(string subnet)
    {
        var configuration = new ConfigurationBuilder().Build();
        var logger = new ListLogger<DeviceDiscoveryService>();
        var service = new DeviceDiscoveryService(configuration, logger, null, () => new List<string>());

        var method = typeof(DeviceDiscoveryService).GetMethod("ExpandAddressPattern", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var result = ((IEnumerable<string>)method.Invoke(service, new object[] { subnet })).ToList();

        Assert.Empty(result);
        Assert.Contains(logger.Logs, l => l.Level == LogLevel.Warning);
    }

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
    public void Constructor_UsesConfiguredTimeouts()
    {
        var configDict = new Dictionary<string, string?>
        {
            ["DeviceDiscovery:LivenessTimeoutMs"] = "1234",
            ["DeviceDiscovery:PortProbeTimeoutMs"] = "5678"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        var service = new DeviceDiscoveryService(configuration, null, null, () => new List<string>());

        var livenessField = typeof(DeviceDiscoveryService).GetField("_livenessTimeoutMs", BindingFlags.NonPublic | BindingFlags.Instance);
        var portField = typeof(DeviceDiscoveryService).GetField("_portProbeTimeoutMs", BindingFlags.NonPublic | BindingFlags.Instance);
        var livenessValue = Assert.IsType<int>(livenessField!.GetValue(service));
        var portValue = Assert.IsType<int>(portField!.GetValue(service));
        Assert.Equal(1234, livenessValue);
        Assert.Equal(5678, portValue);
    }

    [Fact]
    public void Constructor_DefaultsTimeouts()
    {
        var configuration = new ConfigurationBuilder().Build();

        var service = new DeviceDiscoveryService(configuration, null, null, () => new List<string>());

        var livenessField = typeof(DeviceDiscoveryService).GetField("_livenessTimeoutMs", BindingFlags.NonPublic | BindingFlags.Instance);
        var portField = typeof(DeviceDiscoveryService).GetField("_portProbeTimeoutMs", BindingFlags.NonPublic | BindingFlags.Instance);
        var livenessValue = Assert.IsType<int>(livenessField!.GetValue(service));
        var portValue = Assert.IsType<int>(portField!.GetValue(service));
        Assert.Equal(400, livenessValue);
        Assert.Equal(700, portValue);
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

    [Fact]
    public async Task IsAlive_ReturnsTrue_WhenArpEntryExists()
    {
        var configuration = new ConfigurationBuilder().Build();
        var service = new DeviceDiscoveryService(configuration, null, null, () => new List<string>());

        var method = typeof(DeviceDiscoveryService).GetMethod("IsAlive", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var arp = new Dictionary<string, string> { ["192.168.0.5"] = "00-11-22-33-44-55" };

        var result = await (Task<bool>)method.Invoke(service, new object[] { "192.168.0.5", arp, CancellationToken.None })!;
        Assert.True(result);
    }

    [Fact]
    public async Task ResolveName_ReturnsImmediatelyWhenDnsSucceeds()
    {
        var method = typeof(DeviceDiscoveryService).GetMethod("ResolveName", BindingFlags.NonPublic | BindingFlags.Static)!;
        var tcs = new TaskCompletionSource<bool>();
        var task = (Task<string>)method.Invoke(null, new object[] { "127.0.0.1", tcs.Task, CancellationToken.None })!;
        var result = await task.ConfigureAwait(false);
        Assert.False(string.IsNullOrEmpty(result));
        Assert.False(tcs.Task.IsCompleted);
    }

    [Fact]
    public async Task ResolveName_WaitsForSmbDetectionWhenDnsFails()
    {
        var method = typeof(DeviceDiscoveryService).GetMethod("ResolveName", BindingFlags.NonPublic | BindingFlags.Static)!;
        var tcs = new TaskCompletionSource<bool>();
        var task = (Task<string>)method.Invoke(null, new object[] { "192.0.2.1", tcs.Task, CancellationToken.None })!;
        await Task.Delay(100).ConfigureAwait(false);
        Assert.False(task.IsCompleted);
        tcs.SetResult(false);
        var result = await task.ConfigureAwait(false);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task ScanIpAsync_PopulatesMacAddressFromArpTable()
    {
        var configuration = new ConfigurationBuilder().Build();
        Task<bool> PortChecker(string _, int __, CancellationToken ___) => Task.FromResult(false);
        var service = new DeviceDiscoveryService(configuration, null, PortChecker, () => new List<string>());

        var method = typeof(DeviceDiscoveryService).GetMethod("ScanIpAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var arp = new Dictionary<string, string> { ["192.168.0.1"] = "aa-bb-cc-dd-ee-ff" };
        var task = (Task<DiscoveredDevice>)method.Invoke(service, new object[] { "192.168.0.1", arp, CancellationToken.None })!;
        var device = await task.ConfigureAwait(false);

        Assert.Equal("aa-bb-cc-dd-ee-ff", device.MacAddress);
    }

    [Theory]
    [InlineData("  192.168.0.1        00-11-22-33-44-55   dynamic", "192.168.0.1", "00-11-22-33-44-55")]
    [InlineData("? (192.168.0.2) at 00:11:22:33:44:55 [ether] on eth0", "192.168.0.2", "00:11:22:33:44:55")]
    [InlineData("192.168.0.3    0x1    0x2    00:11:22:33:44:55     *    eth0", "192.168.0.3", "00:11:22:33:44:55")]
    public void TryParseArpLine_ParsesVariousFormats(string line, string ip, string mac)
    {
        var method = typeof(DeviceDiscoveryService).GetMethod("TryParseArpLine", BindingFlags.NonPublic | BindingFlags.Static)!;
        var parameters = new object?[] { line, null, null };
        var result = (bool)method.Invoke(null, parameters)!;
        Assert.True(result);
        Assert.Equal(ip, parameters[1]);
        Assert.Equal(mac, parameters[2]);
    }

    [Fact]
    public void TryParseArpLine_Invalid_ReturnsFalse()
    {
        var method = typeof(DeviceDiscoveryService).GetMethod("TryParseArpLine", BindingFlags.NonPublic | BindingFlags.Static)!;
        var parameters = new object?[] { "not an arp line", null, null };
        var result = (bool)method.Invoke(null, parameters)!;
        Assert.False(result);
    }

    [Fact]
    public async Task DiscoverDevicesAsync_SkipsDuplicateIpAddresses()
    {
        var configDict = new Dictionary<string, string?>
        {
            ["DeviceDiscovery:Subnets:0"] = "127.0.0.1",
            ["DeviceDiscovery:Subnets:1"] = "127.0.0.1",
            ["DeviceDiscovery:FtpPorts:0"] = "21"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        var callCount = 0;
        Task<bool> PortChecker(string ip, int port, CancellationToken _)
        {
            Interlocked.Increment(ref callCount);
            return Task.FromResult(false);
        }

        var service = new DeviceDiscoveryService(configuration, null, PortChecker);

        await service.DiscoverDevicesAsync();

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task DiscoverDevicesAsync_SkipsIpsFromBroadcast()
    {
        var configDict = new Dictionary<string, string?>
        {
            ["DeviceDiscovery:Subnets:0"] = "192.168.0.0/30"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        var checkedIps = new List<string>();
        Task<bool> PortChecker(string ip, int port, CancellationToken _)
        {
            lock (checkedIps) checkedIps.Add(ip);
            return Task.FromResult(false);
        }

        Task<ISet<string>> Broadcast(CancellationToken _)
            => Task.FromResult<ISet<string>>(new HashSet<string> { "192.168.0.2" });

        var service = new DeviceDiscoveryService(configuration, null, PortChecker, null, null, Broadcast);

        var devices = (await service.DiscoverDevicesAsync()).ToList();

        Assert.Contains(devices, d => d.Ip == "192.168.0.2");
        Assert.DoesNotContain("192.168.0.2", checkedIps);
        Assert.Contains("192.168.0.1", checkedIps);
    }
}

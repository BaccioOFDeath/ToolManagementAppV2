using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Services.Devices
{
    public class DeviceDiscoveryService : IDeviceDiscoveryService
    {
        private readonly ILogger<DeviceDiscoveryService> _logger;
        private readonly Func<string, int, CancellationToken, Task<bool>> _portChecker;
        private readonly IList<string> _subnets;
        private readonly int _ftpPort;

        public DeviceDiscoveryService(IConfiguration configuration,
            ILogger<DeviceDiscoveryService>? logger = null,
            Func<string, int, CancellationToken, Task<bool>>? portChecker = null)
        {
            _logger = logger ?? NullLogger<DeviceDiscoveryService>.Instance;
            _portChecker = portChecker ?? DefaultPortChecker;
            _subnets = configuration.GetSection("DeviceDiscovery:Subnets").Get<IList<string>>() ?? new List<string>();
            _ftpPort = configuration.GetValue<int?>("DeviceDiscovery:FtpPort") ?? 21;
        }

        public async Task<IReadOnlyList<DiscoveredDevice>> DiscoverDevicesAsync(CancellationToken cancellationToken = default)
        {
            var devices = new ConcurrentBag<DiscoveredDevice>();
            var addresses = _subnets.SelectMany(GetAddresses).Distinct();

            using var semaphore = new SemaphoreSlim(20);

            var tasks = addresses.Select(async ip =>
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    var device = await ScanIpAsync(ip, cancellationToken).ConfigureAwait(false);
                    devices.Add(device);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);
            return devices.OrderBy(d => d.Ip).ToList();
        }

        private async Task<DiscoveredDevice> ScanIpAsync(string ip, CancellationToken ct)
        {
            var protocols = new List<DeviceProtocol>();
            try
            {
                if (await _portChecker(ip, 445, ct).ConfigureAwait(false))
                    protocols.Add(DeviceProtocol.Smb);
                if (await _portChecker(ip, _ftpPort, ct).ConfigureAwait(false))
                    protocols.Add(DeviceProtocol.Ftp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking ports for {Ip}", ip);
            }

            string hostname;
            try
            {
                var entry = await Dns.GetHostEntryAsync(ip).ConfigureAwait(false);
                hostname = entry.HostName;
            }
            catch
            {
                hostname = ip;
            }

            return new DiscoveredDevice
            {
                Ip = ip,
                Hostname = hostname,
                IsOnline = protocols.Count > 0,
                Protocols = protocols
            };
        }

        private static IEnumerable<string> GetAddresses(string cidr)
        {
            if (string.IsNullOrWhiteSpace(cidr))
                yield break;
            var parts = cidr.Split('/');
            if (parts.Length != 2)
                yield break;
            if (!IPAddress.TryParse(parts[0], out var baseAddress))
                yield break;
            if (!int.TryParse(parts[1], out var prefix))
                yield break;
            var baseBytes = baseAddress.GetAddressBytes();
            if (baseBytes.Length != 4)
                yield break;
            if (BitConverter.IsLittleEndian)
                Array.Reverse(baseBytes);
            uint baseUint = BitConverter.ToUInt32(baseBytes, 0);
            uint mask = prefix == 0 ? 0u : uint.MaxValue << (32 - prefix);
            uint network = baseUint & mask;
            uint broadcast = network | ~mask;
            for (uint ip = network + 1; ip < broadcast; ip++)
            {
                var bytes = BitConverter.GetBytes(ip);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                yield return new IPAddress(bytes).ToString();
            }
        }

        private static async Task<bool> DefaultPortChecker(string ip, int port, CancellationToken ct)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(ip, port);
                var timeout = Task.Delay(1000, ct);
                var completed = await Task.WhenAny(connectTask, timeout).ConfigureAwait(false);
                if (completed == connectTask)
                {
                    await connectTask.ConfigureAwait(false);
                    return client.Connected;
                }
            }
            catch
            {
            }
            return false;
        }
    }
}

// Services/Devices/DeviceDiscoveryService.cs  (drop-in replacement)
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
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

        public bool HasConfiguredSubnets => _subnets.Count > 0;

        public DeviceDiscoveryService(IConfiguration configuration,
            ILogger<DeviceDiscoveryService>? logger = null,
            Func<string, int, CancellationToken, Task<bool>>? portChecker = null,
            Func<IList<string>>? subnetResolver = null)
        {
            _logger = logger ?? NullLogger<DeviceDiscoveryService>.Instance;
            _portChecker = portChecker ?? DefaultPortChecker;
            subnetResolver ??= GetLocalSubnets;

            _subnets = configuration.GetSection("DeviceDiscovery:Subnets").Get<IList<string>>() ?? new List<string>();
            if (_subnets.Count == 0)
            {
                _subnets = subnetResolver() ?? new List<string>();
                if (_subnets.Count == 0)
                    _logger.LogWarning("No subnets configured or detected for device discovery.");
                else
                    _logger.LogInformation("Using auto-detected subnets: {Subnets}", string.Join(", ", _subnets));
                }
            }
            _ftpPort = configuration.GetValue<int?>("DeviceDiscovery:FtpPort") ?? 21;
        }

        public async Task<IReadOnlyList<DiscoveredDevice>> DiscoverDevicesAsync(CancellationToken cancellationToken = default)
        {
            if (_subnets.Count == 0)
            {
                _logger.LogWarning("Device discovery attempted with no configured subnets.");
                return Array.Empty<DiscoveredDevice>();
            }

            var bag = new ConcurrentBag<DiscoveredDevice>();
            var addresses = _subnets.SelectMany(ExpandAddressPattern).Distinct();

            using var semaphore = new SemaphoreSlim(128);
            var tasks = addresses.Select(async ip =>
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    var d = await ScanIpAsync(ip, cancellationToken).ConfigureAwait(false);
                    if (d.IsOnline) bag.Add(d); // filter out pure offline noise
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);
            return bag.OrderBy(d => d.Ip, IpComparer.Instance).ToList();
        }

        private async Task<DiscoveredDevice> ScanIpAsync(string ip, CancellationToken ct)
        {
            var alive = await IsAlive(ip, ct).ConfigureAwait(false);
            var protocols = new List<DeviceProtocol>();

            if (alive)
            {
                // SMB first (persistent file access)
                if (await _portChecker(ip, 445, ct).ConfigureAwait(false))
                    protocols.Add(DeviceProtocol.Smb);
                if (await _portChecker(ip, _ftpPort, ct).ConfigureAwait(false))
                    protocols.Add(DeviceProtocol.Ftp);
            }
            catch (Exception ex)
            {
                Ip = ip,
                Hostname = string.IsNullOrWhiteSpace(hostname) ? ip : hostname,
                IsOnline = alive && protocols.Count > 0,
                Protocols = protocols
            };
        }

        // OPTIONAL: make IsAlive use SafeTcpProbeAsync too (prevents stray connect tasks during liveness checks)
        private static async Task<bool> IsAlive(string ip, CancellationToken ct)
        {
            if (await HasArpEntry(ip).ConfigureAwait(false)) return true;

            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ip, 400).ConfigureAwait(false);
                if (reply.Status == IPStatus.Success) return true;
            }
            catch { /* ignore */ }

            if (await SafeTcpProbeAsync(ip, 445, 400, ct).ConfigureAwait(false)) return true;
            if (await SafeTcpProbeAsync(ip, 80, 400, ct).ConfigureAwait(false)) return true;
            if (await SafeTcpProbeAsync(ip, 21, 400, ct).ConfigureAwait(false)) return true;
            if (await SafeTcpProbeAsync(ip, 3721, 400, ct).ConfigureAwait(false)) return true;

            return false;
        }


        private static async Task<bool> HasArpEntry(string ip)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "arp",
                    Arguments = $"-a {ip}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                if (p == null) return false;
                var output = await p.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
                await p.WaitForExitAsync().ConfigureAwait(false);
                return Regex.IsMatch(output, @$"\b{Regex.Escape(ip)}\b\s+([0-9a-f]{{2}}-){5}[0-9a-f]{{2}}", RegexOptions.IgnoreCase);
            }
            catch { return false; }
        }

        

        private static async Task<string> ResolveName(string ip, CancellationToken ct)
        {
            try
            {
                var entry = await Dns.GetHostEntryAsync(ip).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(entry.HostName)) return entry.HostName;
            }
            catch { /* ignore */ }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "nbtstat",
                    Arguments = $"-A {ip}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                if (p == null) return "";
                var read = await p.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
                await p.WaitForExitAsync(ct).ConfigureAwait(false);
                var m = Regex.Match(read, @"^\s*([A-Z0-9\-\._]+)\s+<00>\s+UNIQUE", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                if (m.Success) return m.Groups[1].Value.Trim();
            }
            catch { /* ignore */ }

            return "";
        }

        private static IEnumerable<string> ExpandAddressPattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) yield break;

            // Accept "192.168.1.*"
            if (pattern.EndsWith(".*", StringComparison.Ordinal))
            {
                var baseStr = pattern[..^2];
                for (int i = 1; i <= 254; i++) yield return $"{baseStr}.{i}";
                yield break;
            }

            // Accept CIDR "192.168.1.0/24"
            var parts = pattern.Split('/');
            if (parts.Length == 2 && IPAddress.TryParse(parts[0], out var baseAddress) && int.TryParse(parts[1], out var prefix))
            {
                var baseBytes = baseAddress.GetAddressBytes();
                if (baseBytes.Length != 4) yield break;
                if (BitConverter.IsLittleEndian) Array.Reverse(baseBytes);
                uint baseUint = BitConverter.ToUInt32(baseBytes, 0);
                uint mask = prefix == 0 ? 0u : uint.MaxValue << (32 - prefix);
                uint network = baseUint & mask;
                uint broadcast = network | ~mask;
                for (uint u = network + 1; u < broadcast; u++)
                {
                    var bytes = BitConverter.GetBytes(u);
                    if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
                    yield return new IPAddress(bytes).ToString();
                }
                yield break;
            }

            // Single IP
            if (IPAddress.TryParse(pattern, out _)) yield return pattern;
        }

        private static IList<string> GetLocalSubnets()
        {
            var subnets = new List<string>();
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up || ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                var ipProps = ni.GetIPProperties();
                foreach (var ua in ipProps.UnicastAddresses)
                {
                    if (ua.Address.AddressFamily != AddressFamily.InterNetwork || ua.IPv4Mask == null)
                        continue;

                    var ipBytes = ua.Address.GetAddressBytes();
                    var maskBytes = ua.IPv4Mask.GetAddressBytes();
                    var networkBytes = new byte[4];
                    for (int i = 0; i < 4; i++)
                        networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                    var network = new IPAddress(networkBytes);
                    int prefix = maskBytes.Sum(b => Convert.ToString(b, 2).Count(c => c == '1'));
                    var cidr = $"{network}/{prefix}";
                    if (!subnets.Contains(cidr))
                        subnets.Add(cidr);
                }
            }
            return subnets;
        }

        // PATCH for InventoryManagementApp.Services.Devices.DeviceDiscoveryService
        // Replace your DefaultPortChecker + QuickTcp with these SAFE probes (no unobserved Task exceptions).

        private static async Task<bool> SafeTcpProbeAsync(string ip, int port, int timeoutMs, CancellationToken ct)
        {
            using var client = new TcpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeoutMs);

            Task connectTask = client.ConnectAsync(ip, port);

            // Ensure exceptions are observed even if we "timeout" first
            _ = connectTask.ContinueWith(t => { var _ = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

            try
            {
                var completed = await Task.WhenAny(connectTask, Task.Delay(Timeout.InfiniteTimeSpan, cts.Token)).ConfigureAwait(false);
                if (completed != connectTask)
                {
                    try { client.Close(); } catch { }
                    return false; // timeout/cancel
                }

                // Observe completion (propagate or swallow expected socket failures)
                await connectTask.ConfigureAwait(false);
                return client.Connected;
            }
            catch (OperationCanceledException) { return false; }
            catch (ObjectDisposedException) { return false; }
            catch (SocketException) { return false; }
            catch (Exception)
            {
                return false;
            }
        }

        private static Task<bool> DefaultPortChecker(string ip, int port, CancellationToken ct)
            => SafeTcpProbeAsync(ip, port, timeoutMs: 700, ct);

        // If you have a QuickTcp helper, replace its body with:
        private static Task<bool> QuickTcp(string ip, int port, int timeoutMs, CancellationToken ct)
            => SafeTcpProbeAsync(ip, port, timeoutMs, ct);


        private sealed class IpComparer : IComparer<string>
        {
            public static readonly IpComparer Instance = new();
            public int Compare(string? x, string? y)
            {
                if (x == y) return 0;
                if (x is null) return -1;
                if (y is null) return 1;
                var xb = IPAddress.Parse(x).GetAddressBytes();
                var yb = IPAddress.Parse(y).GetAddressBytes();
                for (int i = 0; i < 4; i++)
                {
                    var c = xb[i].CompareTo(yb[i]);
                    if (c != 0) return c;
                }
                return 0;
            }
        }
    }
}

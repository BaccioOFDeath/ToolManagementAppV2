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
using System.Threading.Channels;
using System.Runtime.CompilerServices;
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
        private readonly int[] _ftpPorts;
        private readonly IDictionary<int, DeviceProtocol> _additionalPorts;
        private readonly int _maxConcurrentScans;
        private readonly int _livenessTimeoutMs;
        private readonly int _portProbeTimeoutMs;

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
                {
                    _logger.LogWarning("No subnets configured or detected for device discovery.");
                }
                else
                {
                    _logger.LogInformation("Using auto-detected subnets: {Subnets}", string.Join(", ", _subnets));
                }
            }

            _ftpPorts = configuration.GetSection("DeviceDiscovery:FtpPorts").Get<int[]>() ?? new[] { 21, 3721 };
            _additionalPorts = configuration.GetSection("DeviceDiscovery:AdditionalPorts").Get<Dictionary<int, DeviceProtocol>>() ?? new Dictionary<int, DeviceProtocol>();
            _maxConcurrentScans = configuration.GetValue<int?>("DeviceDiscovery:MaxConcurrentScans") ?? 128;
            _livenessTimeoutMs = configuration.GetValue<int?>("DeviceDiscovery:LivenessTimeoutMs") ?? 400;
            _portProbeTimeoutMs = configuration.GetValue<int?>("DeviceDiscovery:PortProbeTimeoutMs") ?? 700;
        }

        public async Task<IReadOnlyList<DiscoveredDevice>> DiscoverDevicesAsync(CancellationToken cancellationToken = default)
        {
            var list = new List<DiscoveredDevice>();
            await foreach (var device in DiscoverDevicesAsync(progress: null, cancellationToken).ConfigureAwait(false))
            {
                list.Add(device);
            }
            return list;
        }

        public async IAsyncEnumerable<DiscoveredDevice> DiscoverDevicesAsync(IProgress<double>? progress = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (_subnets.Count == 0)
            {
                _logger.LogWarning("Device discovery attempted with no configured subnets.");
                yield break;
            }

            var addresses = _subnets.SelectMany(ExpandAddressPattern).Distinct().ToList();
            var total = addresses.Count;
            if (total == 0) yield break;

            var channel = Channel.CreateUnbounded<DiscoveredDevice>();
            int processed = 0;

            using var semaphore = new SemaphoreSlim(_maxConcurrentScans);
            var tasks = addresses.Select(async ip =>
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    var d = await ScanIpAsync(ip, cancellationToken).ConfigureAwait(false);
                    if (d.IsOnline)
                        await channel.Writer.WriteAsync(d, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    var done = Interlocked.Increment(ref processed);
                    progress?.Report((double)done / total);
                    semaphore.Release();
                }
            });

            _ = Task.WhenAll(tasks).ContinueWith(_ => channel.Writer.Complete(), cancellationToken);

            await foreach (var device in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                yield return device;
            }
        }

        private async Task<DiscoveredDevice> ScanIpAsync(string ip, CancellationToken ct)
        {
            var alive = await IsAlive(ip, ct).ConfigureAwait(false);
            var protocols = new List<DeviceProtocol>();

            if (alive)
            {
                async Task<DeviceProtocol?> CheckPortAsync(int port, DeviceProtocol protocol, CancellationToken token)
                {
                    try
                    {
                        if (await _portChecker(ip, port, token).ConfigureAwait(false))
                            return protocol;
                    }
                    catch (OperationCanceledException)
                    {
                        // ignore cancellation
                    }
                    catch (Exception ex)
                    {
                        if (protocol == DeviceProtocol.Smb)
                            _logger.LogDebug(ex, "Error checking SMB on {Ip}", ip);
                        else if (protocol == DeviceProtocol.Ftp)
                            _logger.LogDebug(ex, "Error checking FTP port {Port} on {Ip}", port, ip);
                        else
                            _logger.LogDebug(ex, "Error checking port {Port} on {Ip}", port, ip);
                    }
                    return null;
                }

                using var ftpCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var tasks = new List<Task<DeviceProtocol?>>
                {
                    CheckPortAsync(445, DeviceProtocol.Smb, ct)
                };

                foreach (var port in _ftpPorts)
                    tasks.Add(CheckPortAsync(port, DeviceProtocol.Ftp, ftpCts.Token));

                foreach (var kvp in _additionalPorts)
                    tasks.Add(CheckPortAsync(kvp.Key, kvp.Value, ct));

                while (tasks.Count > 0)
                {
                    var completed = await Task.WhenAny(tasks).ConfigureAwait(false);
                    tasks.Remove(completed);
                    var proto = await completed.ConfigureAwait(false);
                    if (proto.HasValue && !protocols.Contains(proto.Value))
                    {
                        protocols.Add(proto.Value);
                        if (proto.Value == DeviceProtocol.Ftp)
                            ftpCts.Cancel();
                    }
                }
            }

            if (alive && protocols.Count == 0)
            {
                protocols.Add(DeviceProtocol.Unknown);
            }

            var hostname = await ResolveName(ip, ct).ConfigureAwait(false);

            return new DiscoveredDevice
            {
                Ip = ip,
                Hostname = string.IsNullOrWhiteSpace(hostname) ? ip : hostname,
                IsOnline = alive,
                Protocols = protocols
            };
        }

        private async Task<bool> IsAlive(string ip, CancellationToken ct)
        {
            if (await HasArpEntry(ip).ConfigureAwait(false)) return true;

            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ip, _livenessTimeoutMs).ConfigureAwait(false);
                if (reply.Status == IPStatus.Success) return true;
            }
            catch { /* ignore */ }

            var ports = new[] { 445, 80, 21, 3721 };
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var probes = ports
                .Select(p => SafeTcpProbeAsync(ip, p, _livenessTimeoutMs, cts.Token))
                .ToList();

            while (probes.Count > 0)
            {
                var completed = await Task.WhenAny(probes).ConfigureAwait(false);
                if (await completed.ConfigureAwait(false))
                {
                    cts.Cancel();
                    return true;
                }
                probes.Remove(completed);
            }

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

        private IEnumerable<string> ExpandAddressPattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) yield break;

            if (pattern.EndsWith(".*", StringComparison.Ordinal))
            {
                var baseStr = pattern[..^2];
                for (int i = 1; i <= 254; i++) yield return $"{baseStr}.{i}";
                yield break;
            }

            var parts = pattern.Split('/');
            if (parts.Length == 2 && IPAddress.TryParse(parts[0], out var baseAddress) && int.TryParse(parts[1], out var prefix))
            {
                if (prefix < 0 || prefix > 32)
                {
                    _logger.LogWarning("Subnet {Subnet} has invalid prefix {Prefix}; skipping.", pattern, prefix);
                    yield break;
                }

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

        private static async Task<bool> SafeTcpProbeAsync(string ip, int port, int timeoutMs, CancellationToken ct)
        {
            using var client = new TcpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeoutMs);

            Task connectTask = client.ConnectAsync(ip, port);
            _ = connectTask.ContinueWith(t => { var _ = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

            try
            {
                var completed = await Task.WhenAny(connectTask, Task.Delay(Timeout.InfiniteTimeSpan, cts.Token)).ConfigureAwait(false);
                if (completed != connectTask)
                {
                    try { client.Close(); } catch { }
                    return false;
                }

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

        private Task<bool> DefaultPortChecker(string ip, int port, CancellationToken ct)
            => SafeTcpProbeAsync(ip, port, _portProbeTimeoutMs, ct);

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


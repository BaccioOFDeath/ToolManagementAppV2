using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.ObjectPool;

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
        private readonly Func<CancellationToken, Task<ISet<string>>> _broadcastDiscoverer;

        private static readonly ObjectPool<Ping> _pingPool =
            new DefaultObjectPoolProvider().Create(new DefaultPooledObjectPolicy<Ping>());
        private static readonly ObjectPool<Socket> _socketPool =
            new DefaultObjectPool<Socket>(new SocketPooledObjectPolicy());
        private static readonly ObjectPool<SocketAsyncEventArgs> _saeaPool =
            new DefaultObjectPool<SocketAsyncEventArgs>(new SocketAsyncEventArgsPooledObjectPolicy());

        public bool HasConfiguredSubnets => _subnets.Count > 0;

        public DeviceDiscoveryService(IConfiguration configuration,
            ILogger<DeviceDiscoveryService>? logger = null,
            Func<string, int, CancellationToken, Task<bool>>? portChecker = null,
            Func<IList<string>>? subnetResolver = null,
            ISettingsService? settingsService = null,
            Func<CancellationToken, Task<ISet<string>>>? broadcastDiscoverer = null)
        {
            _logger = logger ?? NullLogger<DeviceDiscoveryService>.Instance;
            _portChecker = portChecker ?? DefaultPortChecker;
            subnetResolver ??= GetLocalSubnets;
            _broadcastDiscoverer = broadcastDiscoverer ?? DiscoverViaBroadcastAsync;

            _subnets = configuration.GetSection("DeviceDiscovery:Subnets").Get<IList<string>>() ?? new List<string>();
            if (settingsService != null)
            {
                var subnetsOverride = settingsService.GetSettingAsync("DeviceDiscovery_Subnets").GetAwaiter().GetResult();
                if (!string.IsNullOrWhiteSpace(subnetsOverride))
                    _subnets = subnetsOverride.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            }
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
            if (settingsService != null)
            {
                var ftpOverride = settingsService.GetSettingAsync("DeviceDiscovery_FtpPorts").GetAwaiter().GetResult();
                if (!string.IsNullOrWhiteSpace(ftpOverride))
                    _ftpPorts = ftpOverride.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => int.TryParse(p, out var v) ? v : 0).Where(v => v > 0).ToArray();
            }
            _additionalPorts = configuration.GetSection("DeviceDiscovery:AdditionalPorts").Get<Dictionary<int, DeviceProtocol>>() ?? new Dictionary<int, DeviceProtocol>();
            if (settingsService != null)
            {
                var addOverride = settingsService.GetSettingAsync("DeviceDiscovery_AdditionalPorts").GetAwaiter().GetResult();
                if (!string.IsNullOrWhiteSpace(addOverride))
                {
                    _additionalPorts = new Dictionary<int, DeviceProtocol>();
                    foreach (var part in addOverride.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var kv = part.Split(':');
                        if (kv.Length == 2 && int.TryParse(kv[0], out var port)
                            && Enum.TryParse<DeviceProtocol>(kv[1], true, out var proto))
                            _additionalPorts[port] = proto;
                    }
                }
            }
            _maxConcurrentScans = configuration.GetValue<int?>("DeviceDiscovery:MaxConcurrentScans") ?? 128;
            if (settingsService != null)
            {
                var val = settingsService.GetSettingAsync("DeviceDiscovery_MaxConcurrentScans").GetAwaiter().GetResult();
                if (int.TryParse(val, out var mcs))
                    _maxConcurrentScans = mcs;
            }
            _livenessTimeoutMs = configuration.GetValue<int?>("DeviceDiscovery:LivenessTimeoutMs") ?? 400;
            if (settingsService != null)
            {
                var val = settingsService.GetSettingAsync("DeviceDiscovery_LivenessTimeoutMs").GetAwaiter().GetResult();
                if (int.TryParse(val, out var lt))
                    _livenessTimeoutMs = lt;
            }
            _portProbeTimeoutMs = configuration.GetValue<int?>("DeviceDiscovery:PortProbeTimeoutMs") ?? 700;
            if (settingsService != null)
            {
                var val = settingsService.GetSettingAsync("DeviceDiscovery_PortProbeTimeoutMs").GetAwaiter().GetResult();
                if (int.TryParse(val, out var pt))
                    _portProbeTimeoutMs = pt;
            }
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
            CancellationToken cancellationToken = default)
        {
            if (_subnets.Count == 0)
            {
                _logger.LogWarning("Device discovery attempted with no configured subnets.");
                yield break;
            }

            var ipSequence = _subnets.SelectMany(ExpandAddressPattern);
            var arpTable = LoadArpTable();

            var channel = Channel.CreateUnbounded<DiscoveredDevice>();
            long total = 0;
            long processed = 0;

            var seenIps = new HashSet<string>();
            var seenLock = new object();

            var broadcastIps = await _broadcastDiscoverer(cancellationToken).ConfigureAwait(false);
            foreach (var ip in broadcastIps)
            {
                if (seenIps.Add(ip))
                {
                    var device = new DiscoveredDevice
                    {
                        Ip = ip,
                        Hostname = ip,
                        MacAddress = arpTable.TryGetValue(ip, out var mac) ? mac : string.Empty,
                        IsOnline = true,
                        Protocols = new List<DeviceProtocol> { DeviceProtocol.Unknown }
                    };
                    await channel.Writer.WriteAsync(device, cancellationToken).ConfigureAwait(false);
                    Interlocked.Increment(ref total);
                    Interlocked.Increment(ref processed);
                }
            }

            var scanTask = Task.Run(async () =>
            {
                try
                {
                    await Parallel.ForEachAsync(ipSequence, new ParallelOptions
                    {
                        MaxDegreeOfParallelism = _maxConcurrentScans,
                        CancellationToken = cancellationToken
                    }, async (ip, ct) =>
                    {
                        bool isNew;
                        lock (seenLock)
                        {
                            isNew = seenIps.Add(ip);
                        }
                        if (!isNew)
                            return;

                        Interlocked.Increment(ref total);

                        var d = await ScanIpAsync(ip, arpTable, ct).ConfigureAwait(false);
                        if (d.IsOnline)
                            await channel.Writer.WriteAsync(d, ct).ConfigureAwait(false);

                        var done = Interlocked.Increment(ref processed);
                        var currentTotal = Volatile.Read(ref total);
                        if (currentTotal > 0)
                            progress?.Report((double)done / currentTotal);
                    }).ConfigureAwait(false);
                }
                finally
                {
                    channel.Writer.Complete();
                }
            }, cancellationToken);

            await foreach (var device in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                yield return device;
            }

            await scanTask.ConfigureAwait(false);
        }

        private async Task<DiscoveredDevice> ScanIpAsync(string ip, IDictionary<string, string> arpTable, CancellationToken ct)
        {
            var alive = await IsAlive(ip, arpTable, ct).ConfigureAwait(false);
            var protocols = new List<DeviceProtocol>();

            Task<string> nameTask = Task.FromResult(string.Empty);
            TaskCompletionSource<bool>? smbTcs = null;

            if (alive)
            {
                smbTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                nameTask = ResolveName(ip, smbTcs.Task, ct);

                async Task<DeviceProtocol?> CheckPortAsync(int port, DeviceProtocol protocol, CancellationToken token)
                {
                    try
                    {
                        if (await _portChecker(ip, port, token).ConfigureAwait(false))
                        {
                            if (protocol == DeviceProtocol.Smb)
                                smbTcs.TrySetResult(true);
                            return protocol;
                        }
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
                        if (proto.Value == DeviceProtocol.Smb)
                            smbTcs.TrySetResult(true);
                    }
                }

                smbTcs.TrySetResult(false);
            }

            if (alive && protocols.Count == 0)
            {
                protocols.Add(DeviceProtocol.Unknown);
            }

            var hostname = await nameTask.ConfigureAwait(false);
            var macAddress = arpTable.TryGetValue(ip, out var mac) ? mac : string.Empty;

            return new DiscoveredDevice
            {
                Ip = ip,
                Hostname = string.IsNullOrWhiteSpace(hostname) ? ip : hostname,
                MacAddress = macAddress,
                IsOnline = alive,
                Protocols = protocols
            };
        }

        private async Task<bool> IsAlive(string ip, IDictionary<string, string> arpTable, CancellationToken ct)
        {
            if (HasArpEntry(ip, arpTable)) return true;

            var ports = new[] { 445, 80, 21, 3721 };
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            var probes = ports
                .Select(p => SafeTcpProbeAsync(ip, p, _livenessTimeoutMs, cts.Token))
                .ToList();

            var pingTask = SafePingAsync(ip, _livenessTimeoutMs, cts.Token);
            var tasks = new List<Task<bool>>(probes) { pingTask };

            while (tasks.Count > 0)
            {
                var completed = await Task.WhenAny(tasks).ConfigureAwait(false);
                if (await completed.ConfigureAwait(false))
                {
                    cts.Cancel();
                    return true;
                }
                tasks.Remove(completed);
            }

            return false;
        }

        private async Task<ISet<string>> DiscoverViaBroadcastAsync(CancellationToken ct)
        {
            var results = new ConcurrentDictionary<string, byte>();

            var tasks = new List<Task>
            {
                SendUdpDiscoveryAsync(
                    new IPEndPoint(IPAddress.Parse("224.0.0.251"), 5353),
                    MdnsQuery, results, joinMulticast: true, ct: ct),
                SendUdpDiscoveryAsync(
                    new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900),
                    SsdpQuery, results, joinMulticast: true, ct: ct),
                SendUdpDiscoveryAsync(
                    new IPEndPoint(IPAddress.Broadcast, 137),
                    NbnsQuery, results, broadcast: true, ct: ct)
            };

            try { await Task.WhenAll(tasks).ConfigureAwait(false); } catch { }

            return new HashSet<string>(results.Keys);
        }

        private static async Task SendUdpDiscoveryAsync(IPEndPoint endpoint, byte[] payload,
            ConcurrentDictionary<string, byte> results, bool joinMulticast = false,
            bool broadcast = false, CancellationToken ct = default)
        {
            try
            {
                using var client = new UdpClient(AddressFamily.InterNetwork);
                if (broadcast)
                    client.EnableBroadcast = true;
                if (joinMulticast)
                    client.JoinMulticastGroup(endpoint.Address);

                await client.SendAsync(payload, payload.Length, endpoint).ConfigureAwait(false);

                var stop = DateTime.UtcNow.AddMilliseconds(500);
                while (DateTime.UtcNow < stop && !ct.IsCancellationRequested)
                {
                    while (client.Available > 0)
                    {
                        var res = await client.ReceiveAsync(ct).ConfigureAwait(false);
                        results.TryAdd(res.RemoteEndPoint.Address.ToString(), 0);
                    }
                    await Task.Delay(50, ct).ConfigureAwait(false);
                }
            }
            catch { }
        }

        private static readonly byte[] MdnsQuery = new byte[]
        {
            0,0, 0,0, 0,1, 0,0, 0,0, 0,0,
            8,(byte)'_', (byte)'s',(byte)'e',(byte)'r',(byte)'v',(byte)'i',(byte)'c',(byte)'e',(byte)'s',
            7,(byte)'_',(byte)'d',(byte)'n',(byte)'s',(byte)'-',(byte)'s',(byte)'d',
            4,(byte)'_', (byte)'u',(byte)'d',(byte)'p',
            5,(byte)'l',(byte)'o',(byte)'c',(byte)'a',(byte)'l',
            0, 0,12, 0,1
        };

        private static readonly byte[] NbnsQuery = new byte[]
        {
            0,0, 0,0, 0,1, 0,0, 0,0, 0,0,
            32, 0x43,0x4b,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0, 0,0x21, 0,1
        };

        private static readonly byte[] SsdpQuery = Encoding.ASCII.GetBytes(
            "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: ssdp:all\r\n\r\n");

        private static IDictionary<string, string> LoadArpTable()
        {
            var table = new Dictionary<string, string>();
            try
            {
                if (OperatingSystem.IsLinux() && TryLoadProcArp(table))
                    return table;

                return LoadArpViaCommand(table);
            }
            catch { }
            return table;
        }

        private static IDictionary<string, string> LoadArpViaCommand(IDictionary<string, string> table)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "arp",
                    Arguments = "-a",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                if (p == null) return table;
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (TryParseArpLine(line, out var ip, out var mac))
                        table[ip] = mac;
                }
            }
            catch { }
            return table;
        }

        private static bool TryLoadProcArp(IDictionary<string, string> table)
        {
            try
            {
                foreach (var line in File.ReadLines("/proc/net/arp"))
                {
                    if (TryParseArpLine(line, out var ip, out var mac))
                        table[ip] = mac;
                }
                return table.Count > 0;
            }
            catch { }
            return false;
        }

        private static bool TryParseArpLine(string line, out string ip, out string mac)
        {
            ip = string.Empty;
            mac = string.Empty;
            if (string.IsNullOrWhiteSpace(line)) return false;

            var span = line.AsSpan();
            int start = -1;
            for (int i = 0; i <= span.Length; i++)
            {
                if (i == span.Length || char.IsWhiteSpace(span[i]))
                {
                    if (start >= 0)
                    {
                        var token = span[start..i];
                        token = token.Trim('(').Trim(')');
                        if (ip.Length == 0 && IPAddress.TryParse(token.ToString(), out _))
                        {
                            ip = token.ToString();
                        }
                        else if (mac.Length == 0 && IsMac(token))
                        {
                            mac = token.ToString();
                        }

                        if (ip.Length > 0 && mac.Length > 0)
                            return true;

                        start = -1;
                    }
                }
                else if (start < 0)
                {
                    start = i;
                }
            }

            return false;
        }

        private static bool IsMac(ReadOnlySpan<char> token)
        {
            if (token.Length != 17) return false;
            for (int i = 0; i < token.Length; i++)
            {
                if ((i + 1) % 3 == 0)
                {
                    var sep = token[i];
                    if (sep != ':' && sep != '-') return false;
                }
                else if (!Uri.IsHexDigit(token[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool HasArpEntry(string ip, IDictionary<string, string> arpTable)
        {
            return arpTable.ContainsKey(ip);
        }

        private static async Task<string> ResolveName(string ip, Task<bool> smbDetectedTask, CancellationToken ct)
        {
            try
            {
                var entry = await Dns.GetHostEntryAsync(ip).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(entry.HostName)) return entry.HostName;
            }
            catch { /* ignore */ }

            if (await smbDetectedTask.ConfigureAwait(false))
            {
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
            }

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
                    int prefix = maskBytes.Sum(b => BitOperations.PopCount((uint)b));
                    var cidr = $"{network}/{prefix}";
                    if (!subnets.Contains(cidr))
                        subnets.Add(cidr);
                }
            }
            return subnets;
        }

        private static async Task<bool> SafePingAsync(string ip, int timeoutMs, CancellationToken ct)
        {
            var ping = _pingPool.Get();
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var pingTask = ping.SendPingAsync(ip, timeoutMs, Array.Empty<byte>(), new PingOptions(64, true));
                var completed = await Task.WhenAny(pingTask, Task.Delay(Timeout.InfiniteTimeSpan, cts.Token)).ConfigureAwait(false);
                if (completed != pingTask) return false;
                var reply = await pingTask.ConfigureAwait(false);
                return reply.Status == IPStatus.Success;
            }
            catch (OperationCanceledException) { return false; }
            catch { return false; }
            finally
            {
                _pingPool.Return(ping);
            }
        }


        private static async Task<bool> SafeTcpProbeAsync(string ip, int port, int timeoutMs, CancellationToken ct)
        {
            var socket = _socketPool.Get();
            var args = _saeaPool.Get();
            var tcs = new TaskCompletionSource<SocketError>(TaskCreationOptions.RunContinuationsAsynchronously);

            EventHandler<SocketAsyncEventArgs>? handler = null;
            handler = (s, e) => tcs.TrySetResult(e.SocketError);
            args.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            args.Completed += handler;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeoutMs);

            try
            {
                if (!socket.ConnectAsync(args))
                {
                    return args.SocketError == SocketError.Success && socket.Connected;
                }

                var completed = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.InfiniteTimeSpan, cts.Token)).ConfigureAwait(false);
                if (completed != tcs.Task)
                    return false;

                var error = await tcs.Task.ConfigureAwait(false);
                return error == SocketError.Success && socket.Connected;
            }
            catch (OperationCanceledException) { return false; }
            catch (ObjectDisposedException) { return false; }
            catch (SocketException) { return false; }
            catch { return false; }
            finally
            {
                args.Completed -= handler;
                _saeaPool.Return(args);
                _socketPool.Return(socket);
            }
        }

        private Task<bool> DefaultPortChecker(string ip, int port, CancellationToken ct)
            => SafeTcpProbeAsync(ip, port, _portProbeTimeoutMs, ct);

        private static Task<bool> QuickTcp(string ip, int port, int timeoutMs, CancellationToken ct)
            => SafeTcpProbeAsync(ip, port, timeoutMs, ct);

        private sealed class SocketPooledObjectPolicy : PooledObjectPolicy<Socket>
        {
            public override Socket Create() => new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true
            };

            public override bool Return(Socket socket)
            {
                try
                {
                    if (socket.Connected)
                    {
                        try { socket.Shutdown(SocketShutdown.Both); } catch { }
                        socket.Disconnect(reuseSocket: true);
                    }
                    return true;
                }
                catch
                {
                    try { socket.Dispose(); } catch { }
                    return false;
                }
            }
        }

        private sealed class SocketAsyncEventArgsPooledObjectPolicy : PooledObjectPolicy<SocketAsyncEventArgs>
        {
            public override SocketAsyncEventArgs Create() => new SocketAsyncEventArgs();

            public override bool Return(SocketAsyncEventArgs args)
            {
                args.AcceptSocket = null;
                args.RemoteEndPoint = null;
                args.UserToken = null;
                return true;
            }
        }

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


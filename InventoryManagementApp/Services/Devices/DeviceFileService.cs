using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Services.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SMBLibrary;
using SMBLibrary.Client;
using SMBLibrary.Client.Helpers;

namespace InventoryManagementApp.Services.Devices
{
    public class DeviceFileService : IDeviceFileService
    {
        private readonly DatabaseService _db;
        private readonly ILogger<DeviceFileService> _logger;

        public DeviceFileService(DatabaseService db, ILogger<DeviceFileService>? logger = null)
        {
            _db = db;
            _logger = logger ?? NullLogger<DeviceFileService>.Instance;
        }

        public virtual async Task<IEnumerable<string>> ListFilesAsync(Device device, string? extensionFilter = null, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                switch (device.Protocol)
                {
                    case DeviceProtocol.Smb:
                        return await ListSmbFilesAsync(device, extensionFilter, cancellationToken);
                    case DeviceProtocol.Ftp:
                        return await ListFtpFilesAsync(device, extensionFilter, cancellationToken);
                    default:
                        _logger.LogWarning("Unsupported protocol {Protocol} for device {Ip}", device.Protocol, device.Ip);
                        return Array.Empty<string>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list files for device {Ip}", device.Ip);
                return Array.Empty<string>();
            }
        }

        private static bool MatchesExtension(string fileName, string? extensionFilter)
        {
            if (string.IsNullOrWhiteSpace(extensionFilter)) return true;
            var ext = Path.GetExtension(fileName);
            var f = extensionFilter.Trim();
            if (f.StartsWith("*.", StringComparison.Ordinal)) f = f[1..];
            if (!f.StartsWith(".", StringComparison.Ordinal)) f = "." + f;
            return string.Equals(ext, f, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<IEnumerable<string>> ListSmbFilesAsync(Device device, string? extensionFilter, CancellationToken cancellationToken)
        {
            var results = new List<string>();
            SMB2Client client = new SMB2Client();
            try
            {
                bool connected = await Task.Run(() => client.Connect(device.Ip, SMBTransportType.DirectTCPTransport), cancellationToken);
                if (!connected) return results;

                NTStatus status = await Task.Run(() => client.Login(device.Domain ?? string.Empty, device.Username ?? string.Empty, device.Password ?? string.Empty), cancellationToken);
                if (status != NTStatus.STATUS_SUCCESS) return results;

                status = client.ListShares(out var shares);
                if (status != NTStatus.STATUS_SUCCESS) return results;

                foreach (var shareObj in shares)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string shareName;
                    if (shareObj is string sName)
                    {
                        shareName = sName;
                    }
                    else
                    {
                        dynamic d = shareObj;
                        shareName = (string)d.ShareName;
                    }

                    if (string.Equals(shareName, "IPC$", StringComparison.OrdinalIgnoreCase)) continue;

                    status = client.TreeConnect(shareName, out var fileStore);
                    if (status != NTStatus.STATUS_SUCCESS) continue;

                    try
                    {
                        status = fileStore.ListFiles("\\", out var items);
                        if (status != NTStatus.STATUS_SUCCESS) continue;

                        foreach (var info in items)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var name = info.FileName;
                            if (string.IsNullOrEmpty(name)) continue;
                            if (name is "." or "..") continue;
                            if (MatchesExtension(name, extensionFilter)) results.Add(name);
                        }
                    }
                    finally
                    {
                        try { fileStore.Disconnect(); } catch { }
                        try { fileStore.Dispose(); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMB file listing failed for device {Ip}", device.Ip);
            }
            finally
            {
                try { client.Logoff(); } catch { }
                try { client.Disconnect(); } catch { }
                client.Dispose();
            }
            return results;
        }

        private async Task<IEnumerable<string>> ListFtpFilesAsync(Device device, string? extensionFilter, CancellationToken cancellationToken)
        {
            var results = new List<string>();
            using var client = new AsyncFtpClient(device.Ip, device.Username, device.Password);
            try
            {
                await client.Connect(cancellationToken);
                var list = await client.GetNameListing("/", cancellationToken);
                foreach (var item in list)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var name = Path.GetFileName(item);
                    if (MatchesExtension(name, extensionFilter)) results.Add(name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FTP file listing failed for device {Ip}", device.Ip);
            }
            finally
            {
                try
                {
                    if (client.IsConnected)
                        await client.Disconnect(cancellationToken);
                }
                catch { }
            }
            return results;
        }

        protected virtual async Task<byte[]?> DownloadFileAsync(Device device, string file, CancellationToken cancellationToken)
        {
            try
            {
                switch (device.Protocol)
                {
                    case DeviceProtocol.Ftp:
                        using (var client = new AsyncFtpClient(device.Ip, device.Username, device.Password))
                        {
                            await client.Connect(cancellationToken);
                            using var ms = new MemoryStream();
                            await client.DownloadStream(ms, file, token: cancellationToken);
                            if (client.IsConnected)
                                await client.Disconnect(cancellationToken);
                            return ms.ToArray();
                        }
                    case DeviceProtocol.Smb:
                        _logger.LogWarning("SMB download not implemented for device {Ip}", device.Ip);
                        return null;
                    default:
                        _logger.LogWarning("Unsupported protocol {Protocol} for device {Ip}", device.Protocol, device.Ip);
                        return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download {File} from device {Ip}", file, device.Ip);
                return null;
            }
        }

        public async Task<int> DownloadUnseenFilesAsync(Device device, string basePath, CancellationToken cancellationToken = default)
        {
            var deviceDirName = string.IsNullOrWhiteSpace(device.Hostname) ? device.Ip : device.Hostname;
            var deviceDir = Path.Combine(basePath, "Devices", deviceDirName);
            Directory.CreateDirectory(deviceDir);
            var files = await ListFilesAsync(device, null, cancellationToken);
            var count = 0;
            using var conn = _db.CreateConnection();
            foreach (var f in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var data = await DownloadFileAsync(device, f, cancellationToken);
                if (data == null) continue;
                var hash = Convert.ToHexString(SHA256.HashData(data));
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT OR IGNORE INTO PulledDeviceFiles (DeviceIp, Hash) VALUES ($ip,$hash)";
                cmd.Parameters.AddWithValue("$ip", device.Ip);
                cmd.Parameters.AddWithValue("$hash", hash);
                var inserted = cmd.ExecuteNonQuery();
                if (inserted == 0) continue;
                var dest = Path.Combine(deviceDir, Path.GetFileName(f));
                await File.WriteAllBytesAsync(dest, data, cancellationToken);
                count++;
            }
            return count;
        }
    }
}

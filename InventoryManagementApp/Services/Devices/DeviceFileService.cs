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

        private async Task<IEnumerable<string>> ListSmbFilesAsync(Device device, string? extensionFilter, CancellationToken cancellationToken)
        {
            var results = new List<string>();
            try
            {
                using var client = new SMB2Client();
                var status = await Task.Run(() => client.Connect(device.Ip, SMBTransportType.DirectTCPTransport), cancellationToken);
                if (status != NTStatus.STATUS_SUCCESS)
                    return results;
                status = await Task.Run(() => client.Login(device.Domain, device.Username, device.Password), cancellationToken);
                if (status != NTStatus.STATUS_SUCCESS)
                    return results;
                status = client.ListShares(out var shares);
                if (status != NTStatus.STATUS_SUCCESS)
                    return results;
                foreach (var share in shares)
                {
                    string shareName = share is string s ? s : (string)((dynamic)share).ShareName;
                    if (string.Equals(shareName, "IPC$", StringComparison.OrdinalIgnoreCase))
                        continue;
                    status = client.TreeConnect(shareName, out var fileStore);
                    if (status != NTStatus.STATUS_SUCCESS)
                        continue;
                    try
                    {
                        var files = fileStore.ListFiles("\\") as IEnumerable<dynamic>;
                        if (files == null) continue;
                        foreach (var f in files)
                        {
                            string name = f is string s2 ? s2 : (string)f.FileName;
                            if (extensionFilter == null || Path.GetExtension(name).Equals(extensionFilter, StringComparison.OrdinalIgnoreCase))
                                results.Add(name);
                        }
                    }
                    finally
                    {
                        fileStore.Disconnect();
                        fileStore.Dispose();
                    }
                }
                client.Logoff();
                client.Disconnect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMB file listing failed for device {Ip}", device.Ip);
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
                    var name = Path.GetFileName(item);
                    if (extensionFilter == null || Path.GetExtension(name).Equals(extensionFilter, StringComparison.OrdinalIgnoreCase))
                        results.Add(name);
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
                if (inserted == 0) continue; // duplicate
                var dest = Path.Combine(deviceDir, Path.GetFileName(f));
                await File.WriteAllBytesAsync(dest, data, cancellationToken);
                count++;
            }
            return count;
        }
    }
}

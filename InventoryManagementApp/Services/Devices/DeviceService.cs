using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Services.Core;
using Microsoft.Data.Sqlite;

namespace InventoryManagementApp.Services.Devices
{
    public class DeviceService : IDeviceService
    {
        readonly DatabaseService _db;

        public DeviceService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT d.Ip, d.Hostname, d.Protocol, d.Username, d.Password, d.Domain, d.ItemId, i.NameDescription
                               FROM Devices d LEFT JOIN Items i ON i.DeviceId = d.Ip ORDER BY d.Ip";
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var devices = new List<Device>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                devices.Add(new Device
                {
                    Ip = reader.GetString(0),
                    Hostname = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Protocol = Enum.TryParse<DeviceProtocol>(reader.IsDBNull(2) ? string.Empty : reader.GetString(2), true, out var p) ? p : DeviceProtocol.Unknown,
                    Username = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Password = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Domain = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    ItemId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    ItemName = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                });
            }
            return devices;
        }

        public async Task<Device?> GetDeviceAsync(string ip, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT d.Ip, d.Hostname, d.Protocol, d.Username, d.Password, d.Domain, d.ItemId, i.NameDescription
                               FROM Devices d LEFT JOIN Items i ON i.DeviceId = d.Ip WHERE d.Ip=$ip";
            cmd.Parameters.AddWithValue("$ip", ip);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return new Device
                {
                    Ip = reader.GetString(0),
                    Hostname = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Protocol = Enum.TryParse<DeviceProtocol>(reader.IsDBNull(2) ? string.Empty : reader.GetString(2), true, out var p) ? p : DeviceProtocol.Unknown,
                    Username = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Password = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Domain = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    ItemId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    ItemName = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                };
            }
            return null;
        }

        public async Task AddOrUpdateDeviceAsync(Device device, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Devices (Ip, Hostname, Protocol, Username, Password, Domain, ItemId)
                                VALUES ($ip, $hostname, $protocol, $username, $password, $domain, $itemId)
                                ON CONFLICT(Ip) DO UPDATE SET 
                                    Hostname=$hostname,
                                    Protocol=$protocol,
                                    Username=$username,
                                    Password=$password,
                                    Domain=$domain,
                                    ItemId=$itemId";
            cmd.Parameters.AddWithValue("$ip", device.Ip);
            cmd.Parameters.AddWithValue("$hostname", (object?)device.Hostname ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$protocol", device.Protocol.ToString());
            cmd.Parameters.AddWithValue("$username", (object?)device.Username ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$password", (object?)device.Password ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$domain", (object?)device.Domain ?? DBNull.Value);
            if (device.ItemId.HasValue)
                cmd.Parameters.AddWithValue("$itemId", device.ItemId.Value);
            else
                cmd.Parameters.AddWithValue("$itemId", DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteDeviceAsync(string ip, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Devices WHERE Ip=$ip";
            cmd.Parameters.AddWithValue("$ip", ip);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

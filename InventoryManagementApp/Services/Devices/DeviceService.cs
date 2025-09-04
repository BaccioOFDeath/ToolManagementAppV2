using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
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
            cmd.CommandText = @"SELECT d.Ip, d.Port, d.Hostname, d.Protocol, d.Username, d.Password, d.Domain, d.ItemId, i.NameDescription
                               FROM Devices d LEFT JOIN Items i ON i.DeviceId = d.Ip ORDER BY d.Ip, d.Port";
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var devices = new List<Device>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                devices.Add(new Device
                {
                    Ip = reader.GetString(0),
                    Port = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    Hostname = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Protocol = Enum.TryParse<DeviceProtocol>(reader.IsDBNull(3) ? string.Empty : reader.GetString(3), true, out var p) ? p : DeviceProtocol.Unknown,
                    Username = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Password = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Domain = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    ItemId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                    ItemName = reader.IsDBNull(8) ? string.Empty : reader.GetString(8)
                });
            }
            return devices;
        }

        public async Task<Device?> GetDeviceAsync(string ip, int? port, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT d.Ip, d.Port, d.Hostname, d.Protocol, d.Username, d.Password, d.Domain, d.ItemId, i.NameDescription
                               FROM Devices d LEFT JOIN Items i ON i.DeviceId = d.Ip WHERE d.Ip=$ip AND IFNULL(d.Port,-1)=IFNULL($port,-1)";
            cmd.Parameters.AddWithValue("$ip", ip);
            if (port.HasValue)
                cmd.Parameters.AddWithValue("$port", port.Value);
            else
                cmd.Parameters.AddWithValue("$port", DBNull.Value);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return new Device
                {
                    Ip = reader.GetString(0),
                    Port = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    Hostname = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Protocol = Enum.TryParse<DeviceProtocol>(reader.IsDBNull(3) ? string.Empty : reader.GetString(3), true, out var p) ? p : DeviceProtocol.Unknown,
                    Username = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Password = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Domain = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    ItemId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                    ItemName = reader.IsDBNull(8) ? string.Empty : reader.GetString(8)
                };
            }
            return null;
        }

        public async Task AddOrUpdateDeviceAsync(Device device, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Devices (Ip, Port, Hostname, Protocol, Username, Password, Domain, ItemId)
                                VALUES ($ip, $port, $hostname, $protocol, $username, $password, $domain, $itemId)
                                ON CONFLICT(Ip, Port) DO UPDATE SET
                                    Hostname=$hostname,
                                    Protocol=$protocol,
                                    Username=$username,
                                    Password=$password,
                                    Domain=$domain,
                                    ItemId=$itemId";
            cmd.Parameters.AddWithValue("$ip", device.Ip);
            if (device.Port.HasValue)
                cmd.Parameters.AddWithValue("$port", device.Port.Value);
            else
                cmd.Parameters.AddWithValue("$port", DBNull.Value);
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

        public async Task DeleteDeviceAsync(string ip, int? port, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Devices WHERE Ip=$ip AND IFNULL(Port,-1)=IFNULL($port,-1)"; 
            cmd.Parameters.AddWithValue("$ip", ip);
            if (port.HasValue)
                cmd.Parameters.AddWithValue("$port", port.Value);
            else
                cmd.Parameters.AddWithValue("$port", DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

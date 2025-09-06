using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using DeviceManagementApp.Services;
using Microsoft.Data.Sqlite;

namespace DeviceManagementApp.Services
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
            cmd.CommandText = @"SELECT Ip, Port, Hostname, Protocol, Username, Password, Domain
                               FROM Devices ORDER BY Ip, Port";
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
                    Domain = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                });
            }
            return devices;
        }

        public async Task<Device?> GetDeviceAsync(string ip, int? port, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Ip, Port, Hostname, Protocol, Username, Password, Domain
                               FROM Devices WHERE Ip=$ip AND IFNULL(Port,-1)=IFNULL($port,-1)";
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
                    Domain = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                };
            }
            return null;
        }

        public async Task AddOrUpdateDeviceAsync(Device device, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Devices (Ip, Port, Hostname, Protocol, Username, Password, Domain)
                                VALUES ($ip, $port, $hostname, $protocol, $username, $password, $domain)
                                ON CONFLICT(Ip, Port) DO UPDATE SET
                                    Hostname=$hostname,
                                    Protocol=$protocol,
                                    Username=$username,
                                    Password=$password,
                                    Domain=$domain";
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

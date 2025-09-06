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
            cmd.CommandText = @"SELECT d.Ip, d.Port, d.Hostname, d.Protocol, d.Username, d.Password, d.Domain,
                                        d.AssignedUserId, d.DepartmentId, d.Cpu, d.MemoryGb, d.StorageGb, d.OperatingSystem
                               FROM Devices d ORDER BY d.Ip, d.Port";
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
                    AssignedUserId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                    DepartmentId = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                    Cpu = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    MemoryGb = reader.IsDBNull(10) ? null : reader.GetInt32(10),
                    StorageGb = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                    OperatingSystem = reader.IsDBNull(12) ? string.Empty : reader.GetString(12)
                });
            }
            return devices;
        }

        public async Task<Device?> GetDeviceAsync(string ip, int? port, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT d.Ip, d.Port, d.Hostname, d.Protocol, d.Username, d.Password, d.Domain,
                                        d.AssignedUserId, d.DepartmentId, d.Cpu, d.MemoryGb, d.StorageGb, d.OperatingSystem
                               FROM Devices d WHERE d.Ip=$ip AND IFNULL(d.Port,-1)=IFNULL($port,-1)";
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
                    AssignedUserId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                    DepartmentId = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                    Cpu = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    MemoryGb = reader.IsDBNull(10) ? null : reader.GetInt32(10),
                    StorageGb = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                    OperatingSystem = reader.IsDBNull(12) ? string.Empty : reader.GetString(12)
                };
            }
            return null;
        }

        public async Task AddOrUpdateDeviceAsync(Device device, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Devices (Ip, Port, Hostname, Protocol, Username, Password, Domain, AssignedUserId, DepartmentId, Cpu, MemoryGb, StorageGb, OperatingSystem)
                                VALUES ($ip, $port, $hostname, $protocol, $username, $password, $domain, $assignedUserId, $departmentId, $cpu, $memoryGb, $storageGb, $operatingSystem)
                                ON CONFLICT(Ip, Port) DO UPDATE SET
                                    Hostname=$hostname,
                                    Protocol=$protocol,
                                    Username=$username,
                                    Password=$password,
                                    Domain=$domain,
                                    AssignedUserId=$assignedUserId,
                                    DepartmentId=$departmentId,
                                    Cpu=$cpu,
                                    MemoryGb=$memoryGb,
                                    StorageGb=$storageGb,
                                    OperatingSystem=$operatingSystem";
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
            if (device.AssignedUserId.HasValue)
                cmd.Parameters.AddWithValue("$assignedUserId", device.AssignedUserId.Value);
            else
                cmd.Parameters.AddWithValue("$assignedUserId", DBNull.Value);
            if (device.DepartmentId.HasValue)
                cmd.Parameters.AddWithValue("$departmentId", device.DepartmentId.Value);
            else
                cmd.Parameters.AddWithValue("$departmentId", DBNull.Value);
            cmd.Parameters.AddWithValue("$cpu", (object?)device.Cpu ?? DBNull.Value);
            if (device.MemoryGb.HasValue)
                cmd.Parameters.AddWithValue("$memoryGb", device.MemoryGb.Value);
            else
                cmd.Parameters.AddWithValue("$memoryGb", DBNull.Value);
            if (device.StorageGb.HasValue)
                cmd.Parameters.AddWithValue("$storageGb", device.StorageGb.Value);
            else
                cmd.Parameters.AddWithValue("$storageGb", DBNull.Value);
            cmd.Parameters.AddWithValue("$operatingSystem", (object?)device.OperatingSystem ?? DBNull.Value);
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

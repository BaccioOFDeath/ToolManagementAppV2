using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using Microsoft.Data.Sqlite;

namespace DeviceManagementApp.Services
{
    public class DeviceSoftwareService : IDeviceSoftwareService
    {
        readonly DatabaseService _db;

        public DeviceSoftwareService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<IEnumerable<DeviceSoftware>> GetSoftwareAsync(string deviceIp, int? devicePort, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Name, Version FROM DeviceSoftware WHERE DeviceIp=$ip AND IFNULL(DevicePort,-1)=IFNULL($port,-1) ORDER BY Name";
            cmd.Parameters.AddWithValue("$ip", deviceIp);
            if (devicePort.HasValue)
                cmd.Parameters.AddWithValue("$port", devicePort.Value);
            else
                cmd.Parameters.AddWithValue("$port", DBNull.Value);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var list = new List<DeviceSoftware>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                list.Add(new DeviceSoftware
                {
                    DeviceIp = deviceIp,
                    DevicePort = devicePort,
                    Name = reader.GetString(0),
                    Version = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                });
            }
            return list;
        }

        public async Task AddOrUpdateAsync(DeviceSoftware software, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO DeviceSoftware (DeviceIp, DevicePort, Name, Version)
                                VALUES ($ip, $port, $name, $version)
                                ON CONFLICT(DeviceIp, DevicePort, Name) DO UPDATE SET Version=$version";
            cmd.Parameters.AddWithValue("$ip", software.DeviceIp);
            if (software.DevicePort.HasValue)
                cmd.Parameters.AddWithValue("$port", software.DevicePort.Value);
            else
                cmd.Parameters.AddWithValue("$port", DBNull.Value);
            cmd.Parameters.AddWithValue("$name", software.Name);
            cmd.Parameters.AddWithValue("$version", (object?)software.Version ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(string deviceIp, int? devicePort, string name, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"DELETE FROM DeviceSoftware WHERE DeviceIp=$ip AND IFNULL(DevicePort,-1)=IFNULL($port,-1) AND Name=$name";
            cmd.Parameters.AddWithValue("$ip", deviceIp);
            if (devicePort.HasValue)
                cmd.Parameters.AddWithValue("$port", devicePort.Value);
            else
                cmd.Parameters.AddWithValue("$port", DBNull.Value);
            cmd.Parameters.AddWithValue("$name", name);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

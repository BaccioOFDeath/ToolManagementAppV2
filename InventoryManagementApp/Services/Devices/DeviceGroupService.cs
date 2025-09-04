using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using InventoryManagementApp.Services.Core;
using Microsoft.Data.Sqlite;

namespace InventoryManagementApp.Services.Devices
{
    public class DeviceGroupService : IDeviceGroupService
    {
        readonly DatabaseService _db;

        public DeviceGroupService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<int> CreateGroupAsync(string name, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO DeviceGroups (Name) VALUES ($name); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$name", name);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return (int)(long)result!;
        }

        public async Task<IEnumerable<DeviceGroup>> GetGroupsAsync(CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT GroupId, Name FROM DeviceGroups ORDER BY Name";
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var groups = new List<DeviceGroup>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                groups.Add(new DeviceGroup
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }
            return groups;
        }

        public async Task UpdateGroupAsync(DeviceGroup group, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE DeviceGroups SET Name=$name WHERE GroupId=$id";
            cmd.Parameters.AddWithValue("$name", group.Name);
            cmd.Parameters.AddWithValue("$id", group.Id);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM DeviceGroups WHERE GroupId=$id";
            cmd.Parameters.AddWithValue("$id", groupId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task AssignDeviceToGroupAsync(string deviceIp, int? devicePort, int? groupId, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            if (groupId.HasValue)
            {
                cmd.CommandText = "INSERT INTO DeviceGroupAssignments (DeviceIp, DevicePort, GroupId) VALUES ($ip, $port, $gid) ON CONFLICT(DeviceIp, DevicePort) DO UPDATE SET GroupId=$gid";
                cmd.Parameters.AddWithValue("$ip", deviceIp);
                if (devicePort.HasValue)
                    cmd.Parameters.AddWithValue("$port", devicePort.Value);
                else
                    cmd.Parameters.AddWithValue("$port", DBNull.Value);
                cmd.Parameters.AddWithValue("$gid", groupId.Value);
            }
            else
            {
                cmd.CommandText = "DELETE FROM DeviceGroupAssignments WHERE DeviceIp=$ip AND IFNULL(DevicePort,-1)=IFNULL($port,-1)";
                cmd.Parameters.AddWithValue("$ip", deviceIp);
                if (devicePort.HasValue)
                    cmd.Parameters.AddWithValue("$port", devicePort.Value);
                else
                    cmd.Parameters.AddWithValue("$port", DBNull.Value);
            }
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<int?> GetDeviceGroupIdAsync(string deviceIp, int? devicePort, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT GroupId FROM DeviceGroupAssignments WHERE DeviceIp=$ip AND IFNULL(DevicePort,-1)=IFNULL($port,-1)";
            cmd.Parameters.AddWithValue("$ip", deviceIp);
            if (devicePort.HasValue)
                cmd.Parameters.AddWithValue("$port", devicePort.Value);
            else
                cmd.Parameters.AddWithValue("$port", DBNull.Value);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            if (result == null || result == DBNull.Value)
                return null;
            return (int)(long)result;
        }
    }
}

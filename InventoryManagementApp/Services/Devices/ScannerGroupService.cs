using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Services.Core;
using Microsoft.Data.Sqlite;

namespace InventoryManagementApp.Services.Devices
{
    public class ScannerGroupService : IScannerGroupService
    {
        readonly DatabaseService _db;

        public ScannerGroupService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<int> CreateGroupAsync(string name, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO ScannerGroups (Name) VALUES ($name); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$name", name);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return (int)(long)result!;
        }

        public async Task<IEnumerable<ScannerGroup>> GetGroupsAsync(CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT GroupId, Name FROM ScannerGroups ORDER BY Name";
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var groups = new List<ScannerGroup>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                groups.Add(new ScannerGroup
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }
            return groups;
        }

        public async Task UpdateGroupAsync(ScannerGroup group, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE ScannerGroups SET Name=$name WHERE GroupId=$id";
            cmd.Parameters.AddWithValue("$name", group.Name);
            cmd.Parameters.AddWithValue("$id", group.Id);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM ScannerGroups WHERE GroupId=$id";
            cmd.Parameters.AddWithValue("$id", groupId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task AssignDeviceToGroupAsync(string deviceIp, int? groupId, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            if (groupId.HasValue)
            {
                cmd.CommandText = "INSERT INTO ScannerDeviceGroups (DeviceIp, GroupId) VALUES ($ip, $gid) ON CONFLICT(DeviceIp) DO UPDATE SET GroupId=$gid";
                cmd.Parameters.AddWithValue("$ip", deviceIp);
                cmd.Parameters.AddWithValue("$gid", groupId.Value);
            }
            else
            {
                cmd.CommandText = "DELETE FROM ScannerDeviceGroups WHERE DeviceIp=$ip";
                cmd.Parameters.AddWithValue("$ip", deviceIp);
            }
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<int?> GetDeviceGroupIdAsync(string deviceIp, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT GroupId FROM ScannerDeviceGroups WHERE DeviceIp=$ip";
            cmd.Parameters.AddWithValue("$ip", deviceIp);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            if (result == null || result == DBNull.Value)
                return null;
            return (int)(long)result;
        }
    }
}

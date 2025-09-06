using System;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using Microsoft.Data.Sqlite;

namespace DeviceManagementApp.Services
{
    public class DeviceAssignmentService : IDeviceAssignmentService
    {
        readonly DatabaseService _db;

        public DeviceAssignmentService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<int> AssignDeviceAsync(DeviceAssignment assignment, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO DeviceAssignments (DeviceIp, UserId, AssignedDate, DepartmentId)
                                VALUES ($ip, $userId, $assigned, $dept); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$ip", assignment.DeviceIp);
            cmd.Parameters.AddWithValue("$userId", assignment.UserId);
            cmd.Parameters.AddWithValue("$assigned", assignment.AssignedDate);
            if (assignment.DepartmentId.HasValue)
                cmd.Parameters.AddWithValue("$dept", assignment.DepartmentId.Value);
            else
                cmd.Parameters.AddWithValue("$dept", DBNull.Value);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return (int)(long)result!;
        }

        public async Task<DeviceAssignment?> GetCurrentAssignmentAsync(string deviceIp, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT AssignmentId, DeviceIp, UserId, AssignedDate, ReturnedDate, DepartmentId
                                FROM DeviceAssignments
                                WHERE DeviceIp=$ip AND ReturnedDate IS NULL ORDER BY AssignedDate DESC LIMIT 1";
            cmd.Parameters.AddWithValue("$ip", deviceIp);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return new DeviceAssignment
                {
                    AssignmentId = reader.GetInt32(0),
                    DeviceIp = reader.GetString(1),
                    UserId = reader.GetInt32(2),
                    AssignedDate = reader.GetDateTime(3),
                    ReturnedDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    DepartmentId = reader.IsDBNull(5) ? null : reader.GetInt32(5)
                };
            }
            return null;
        }

        public async Task ReturnDeviceAsync(string deviceIp, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE DeviceAssignments SET ReturnedDate=$ret WHERE DeviceIp=$ip AND ReturnedDate IS NULL";
            cmd.Parameters.AddWithValue("$ret", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("$ip", deviceIp);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

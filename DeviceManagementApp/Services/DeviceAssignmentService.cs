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

        public async Task<DeviceAssignment?> GetCurrentAssignmentAsync(string deviceIp, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT DeviceIp, UserId, AssignedDate, ReturnedDate, DepartmentId
                                FROM DeviceAssignments
                                WHERE DeviceIp=$ip AND ReturnedDate IS NULL
                                ORDER BY AssignedDate DESC LIMIT 1";
            cmd.Parameters.AddWithValue("$ip", deviceIp);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return new DeviceAssignment
                {
                    DeviceIp = reader.GetString(0),
                    UserId = reader.GetInt32(1),
                    AssignedDate = reader.GetDateTime(2),
                    ReturnedDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    DepartmentId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                };
            }
            return null;
        }

        public async Task AssignAsync(DeviceAssignment assignment, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var tran = conn.BeginTransaction();
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = @"INSERT INTO DeviceAssignments (DeviceIp, UserId, AssignedDate, DepartmentId)
                                    VALUES ($ip, $uid, $adate, $dept)";
                cmd.Parameters.AddWithValue("$ip", assignment.DeviceIp);
                cmd.Parameters.AddWithValue("$uid", assignment.UserId);
                cmd.Parameters.AddWithValue("$adate", assignment.AssignedDate);
                if (assignment.DepartmentId.HasValue)
                    cmd.Parameters.AddWithValue("$dept", assignment.DepartmentId.Value);
                else
                    cmd.Parameters.AddWithValue("$dept", DBNull.Value);
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = @"UPDATE Devices SET AssignedUserId=$uid, DepartmentId=$dept WHERE Ip=$ip";
                cmd.Parameters.AddWithValue("$uid", assignment.UserId);
                if (assignment.DepartmentId.HasValue)
                    cmd.Parameters.AddWithValue("$dept", assignment.DepartmentId.Value);
                else
                    cmd.Parameters.AddWithValue("$dept", DBNull.Value);
                cmd.Parameters.AddWithValue("$ip", assignment.DeviceIp);
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            await tran.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task ReturnAsync(string deviceIp, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var tran = conn.BeginTransaction();
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = @"UPDATE DeviceAssignments SET ReturnedDate=$rdate WHERE DeviceIp=$ip AND ReturnedDate IS NULL";
                cmd.Parameters.AddWithValue("$ip", deviceIp);
                cmd.Parameters.AddWithValue("$rdate", DateTime.UtcNow);
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = @"UPDATE Devices SET AssignedUserId=NULL WHERE Ip=$ip";
                cmd.Parameters.AddWithValue("$ip", deviceIp);
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            await tran.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

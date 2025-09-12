using System;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using Microsoft.Data.Sqlite;

namespace DeviceManagementApp.Services
{
    public class AssetAssignmentService : IAssetAssignmentService
    {
        readonly DatabaseService _db;

        public AssetAssignmentService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<AssetAssignment?> GetCurrentAssignmentAsync(int assetId, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT AssetId, UserId, AssignedDate, ReturnedDate, DepartmentId
                                FROM AssetAssignments
                                WHERE AssetId=$id AND ReturnedDate IS NULL
                                ORDER BY AssignedDate DESC LIMIT 1";
            cmd.Parameters.AddWithValue("$id", assetId);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return new AssetAssignment
                {
                    AssetId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    AssignedDate = reader.GetDateTime(2),
                    ReturnedDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    DepartmentId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                };
            }
            return null;
        }

        public async Task AssignAsync(AssetAssignment assignment, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var tran = conn.BeginTransaction();
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = @"INSERT INTO AssetAssignments (AssetId, UserId, AssignedDate, DepartmentId)
                                    VALUES ($id, $uid, $adate, $dept)";
                cmd.Parameters.AddWithValue("$id", assignment.AssetId);
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
                cmd.CommandText = @"UPDATE Assets SET AssignedUserId=$uid, DepartmentId=$dept WHERE AssetId=$id";
                cmd.Parameters.AddWithValue("$uid", assignment.UserId);
                if (assignment.DepartmentId.HasValue)
                    cmd.Parameters.AddWithValue("$dept", assignment.DepartmentId.Value);
                else
                    cmd.Parameters.AddWithValue("$dept", DBNull.Value);
                cmd.Parameters.AddWithValue("$id", assignment.AssetId);
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            await tran.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task ReturnAsync(int assetId, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var tran = conn.BeginTransaction();
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = @"UPDATE AssetAssignments SET ReturnedDate=$rdate WHERE AssetId=$id AND ReturnedDate IS NULL";
                cmd.Parameters.AddWithValue("$id", assetId);
                cmd.Parameters.AddWithValue("$rdate", DateTime.UtcNow);
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = @"UPDATE Assets SET AssignedUserId=NULL, DepartmentId=NULL WHERE AssetId=$id";
                cmd.Parameters.AddWithValue("$id", assetId);
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            await tran.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

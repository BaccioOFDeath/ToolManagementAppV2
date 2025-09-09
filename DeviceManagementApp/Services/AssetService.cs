using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using Microsoft.Data.Sqlite;

namespace DeviceManagementApp.Services
{
    public class AssetService : IAssetService
    {
        readonly DatabaseService _db;

        public AssetService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Asset>> GetAssetsAsync(CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT AssetId, Name, SerialNumber, AssignedUserId, DepartmentId FROM Assets ORDER BY Name";
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var assets = new List<Asset>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                assets.Add(new Asset
                {
                    AssetId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    SerialNumber = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    AssignedUserId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    DepartmentId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                });
            }
            return assets;
        }

        public async Task<Asset?> GetAssetAsync(int assetId, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT AssetId, Name, SerialNumber, AssignedUserId, DepartmentId FROM Assets WHERE AssetId=$id";
            cmd.Parameters.AddWithValue("$id", assetId);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return new Asset
                {
                    AssetId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    SerialNumber = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    AssignedUserId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    DepartmentId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                };
            }
            return null;
        }

        public async Task AddOrUpdateAssetAsync(Asset asset, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Assets (AssetId, Name, SerialNumber, AssignedUserId, DepartmentId)
                                VALUES ($id, $name, $serial, $user, $dept)
                                ON CONFLICT(AssetId) DO UPDATE SET
                                    Name=$name,
                                    SerialNumber=$serial,
                                    AssignedUserId=$user,
                                    DepartmentId=$dept";
            if (asset.AssetId > 0)
                cmd.Parameters.AddWithValue("$id", asset.AssetId);
            else
                cmd.Parameters.AddWithValue("$id", DBNull.Value);
            cmd.Parameters.AddWithValue("$name", asset.Name);
            cmd.Parameters.AddWithValue("$serial", (object?)asset.SerialNumber ?? DBNull.Value);
            if (asset.AssignedUserId.HasValue)
                cmd.Parameters.AddWithValue("$user", asset.AssignedUserId.Value);
            else
                cmd.Parameters.AddWithValue("$user", DBNull.Value);
            if (asset.DepartmentId.HasValue)
                cmd.Parameters.AddWithValue("$dept", asset.DepartmentId.Value);
            else
                cmd.Parameters.AddWithValue("$dept", DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            if (asset.AssetId == 0)
            {
                using var lastIdCmd = conn.CreateCommand();
                lastIdCmd.CommandText = "SELECT last_insert_rowid()";
                asset.AssetId = Convert.ToInt32(await lastIdCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
            }
        }

        public async Task DeleteAssetAsync(int assetId, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Assets WHERE AssetId=$id";
            cmd.Parameters.AddWithValue("$id", assetId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;

namespace DeviceManagementApp.Services
{
    public class StaffService : IStaffService
    {
        readonly DatabaseService _db;
        public StaffService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Staff>> GetStaffAsync(CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT StaffId, Name, Role, Email, Phone FROM Staff ORDER BY Name";
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var list = new List<Staff>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                list.Add(new Staff
                {
                    StaffId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Role = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Phone = reader.IsDBNull(4) ? null : reader.GetString(4)
                });
            }
            return list;
        }

        public async Task<int> AddStaffAsync(Staff staff, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Staff (Name, Role, Email, Phone) VALUES ($name,$role,$email,$phone); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$name", staff.Name);
            cmd.Parameters.AddWithValue("$role", (object?)staff.Role ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$email", (object?)staff.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$phone", (object?)staff.Phone ?? DBNull.Value);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return (int)(long)result!;
        }

        public async Task UpdateStaffAsync(Staff staff, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Staff SET Name=$name, Role=$role, Email=$email, Phone=$phone WHERE StaffId=$id";
            cmd.Parameters.AddWithValue("$name", staff.Name);
            cmd.Parameters.AddWithValue("$role", (object?)staff.Role ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$email", (object?)staff.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$phone", (object?)staff.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$id", staff.StaffId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteStaffAsync(int staffId, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Staff WHERE StaffId=$id";
            cmd.Parameters.AddWithValue("$id", staffId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

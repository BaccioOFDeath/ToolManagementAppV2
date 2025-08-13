using System.Data.SQLite;
using System;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ToolManagementAppV2.Services.Settings
{
    public class SettingsService : ISettingsService
    {
        readonly DatabaseService _dbService;
        readonly ILogger<SettingsService> _logger;
        const string UpsertSql = @"
            INSERT INTO Settings (Key, Value) 
            VALUES (@Key, @Value)
            ON CONFLICT(Key) DO UPDATE SET Value = @Value";

        public SettingsService(DatabaseService dbService, ILogger<SettingsService>? logger = null)
        {
            _dbService = dbService;
            _logger = logger ?? NullLogger<SettingsService>.Instance;
        }

        public void SaveSetting(string key, string value)
        {
            var p = new[]
            {
                new SQLiteParameter("@Key", key),
                new SQLiteParameter("@Value", value)
            };
            using var conn = _dbService.CreateConnection();
            SqliteHelper.ExecuteNonQuery(conn, UpsertSql, p);
        }

        public string GetSetting(string key)
        {
            const string sql = "SELECT Value FROM Settings WHERE Key = @Key";
            using var conn = _dbService.CreateConnection();
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Key", key);
            return cmd.ExecuteScalar()?.ToString();
        }

        public Dictionary<string, string> GetAllSettings()
        {
            var dict = new Dictionary<string, string>();
            const string sql = "SELECT Key, Value FROM Settings";
            using var conn = _dbService.CreateConnection();
            using var cmd = new SQLiteCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                dict[rdr["Key"].ToString()] = rdr["Value"].ToString();
            return dict;
        }

        /// <summary>
        /// Updates or inserts multiple settings within a single transaction.
        /// </summary>
        /// <param name="settings">Key/value pairs to upsert.</param>
        /// <exception cref="SQLiteException">
        /// Thrown when a database error occurs. The original exception is propagated to the caller.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a transaction cannot be started. The original exception is propagated to the caller.
        /// </exception>
        public void UpdateSettings(Dictionary<string, string> settings)
        {
            using var conn = _dbService.CreateConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                foreach (var kv in settings)
                {
                    var p = new[]
                    {
                        new SQLiteParameter("@Key", kv.Key),
                        new SQLiteParameter("@Value", kv.Value)
                    };
                    SqliteHelper.ExecuteNonQuery(conn, tx, UpsertSql, p);
                }
                tx.Commit();
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError(ex, "Failed to update settings");
                throw;
            }
        }

        public void DeleteSetting(string key)
        {
            const string sql = "DELETE FROM Settings WHERE Key = @Key";
            var p = new[] { new SQLiteParameter("@Key", key) };
            using var conn = _dbService.CreateConnection();
            SqliteHelper.ExecuteNonQuery(conn, sql, p);
        }
    }
}

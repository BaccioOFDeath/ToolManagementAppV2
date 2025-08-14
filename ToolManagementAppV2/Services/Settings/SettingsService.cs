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
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            try
            {
                var p = new[]
                {
                    new SQLiteParameter("@Key", key),
                    new SQLiteParameter("@Value", value)
                };
                using var conn = _dbService.CreateConnection();
                SqliteHelper.ExecuteNonQuery(conn, UpsertSql, p);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save setting {Key}", key);
                throw new InvalidOperationException($"Failed to save setting '{key}'.", ex);
            }
        }

        public string? GetSetting(string key)
        {
            try
            {
                const string sql = "SELECT Value FROM Settings WHERE Key = @Key";
                using var conn = _dbService.CreateConnection();
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Key", key);
                return cmd.ExecuteScalar()?.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve setting {Key}", key);
                throw new InvalidOperationException($"Failed to retrieve setting '{key}'.", ex);
            }
        }

        public Dictionary<string, string> GetAllSettings()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all settings");
                throw new InvalidOperationException("Failed to retrieve all settings.", ex);
            }
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
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            foreach (var kv in settings)
            {
                if (string.IsNullOrWhiteSpace(kv.Key))
                    throw new ArgumentException("Key cannot be null or empty.", nameof(settings));
            }

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
                throw new InvalidOperationException("Failed to update settings.", ex);
            }
        }

        public void DeleteSetting(string key)
        {
            try
            {
                const string sql = "DELETE FROM Settings WHERE Key = @Key";
                var p = new[] { new SQLiteParameter("@Key", key) };
                using var conn = _dbService.CreateConnection();
                var affected = SqliteHelper.ExecuteNonQuery(conn, sql, p);
                if (affected == 0)
                    _logger.LogWarning("No setting found for key {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete setting {Key}", key);
                throw new InvalidOperationException($"Failed to delete setting '{key}'.", ex);
            }
        }

        const string ScannerIpKey = "ScannerIpAddresses";

        public IEnumerable<string> GetScannerIpAddresses()
        {
            var value = GetSetting(ScannerIpKey);
            if (string.IsNullOrWhiteSpace(value))
                return Array.Empty<string>();
            return value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        public void SaveScannerIpAddresses(IEnumerable<string> ipAddresses)
        {
            var value = string.Join(';', ipAddresses);
            SaveSetting(ScannerIpKey, value);
        }
    }
}

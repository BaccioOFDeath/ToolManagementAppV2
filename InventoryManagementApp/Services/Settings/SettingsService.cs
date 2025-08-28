using Microsoft.Data.Sqlite;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Services.Users;

namespace InventoryManagementApp.Services.Settings
{
    public class SettingsService : ISettingsService
    {
        readonly DatabaseService _dbService;
        readonly ILogger<SettingsService> _logger;
        readonly IAuthorizationService _auth;
        public event EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;
        const string UpsertSql = @"
            INSERT INTO Settings (Key, Value) 
            VALUES (@Key, @Value)
            ON CONFLICT(Key) DO UPDATE SET Value = @Value";

        public SettingsService(DatabaseService dbService, IAuthorizationService? authorizationService = null, ILogger<SettingsService>? logger = null)
        {
            _dbService = dbService;
            _auth = authorizationService ?? new NoOpAuthorizationService();
            _logger = logger ?? NullLogger<SettingsService>.Instance;
        }

        public async Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            try
            {
                var p = new[]
                {
                    new SqliteParameter("@Key", key),
                    new SqliteParameter("@Value", value)
                };
                using var conn = _dbService.CreateConnection();
                await SqliteHelper.ExecuteNonQueryAsync(conn, UpsertSql, p, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Saving setting {Key} canceled or timed out", key);
                throw;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_BUSY)
            {
                _logger.LogWarning(ex, "Saving setting {Key} timed out", key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save setting {Key}", key);
                throw new InvalidOperationException($"Failed to save setting '{key}'.", ex);
            }
        }

        public async Task<string?> GetSettingAsync(string? key, CancellationToken cancellationToken = default)
        {
            if (key is null)
                return null;

            try
            {
                const string sql = "SELECT Value FROM Settings WHERE Key = @Key";
                using var conn = _dbService.CreateConnection();
                var p = new[] { new SqliteParameter("@Key", key) };
                var result = await SqliteHelper.ExecuteScalarAsync(conn, sql, p, cancellationToken).ConfigureAwait(false);
                return result?.ToString();
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Retrieving setting {Key} canceled or timed out", key);
                throw;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_BUSY)
            {
                _logger.LogWarning(ex, "Retrieving setting {Key} timed out", key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve setting {Key}", key);
                throw new InvalidOperationException($"Failed to retrieve setting '{key}'.", ex);
            }
        }

        public async Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var dict = new Dictionary<string, string>();
                const string sql = "SELECT Key, Value FROM Settings";
                using var conn = _dbService.CreateConnection();
                using var cmd = new SqliteCommand(sql, conn);
                using var rdr = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var key = rdr["Key"]?.ToString();
                    var value = rdr["Value"]?.ToString();
                    if (key != null && value != null)
                        dict[key] = value;
                }
                return dict;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Retrieving all settings canceled or timed out");
                throw;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_BUSY)
            {
                _logger.LogWarning(ex, "Retrieving all settings timed out");
                throw;
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
        /// <exception cref="SqliteException">
        /// Thrown when a database error occurs. The original exception is propagated to the caller.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a transaction cannot be started. The original exception is propagated to the caller.
        /// </exception>
        public async Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
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
                        new SqliteParameter("@Key", kv.Key),
                        new SqliteParameter("@Value", kv.Value)
                    };
                    await SqliteHelper.ExecuteNonQueryAsync(conn, tx, UpsertSql, p, cancellationToken).ConfigureAwait(false);
                }
                tx.Commit();
            }
            catch (OperationCanceledException ex)
            {
                tx.Rollback();
                _logger.LogWarning(ex, "Updating settings canceled or timed out");
                throw;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_BUSY)
            {
                tx.Rollback();
                _logger.LogWarning(ex, "Updating settings timed out");
                throw;
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError(ex, "Failed to update settings");
                throw new InvalidOperationException("Failed to update settings.", ex);
            }
        }

        public async Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            try
            {
                const string sql = "DELETE FROM Settings WHERE Key = @Key";
                var p = new[] { new SqliteParameter("@Key", key) };
                using var conn = _dbService.CreateConnection();
                var affected = await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken).ConfigureAwait(false);
                if (affected == 0)
                    _logger.LogWarning("No setting found for key {Key}", key);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Deleting setting {Key} canceled or timed out", key);
                throw;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_BUSY)
            {
                _logger.LogWarning(ex, "Deleting setting {Key} timed out", key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete setting {Key}", key);
                throw new InvalidOperationException($"Failed to delete setting '{key}'.", ex);
            }
        }

        const string ScannerIpKey = "ScannerIpAddresses";
        const string PasswordIterationsKey = "PasswordIterations";
        const string AutoLogoutMinutesKey = "AutoLogoutMinutes";
        const string ItemLabelSingularKey = "ItemLabelSingular";
        const string ItemLabelPluralKey = "ItemLabelPlural";
        const string ItemDetailVisibilityKey = "ItemDetailVisibility";

        public async Task<IEnumerable<string>> GetScannerIpAddressesAsync(CancellationToken cancellationToken = default)
        {
            var value = await GetSettingAsync(ScannerIpKey, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(value))
                return Array.Empty<string>();

            var valid = new List<string>();
            foreach (var ip in value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (IPAddress.TryParse(ip, out _))
                    valid.Add(ip);
            }

            return valid;
        }

        public async Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            if (ipAddresses == null)
            {
                await DeleteSettingAsync(ScannerIpKey, cancellationToken).ConfigureAwait(false);
                return Array.Empty<string>();
            }

            var valid = new List<string>();
            var invalid = new List<string>();
            foreach (var ip in ipAddresses)
            {
                if (IPAddress.TryParse(ip, out _))
                    valid.Add(ip);
                else
                    invalid.Add(ip);
            }

            if (invalid.Count > 0)
                _logger.LogWarning("Ignoring invalid IP addresses: {InvalidIps}", string.Join(", ", invalid));

            if (valid.Count > 0)
            {
                var value = string.Join(';', valid);
                await SaveSettingAsync(ScannerIpKey, value, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await DeleteSettingAsync(ScannerIpKey, cancellationToken).ConfigureAwait(false);
            }

            return invalid;
        }

        // Password hashing configuration
        public async Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default)
        {
            var value = await GetSettingAsync(PasswordIterationsKey, cancellationToken).ConfigureAwait(false);
            return int.TryParse(value, out var i) && i > 0 ? i : 100_000;
        }

        public async Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            if (iterations <= 0)
                throw new ArgumentOutOfRangeException(nameof(iterations));
            await SaveSettingAsync(PasswordIterationsKey, iterations.ToString(), cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default)
        {
            var value = await GetSettingAsync(AutoLogoutMinutesKey, cancellationToken).ConfigureAwait(false);
            return int.TryParse(value, out var i) && i >= 0 ? i : 0;
        }

        public async Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            if (minutes < 0)
                throw new ArgumentOutOfRangeException(nameof(minutes));
            await SaveSettingAsync(AutoLogoutMinutesKey, minutes.ToString(), cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetItemLabelSingularAsync(CancellationToken cancellationToken = default)
        {
            var value = await GetSettingAsync(ItemLabelSingularKey, cancellationToken).ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(value) ? "Item" : value;
        }

        public async Task SaveItemLabelSingularAsync(string label, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            await SaveSettingAsync(ItemLabelSingularKey, label, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetItemLabelPluralAsync(CancellationToken cancellationToken = default)
        {
            var value = await GetSettingAsync(ItemLabelPluralKey, cancellationToken).ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(value) ? "Items" : value;
        }

        public async Task SaveItemLabelPluralAsync(string label, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            await SaveSettingAsync(ItemLabelPluralKey, label, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IDictionary<ItemDetailField, bool>> GetItemDetailVisibilityAsync(CancellationToken cancellationToken = default)
        {
            var json = await GetSettingAsync(ItemDetailVisibilityKey, cancellationToken).ConfigureAwait(false);
            Dictionary<ItemDetailField, bool> visibility;
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    var dict = JsonSerializer.Deserialize<Dictionary<string, bool>>(json!) ?? new();
                    visibility = Enum.GetValues<ItemDetailField>().ToDictionary(f => f, f => dict.TryGetValue(f.ToString(), out var v) ? v : true);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse item detail visibility settings");
                    visibility = Enum.GetValues<ItemDetailField>().ToDictionary(f => f, _ => true);
                }
            }
            else
            {
                visibility = Enum.GetValues<ItemDetailField>().ToDictionary(f => f, _ => true);
                if (_auth.IsAdmin)
                {
                    await SaveItemDetailVisibilityAsync(visibility, cancellationToken).ConfigureAwait(false);
                }
            }
            return visibility;
        }

        public async Task SaveItemDetailVisibilityAsync(IDictionary<ItemDetailField, bool> visibility, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            var dict = visibility.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);
            var json = JsonSerializer.Serialize(dict);
            await SaveSettingAsync(ItemDetailVisibilityKey, json, cancellationToken).ConfigureAwait(false);
            ItemDetailVisibilityChanged?.Invoke(this, new Dictionary<ItemDetailField, bool>(visibility));
        }
    }
}

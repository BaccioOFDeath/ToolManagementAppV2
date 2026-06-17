using Microsoft.Data.Sqlite;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Services.Users;

namespace InventoryManagementApp.Services.Settings
{
    /// <summary>
    /// Service for managing application settings stored in the database, including item detail visibility and configuration options.
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly DatabaseService _dbService;
        private readonly ILogger<SettingsService> _logger;
        private readonly IAuthorizationService _auth;
        
        /// <summary>
        /// Raised when item detail field visibility settings are changed.
        /// </summary>
        public event EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;
        public event EventHandler<double>? ItemCardSizeChanged;
        
        private const string UpsertSql = @"
            INSERT INTO Settings (Key, Value)
            VALUES (@Key, @Value)
            ON CONFLICT(Key) DO UPDATE SET Value = @Value";

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsService"/> class.
        /// </summary>
        /// <param name="dbService">Database service for data access.</param>
        /// <param name="authorizationService">Optional authorization service for access control.</param>
        /// <param name="logger">Optional logger for diagnostic output.</param>
        public SettingsService(DatabaseService dbService, IAuthorizationService? authorizationService = null, ILogger<SettingsService>? logger = null)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _auth = authorizationService ?? new NoOpAuthorizationService();
            _logger = logger ?? NullLogger<SettingsService>.Instance;
        }

        /// <summary>
        /// Saves a setting to the database. Requires settings permission.
        /// </summary>
        /// <param name="key">The setting key.</param>
        /// <param name="value">The setting value.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <exception cref="ArgumentException">Thrown if key is null or whitespace.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if user lacks settings permission.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the database operation fails.</exception>
        public async Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            _auth.EnsurePermission(User.PermissionSettings);
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

        /// <summary>
        /// Retrieves a setting value from the database.
        /// </summary>
        /// <param name="key">The setting key to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>The setting value if found; otherwise, null.</returns>
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
            _auth.EnsurePermission(User.PermissionSettings);
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
            _auth.EnsurePermission(User.PermissionSettings);
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

        const string PasswordIterationsKey = "PasswordIterations";
        const string AutoLogoutMinutesKey = "AutoLogoutMinutes";
        const string ThemeKey = "Theme";
        const string ItemLabelSingularKey = "ItemLabelSingular";
        const string ItemLabelPluralKey = "ItemLabelPlural";
        const string ItemDetailVisibilityKey = "ItemDetailVisibility";
        const string ItemCardSizeKey = "ItemCardSize";

        // Theme configuration
        public Task<string?> GetThemeAsync(CancellationToken cancellationToken = default)
            => GetSettingAsync(ThemeKey, cancellationToken);

        public Task SaveThemeAsync(string theme, CancellationToken cancellationToken = default)
            => SaveSettingAsync(ThemeKey, theme, cancellationToken);

        // Password hashing configuration
        public async Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default)
        {
            var value = await GetSettingAsync(PasswordIterationsKey, cancellationToken).ConfigureAwait(false);
            return int.TryParse(value, out var i) && i > 0 ? i : 100_000;
        }

        public async Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default)
        {
            _auth.EnsurePermission(User.PermissionSettings);
            if (iterations <= 0)
                throw new ArgumentOutOfRangeException(nameof(iterations));
            await SaveSettingAsync(PasswordIterationsKey, iterations.ToString(), cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default)
        {
            var value = await GetSettingAsync(AutoLogoutMinutesKey, cancellationToken).ConfigureAwait(false);
            return int.TryParse(value, out var i) && i >= 0 ? i : 1;
        }

        public async Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default)
        {
            _auth.EnsurePermission(User.PermissionSettings);
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
            _auth.EnsurePermission(User.PermissionSettings);
            await SaveSettingAsync(ItemLabelSingularKey, label, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetItemLabelPluralAsync(CancellationToken cancellationToken = default)
        {
            var value = await GetSettingAsync(ItemLabelPluralKey, cancellationToken).ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(value) ? "Items" : value;
        }

        public async Task SaveItemLabelPluralAsync(string label, CancellationToken cancellationToken = default)
        {
            _auth.EnsurePermission(User.PermissionSettings);
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
                if (_auth.HasPermission(User.PermissionSettings))
                {
                    await SaveItemDetailVisibilityAsync(visibility, cancellationToken).ConfigureAwait(false);
                }
            }
            return visibility;
        }

        public async Task SaveItemDetailVisibilityAsync(IDictionary<ItemDetailField, bool> visibility, CancellationToken cancellationToken = default)
        {
            _auth.EnsurePermission(User.PermissionSettings);
            var dict = visibility.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);
            var json = JsonSerializer.Serialize(dict);
            await SaveSettingAsync(ItemDetailVisibilityKey, json, cancellationToken).ConfigureAwait(false);
            ItemDetailVisibilityChanged?.Invoke(this, new Dictionary<ItemDetailField, bool>(visibility));
        }

        public async Task<double> GetItemCardSizeAsync(CancellationToken cancellationToken = default)
        {
            var value = await GetSettingAsync(ItemCardSizeKey, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(value))
                return 1.0;

            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) && parsed > 0.2)
                return parsed;

            return 1.0;
        }

        public async Task SaveItemCardSizeAsync(double size, CancellationToken cancellationToken = default)
        {
            _auth.EnsurePermission(User.PermissionSettings);
            if (size <= 0.2)
                throw new ArgumentOutOfRangeException(nameof(size));
            var value = size.ToString("0.###", CultureInfo.InvariantCulture);
            await SaveSettingAsync(ItemCardSizeKey, value, cancellationToken).ConfigureAwait(false);
            ItemCardSizeChanged?.Invoke(this, size);
        }
    }
}

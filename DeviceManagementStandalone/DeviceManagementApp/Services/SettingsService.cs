using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;

namespace DeviceManagementApp.Services
{
    public class SettingsService : ISettingsService
    {
        readonly DatabaseService _dbService;
        readonly ILogger<SettingsService> _logger;
        public event EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;

        const string UpsertSql = @"INSERT INTO Settings (Key, Value) VALUES (@Key, @Value) ON CONFLICT(Key) DO UPDATE SET Value = @Value";

        public SettingsService(DatabaseService dbService, ILogger<SettingsService>? logger = null)
        {
            _dbService = dbService;
            _logger = logger ?? NullLogger<SettingsService>.Instance;
        }

        public async Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            var p = new[] { new SqliteParameter("@Key", key), new SqliteParameter("@Value", value) };
            using var conn = _dbService.CreateConnection();
            await SqliteHelper.ExecuteNonQueryAsync(conn, UpsertSql, p, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string?> GetSettingAsync(string? key, CancellationToken cancellationToken = default)
        {
            if (key is null) return null;
            const string sql = "SELECT Value FROM Settings WHERE Key = @Key";
            using var conn = _dbService.CreateConnection();
            var p = new[] { new SqliteParameter("@Key", key) };
            var result = await SqliteHelper.ExecuteScalarAsync(conn, sql, p, cancellationToken).ConfigureAwait(false);
            return result?.ToString();
        }

        public async Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default)
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

        public async Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default)
        {
            foreach (var kv in settings)
                await SaveSettingAsync(kv.Key, kv.Value, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM Settings WHERE Key = @Key";
            using var conn = _dbService.CreateConnection();
            var p = new[] { new SqliteParameter("@Key", key) };
            await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken).ConfigureAwait(false);
        }

        const string ThemeKey = "Theme";
        const string PasswordIterationsKey = "PasswordIterations";
        const string AutoLogoutMinutesKey = "AutoLogoutMinutes";
        const string ItemLabelSingularKey = "ItemLabelSingular";
        const string ItemLabelPluralKey = "ItemLabelPlural";
        const string ItemDetailVisibilityKey = "ItemDetailVisibility";

        public Task<string?> GetThemeAsync(CancellationToken cancellationToken = default)
            => GetSettingAsync(ThemeKey, cancellationToken);

        public Task SaveThemeAsync(string theme, CancellationToken cancellationToken = default)
            => SaveSettingAsync(ThemeKey, theme, cancellationToken);

        public async Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default)
        {
            var value = await GetSettingAsync(PasswordIterationsKey, cancellationToken).ConfigureAwait(false);
            return int.TryParse(value, out var i) && i > 0 ? i : 100_000;
        }

        public Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default)
            => SaveSettingAsync(PasswordIterationsKey, iterations.ToString(), cancellationToken);

        public async Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default)
        {
            var value = await GetSettingAsync(AutoLogoutMinutesKey, cancellationToken).ConfigureAwait(false);
            return int.TryParse(value, out var i) && i >= 0 ? i : 1;
        }

        public Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default)
            => SaveSettingAsync(AutoLogoutMinutesKey, minutes.ToString(), cancellationToken);

        public async Task<string> GetItemLabelSingularAsync(CancellationToken cancellationToken = default)
        {
            var value = await GetSettingAsync(ItemLabelSingularKey, cancellationToken).ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(value) ? "Device" : value;
        }

        public Task SaveItemLabelSingularAsync(string label, CancellationToken cancellationToken = default)
            => SaveSettingAsync(ItemLabelSingularKey, label, cancellationToken);

        public async Task<string> GetItemLabelPluralAsync(CancellationToken cancellationToken = default)
        {
            var value = await GetSettingAsync(ItemLabelPluralKey, cancellationToken).ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(value) ? "Devices" : value;
        }

        public Task SaveItemLabelPluralAsync(string label, CancellationToken cancellationToken = default)
            => SaveSettingAsync(ItemLabelPluralKey, label, cancellationToken);

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
                await SaveItemDetailVisibilityAsync(visibility, cancellationToken).ConfigureAwait(false);
            }
            return visibility;
        }

        public async Task SaveItemDetailVisibilityAsync(IDictionary<ItemDetailField, bool> visibility, CancellationToken cancellationToken = default)
        {
            var dict = visibility.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);
            var json = JsonSerializer.Serialize(dict);
            await SaveSettingAsync(ItemDetailVisibilityKey, json, cancellationToken).ConfigureAwait(false);
            ItemDetailVisibilityChanged?.Invoke(this, new Dictionary<ItemDetailField, bool>(visibility));
        }
    }
}

using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class SettingsServiceWriteGuardContractTests
    {
        [Fact]
        public void SaveSettingNormalizesKeysAndGuardsUpsertRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Settings", "SettingsService.cs");
            var method = ExtractMethod(
                source,
                "public async Task SaveSettingAsync",
                "        /// <summary>\n        /// Retrieves a setting value from the database.");

            Assert.Contains("var normalizedKey = NormalizeRequiredSettingKey(key, nameof(key));", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Key\", normalizedKey)", method, StringComparison.Ordinal);
            Assert.Contains("var affectedRows = await SqliteHelper.ExecuteNonQueryAsync", method, StringComparison.Ordinal);
            Assert.Contains("EnsureSettingsWriteSucceeded(affectedRows, normalizedKey);", method, StringComparison.Ordinal);
            Assert.Contains("Failed to save setting '{normalizedKey}'.", method, StringComparison.Ordinal);
            Assert.DoesNotContain("new SqliteParameter(\"@Key\", key)", method, StringComparison.Ordinal);
            Assert.DoesNotContain("await SqliteHelper.ExecuteNonQueryAsync(conn, UpsertSql, p, cancellationToken).ConfigureAwait(false);", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("var normalizedKey = NormalizeRequiredSettingKey(key, nameof(key));", StringComparison.Ordinal) < method.IndexOf("new SqliteParameter(\"@Key\", normalizedKey)", StringComparison.Ordinal),
                "Single setting writes should bind the normalized key.");
            Assert.True(
                method.IndexOf("var affectedRows = await SqliteHelper.ExecuteNonQueryAsync", StringComparison.Ordinal) < method.IndexOf("EnsureSettingsWriteSucceeded(affectedRows, normalizedKey);", StringComparison.Ordinal),
                "Single setting writes should capture affected rows before checking the upsert result.");
        }

        [Fact]
        public void GetSettingNormalizesKeysAndRejectsBlankKeysBeforeConnectionWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Settings", "SettingsService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<string?> GetSettingAsync",
                "public async Task<Dictionary<string, string>> GetAllSettingsAsync");

            Assert.Contains("var normalizedKey = NormalizeOptionalSettingKey(key);", method, StringComparison.Ordinal);
            Assert.Contains("if (normalizedKey is null)", method, StringComparison.Ordinal);
            Assert.Contains("return null;", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Key\", normalizedKey)", method, StringComparison.Ordinal);
            Assert.Contains("Retrieving setting {Key} canceled or timed out", method, StringComparison.Ordinal);
            Assert.Contains("Failed to retrieve setting '{normalizedKey}'.", method, StringComparison.Ordinal);
            Assert.DoesNotContain("new SqliteParameter(\"@Key\", key)", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("if (normalizedKey is null)", StringComparison.Ordinal) < method.IndexOf("using var conn = _dbService.CreateConnection();", StringComparison.Ordinal),
                "Blank read keys should return without opening a database connection.");
            Assert.True(
                method.IndexOf("var normalizedKey = NormalizeOptionalSettingKey(key);", StringComparison.Ordinal) < method.IndexOf("new SqliteParameter(\"@Key\", normalizedKey)", StringComparison.Ordinal),
                "Setting reads should bind the normalized key.");
        }

        [Fact]
        public void GetAllSettingsNormalizesReturnedKeysAndSkipsBlankStoredKeys()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Settings", "SettingsService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<Dictionary<string, string>> GetAllSettingsAsync",
                "        /// <summary>\n        /// Updates or inserts multiple settings within a single transaction.");

            Assert.Contains("var key = NormalizeOptionalSettingKey(rdr[\"Key\"]?.ToString());", method, StringComparison.Ordinal);
            Assert.Contains("if (key != null && value != null)", method, StringComparison.Ordinal);
            Assert.Contains("dict[key] = value;", method, StringComparison.Ordinal);
            Assert.DoesNotContain("dict[rdr[\"Key\"]", method, StringComparison.Ordinal);
        }

        [Fact]
        public void BatchSettingsNormalizeAndValidateKeysBeforeOpeningTransaction()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Settings", "SettingsService.cs");
            var method = ExtractMethod(
                source,
                "public async Task UpdateSettingsAsync",
                "public async Task DeleteSettingAsync");

            Assert.Contains("var normalizedSettings = new List<KeyValuePair<string, string>>(settings.Count);", method, StringComparison.Ordinal);
            Assert.Contains("var normalizedKeys = new HashSet<string>(StringComparer.Ordinal);", method, StringComparison.Ordinal);
            Assert.Contains("var normalizedKey = NormalizeRequiredSettingKey(kv.Key, nameof(settings));", method, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentException(\"Duplicate setting keys are not allowed.\", nameof(settings));", method, StringComparison.Ordinal);
            Assert.Contains("normalizedSettings.Add(new KeyValuePair<string, string>(normalizedKey, kv.Value));", method, StringComparison.Ordinal);
            Assert.Contains("foreach (var kv in normalizedSettings)", method, StringComparison.Ordinal);
            Assert.Contains("EnsureSettingsWriteSucceeded(affectedRows, kv.Key);", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("var normalizedSettings = new List<KeyValuePair<string, string>>(settings.Count);", StringComparison.Ordinal) < method.IndexOf("using var conn = _dbService.CreateConnection();", StringComparison.Ordinal),
                "Batch settings should normalize and validate keys before opening a database connection.");
            Assert.True(
                method.IndexOf("throw new ArgumentException(\"Duplicate setting keys are not allowed.\", nameof(settings));", StringComparison.Ordinal) < method.IndexOf("using var conn = _dbService.CreateConnection();", StringComparison.Ordinal),
                "Duplicate normalized keys should fail before transaction work begins.");
            Assert.True(
                method.IndexOf("var affectedRows = await SqliteHelper.ExecuteNonQueryAsync", StringComparison.Ordinal) < method.IndexOf("EnsureSettingsWriteSucceeded(affectedRows, kv.Key);", StringComparison.Ordinal),
                "Batch setting writes should check each upsert result before committing.");
            Assert.True(
                method.IndexOf("EnsureSettingsWriteSucceeded(affectedRows, kv.Key);", StringComparison.Ordinal) < method.IndexOf("tx.Commit();", StringComparison.Ordinal),
                "Batch setting writes should fail the transaction before commit when an upsert writes no rows.");
        }

        [Fact]
        public void DeleteSettingRejectsBlankKeysBeforeConnectionWorkAndBindsNormalizedKeys()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Settings", "SettingsService.cs");
            var method = ExtractMethod(
                source,
                "public async Task DeleteSettingAsync",
                "static string? NormalizeOptionalSettingKey");

            Assert.Contains("var normalizedKey = NormalizeRequiredSettingKey(key, nameof(key));", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Key\", normalizedKey)", method, StringComparison.Ordinal);
            Assert.Contains("No setting found for key {Key}", method, StringComparison.Ordinal);
            Assert.DoesNotContain("new SqliteParameter(\"@Key\", key)", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("var normalizedKey = NormalizeRequiredSettingKey(key, nameof(key));", StringComparison.Ordinal) < method.IndexOf("using var conn = _dbService.CreateConnection();", StringComparison.Ordinal),
                "Invalid delete keys should fail before opening a database connection.");
            Assert.True(
                method.IndexOf("var normalizedKey = NormalizeRequiredSettingKey(key, nameof(key));", StringComparison.Ordinal) < method.IndexOf("new SqliteParameter(\"@Key\", normalizedKey)", StringComparison.Ordinal),
                "Setting delete should bind the normalized key.");
        }

        [Fact]
        public void SettingKeyNormalizationHelpersPreserveReadAndWriteContracts()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Settings", "SettingsService.cs");

            Assert.Contains("static string? NormalizeOptionalSettingKey(string? key)", source, StringComparison.Ordinal);
            Assert.Contains("string.IsNullOrWhiteSpace(key) ? null : key.Trim();", source, StringComparison.Ordinal);
            Assert.Contains("static string NormalizeRequiredSettingKey(string key, string parameterName)", source, StringComparison.Ordinal);
            Assert.Contains("var normalizedKey = NormalizeOptionalSettingKey(key);", source, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentException(\"Key cannot be null or empty.\", parameterName);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemDisplaySettingsNormalizeLabelsAndCanonicalizeVisibilitySaves()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Settings", "SettingsService.cs");
            var singular = ExtractMethod(
                source,
                "public async Task SaveItemLabelSingularAsync",
                "public async Task<string> GetItemLabelPluralAsync");
            var plural = ExtractMethod(
                source,
                "public async Task SaveItemLabelPluralAsync",
                "public async Task<IDictionary<ItemDetailField, bool>> GetItemDetailVisibilityAsync");
            var visibility = ExtractMethod(
                source,
                "public async Task SaveItemDetailVisibilityAsync",
                "public async Task<double> GetItemCardSizeAsync");

            Assert.Contains("return NormalizeDisplayLabel(value, \"Item\");", source, StringComparison.Ordinal);
            Assert.Contains("return NormalizeDisplayLabel(value, \"Items\");", source, StringComparison.Ordinal);
            Assert.Contains("var normalizedLabel = NormalizeDisplayLabel(label, \"Item\");", singular, StringComparison.Ordinal);
            Assert.Contains("await SaveSettingAsync(ItemLabelSingularKey, normalizedLabel, cancellationToken).ConfigureAwait(false);", singular, StringComparison.Ordinal);
            Assert.Contains("var normalizedLabel = NormalizeDisplayLabel(label, \"Items\");", plural, StringComparison.Ordinal);
            Assert.Contains("await SaveSettingAsync(ItemLabelPluralKey, normalizedLabel, cancellationToken).ConfigureAwait(false);", plural, StringComparison.Ordinal);
            Assert.Contains("static string NormalizeDisplayLabel(string? label, string defaultLabel)", source, StringComparison.Ordinal);
            Assert.Contains("string.IsNullOrWhiteSpace(label) ? defaultLabel : label.Trim();", source, StringComparison.Ordinal);

            Assert.Contains("if (visibility is null)", visibility, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentNullException(nameof(visibility));", visibility, StringComparison.Ordinal);
            Assert.Contains("var normalizedVisibility = Enum.GetValues<ItemDetailField>()", visibility, StringComparison.Ordinal);
            Assert.Contains("visibility.TryGetValue(f, out var visible) ? visible : true", visibility, StringComparison.Ordinal);
            Assert.Contains("var dict = normalizedVisibility.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);", visibility, StringComparison.Ordinal);
            Assert.Contains("ItemDetailVisibilityChanged?.Invoke(this, normalizedVisibility);", visibility, StringComparison.Ordinal);
            Assert.DoesNotContain("ItemDetailVisibilityChanged?.Invoke(this, new Dictionary<ItemDetailField, bool>(visibility));", visibility, StringComparison.Ordinal);
        }

        [Fact]
        public void SettingsWriteGuardThrowsWhenNoRowsAreWritten()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Settings", "SettingsService.cs");

            Assert.Contains("static void EnsureSettingsWriteSucceeded(int affectedRows, string key)", source, StringComparison.Ordinal);
            Assert.Contains("if (affectedRows < 1)", source, StringComparison.Ordinal);
            Assert.Contains("throw new InvalidOperationException($\"Failed to save setting '{key}'.\");", source, StringComparison.Ordinal);
        }

        private static string ExtractMethod(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find method start marker: {startMarker}");

            var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
            Assert.True(end > start, $"Could not find method end marker: {endMarker}");

            return source[start..end];
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return NormalizeLineEndings(File.ReadAllText(candidate));

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }

        private static string NormalizeLineEndings(string text) =>
            text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
    }
}

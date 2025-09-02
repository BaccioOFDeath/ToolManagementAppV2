using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Services.Core;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Services.Devices
{
    public class ScannerRuleService : IScannerRuleService, IDisposable
    {
        readonly DatabaseService _db;
        readonly ILogger<ScannerRuleService> _logger;
        readonly Dictionary<int, FileSystemWatcher> _watchers = new();
        bool _disposed;

        public ScannerRuleService(DatabaseService db, ILogger<ScannerRuleService>? logger = null)
        {
            _db = db;
            _logger = logger ?? NullLogger<ScannerRuleService>.Instance;
        }

        public async Task<int> AddRuleAsync(ScannerFileRule rule, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO ScannerFileRules (DeviceId, SourcePath, DestinationPath, Pattern) VALUES ($device,$src,$dest,$pat); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$device", rule.DeviceId);
            cmd.Parameters.AddWithValue("$src", rule.SourcePath);
            cmd.Parameters.AddWithValue("$dest", rule.DestinationPath);
            cmd.Parameters.AddWithValue("$pat", rule.Pattern);
            var idObj = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            rule.Id = (int)(long)idObj!;
            StartWatcher(rule);
            return rule.Id;
        }

        public async Task<IEnumerable<ScannerFileRule>> GetRulesAsync(string deviceId, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT RuleId, DeviceId, SourcePath, DestinationPath, Pattern FROM ScannerFileRules WHERE DeviceId=$device";
            cmd.Parameters.AddWithValue("$device", deviceId);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var rules = new List<ScannerFileRule>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                rules.Add(new ScannerFileRule
                {
                    Id = reader.GetInt32(0),
                    DeviceId = reader.GetString(1),
                    SourcePath = reader.GetString(2),
                    DestinationPath = reader.GetString(3),
                    Pattern = reader.GetString(4)
                });
            }
            return rules;
        }

        public async Task DeleteRuleAsync(int ruleId, CancellationToken cancellationToken = default)
        {
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM ScannerFileRules WHERE RuleId=$id";
            cmd.Parameters.AddWithValue("$id", ruleId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            if (_watchers.TryGetValue(ruleId, out var watcher))
            {
                watcher.Dispose();
                _watchers.Remove(ruleId);
            }
        }

        void StartWatcher(ScannerFileRule rule)
        {
            try
            {
                if (!Directory.Exists(rule.SourcePath)) Directory.CreateDirectory(rule.SourcePath);
                if (!Directory.Exists(rule.DestinationPath)) Directory.CreateDirectory(rule.DestinationPath);
                var watcher = new FileSystemWatcher(rule.SourcePath, rule.Pattern)
                {
                    EnableRaisingEvents = true
                };
                watcher.Created += (s, e) => CopyFileWithRetry(e.FullPath, rule.DestinationPath);
                _watchers[rule.Id] = watcher;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start watcher for rule {RuleId}", rule.Id);
            }
        }

        void CopyFileWithRetry(string source, string destDir)
        {
            Task.Run(() =>
            {
                var dest = Path.Combine(destDir, Path.GetFileName(source));
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        File.Copy(source, dest, true);
                        return;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to copy file {File}", source);
                        return;
                    }
                }
            });
        }

        public void Dispose()
        {
            if (_disposed) return;
            foreach (var w in _watchers.Values)
                w.Dispose();
            _watchers.Clear();
            _disposed = true;
        }
    }
}

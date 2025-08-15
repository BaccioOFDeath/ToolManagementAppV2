using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Interfaces;
using Xunit;

namespace ToolManagementAppV2.Tests.Services
{
    public class SettingsServiceTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void SaveSetting_ThrowsOnNullOrEmptyKey(string key)
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ISettingsService service = new SettingsService(dbService);

                Assert.Throws<ArgumentException>(() => service.SaveSetting(key!, "Value1"));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void UpdateSettings_ThrowsOnEmptyKey()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ISettingsService service = new SettingsService(dbService);

                var settings = new Dictionary<string, string> { [""] = "Value1" };
                Assert.Throws<ArgumentException>(() => service.UpdateSettings(settings));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void UpdateSettings_ThrowsOnNullSettings()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ISettingsService service = new SettingsService(dbService);

                Assert.Throws<ArgumentNullException>(() => service.UpdateSettings(null!));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void UpdateSettings_ThrowsOnFailure()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ISettingsService service = new SettingsService(dbService);

                using (var conn = dbService.CreateConnection())
                {
                    using var cmd = new SQLiteCommand("DROP TABLE Settings", conn);
                    cmd.ExecuteNonQuery();
                }

                var settings = new Dictionary<string, string> { ["Key1"] = "Value1" };
                Assert.Throws<InvalidOperationException>(() => service.UpdateSettings(settings));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void SaveSetting_ThrowsOnFailure()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ISettingsService service = new SettingsService(dbService);

                using (var conn = dbService.CreateConnection())
                {
                    using var cmd = new SQLiteCommand("DROP TABLE Settings", conn);
                    cmd.ExecuteNonQuery();
                }

                Assert.Throws<InvalidOperationException>(() => service.SaveSetting("Key1", "Value1"));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void GetSetting_ThrowsOnFailure()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ISettingsService service = new SettingsService(dbService);

                using (var conn = dbService.CreateConnection())
                {
                    using var cmd = new SQLiteCommand("DROP TABLE Settings", conn);
                    cmd.ExecuteNonQuery();
                }

                Assert.Throws<InvalidOperationException>(() => service.GetSetting("Key1"));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void GetSetting_ReturnsNull_WhenKeyMissing()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ISettingsService service = new SettingsService(dbService);

                var result = service.GetSetting("NonExistentKey");
                Assert.Null(result);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void DeleteSetting_ThrowsOnFailure()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ISettingsService service = new SettingsService(dbService);

                using (var conn = dbService.CreateConnection())
                {
                    using var cmd = new SQLiteCommand("DROP TABLE Settings", conn);
                    cmd.ExecuteNonQuery();
                }

                Assert.Throws<InvalidOperationException>(() => service.DeleteSetting("Key1"));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void DeleteSetting_NonExistingKey_LogsWarning()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var logs = new List<LogEntry>();
                using var factory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
                var logger = factory.CreateLogger<SettingsService>();
                ISettingsService service = new SettingsService(dbService, logger);

                service.DeleteSetting("MissingKey");

                Assert.Single(logs);
                Assert.Equal(LogLevel.Warning, logs[0].Level);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void SaveScannerIpAddresses_SkipsInvalidAndLogsWarning()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var logs = new List<LogEntry>();
                using var factory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
                var logger = factory.CreateLogger<SettingsService>();
                var service = new SettingsService(dbService, logger);

                var invalid = service.SaveScannerIpAddresses(new[] { "192.168.1.1", "bad", "999.999.999.999" }).ToList();

                Assert.Equal(new[] { "bad", "999.999.999.999" }, invalid);

                var saved = service.GetScannerIpAddresses().ToList();
                Assert.Single(saved);
                Assert.Equal("192.168.1.1", saved[0]);

                Assert.Single(logs);
                Assert.Equal(LogLevel.Warning, logs[0].Level);
                Assert.Contains("bad", logs[0].Message);
                Assert.Contains("999.999.999.999", logs[0].Message);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void SaveScannerIpAddresses_AllValid_NoWarning()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var logs = new List<LogEntry>();
                using var factory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
                var logger = factory.CreateLogger<SettingsService>();
                var service = new SettingsService(dbService, logger);

                var invalid = service.SaveScannerIpAddresses(new[] { "127.0.0.1", "10.0.0.2" });
                Assert.Empty(invalid);

                var saved = service.GetScannerIpAddresses().ToList();
                Assert.Equal(2, saved.Count);
                Assert.Contains("127.0.0.1", saved);
                Assert.Contains("10.0.0.2", saved);

                Assert.Empty(logs);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void GetPasswordIterations_ReturnsDefault()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ISettingsService service = new SettingsService(dbService);

                var iterations = service.GetPasswordIterations();
                Assert.Equal(100_000, iterations);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void SaveAndRetrievePasswordIterations()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ISettingsService service = new SettingsService(dbService);

                service.SavePasswordIterations(50_000);
                Assert.Equal(50_000, service.GetPasswordIterations());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}

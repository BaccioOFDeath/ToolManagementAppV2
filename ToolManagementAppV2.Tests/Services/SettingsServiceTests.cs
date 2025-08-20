using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

                Assert.Throws<ArgumentException>(() => service.SaveSettingAsync(key!, "Value1").GetAwaiter().GetResult());
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
                Assert.Throws<ArgumentException>(() => service.UpdateSettingsAsync(settings).GetAwaiter().GetResult());
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

                Assert.Throws<ArgumentNullException>(() => service.UpdateSettingsAsync(null!).GetAwaiter().GetResult());
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
                Assert.Throws<InvalidOperationException>(() => service.UpdateSettingsAsync(settings).GetAwaiter().GetResult());
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

                Assert.Throws<InvalidOperationException>(() => service.SaveSettingAsync("Key1", "Value1").GetAwaiter().GetResult());
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

                Assert.Throws<InvalidOperationException>(() => service.GetSettingAsync("Key1").GetAwaiter().GetResult());
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

                var result = service.GetSettingAsync("NonExistentKey").GetAwaiter().GetResult();
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

                Assert.Throws<InvalidOperationException>(() => service.DeleteSettingAsync("Key1").GetAwaiter().GetResult());
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

                service.DeleteSettingAsync("MissingKey").GetAwaiter().GetResult();

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

                var invalid = service.SaveScannerIpAddressesAsync(new[] { "192.168.1.1", "bad", "999.999.999.999" }).GetAwaiter().GetResult().ToList();

                Assert.Equal(new[] { "bad", "999.999.999.999" }, invalid);

                var saved = service.GetScannerIpAddressesAsync().GetAwaiter().GetResult().ToList();
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

                var invalid = service.SaveScannerIpAddressesAsync(new[] { "127.0.0.1", "10.0.0.2" }).GetAwaiter().GetResult();
                Assert.Empty(invalid);

                var saved = service.GetScannerIpAddressesAsync().GetAwaiter().GetResult().ToList();
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

        [Theory]
        [InlineData(null)]
        [InlineData(new string[0])]
        public void SaveScannerIpAddresses_NullOrEmpty_DeletesSetting(string[]? input)
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var service = new SettingsService(dbService);

                service.SaveScannerIpAddressesAsync(new[] { "192.168.0.1" }).GetAwaiter().GetResult();
                Assert.Single(service.GetScannerIpAddressesAsync().GetAwaiter().GetResult());

                var invalid = service.SaveScannerIpAddressesAsync(input).GetAwaiter().GetResult().ToList();
                Assert.Empty(invalid);
                Assert.Empty(service.GetScannerIpAddressesAsync().GetAwaiter().GetResult());
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

                var iterations = service.GetPasswordIterationsAsync().GetAwaiter().GetResult();
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

                service.SavePasswordIterationsAsync(50_000).GetAwaiter().GetResult();
                Assert.Equal(50_000, service.GetPasswordIterationsAsync().GetAwaiter().GetResult());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void SavePasswordIterations_Invalid_Throws(int iterations)
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ISettingsService service = new SettingsService(dbService);

                Assert.Throws<ArgumentOutOfRangeException>(() => service.SavePasswordIterationsAsync(iterations).GetAwaiter().GetResult());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void GetPasswordIterations_InvalidStoredValue_ReturnsDefault()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var service = new SettingsService(dbService);
                service.SaveSettingAsync("PasswordIterations", "-5").GetAwaiter().GetResult();
                Assert.Equal(100_000, service.GetPasswordIterationsAsync().GetAwaiter().GetResult());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SaveAndRetrieveSettingAsync()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var service = new SettingsService(dbService);

                await service.SaveSettingAsync("Key1", "Value1");
                var value = await service.GetSettingAsync("Key1");
                Assert.Equal("Value1", value);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task GetAllSettingsAsync_ReturnsAllEntries()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var service = new SettingsService(dbService);

                await service.SaveSettingAsync("A", "1");
                await service.SaveSettingAsync("B", "2");

                var settings = await service.GetAllSettingsAsync();
                Assert.Equal(2, settings.Count);
                Assert.Equal("1", settings["A"]);
                Assert.Equal("2", settings["B"]);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task DeleteSettingAsync_RemovesEntry()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var service = new SettingsService(dbService);

                await service.SaveSettingAsync("Key1", "Value1");
                await service.DeleteSettingAsync("Key1");

                var value = await service.GetSettingAsync("Key1");
                Assert.Null(value);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ItemLabels_SaveAndRetrieve()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var service = new SettingsService(dbService);
                Assert.Equal("Item", service.GetItemLabelSingularAsync().GetAwaiter().GetResult());
                Assert.Equal("Items", service.GetItemLabelPluralAsync().GetAwaiter().GetResult());
                service.SaveItemLabelSingularAsync("Widget").GetAwaiter().GetResult();
                service.SaveItemLabelPluralAsync("Widgets").GetAwaiter().GetResult();
                Assert.Equal("Widget", service.GetItemLabelSingularAsync().GetAwaiter().GetResult());
                Assert.Equal("Widgets", service.GetItemLabelPluralAsync().GetAwaiter().GetResult());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}

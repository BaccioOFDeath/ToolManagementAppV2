using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Users;
using Xunit;

namespace InventoryManagementApp.Tests.Services
{
    public class ActivityLogServiceTests
    {
        [Fact]
        public async Task LogAction_ReturnsFailure_WhenTableMissing()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using var db = new DatabaseService(dbPath);
                using (var conn = db.CreateConnection())
                using (var cmd = new SQLiteCommand("DROP TABLE ActivityLogs", conn))
                    cmd.ExecuteNonQuery();

                var service = new ActivityLogService(db);
                var result = await service.LogActionAsync(1, "user", "action");
                Assert.False(result.Success);
                Assert.NotNull(result.ErrorMessage);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task GetRecentLogs_ReturnsFailure_WhenTableMissing()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using var db = new DatabaseService(dbPath);
                using (var conn = db.CreateConnection())
                using (var cmd = new SQLiteCommand("DROP TABLE ActivityLogs", conn))
                    cmd.ExecuteNonQuery();

                var service = new ActivityLogService(db);
                var result = await service.GetRecentLogsAsync();
                Assert.False(result.Success);
                Assert.NotNull(result.ErrorMessage);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task LogAction_And_GetRecentLogs_WorkTogether()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using var db = new DatabaseService(dbPath);
                var service = new ActivityLogService(db);
                var logResult = await service.LogActionAsync(1, "user", "action");
                Assert.True(logResult.Success);

                var recent = await service.GetRecentLogsAsync();
                Assert.True(recent.Success);
                Assert.NotNull(recent.Value);
                Assert.Single(recent.Value);
                Assert.Equal("action", recent.Value[0].Action);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task GetRecentLogs_InvalidTimestamp_DoesNotThrow()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using var db = new DatabaseService(dbPath);
                using (var conn = db.CreateConnection())
                using (var cmd = new SQLiteCommand("INSERT INTO ActivityLogs(UserID, UserName, Action, Timestamp) VALUES (1, 'user', 'action', 'not-a-date')", conn))
                    cmd.ExecuteNonQuery();

                var service = new ActivityLogService(db);
                var result = await service.GetRecentLogsAsync();

                Assert.True(result.Success);
                Assert.NotNull(result.Value);
                Assert.Equal(DateTime.MinValue, result.Value[0].Timestamp);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task GetRecentLogs_ConvertsTimestampToLocalTime()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using var db = new DatabaseService(dbPath);
                using (var conn = db.CreateConnection())
                using (var cmd = new SQLiteCommand("INSERT INTO ActivityLogs(UserID, UserName, Action, Timestamp) VALUES (1, 'user', 'action', '2024-01-01 00:00:00')", conn))
                    cmd.ExecuteNonQuery();

                var service = new ActivityLogService(db);
                var result = await service.GetRecentLogsAsync();

                Assert.True(result.Success);
                var log = Assert.Single(result.Value);
                Assert.Equal(DateTimeKind.Local, log.Timestamp.Kind);
                var expected = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
                Assert.Equal(expected, log.Timestamp);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}

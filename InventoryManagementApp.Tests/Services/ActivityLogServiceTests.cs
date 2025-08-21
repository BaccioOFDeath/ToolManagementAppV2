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
    }
}

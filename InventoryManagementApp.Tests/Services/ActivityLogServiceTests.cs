using System;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Users;
using Xunit;

namespace InventoryManagementApp.Tests.Services
{
    public class ActivityLogServiceTests
    {
        [Fact]
        public async Task GetRecentLogsAsync_ParsesDdMmYyyyTimestamp()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using var db = new DatabaseService(dbPath);
                var service = new ActivityLogService(db);

                using (var conn = db.CreateConnection())
                using (var cmd = new SQLiteCommand(@"INSERT INTO ActivityLogs (UserID, UserName, Action, Timestamp)
                                                     VALUES (1, 'user', 'action', @ts);", conn))
                {
                    cmd.Parameters.AddWithValue("@ts", "25/12/2023 13:45");
                    cmd.ExecuteNonQuery();
                }

                var originalCulture = CultureInfo.CurrentCulture;
                CultureInfo.CurrentCulture = new CultureInfo("en-GB");
                try
                {
                    var result = await service.GetRecentLogsAsync(1);
                    var log = result.Data[0];
                    var expected = new DateTime(2023, 12, 25, 13, 45, 0, DateTimeKind.Local);
                    Assert.Equal(expected, log.Timestamp);
                }
                finally
                {
                    CultureInfo.CurrentCulture = originalCulture;
                }
            }
            finally
            {
                File.Delete(dbPath);
            }
        }
    }
}

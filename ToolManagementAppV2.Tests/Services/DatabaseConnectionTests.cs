using System;
using System.IO;
using System.Data.SQLite;
using ToolManagementAppV2.Services.Core;
using Xunit;
using System.Threading.Tasks;

namespace ToolManagementAppV2.Tests.Services
{
    public class DatabaseConnectionTests
    {
        [Fact]
        public async Task MultipleConnections_NoLockingErrors()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                using var conn1 = dbService.CreateConnection();
                using var conn2 = dbService.CreateConnection();

                using var tx = conn1.BeginTransaction();
                using (var cmd = new SQLiteCommand("INSERT INTO Settings(Key,Value) VALUES('Test','1')", conn1, tx))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Settings", conn2))
                {
                    var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    Assert.Equal(1, count);
                }
                tx.Commit();
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void WalModeAndBusyTimeoutConfigured()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                using var conn = dbService.CreateConnection();
                using (var cmd = new SQLiteCommand("PRAGMA journal_mode;", conn))
                {
                    var mode = Convert.ToString(cmd.ExecuteScalar());
                    Assert.Equal("wal", mode?.ToLowerInvariant());
                }
                using (var cmd = new SQLiteCommand("PRAGMA busy_timeout;", conn))
                {
                    var timeout = Convert.ToInt32(cmd.ExecuteScalar());
                    Assert.Equal(5000, timeout);
                }
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}


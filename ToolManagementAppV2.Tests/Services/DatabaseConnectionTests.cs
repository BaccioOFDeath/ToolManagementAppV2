using System;
using System.IO;
using System.Data.SQLite;
using ToolManagementAppV2.Services.Core;
using Xunit;

namespace ToolManagementAppV2.Tests.Services
{
    public class DatabaseConnectionTests
    {
        [Fact]
        public void MultipleConnections_NoLockingErrors()
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
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Settings", conn2))
                {
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
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
    }
}


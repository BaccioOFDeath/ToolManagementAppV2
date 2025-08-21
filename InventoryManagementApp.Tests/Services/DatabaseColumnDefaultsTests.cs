using System;
using System.Data.SQLite;
using System.IO;
using InventoryManagementApp.Services.Core;
using Xunit;

namespace InventoryManagementApp.Tests.Services
{
    public class DatabaseColumnDefaultsTests
    {
        [Fact]
        public void NumericColumnsBackfilledWithZero()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();
                    using var cmd = new SQLiteCommand(@"CREATE TABLE Items (
                            ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
                            ItemNumber TEXT NOT NULL
                        );
                        INSERT INTO Items (ItemNumber) VALUES ('T1');", conn);
                    cmd.ExecuteNonQuery();
                }

                var service = new DatabaseService(dbPath);

                using var conn2 = service.CreateConnection();
                using (var cmdCheck = new SQLiteCommand("SELECT IsPowered, IsCheckedOut FROM Items", conn2))
                using (var reader = cmdCheck.ExecuteReader())
                {
                    Assert.True(reader.Read());
                    Assert.Equal(0, reader.GetInt32(0));
                    Assert.Equal(0, reader.GetInt32(1));
                }

                using var cmdNulls = new SQLiteCommand("SELECT COUNT(*) FROM Items WHERE IsPowered IS NULL OR IsCheckedOut IS NULL", conn2);
                var nullCount = Convert.ToInt32(cmdNulls.ExecuteScalar());
                Assert.Equal(0, nullCount);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

    }
}

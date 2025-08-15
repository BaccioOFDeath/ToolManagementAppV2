using System;
using System.Data.SQLite;
using System.IO;
using ToolManagementAppV2.Services.Core;
using Xunit;

namespace ToolManagementAppV2.Tests.Services
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
                    using var cmd = new SQLiteCommand(@"CREATE TABLE Tools (
                            ToolID INTEGER PRIMARY KEY AUTOINCREMENT,
                            ToolNumber TEXT NOT NULL
                        );
                        INSERT INTO Tools (ToolNumber) VALUES ('T1');", conn);
                    cmd.ExecuteNonQuery();
                }

                var service = new DatabaseService(dbPath);

                using var conn2 = service.CreateConnection();
                using (var cmdCheck = new SQLiteCommand("SELECT IsPowerTool, IsCheckedOut FROM Tools", conn2))
                using (var reader = cmdCheck.ExecuteReader())
                {
                    Assert.True(reader.Read());
                    Assert.Equal(0, reader.GetInt32(0));
                    Assert.Equal(0, reader.GetInt32(1));
                }

                using var cmdNulls = new SQLiteCommand("SELECT COUNT(*) FROM Tools WHERE IsPowerTool IS NULL OR IsCheckedOut IS NULL", conn2);
                var nullCount = Convert.ToInt32(cmdNulls.ExecuteScalar());
                Assert.Equal(0, nullCount);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void UserColumnsBackfilledWithDefaults()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();
                    using var cmd = new SQLiteCommand(@"CREATE TABLE Users (
                            UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                            UserName TEXT NOT NULL
                        );
                        INSERT INTO Users (UserName) VALUES ('user1');", conn);
                    cmd.ExecuteNonQuery();
                }

                var service = new DatabaseService(dbPath);

                using var conn2 = service.CreateConnection();
                using (var cmdCheck = new SQLiteCommand("SELECT FailedAttempts, LockoutUntil, PasswordExpired FROM Users", conn2))
                using (var reader = cmdCheck.ExecuteReader())
                {
                    Assert.True(reader.Read());
                    Assert.Equal(0, reader.GetInt32(0));
                    Assert.True(reader.IsDBNull(1));
                    Assert.Equal(0, reader.GetInt32(2));
                }

                using var cmdNulls = new SQLiteCommand("SELECT COUNT(*) FROM Users WHERE FailedAttempts IS NULL OR PasswordExpired IS NULL", conn2);
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

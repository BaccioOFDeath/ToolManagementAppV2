using System;
using System.IO;
using InventoryManagementApp.Services.Core;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class DatabaseServiceJournalModeTests
    {
        [Fact]
        public void Constructor_WhenWalJournalDisabled_UsesDeleteJournalMode()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

            try
            {
                using var db = new DatabaseService(
                    dbPath,
                    NullLogger<DatabaseService>.Instance,
                    secureDatabaseFile: false,
                    useWalJournal: false);

                using var conn = new SqliteConnection(db.ConnectionString);
                conn.Open();
                using var cmd = new SqliteCommand("PRAGMA journal_mode;", conn);

                Assert.Equal("delete", Convert.ToString(cmd.ExecuteScalar())?.ToLowerInvariant());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}

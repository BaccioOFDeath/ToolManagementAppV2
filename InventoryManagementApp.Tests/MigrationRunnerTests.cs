using System;
using System.IO;
using Microsoft.Data.Sqlite;
using InventoryManagementApp.Services.Core;
using Xunit;

public class MigrationRunnerTests
{
    [Fact]
    public void Migrate_CreatesSchemaInfoAndAppliesMigrations()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        try
        {
            using var db = new DatabaseService(dbPath);
            var runner = new MigrationRunner(db);
            runner.Migrate();

            using var conn = db.CreateConnection();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT IFNULL(MAX(Version),0) FROM SchemaInfo;";
                var version = Convert.ToInt32(cmd.ExecuteScalar());
                Assert.Equal(1, version);
            }

            using (var pragma = new SqliteCommand("PRAGMA table_info(Items);", conn))
            using (var reader = pragma.ExecuteReader())
            {
                var found = false;
                while (reader.Read())
                {
                    if (reader["name"].ToString() == "Keywords")
                    {
                        found = true;
                        break;
                    }
                }
                Assert.True(found);
            }
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}

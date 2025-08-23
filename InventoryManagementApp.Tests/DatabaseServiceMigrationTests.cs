using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Data.Sqlite;
using InventoryManagementApp.Services.Core;
using System.Text.RegularExpressions;
using Xunit;

public class DatabaseServiceMigrationTests
{
    [Fact]
    public void Migrations_DoNotAttemptSelfRenames()
    {
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
        var path = Path.Combine(repoRoot, "InventoryManagementApp", "Services", "Core", "DatabaseService.cs");
        var code = File.ReadAllText(path);
        var pattern = @"RenameColumnIfExists\s*\(\s*[^,]+,\s*[^,]+,\s*""([^""]+)""\s*,\s*""\1""\s*\)";
        Assert.False(Regex.IsMatch(code, pattern));
    }

    [Fact]
    public void RenameColumnIfExists_DoesNothingWhenNamesMatch()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var create = new SqliteCommand("CREATE TABLE Items (IsPowered INTEGER);", conn);
        create.ExecuteNonQuery();

        var service = (DatabaseService)FormatterServices.GetUninitializedObject(typeof(DatabaseService));
        var method = typeof(DatabaseService).GetMethod("RenameColumnIfExists", BindingFlags.NonPublic | BindingFlags.Instance)!;
        method.Invoke(service, new object[] { conn, "Items", "IsPowered", "IsPowered" });

        using var pragma = new SqliteCommand("PRAGMA table_info(Items);", conn);
        using var reader = pragma.ExecuteReader();
        var count = 0;
        while (reader.Read())
        {
            if (reader["name"]?.ToString() == "IsPowered")
                count++;
        }

        Assert.Equal(1, count);
    }
}


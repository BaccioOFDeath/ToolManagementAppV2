using System;
using System.Collections.Generic;
using System.IO;
using InventoryManagementApp.Services.Core;
using Xunit;

public class DatabaseServiceIndexTests
{
    [Fact]
    public void Initialize_CreatesExpectedItemIndexes()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");
        try
        {
            using var db = new DatabaseService(dbPath);
            using var conn = db.CreateConnection();
            using var check = conn.CreateCommand();
            check.CommandText = "SELECT name FROM sqlite_master WHERE type='index' AND tbl_name='Items' ORDER BY name;";
            using var reader = check.ExecuteReader();
            var indexes = new List<string>();
            while (reader.Read())
                indexes.Add(reader.GetString(0));
            Assert.Contains("idx_Items_IsRentalItem", indexes);
            Assert.Contains("idx_Items_AvailableQuantity", indexes);
            Assert.Contains("idx_Items_Keywords", indexes);
            Assert.Contains("idx_Items_UpdatedAt", indexes);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}


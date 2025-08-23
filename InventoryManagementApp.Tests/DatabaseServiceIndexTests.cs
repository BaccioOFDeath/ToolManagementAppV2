using System;
using System.IO;
using InventoryManagementApp.Services.Core;
using Xunit;

public class DatabaseServiceIndexTests
{
    [Fact]
    public void Initialize_CreatesIsRentalItemIndex()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");
        try
        {
            using var db = new DatabaseService(dbPath);
            using var conn = db.CreateConnection();
            using var check = conn.CreateCommand();
            check.CommandText = "SELECT name FROM sqlite_master WHERE type='index' AND name='idx_Items_IsRentalItem';";
            var result = check.ExecuteScalar();
            Assert.NotNull(result);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}


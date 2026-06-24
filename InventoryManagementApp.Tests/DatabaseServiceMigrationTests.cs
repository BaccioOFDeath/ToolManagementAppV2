using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Data.Sqlite;
using InventoryManagementApp.Data;
using InventoryManagementApp.Models.Domain;
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

    [Fact]
    public async Task InitializeDatabase_UpgradesExistingTablesUsedBySavePaths()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"inventory-migration-{Guid.NewGuid():N}.db");
        try
        {
            using (var conn = new SqliteConnection($"Data Source={dbPath}"))
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
CREATE TABLE Tools (ToolID INTEGER PRIMARY KEY AUTOINCREMENT, ToolNumber TEXT NOT NULL, Description TEXT, Quantity INTEGER);
INSERT INTO Tools (ToolNumber, Description, Quantity) VALUES ('OLD-1', 'Legacy item', 2);
CREATE TABLE Users (UserID INTEGER PRIMARY KEY AUTOINCREMENT, UserName TEXT NOT NULL);
CREATE TABLE Customers (CustomerID INTEGER PRIMARY KEY AUTOINCREMENT);
CREATE TABLE Rentals (RentalID INTEGER PRIMARY KEY AUTOINCREMENT);
CREATE TABLE ActivityLogs (LogID INTEGER PRIMARY KEY AUTOINCREMENT);
CREATE TABLE Settings (Key TEXT PRIMARY KEY, Value TEXT);
CREATE TABLE MaintenanceRecords (MaintenanceID INTEGER PRIMARY KEY AUTOINCREMENT);
CREATE TABLE CalibrationRecords (CalibrationID INTEGER PRIMARY KEY AUTOINCREMENT);
CREATE TABLE Reservations (ReservationID INTEGER PRIMARY KEY AUTOINCREMENT);
CREATE TABLE Kits (KitID INTEGER PRIMARY KEY AUTOINCREMENT);
CREATE TABLE KitItems (KitItemID INTEGER PRIMARY KEY AUTOINCREMENT);";
                cmd.ExecuteNonQuery();
            }

            using var service = new DatabaseService(dbPath);
            var repository = new ItemRepository(new SqliteConnectionFactory(service.ConnectionString));
            var item = await repository.GetByIdAsync(1, CancellationToken.None);

            Assert.NotNull(item);
            Assert.Equal("Legacy item", item!.Name);
            Assert.Equal(2, item.QuantityOnHand);

            item.Name = "Migrated save check";
            item.Location = "Shelf A";
            item.QuantityOnHand = 4;
            item.RentedQuantity = 1;
            item.IsRentalItem = true;
            item.IsPowered = true;
            item.IsIncomplete = true;
            item.MissingComponentsNotes = "Case";
            item.IssuesNotes = "None";
            item.CheckoutCount = 2;

            await repository.UpdateAsync(item, CancellationToken.None);

            using var verify = service.CreateConnection();
            Assert.True(ColumnExists(verify, "Items", "CheckoutCount"));
            Assert.True(ColumnExists(verify, "Users", "Permissions"));
            Assert.True(ColumnExists(verify, "Reservations", "RentalID"));
            using var select = new SqliteCommand("SELECT NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, IsPowered, IsIncomplete, MissingComponentsNotes, IssuesNotes, CheckoutCount FROM Items WHERE ItemID = 1", verify);
            using var reader = select.ExecuteReader();
            Assert.True(reader.Read());
            Assert.Equal("Migrated save check", reader.GetString(0));
            Assert.Equal(4, reader.GetInt32(1));
            Assert.Equal(1, reader.GetInt32(2));
            Assert.Equal(1, reader.GetInt32(3));
            Assert.Equal(1, reader.GetInt32(4));
            Assert.Equal(1, reader.GetInt32(5));
            Assert.Equal("Case", reader.GetString(6));
            Assert.Equal("None", reader.GetString(7));
            Assert.Equal(2, reader.GetInt32(8));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }

        static bool ColumnExists(SqliteConnection conn, string table, string column)
        {
            using var pragma = new SqliteCommand($"PRAGMA table_info({table});", conn);
            using var reader = pragma.ExecuteReader();
            while (reader.Read())
            {
                if (string.Equals(reader["name"]?.ToString(), column, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}

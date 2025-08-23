using System.Collections.Generic;
using InventoryManagementApp.Data;
using Xunit;

public class SqliteConnectionFactoryTests
{
    [Fact]
    public void Create_ExecutesPragmasEachTime()
    {
        SqliteConnectionFactory.Reset();
        var factory = new SqliteConnectionFactory("Data Source=:memory:");
        using var first = factory.Create();
        using var second = factory.Create();
        Assert.Equal(2, SqliteConnectionFactory.PragmasExecutionCount);
    }

    [Fact]
    public void Create_CreatesIndexesWhenItemsTableExists()
    {
        SqliteConnectionFactory.Reset();
        var factory = new SqliteConnectionFactory("Data Source=:memory:");
        using (var conn = factory.Create())
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "CREATE TABLE Items (ItemNumber TEXT, NameDescription TEXT, AvailableQuantity INTEGER, UpdatedAt TEXT);";
            cmd.ExecuteNonQuery();
        }

        using var verify = factory.Create();
        using var check = verify.CreateCommand();
        check.CommandText = "SELECT name FROM sqlite_master WHERE type='index' AND tbl_name='Items' ORDER BY name;";
        using var reader = check.ExecuteReader();
        var indexes = new List<string>();
        while (reader.Read())
            indexes.Add(reader.GetString(0));
        Assert.Contains("IX_Items_ItemNumber", indexes);
        Assert.Contains("IX_Items_NameDescription", indexes);
        Assert.Contains("IX_Items_AvailableQuantity", indexes);
        Assert.Contains("IX_Items_UpdatedAt", indexes);
    }
}

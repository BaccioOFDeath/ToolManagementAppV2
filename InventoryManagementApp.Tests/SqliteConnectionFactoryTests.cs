using InventoryManagementApp.Data;
using Microsoft.Data.Sqlite;
using Xunit;

public class SqliteConnectionFactoryTests
{
    [Fact]
    public void Create_ExecutesPragmasOnlyOnce()
    {
        SqliteConnectionFactory.Reset();
        var factory = new SqliteConnectionFactory("Data Source=:memory:");
        using var first = factory.Create();
        using var second = factory.Create();
        Assert.Equal(1, SqliteConnectionFactory.PragmasExecutionCount);
    }

    [Fact]
    public void Create_WhenWalDisabled_UsesDeleteJournalMode()
    {
        SqliteConnectionFactory.Reset();
        var dbPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{System.Guid.NewGuid():N}.db");

        try
        {
            var factory = new SqliteConnectionFactory($"Data Source={dbPath}", useWalJournal: false, useConnectionPooling: false);
            using var connection = (SqliteConnection)factory.Create();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "PRAGMA journal_mode;";

            Assert.Equal("delete", System.Convert.ToString(cmd.ExecuteScalar())?.ToLowerInvariant());
            Assert.Equal(0, SqliteConnectionFactory.PragmasExecutionCount);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (System.IO.File.Exists(dbPath))
                System.IO.File.Delete(dbPath);
        }
    }
}

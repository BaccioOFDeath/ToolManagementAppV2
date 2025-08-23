using System.Collections.Generic;
using InventoryManagementApp.Data;
using Microsoft.Extensions.Logging;
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
            cmd.CommandText = "CREATE TABLE Items (ItemNumber TEXT, NameDescription TEXT, AvailableQuantity INTEGER, Price NUMERIC NOT NULL DEFAULT 0, UpdatedAt TEXT);";
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

    [Fact]
    public void Create_DoesNotCreateIndexWhenColumnMissing()
    {
        SqliteConnectionFactory.Reset();
        var factory = new SqliteConnectionFactory("Data Source=:memory:");
        using (var conn = factory.Create())
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "CREATE TABLE Items (ItemNumber TEXT, NameDescription TEXT, AvailableQuantity INTEGER);";
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
        Assert.DoesNotContain("IX_Items_UpdatedAt", indexes);
    }

    [Fact]
    public void Create_LogsWarningWhenUpdatedAtColumnMissing()
    {
        SqliteConnectionFactory.Reset();
        var logger = new ListLogger<SqliteConnectionFactory>();
        var factory = new SqliteConnectionFactory("Data Source=:memory:", logger);
        using (var conn = factory.Create())
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "CREATE TABLE Items (ItemNumber TEXT, NameDescription TEXT, AvailableQuantity INTEGER);";
            cmd.ExecuteNonQuery();
        }

        using (factory.Create()) { }

        Assert.Contains(LogLevel.Warning, logger.Levels);
        Assert.Contains(logger.Messages, m => m.Contains("UpdatedAt"));
    }
}

internal sealed class ListLogger<T> : ILogger<T>
{
    public List<LogLevel> Levels { get; } = new();
    public List<string> Messages { get; } = new();

    public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Levels.Add(logLevel);
        Messages.Add(formatter(state, exception));
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}

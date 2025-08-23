using Microsoft.Data.Sqlite;

namespace InventoryManagementApp.Data;

public sealed class SqliteConnectionFactory
{
    private readonly string _connectionString;
    private static bool _pragmasExecuted;
    internal static int PragmasExecutionCount { get; private set; }

    public SqliteConnectionFactory(string connectionString)
        => _connectionString = connectionString;

    internal static void Reset()
    {
        _pragmasExecuted = false;
        PragmasExecutionCount = 0;
    }

    public SqliteConnection Create()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        if (!_pragmasExecuted)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;";
            cmd.ExecuteNonQuery();
            _pragmasExecuted = true;
            PragmasExecutionCount++;
        }
        return connection;
    }
}

using System.Data;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace InventoryManagementApp.Data;

public sealed class SqliteConnectionFactory
{
    private readonly string _connectionString;
    private static readonly object _lock = new();
    internal static int PragmasExecutionCount { get; private set; }

    public SqliteConnectionFactory(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString)
        {
            Pooling = true
        };
        _connectionString = builder.ToString();
    }

    internal static void Reset()
    {
        PragmasExecutionCount = 0;
    }

    public IDbConnection Create()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();

        connection.CreateCollation("NOCASE_NOACCENT", static (x, y) =>
            string.Compare(x, y, CultureInfo.InvariantCulture,
                CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace));

        lock (_lock)
        {
            if (PragmasExecutionCount == 0)
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;";
                cmd.ExecuteNonQuery();
                PragmasExecutionCount++;
            }
        }

        return connection;
    }
}

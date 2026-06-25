using System.Data;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace InventoryManagementApp.Data;

public sealed class SqliteConnectionFactory
{
    private readonly string _connectionString;
    private readonly bool _useWalJournal;
    private readonly int _busyTimeoutMilliseconds;
    private bool _pragmasConfigured;
    private static readonly object _lock = new();
    internal static int PragmasExecutionCount { get; private set; }

    public SqliteConnectionFactory(
        string connectionString,
        bool useWalJournal = true,
        bool useConnectionPooling = true,
        int busyTimeoutMilliseconds = 15000)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString)
        {
            Pooling = useConnectionPooling
        };
        _connectionString = builder.ToString();
        _useWalJournal = useWalJournal;
        _busyTimeoutMilliseconds = busyTimeoutMilliseconds;
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

        using (var timeout = connection.CreateCommand())
        {
            timeout.CommandText = $"PRAGMA busy_timeout={_busyTimeoutMilliseconds};";
            timeout.ExecuteNonQuery();
        }

        lock (_lock)
        {
            if (_useWalJournal && !_pragmasConfigured)
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;";
                cmd.ExecuteNonQuery();
                _pragmasConfigured = true;
                PragmasExecutionCount++;
            }
        }

        return connection;
    }
}

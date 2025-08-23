using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace InventoryManagementApp.Data;

public sealed class SqliteConnectionFactory
{
    private readonly string _connectionString;
    private readonly ILogger<SqliteConnectionFactory>? _logger;
    private static readonly object _lock = new();
    internal static int PragmasExecutionCount { get; private set; }

    public SqliteConnectionFactory(string connectionString, ILogger<SqliteConnectionFactory>? logger = null)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString)
        {
            Pooling = true
        };
        _connectionString = builder.ToString();
        _logger = logger;
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
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT 1 FROM sqlite_master WHERE type='table' AND name='Items';";
            var exists = cmd.ExecuteScalar() != null;

            if (exists)
            {
                cmd.CommandText = "PRAGMA table_info('Items');";
                using var reader = cmd.ExecuteReader();
                var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                while (reader.Read())
                    columns.Add(reader.GetString(1));
                reader.Close();

                var sb = new StringBuilder();
                if (columns.Contains("ItemNumber"))
                    sb.AppendLine("CREATE INDEX IF NOT EXISTS IX_Items_ItemNumber ON Items(ItemNumber);");
                if (columns.Contains("NameDescription"))
                    sb.AppendLine("CREATE INDEX IF NOT EXISTS IX_Items_NameDescription ON Items(NameDescription);");
                if (columns.Contains("AvailableQuantity"))
                    sb.AppendLine("CREATE INDEX IF NOT EXISTS IX_Items_AvailableQuantity ON Items(AvailableQuantity);");
                if (columns.Contains("UpdatedAt"))
                    sb.AppendLine("CREATE INDEX IF NOT EXISTS IX_Items_UpdatedAt ON Items(UpdatedAt);");
                else
                    _logger?.LogWarning("Column 'UpdatedAt' not found in Items table; skipping index creation for IX_Items_UpdatedAt.");
                if (sb.Length > 0)
                {
                    cmd.CommandText = sb.ToString();
                    cmd.ExecuteNonQuery();
                }
            }

            PragmasExecutionCount++;
        }

        return connection;
    }
}

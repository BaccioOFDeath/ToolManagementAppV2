using System.IO;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
using Microsoft.Data.Sqlite;
using Xunit;

namespace InventoryManagementApp.Tests.Services;

public class SqliteConnectionFactoryTests
{
    [Fact]
    public async Task Create_AppliesPragmas()
    {
        var path = Path.GetTempFileName();
        try
        {
            var builder = new SqliteConnectionStringBuilder { DataSource = path, Pooling = true };
            var factory = new SqliteConnectionFactory(builder.ToString());
            await using var conn = factory.Create();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA journal_mode; PRAGMA synchronous;";
            using var reader = await cmd.ExecuteReaderAsync();
            Assert.True(reader.Read());
            Assert.Equal("wal", reader.GetString(0).ToLowerInvariant());
            Assert.True(reader.NextResult());
            Assert.True(reader.Read());
            Assert.Equal(1, reader.GetInt32(0));
        }
        finally
        {
            File.Delete(path);
        }
    }
}

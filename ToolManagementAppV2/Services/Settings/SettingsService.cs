using System.Data.SQLite;
using ToolManagementAppV2.Services.Core;

namespace ToolManagementAppV2.Services.Settings
{
    public class SettingsService
    {
        readonly string _connString;
        const string UpsertSql = @"
            INSERT INTO Settings (Key, Value) 
            VALUES (@Key, @Value)
            ON CONFLICT(Key) DO UPDATE SET Value = @Value";

        public SettingsService(DatabaseService dbService) =>
            _connString = dbService.ConnectionString;

        public void SaveSetting(string key, string value)
        {
            var p = new[]
            {
                new SQLiteParameter("@Key", key),
                new SQLiteParameter("@Value", value)
            };
            SqliteHelper.ExecuteNonQuery(_connString, UpsertSql, p);
        }

        public string GetSetting(string key)
        {
            const string sql = "SELECT Value FROM Settings WHERE Key = @Key";
            using var conn = new SQLiteConnection(_connString);
            conn.Open();
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Key", key);
            return cmd.ExecuteScalar()?.ToString();
        }

        public Dictionary<string, string> GetAllSettings()
        {
            var dict = new Dictionary<string, string>();
            const string sql = "SELECT Key, Value FROM Settings";
            using var conn = new SQLiteConnection(_connString);
            conn.Open();
            using var cmd = new SQLiteCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                dict[rdr["Key"].ToString()] = rdr["Value"].ToString();
            return dict;
        }

        public void UpdateSettings(Dictionary<string, string> settings)
        {
            using var conn = new SQLiteConnection(_connString);
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                foreach (var kv in settings)
                {
                    var p = new[]
                    {
                        new SQLiteParameter("@Key", kv.Key),
                        new SQLiteParameter("@Value", kv.Value)
                    };
                    SqliteHelper.ExecuteNonQuery(conn, tx, UpsertSql, p);
                }
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void DeleteSetting(string key)
        {
            const string sql = "DELETE FROM Settings WHERE Key = @Key";
            var p = new[] { new SQLiteParameter("@Key", key) };
            SqliteHelper.ExecuteNonQuery(_connString, sql, p);
        }
    }
}

using System.Data.SQLite;
using System;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.Services.Settings
{
    public class SettingsService : ISettingsService
    {
        readonly DatabaseService _dbService;
        const string UpsertSql = @"
            INSERT INTO Settings (Key, Value) 
            VALUES (@Key, @Value)
            ON CONFLICT(Key) DO UPDATE SET Value = @Value";

        public SettingsService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public void SaveSetting(string key, string value)
        {
            var p = new[]
            {
                new SQLiteParameter("@Key", key),
                new SQLiteParameter("@Value", value)
            };
            using var conn = _dbService.CreateConnection();
            SqliteHelper.ExecuteNonQuery(conn, UpsertSql, p);
        }

        public string GetSetting(string key)
        {
            const string sql = "SELECT Value FROM Settings WHERE Key = @Key";
            using var conn = _dbService.CreateConnection();
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Key", key);
            return cmd.ExecuteScalar()?.ToString();
        }

        public Dictionary<string, string> GetAllSettings()
        {
            var dict = new Dictionary<string, string>();
            const string sql = "SELECT Key, Value FROM Settings";
            using var conn = _dbService.CreateConnection();
            using var cmd = new SQLiteCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                dict[rdr["Key"].ToString()] = rdr["Value"].ToString();
            return dict;
        }

        public void UpdateSettings(Dictionary<string, string> settings)
        {
            using var conn = _dbService.CreateConnection();
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
            catch (Exception ex)
            {
                tx.Rollback();
                Console.WriteLine(ex);
                return;
            }
        }

        public void DeleteSetting(string key)
        {
            const string sql = "DELETE FROM Settings WHERE Key = @Key";
            var p = new[] { new SQLiteParameter("@Key", key) };
            using var conn = _dbService.CreateConnection();
            SqliteHelper.ExecuteNonQuery(conn, sql, p);
        }
    }
}

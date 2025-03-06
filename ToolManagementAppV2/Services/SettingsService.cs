using System.Collections.Generic;
using System.Data.SQLite;

namespace ToolManagementAppV2.Services
{
    public class SettingsService
    {
        private readonly DatabaseService _dbService;

        public SettingsService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public void SaveSetting(string key, string value)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = @"
                INSERT INTO Settings (Key, Value) 
                VALUES (@Key, @Value)
                ON CONFLICT(Key) DO UPDATE SET Value = @Value";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Key", key);
            command.Parameters.AddWithValue("@Value", value);
            command.ExecuteNonQuery();
        }

        public string GetSetting(string key)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = "SELECT Value FROM Settings WHERE Key = @Key";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Key", key);
            return command.ExecuteScalar()?.ToString();
        }

        public Dictionary<string, string> GetAllSettings()
        {
            var settings = new Dictionary<string, string>();
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = "SELECT Key, Value FROM Settings";
            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                settings[reader["Key"].ToString()] = reader["Value"].ToString();
            }

            return settings;
        }

        public void UpdateSettings(Dictionary<string, string> settings)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var kv in settings)
                {
                    var query = @"
                INSERT INTO Settings (Key, Value) 
                VALUES (@Key, @Value)
                ON CONFLICT(Key) DO UPDATE SET Value = @Value";
                    using var command = new SQLiteCommand(query, connection, transaction);
                    command.Parameters.AddWithValue("@Key", kv.Key);
                    command.Parameters.AddWithValue("@Value", kv.Value);
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void DeleteSetting(string key)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "DELETE FROM Settings WHERE Key = @Key";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Key", key);
            command.ExecuteNonQuery();
        }


    }
}

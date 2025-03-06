using System;
using System.Data.SQLite;
using ToolManagementAppV2.Models;

namespace ToolManagementAppV2.Services
{
    public class ActivityLogService
    {
        private readonly DatabaseService _dbService;
        public ActivityLogService(DatabaseService dbService) => _dbService = dbService;

        public void LogAction(int userID, string userName, string action)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = "INSERT INTO ActivityLogs (UserID, UserName, Action) VALUES (@UserID, @UserName, @Action)";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@UserID", userID);
            command.Parameters.AddWithValue("@UserName", userName);
            command.Parameters.AddWithValue("@Action", action);
            command.ExecuteNonQuery();
        }

        public List<ActivityLog> GetRecentLogs(int count = 50)
        {
            var logs = new List<ActivityLog>();
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "SELECT * FROM ActivityLogs ORDER BY Timestamp DESC LIMIT @Count";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Count", count);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                logs.Add(new ActivityLog
                {
                    LogID = Convert.ToInt32(reader["LogID"]),
                    UserID = Convert.ToInt32(reader["UserID"]),
                    UserName = reader["UserName"].ToString(),
                    Action = reader["Action"].ToString(),
                    Timestamp = Convert.ToDateTime(reader["Timestamp"])
                });
            }
            return logs;
        }

        public void PurgeOldLogs(DateTime threshold)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "DELETE FROM ActivityLogs WHERE Timestamp < @Threshold";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Threshold", threshold);
            command.ExecuteNonQuery();
        }


    }
}

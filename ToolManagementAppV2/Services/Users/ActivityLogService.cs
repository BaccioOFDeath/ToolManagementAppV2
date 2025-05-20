using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;

namespace ToolManagementAppV2.Services.Users
{
    public class ActivityLogService
    {
        readonly string _connString;

        public ActivityLogService(DatabaseService dbService) =>
            _connString = dbService.ConnectionString;

        public void LogAction(int userID, string userName, string action)
        {
            const string sql = @"
                INSERT INTO ActivityLogs (UserID, UserName, Action)
                VALUES (@UserID, @UserName, @Action)";
            var p = new[]
            {
                new SQLiteParameter("@UserID",   userID),
                new SQLiteParameter("@UserName", userName),
                new SQLiteParameter("@Action",   action)
            };
            SqliteHelper.ExecuteNonQuery(_connString, sql, p);
        }

        public List<ActivityLog> GetRecentLogs(int count = 50)
        {
            const string sql = @"
                SELECT * FROM ActivityLogs
                 ORDER BY Timestamp DESC
                 LIMIT @Count";
            var p = new[] { new SQLiteParameter("@Count", count) };
            return SqliteHelper.ExecuteReader(_connString, sql, p, MapLog);
        }

        public void PurgeOldLogs(DateTime threshold)
        {
            const string sql = @"
                DELETE FROM ActivityLogs
                 WHERE Timestamp < @Threshold";
            var p = new[] { new SQLiteParameter("@Threshold", threshold) };
            SqliteHelper.ExecuteNonQuery(_connString, sql, p);
        }

        ActivityLog MapLog(IDataRecord r) => new()
        {
            LogID = Convert.ToInt32(r["LogID"]),
            UserID = Convert.ToInt32(r["UserID"]),
            UserName = r["UserName"].ToString(),
            Action = r["Action"].ToString(),
            Timestamp = Convert.ToDateTime(r["Timestamp"])
        };
    }
}

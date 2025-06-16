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
        readonly DatabaseService _dbService;

        public ActivityLogService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

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
            using var conn = _dbService.CreateConnection();
            SqliteHelper.ExecuteNonQuery(conn, sql, p);
        }

        public List<ActivityLog> GetRecentLogs(int count = 50)
        {
            const string sql = @"
                SELECT * FROM ActivityLogs
                 ORDER BY Timestamp DESC
                 LIMIT @Count";
            var p = new[] { new SQLiteParameter("@Count", count) };
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReader(conn, sql, p, MapLog);
        }

        public void PurgeOldLogs(DateTime threshold)
        {
            const string sql = @"
                DELETE FROM ActivityLogs
                 WHERE Timestamp < @Threshold";
            var p = new[] { new SQLiteParameter("@Threshold", threshold) };
            using var conn = _dbService.CreateConnection();
            SqliteHelper.ExecuteNonQuery(conn, sql, p);
        }

        ActivityLog MapLog(IDataRecord r)
        {
            var log = new ActivityLog
            {
                LogID = Convert.ToInt32(r["LogID"]),
                UserName = r["UserName"].ToString(),
                Action = r["Action"].ToString(),
                Timestamp = Convert.ToDateTime(r["Timestamp"])
            };

            log.UserID = r["UserID"] == DBNull.Value
                ? 0
                : Convert.ToInt32(r["UserID"]);

            return log;
        }
    }
}

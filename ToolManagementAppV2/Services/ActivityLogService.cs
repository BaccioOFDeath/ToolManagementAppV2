// File: Services/ActivityLogService.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using ToolManagementAppV2.Models;

namespace ToolManagementAppV2.Services
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
            ExecuteNonQuery(sql, p);
        }

        public List<ActivityLog> GetRecentLogs(int count = 50)
        {
            const string sql = @"
                SELECT * FROM ActivityLogs
                 ORDER BY Timestamp DESC
                 LIMIT @Count";
            var p = new[] { new SQLiteParameter("@Count", count) };
            return ExecuteReader(sql, p);
        }

        public void PurgeOldLogs(DateTime threshold)
        {
            const string sql = @"
                DELETE FROM ActivityLogs
                 WHERE Timestamp < @Threshold";
            var p = new[] { new SQLiteParameter("@Threshold", threshold) };
            ExecuteNonQuery(sql, p);
        }

        // --- Helpers ---
        List<ActivityLog> ExecuteReader(string sql, SQLiteParameter[] parameters)
        {
            var list = new List<ActivityLog>();
            using var conn = new SQLiteConnection(_connString);
            conn.Open();
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                list.Add(MapLog(rdr));
            return list;
        }

        void ExecuteNonQuery(string sql, SQLiteParameter[] parameters)
        {
            using var conn = new SQLiteConnection(_connString);
            conn.Open();
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);
            cmd.ExecuteNonQuery();
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

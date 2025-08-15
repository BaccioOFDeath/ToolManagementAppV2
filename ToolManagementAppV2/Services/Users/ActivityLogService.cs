using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;

namespace ToolManagementAppV2.Services.Users
{
    public class ActivityLogService
    {
        readonly DatabaseService _dbService;
        readonly ILogger<ActivityLogService> _logger;

        public ActivityLogService(DatabaseService dbService, ILogger<ActivityLogService>? logger = null)
        {
            _dbService = dbService;
            _logger = logger ?? NullLogger<ActivityLogService>.Instance;
        }

        public virtual Result LogAction(int userID, string userName, string action)
        {
            try
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
                return new Result(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log activity {Action}", action);
                return new Result(false, ex.Message);
            }
        }

        public virtual Result<List<ActivityLog>> GetRecentLogs(int count = 50)
        {
            try
            {
                const string sql = @"
                    SELECT * FROM ActivityLogs
                     ORDER BY Timestamp DESC
                     LIMIT @Count";
                var p = new[] { new SQLiteParameter("@Count", count) };
                using var conn = _dbService.CreateConnection();
                var logs = SqliteHelper.ExecuteReader(conn, sql, p, MapLog);
                return new Result<List<ActivityLog>>(logs, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve recent activity logs");
                return new Result<List<ActivityLog>>(null, false, ex.Message);
            }
        }

        public virtual async Task<Result<List<ActivityLog>>> GetRecentLogsAsync(int count = 50)
        {
            try
            {
                const string sql = @"
                    SELECT * FROM ActivityLogs
                     ORDER BY Timestamp DESC
                     LIMIT @Count";
                var p = new[] { new SQLiteParameter("@Count", count) };
                using var conn = _dbService.CreateConnection();
                var logs = await SqliteHelper.ExecuteReaderAsync(conn, sql, p, MapLog);
                return new Result<List<ActivityLog>>(logs, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve recent activity logs");
                return new Result<List<ActivityLog>>(null, false, ex.Message);
            }
        }

        public virtual Result PurgeOldLogs(DateTime threshold)
        {
            try
            {
                const string sql = @"
                    DELETE FROM ActivityLogs
                     WHERE Timestamp < @Threshold";
                var p = new[] { new SQLiteParameter("@Threshold", threshold) };
                using var conn = _dbService.CreateConnection();
                SqliteHelper.ExecuteNonQuery(conn, sql, p);
                return new Result(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to purge old activity logs prior to {Threshold}", threshold);
                return new Result(false, ex.Message);
            }
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

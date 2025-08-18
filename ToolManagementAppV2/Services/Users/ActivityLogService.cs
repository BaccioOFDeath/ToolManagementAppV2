using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using System.Globalization;

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

        public virtual async Task<Result> LogActionAsync(int userID, string userName, string action, CancellationToken cancellationToken = default)
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
                await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken).ConfigureAwait(false);
                return new Result(true);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Logging action {Action} canceled or timed out", action);
                return new Result(false, "Operation canceled");
            }
            catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Busy)
            {
                _logger.LogWarning(ex, "Logging action {Action} timed out", action);
                return new Result(false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log activity {Action}", action);
                return new Result(false, ex.Message);
            }
        }

        public virtual async Task<Result<List<ActivityLog>>> GetRecentLogsAsync(int count = 50, CancellationToken cancellationToken = default)
        {
            try
            {
                const string sql = @"
                    SELECT * FROM ActivityLogs
                     ORDER BY Timestamp DESC
                     LIMIT @Count";
                var p = new[] { new SQLiteParameter("@Count", count) };
                using var conn = _dbService.CreateConnection();
                var logs = await SqliteHelper.ExecuteReaderAsync(conn, sql, p, MapLog, cancellationToken).ConfigureAwait(false);
                return new Result<List<ActivityLog>>(logs, true);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Retrieving recent activity logs canceled or timed out");
                return new Result<List<ActivityLog>>(null, false, "Operation canceled");
            }
            catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Busy)
            {
                _logger.LogWarning(ex, "Retrieving recent activity logs timed out");
                return new Result<List<ActivityLog>>(null, false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve recent activity logs");
                return new Result<List<ActivityLog>>(null, false, ex.Message);
            }
        }

        public virtual async Task<Result> PurgeOldLogsAsync(DateTime threshold, CancellationToken cancellationToken = default)
        {
            try
            {
                const string sql = @"
                    DELETE FROM ActivityLogs
                     WHERE Timestamp < @Threshold";
                var p = new[] { new SQLiteParameter("@Threshold", threshold) };
                using var conn = _dbService.CreateConnection();
                await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken).ConfigureAwait(false);
                return new Result(true);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Purging logs prior to {Threshold} canceled or timed out", threshold);
                return new Result(false, "Operation canceled");
            }
            catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Busy)
            {
                _logger.LogWarning(ex, "Purging logs prior to {Threshold} timed out", threshold);
                return new Result(false, ex.Message);
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
                Timestamp = DateTime.Parse(r["Timestamp"].ToString()!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)
            };

            log.UserID = r["UserID"] == DBNull.Value
                ? 0
                : Convert.ToInt32(r["UserID"]);

            return log;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using System.Globalization;

namespace InventoryManagementApp.Services.Users
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
                    new SqliteParameter("@UserID",   userID),
                    new SqliteParameter("@UserName", userName),
                    new SqliteParameter("@Action",   action)
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
            catch (SqliteException ex) when (ex.SqliteErrorCode == (int)SqliteError.Busy)
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
                var p = new[] { new SqliteParameter("@Count", count) };
                using var conn = _dbService.CreateConnection();
                var logs = await SqliteHelper.ExecuteReaderAsync(conn, sql, MapLog, p, cancellationToken).ConfigureAwait(false);
                return new Result<List<ActivityLog>>(logs, true);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Retrieving recent activity logs canceled or timed out");
                return new Result<List<ActivityLog>>(null, false, "Operation canceled");
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == (int)SqliteError.Busy)
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
                var p = new[] { new SqliteParameter("@Threshold", threshold) };
                using var conn = _dbService.CreateConnection();
                await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken).ConfigureAwait(false);
                return new Result(true);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Purging logs prior to {Threshold} canceled or timed out", threshold);
                return new Result(false, "Operation canceled");
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == (int)SqliteError.Busy)
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
            var rawTimestamp = r["Timestamp"]?.ToString();
            DateTime timestamp;

            if (!DateTime.TryParseExact(rawTimestamp, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out timestamp) &&
                !DateTime.TryParse(rawTimestamp, CultureInfo.CurrentCulture, DateTimeStyles.None, out timestamp))
            {
                _logger.LogWarning("Invalid timestamp '{Timestamp}' for log {LogID}", rawTimestamp, r["LogID"]);
                timestamp = DateTime.MinValue;
            }
            else
            {
                if (timestamp.Kind == DateTimeKind.Unspecified)
                {
                    timestamp = DateTime.SpecifyKind(timestamp, DateTimeKind.Local);
                }
                timestamp = timestamp.ToLocalTime();
            }

            var log = new ActivityLog
            {
                LogID = Convert.ToInt32(r["LogID"]),
                UserName = r["UserName"].ToString(),
                Action = r["Action"].ToString(),
                Timestamp = timestamp
            };

            log.UserID = r["UserID"] == DBNull.Value
                ? 0
                : Convert.ToInt32(r["UserID"]);

            return log;
        }
    }
}

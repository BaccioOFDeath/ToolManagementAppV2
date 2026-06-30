using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Services.Rentals
{
    public class RentalContactLogService
    {
        const int MaxContactLogCount = 500;

        readonly DatabaseService _dbService;
        readonly IUserContext? _userContext;
        readonly ILogger<RentalContactLogService> _logger;

        public RentalContactLogService(DatabaseService dbService, IUserContext? userContext = null, ILogger<RentalContactLogService>? logger = null)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _userContext = userContext;
            _logger = logger ?? NullLogger<RentalContactLogService>.Instance;
        }

        public async Task<Result> AddContactLogAsync(RentalContactLog log, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                Validate(log);
                Normalize(log);

                if (string.IsNullOrWhiteSpace(log.CreatedBy))
                    log.CreatedBy = _userContext?.UserName ?? string.Empty;

                const string sql = @"
                    INSERT INTO RentalContactLogs (RentalID, Channel, Direction, Recipient, Subject, Message, CreatedBy)
                    VALUES (@RentalID, @Channel, @Direction, @Recipient, @Subject, @Message, @CreatedBy);";

                using var conn = _dbService.CreateConnection();
                var rows = await SqliteHelper.ExecuteNonQueryAsync(conn, sql, new[]
                {
                    new SqliteParameter("@RentalID", log.RentalID),
                    new SqliteParameter("@Channel", log.Channel),
                    new SqliteParameter("@Direction", log.Direction),
                    new SqliteParameter("@Recipient", log.Recipient),
                    new SqliteParameter("@Subject", log.Subject),
                    new SqliteParameter("@Message", log.Message),
                    new SqliteParameter("@CreatedBy", log.CreatedBy)
                }, cancellationToken).ConfigureAwait(false);

                return rows == 0
                    ? new Result(false, "Unable to save rental contact log.")
                    : new Result(true);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to save rental contact log for rental {RentalID}", log?.RentalID);
                return new Result(false, ex.Message);
            }
        }

        public async Task<Result<List<RentalContactLog>>> GetContactLogsForRentalAsync(int rentalID, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (rentalID < 1)
                    return new Result<List<RentalContactLog>>(null, false, "Rental ID must be greater than 0.");

                const string sql = @"
                    SELECT *
                    FROM RentalContactLogs
                    WHERE RentalID = @RentalID
                    ORDER BY CreatedAt DESC
                    LIMIT @Limit";

                using var conn = _dbService.CreateConnection();
                var logs = await SqliteHelper.ExecuteReaderAsync(conn, sql, MapLog, new[]
                {
                    new SqliteParameter("@RentalID", rentalID),
                    new SqliteParameter("@Limit", MaxContactLogCount)
                }, cancellationToken).ConfigureAwait(false);

                return new Result<List<RentalContactLog>>(logs, true);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to load rental contact logs for rental {RentalID}", rentalID);
                return new Result<List<RentalContactLog>>(null, false, ex.Message);
            }
        }

        static void Normalize(RentalContactLog log)
        {
            log.Channel = (log.Channel ?? string.Empty).Trim();
            log.Direction = (log.Direction ?? string.Empty).Trim();
            log.Recipient = (log.Recipient ?? string.Empty).Trim();
            log.Subject = (log.Subject ?? string.Empty).Trim();
            log.Message = (log.Message ?? string.Empty).Trim();
            log.CreatedBy = (log.CreatedBy ?? string.Empty).Trim();
        }

        static void Validate(RentalContactLog log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));
            if (log.RentalID < 1)
                throw new ArgumentOutOfRangeException(nameof(log), "Rental ID must be greater than 0.");
            if (string.IsNullOrWhiteSpace(log.Channel))
                throw new ArgumentException("Channel is required.", nameof(log));
            if (string.IsNullOrWhiteSpace(log.Direction))
                throw new ArgumentException("Direction is required.", nameof(log));
            if (string.IsNullOrWhiteSpace(log.Message))
                throw new ArgumentException("Message is required.", nameof(log));
        }

        static RentalContactLog MapLog(IDataRecord r)
        {
            var createdAtText = r["CreatedAt"]?.ToString();
            if (!DateTime.TryParseExact(createdAtText, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var createdAt) &&
                !DateTime.TryParse(createdAtText, CultureInfo.CurrentCulture, DateTimeStyles.None, out createdAt))
            {
                createdAt = DateTime.MinValue;
            }

            if (createdAt.Kind == DateTimeKind.Unspecified)
                createdAt = DateTime.SpecifyKind(createdAt, DateTimeKind.Local);

            return new RentalContactLog
            {
                ContactLogID = Convert.ToInt32(r["ContactLogID"]),
                RentalID = Convert.ToInt32(r["RentalID"]),
                Channel = r["Channel"]?.ToString() ?? string.Empty,
                Direction = r["Direction"]?.ToString() ?? string.Empty,
                Recipient = r["Recipient"]?.ToString() ?? string.Empty,
                Subject = r["Subject"]?.ToString() ?? string.Empty,
                Message = r["Message"]?.ToString() ?? string.Empty,
                CreatedBy = r["CreatedBy"]?.ToString() ?? string.Empty,
                CreatedAt = createdAt.ToLocalTime()
            };
        }
    }
}

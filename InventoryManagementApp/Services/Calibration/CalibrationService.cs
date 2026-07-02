using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace InventoryManagementApp.Services.Calibration
{
    /// <summary>
    /// Service for managing equipment calibration records including scheduling and tracking calibration status.
    /// </summary>
    public class CalibrationService
    {
        private const int MaxCalibrationListCount = 500;

        private readonly DatabaseService _databaseService;
        private readonly IUserContext _userContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationService"/> class.
        /// </summary>
        /// <param name="databaseService">Database service for data access.</param>
        /// <param name="userContext">User context for tracking current user.</param>
        public CalibrationService(DatabaseService databaseService, IUserContext userContext)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        /// <summary>
        /// Retrieves all calibration records from the database, ordered by calibration date descending.
        /// </summary>
        /// <returns>A list of all calibration records.</returns>
        public async Task<List<CalibrationRecord>> GetAllCalibrationRecordsAsync()
        {
            return await Task.Run(() =>
            {
                var records = new List<CalibrationRecord>();
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT c.*, i.ItemNumber, i.NameDescription as ItemName
                    FROM CalibrationRecords c
                    JOIN Items i ON c.ItemID = i.ItemID
                    ORDER BY c.CalibrationDate DESC
                    LIMIT @CalibrationListLimit";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CalibrationListLimit", MaxCalibrationListCount);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(MapCalibrationRecord(reader));
                }
                return records;
            });
        }

        public async Task<int> CountCalibrationRecordsAsync()
        {
            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT COUNT(c.CalibrationID)
                    FROM CalibrationRecords c
                    JOIN Items i ON c.ItemID = i.ItemID";
                using var cmd = new SqliteCommand(sql, conn);
                return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            });
        }

        /// <summary>
        /// Retrieves all calibration records for a specific item.
        /// </summary>
        /// <param name="itemID">The ID of the item.</param>
        /// <returns>A list of calibration records for the specified item.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if itemID is less than 1.</exception>
        public async Task<List<CalibrationRecord>> GetCalibrationRecordsByItemAsync(int itemID)
        {
            if (itemID < 1)
                throw new ArgumentOutOfRangeException(nameof(itemID), "Item ID must be greater than 0.");
            return await Task.Run(() =>
            {
                var records = new List<CalibrationRecord>();
                using var conn = _databaseService.CreateConnection();
                EnsureItemExists(conn, itemID);
                var sql = @"
                    SELECT c.*, i.ItemNumber, i.NameDescription as ItemName
                    FROM CalibrationRecords c
                    JOIN Items i ON c.ItemID = i.ItemID
                    WHERE c.ItemID = @ItemID
                    ORDER BY c.CalibrationDate DESC
                    LIMIT @CalibrationListLimit";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ItemID", itemID);
                cmd.Parameters.AddWithValue("@CalibrationListLimit", MaxCalibrationListCount);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(MapCalibrationRecord(reader));
                }
                return records;
            });
        }

        public async Task<List<CalibrationRecord>> GetOverdueCalibrationAsync()
        {
            return await Task.Run(() =>
            {
                var records = new List<CalibrationRecord>();
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT c.*, i.ItemNumber, i.NameDescription as ItemName
                    FROM CalibrationRecords c
                    JOIN Items i ON c.ItemID = i.ItemID
                    WHERE c.NextCalibrationDue < @Now
                    ORDER BY c.NextCalibrationDue ASC
                    LIMIT @CalibrationListLimit";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                cmd.Parameters.AddWithValue("@CalibrationListLimit", MaxCalibrationListCount);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(MapCalibrationRecord(reader));
                }
                return records;
            });
        }

        public async Task<int> CountOverdueCalibrationAsync()
        {
            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT COUNT(c.CalibrationID)
                    FROM CalibrationRecords c
                    JOIN Items i ON c.ItemID = i.ItemID
                    WHERE c.NextCalibrationDue < @Now";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            });
        }

        public async Task<List<CalibrationRecord>> GetUpcomingCalibrationAsync(int days = 30)
        {
            if (days < 0)
                throw new ArgumentOutOfRangeException(nameof(days), "Days must be greater than or equal to 0.");

            return await Task.Run(() =>
            {
                var records = new List<CalibrationRecord>();
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT c.*, i.ItemNumber, i.NameDescription as ItemName
                    FROM CalibrationRecords c
                    JOIN Items i ON c.ItemID = i.ItemID
                    WHERE c.NextCalibrationDue >= @Now 
                    AND c.NextCalibrationDue <= @FutureDate
                    ORDER BY c.NextCalibrationDue ASC
                    LIMIT @CalibrationListLimit";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                cmd.Parameters.AddWithValue("@FutureDate", DateTime.Now.AddDays(days));
                cmd.Parameters.AddWithValue("@CalibrationListLimit", MaxCalibrationListCount);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(MapCalibrationRecord(reader));
                }
                return records;
            });
        }

        public async Task<int> CountUpcomingCalibrationAsync(int days = 30)
        {
            if (days < 0)
                throw new ArgumentOutOfRangeException(nameof(days), "Days must be greater than or equal to 0.");

            return await Task.Run(() =>
            {
                var now = DateTime.Now;
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT COUNT(c.CalibrationID)
                    FROM CalibrationRecords c
                    JOIN Items i ON c.ItemID = i.ItemID
                    WHERE c.NextCalibrationDue >= @Now
                    AND c.NextCalibrationDue <= @FutureDate";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Now", now);
                cmd.Parameters.AddWithValue("@FutureDate", now.AddDays(days));
                return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            });
        }

        public async Task<CalibrationRecord?> GetLatestCalibrationForItemAsync(int itemID)
        {
            if (itemID < 1)
                throw new ArgumentOutOfRangeException(nameof(itemID), "Item ID must be greater than 0.");

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                EnsureItemExists(conn, itemID);
                var sql = @"
                    SELECT c.*, i.ItemNumber, i.NameDescription as ItemName
                    FROM CalibrationRecords c
                    JOIN Items i ON c.ItemID = i.ItemID
                    WHERE c.ItemID = @ItemID
                    ORDER BY c.CalibrationDate DESC
                    LIMIT 1";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ItemID", itemID);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return MapCalibrationRecord(reader);
                }
                return null;
            });
        }

        public async Task<CalibrationRecord?> GetCalibrationRecordByIdAsync(int calibrationID)
        {
            if (calibrationID < 1)
                throw new ArgumentOutOfRangeException(nameof(calibrationID), "Calibration ID must be greater than 0.");

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT c.*, i.ItemNumber, i.NameDescription as ItemName
                    FROM CalibrationRecords c
                    JOIN Items i ON c.ItemID = i.ItemID
                    WHERE c.CalibrationID = @CalibrationID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CalibrationID", calibrationID);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return MapCalibrationRecord(reader);
                }
                return null;
            });
        }

        public async Task<int> CreateCalibrationRecordAsync(CalibrationRecord record)
        {
            if (record is null)
                throw new ArgumentNullException(nameof(record));
            if (record.ItemID < 1)
                throw new ArgumentOutOfRangeException(nameof(record.ItemID), "Item ID must be greater than 0.");

            NormalizeCalibrationRecordForSave(record);

            var id = await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                EnsureItemExists(conn, record.ItemID);

                var sql = @"
                    INSERT INTO CalibrationRecords 
                    (ItemID, CalibrationDate, NextCalibrationDue, CalibratedBy, 
                     CertificateNumber, Standard, Result, Cost, Notes, UserID, CreatedAt)
                    VALUES 
                    (@ItemID, @CalibrationDate, @NextCalibrationDue, @CalibratedBy, 
                     @CertificateNumber, @Standard, @Result, @Cost, @Notes, @UserID, @CreatedAt)";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ItemID", record.ItemID);
                cmd.Parameters.AddWithValue("@CalibrationDate", record.CalibrationDate);
                cmd.Parameters.AddWithValue("@NextCalibrationDue", record.NextCalibrationDue);
                cmd.Parameters.AddWithValue("@CalibratedBy", record.CalibratedBy);
                cmd.Parameters.AddWithValue("@CertificateNumber", record.CertificateNumber);
                cmd.Parameters.AddWithValue("@Standard", record.Standard);
                cmd.Parameters.AddWithValue("@Result", record.Result);
                cmd.Parameters.AddWithValue("@Cost", record.Cost);
                cmd.Parameters.AddWithValue("@Notes", record.Notes);
                cmd.Parameters.AddWithValue("@UserID", _userContext.CurrentUser?.UserID ?? 0);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                var insertedRows = cmd.ExecuteNonQuery();
                EnsureCalibrationCreateSucceeded(insertedRows);

                using var idCmd = new SqliteCommand("SELECT last_insert_rowid();", conn);
                var id = Convert.ToInt32(idCmd.ExecuteScalar());
                if (id < 1)
                    throw new InvalidOperationException("Unable to create calibration record.");

                return id;
            });
            NotifyChanged(DomainDataScope.Calibration | DomainDataScope.Reports, id);
            return id;
        }

        public async Task<bool> UpdateCalibrationRecordAsync(CalibrationRecord record)
        {
            if (record is null)
                throw new ArgumentNullException(nameof(record));
            if (record.CalibrationID < 1)
                throw new ArgumentOutOfRangeException(nameof(record.CalibrationID), "Calibration ID must be greater than 0.");
            if (record.ItemID < 1)
                throw new ArgumentOutOfRangeException(nameof(record.ItemID), "Item ID must be greater than 0.");

            NormalizeCalibrationRecordForSave(record);

            var updated = await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                EnsureCalibrationRecordExists(conn, record.CalibrationID);
                EnsureItemExists(conn, record.ItemID);

                var sql = @"
                    UPDATE CalibrationRecords 
                    SET ItemID = @ItemID,
                        CalibrationDate = @CalibrationDate,
                        NextCalibrationDue = @NextCalibrationDue,
                        CalibratedBy = @CalibratedBy,
                        CertificateNumber = @CertificateNumber,
                        Standard = @Standard,
                        Result = @Result,
                        Cost = @Cost,
                        Notes = @Notes
                    WHERE CalibrationID = @CalibrationID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CalibrationID", record.CalibrationID);
                cmd.Parameters.AddWithValue("@ItemID", record.ItemID);
                cmd.Parameters.AddWithValue("@CalibrationDate", record.CalibrationDate);
                cmd.Parameters.AddWithValue("@NextCalibrationDue", record.NextCalibrationDue);
                cmd.Parameters.AddWithValue("@CalibratedBy", record.CalibratedBy);
                cmd.Parameters.AddWithValue("@CertificateNumber", record.CertificateNumber);
                cmd.Parameters.AddWithValue("@Standard", record.Standard);
                cmd.Parameters.AddWithValue("@Result", record.Result);
                cmd.Parameters.AddWithValue("@Cost", record.Cost);
                cmd.Parameters.AddWithValue("@Notes", record.Notes);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("Calibration record not found.");

                return true;
            });
            NotifyChanged(DomainDataScope.Calibration | DomainDataScope.Reports, record.CalibrationID);
            return updated;
        }

        public async Task<bool> DeleteCalibrationRecordAsync(int calibrationID)
        {
            if (calibrationID < 1)
                throw new ArgumentOutOfRangeException(nameof(calibrationID), "Calibration ID must be greater than 0.");

            var deleted = await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                EnsureCalibrationRecordExists(conn, calibrationID);

                var sql = "DELETE FROM CalibrationRecords WHERE CalibrationID = @CalibrationID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CalibrationID", calibrationID);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("Calibration record not found.");

                return true;
            });
            NotifyChanged(DomainDataScope.Calibration | DomainDataScope.Reports, calibrationID);
            return deleted;
        }

        private CalibrationRecord MapCalibrationRecord(SqliteDataReader reader)
        {
            return new CalibrationRecord
            {
                CalibrationID = reader.GetInt32(reader.GetOrdinal("CalibrationID")),
                ItemID = reader.GetInt32(reader.GetOrdinal("ItemID")),
                ItemNumber = NormalizeCalibrationReadText(reader, "ItemNumber"),
                ItemName = NormalizeCalibrationReadText(reader, "ItemName"),
                CalibrationDate = reader.GetDateTime(reader.GetOrdinal("CalibrationDate")),
                NextCalibrationDue = reader.GetDateTime(reader.GetOrdinal("NextCalibrationDue")),
                CalibratedBy = NormalizeCalibrationReadText(reader, "CalibratedBy"),
                CertificateNumber = NormalizeCalibrationReadText(reader, "CertificateNumber"),
                Standard = NormalizeCalibrationReadText(reader, "Standard"),
                Result = NormalizeCalibrationReadText(reader, "Result"),
                Cost = reader.GetDecimal(reader.GetOrdinal("Cost")),
                Notes = NormalizeCalibrationReadText(reader, "Notes"),
                UserID = reader.IsDBNull(reader.GetOrdinal("UserID")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserID")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        private static string NormalizeCalibrationReadText(SqliteDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal).Trim();
        }

        private static void NormalizeCalibrationRecordForSave(CalibrationRecord record)
        {
            record.CalibratedBy = NormalizeOptionalText(record.CalibratedBy);
            record.CertificateNumber = NormalizeOptionalText(record.CertificateNumber);
            record.Standard = NormalizeOptionalText(record.Standard);
            record.Result = NormalizeOptionalText(record.Result);
            record.Notes = NormalizeOptionalText(record.Notes);
        }

        private static string NormalizeOptionalText(string? value) => value?.Trim() ?? string.Empty;

        private static void EnsureCalibrationCreateSucceeded(int affectedRows)
        {
            if (affectedRows == 0)
                throw new InvalidOperationException("Unable to create calibration record.");
        }

        private static void EnsureItemExists(SqliteConnection conn, int itemID)
        {
            using var itemCmd = new SqliteCommand("SELECT COUNT(*) FROM Items WHERE ItemID = @ItemID", conn);
            itemCmd.Parameters.AddWithValue("@ItemID", itemID);
            var itemCount = Convert.ToInt32(itemCmd.ExecuteScalar() ?? 0);
            if (itemCount < 1)
                throw new InvalidOperationException("Item not found.");
        }

        private static void EnsureCalibrationRecordExists(SqliteConnection conn, int calibrationID)
        {
            using var calibrationCmd = new SqliteCommand("SELECT COUNT(*) FROM CalibrationRecords WHERE CalibrationID = @CalibrationID", conn);
            calibrationCmd.Parameters.AddWithValue("@CalibrationID", calibrationID);
            var calibrationCount = Convert.ToInt32(calibrationCmd.ExecuteScalar() ?? 0);
            if (calibrationCount < 1)
                throw new InvalidOperationException("Calibration record not found.");
        }

        static void NotifyChanged(DomainDataScope scope, int? entityId = null)
        {
            WeakReferenceMessenger.Default.Send(new DomainDataChangedMessage(scope, entityId));
        }
    }
}
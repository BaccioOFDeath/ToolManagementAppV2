using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Services.Calibration
{
    /// <summary>
    /// Service for managing equipment calibration records including scheduling and tracking calibration status.
    /// </summary>
    public class CalibrationService
    {
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
                    LEFT JOIN Items i ON c.ItemID = i.ItemID
                    ORDER BY c.CalibrationDate DESC";
                using var cmd = new SqliteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(MapCalibrationRecord(reader));
                }
                return records;
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
                var sql = @"
                    SELECT c.*, i.ItemNumber, i.NameDescription as ItemName
                    FROM CalibrationRecords c
                    LEFT JOIN Items i ON c.ItemID = i.ItemID
                    WHERE c.ItemID = @ItemID
                    ORDER BY c.CalibrationDate DESC";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ItemID", itemID);
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
                    LEFT JOIN Items i ON c.ItemID = i.ItemID
                    WHERE c.NextCalibrationDue < @Now
                    ORDER BY c.NextCalibrationDue ASC";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(MapCalibrationRecord(reader));
                }
                return records;
            });
        }

        public async Task<List<CalibrationRecord>> GetUpcomingCalibrationAsync(int days = 30)
        {
            return await Task.Run(() =>
            {
                var records = new List<CalibrationRecord>();
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT c.*, i.ItemNumber, i.NameDescription as ItemName
                    FROM CalibrationRecords c
                    LEFT JOIN Items i ON c.ItemID = i.ItemID
                    WHERE c.NextCalibrationDue >= @Now 
                    AND c.NextCalibrationDue <= @FutureDate
                    ORDER BY c.NextCalibrationDue ASC";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                cmd.Parameters.AddWithValue("@FutureDate", DateTime.Now.AddDays(days));
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(MapCalibrationRecord(reader));
                }
                return records;
            });
        }

        public async Task<CalibrationRecord?> GetLatestCalibrationForItemAsync(int itemID)
        {
            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT c.*, i.ItemNumber, i.NameDescription as ItemName
                    FROM CalibrationRecords c
                    LEFT JOIN Items i ON c.ItemID = i.ItemID
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
            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT c.*, i.ItemNumber, i.NameDescription as ItemName
                    FROM CalibrationRecords c
                    LEFT JOIN Items i ON c.ItemID = i.ItemID
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
            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    INSERT INTO CalibrationRecords 
                    (ItemID, CalibrationDate, NextCalibrationDue, CalibratedBy, 
                     CertificateNumber, Standard, Result, Cost, Notes, UserID, CreatedAt)
                    VALUES 
                    (@ItemID, @CalibrationDate, @NextCalibrationDue, @CalibratedBy, 
                     @CertificateNumber, @Standard, @Result, @Cost, @Notes, @UserID, @CreatedAt);
                    SELECT last_insert_rowid();";
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
                var id = Convert.ToInt32(cmd.ExecuteScalar());
                return id;
            });
        }

        public async Task<bool> UpdateCalibrationRecordAsync(CalibrationRecord record)
        {
            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
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
                return cmd.ExecuteNonQuery() > 0;
            });
        }

        public async Task<bool> DeleteCalibrationRecordAsync(int calibrationID)
        {
            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = "DELETE FROM CalibrationRecords WHERE CalibrationID = @CalibrationID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CalibrationID", calibrationID);
                return cmd.ExecuteNonQuery() > 0;
            });
        }

        private CalibrationRecord MapCalibrationRecord(SqliteDataReader reader)
        {
            return new CalibrationRecord
            {
                CalibrationID = reader.GetInt32(reader.GetOrdinal("CalibrationID")),
                ItemID = reader.GetInt32(reader.GetOrdinal("ItemID")),
                ItemNumber = reader.IsDBNull(reader.GetOrdinal("ItemNumber")) ? "" : reader.GetString(reader.GetOrdinal("ItemNumber")),
                ItemName = reader.IsDBNull(reader.GetOrdinal("ItemName")) ? "" : reader.GetString(reader.GetOrdinal("ItemName")),
                CalibrationDate = reader.GetDateTime(reader.GetOrdinal("CalibrationDate")),
                NextCalibrationDue = reader.GetDateTime(reader.GetOrdinal("NextCalibrationDue")),
                CalibratedBy = reader.IsDBNull(reader.GetOrdinal("CalibratedBy")) ? "" : reader.GetString(reader.GetOrdinal("CalibratedBy")),
                CertificateNumber = reader.IsDBNull(reader.GetOrdinal("CertificateNumber")) ? "" : reader.GetString(reader.GetOrdinal("CertificateNumber")),
                Standard = reader.IsDBNull(reader.GetOrdinal("Standard")) ? "" : reader.GetString(reader.GetOrdinal("Standard")),
                Result = reader.IsDBNull(reader.GetOrdinal("Result")) ? "" : reader.GetString(reader.GetOrdinal("Result")),
                Cost = reader.GetDecimal(reader.GetOrdinal("Cost")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? "" : reader.GetString(reader.GetOrdinal("Notes")),
                UserID = reader.IsDBNull(reader.GetOrdinal("UserID")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserID")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}

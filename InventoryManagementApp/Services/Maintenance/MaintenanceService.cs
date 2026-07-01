using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace InventoryManagementApp.Services.Maintenance
{
    /// <summary>
    /// Service for managing equipment maintenance records including scheduling, tracking, and reporting.
    /// </summary>
    public class MaintenanceService
    {
        private const int MaxMaintenanceListCount = 500;

        private readonly DatabaseService _databaseService;
        private readonly IUserContext _userContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaintenanceService"/> class.
        /// </summary>
        /// <param name="databaseService">Database service for data access.</param>
        /// <param name="userContext">User context for tracking current user.</param>
        public MaintenanceService(DatabaseService databaseService, IUserContext userContext)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        /// <summary>
        /// Retrieves all maintenance records from the database, ordered by scheduled date descending.
        /// </summary>
        /// <returns>A list of all maintenance records.</returns>
        public async Task<List<MaintenanceRecord>> GetAllMaintenanceRecordsAsync()
        {
            return await Task.Run(() =>
            {
                var records = new List<MaintenanceRecord>();
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT m.*, i.ItemNumber, i.NameDescription as ItemName
                    FROM MaintenanceRecords m
                    JOIN Items i ON m.ItemID = i.ItemID
                    ORDER BY m.ScheduledDate DESC
                    LIMIT @MaintenanceListLimit";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@MaintenanceListLimit", MaxMaintenanceListCount);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(MapMaintenanceRecord(reader));
                }
                return records;
            });
        }

        public async Task<int> CountMaintenanceRecordsAsync()
        {
            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT COUNT(m.MaintenanceID)
                    FROM MaintenanceRecords m
                    JOIN Items i ON m.ItemID = i.ItemID";
                using var cmd = new SqliteCommand(sql, conn);
                return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            });
        }

        /// <summary>
        /// Retrieves all maintenance records for a specific item.
        /// </summary>
        /// <param name="itemID">The ID of the item.</param>
        /// <returns>A list of maintenance records for the specified item.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if itemID is less than 1.</exception>
        public async Task<List<MaintenanceRecord>> GetMaintenanceRecordsByItemAsync(int itemID)
        {
            if (itemID < 1)
                throw new ArgumentOutOfRangeException(nameof(itemID), "Item ID must be greater than 0.");
            return await Task.Run(() =>
            {
                var records = new List<MaintenanceRecord>();
                using var conn = _databaseService.CreateConnection();
                EnsureItemExists(conn, itemID);
                var sql = @"
                    SELECT m.*, i.ItemNumber, i.NameDescription as ItemName
                    FROM MaintenanceRecords m
                    JOIN Items i ON m.ItemID = i.ItemID
                    WHERE m.ItemID = @ItemID
                    ORDER BY m.ScheduledDate DESC
                    LIMIT @MaintenanceListLimit";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ItemID", itemID);
                cmd.Parameters.AddWithValue("@MaintenanceListLimit", MaxMaintenanceListCount);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(MapMaintenanceRecord(reader));
                }
                return records;
            });
        }

        /// <summary>
        /// Retrieves all overdue maintenance records (scheduled but not completed before the scheduled date).
        /// </summary>
        /// <returns>A list of overdue maintenance records ordered by scheduled date ascending.</returns>
        public async Task<List<MaintenanceRecord>> GetOverdueMaintenanceAsync()
        {
            return await Task.Run(() =>
            {
                var records = new List<MaintenanceRecord>();
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT m.*, i.ItemNumber, i.NameDescription as ItemName
                    FROM MaintenanceRecords m
                    JOIN Items i ON m.ItemID = i.ItemID
                    WHERE m.Status = 'Scheduled' AND m.ScheduledDate < @Now
                    ORDER BY m.ScheduledDate ASC
                    LIMIT @MaintenanceListLimit";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                cmd.Parameters.AddWithValue("@MaintenanceListLimit", MaxMaintenanceListCount);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(MapMaintenanceRecord(reader));
                }
                return records;
            });
        }

        public async Task<int> CountOverdueMaintenanceAsync()
        {
            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT COUNT(m.MaintenanceID)
                    FROM MaintenanceRecords m
                    JOIN Items i ON m.ItemID = i.ItemID
                    WHERE m.Status = 'Scheduled' AND m.ScheduledDate < @Now";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            });
        }

        public async Task<List<MaintenanceRecord>> GetUpcomingMaintenanceAsync(int days = 30)
        {
            if (days < 0)
                throw new ArgumentOutOfRangeException(nameof(days), "Days must be greater than or equal to 0.");

            return await Task.Run(() =>
            {
                var records = new List<MaintenanceRecord>();
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT m.*, i.ItemNumber, i.NameDescription as ItemName
                    FROM MaintenanceRecords m
                    JOIN Items i ON m.ItemID = i.ItemID
                    WHERE m.Status = 'Scheduled' 
                    AND m.ScheduledDate >= @Now 
                    AND m.ScheduledDate <= @FutureDate
                    ORDER BY m.ScheduledDate ASC
                    LIMIT @MaintenanceListLimit";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                cmd.Parameters.AddWithValue("@FutureDate", DateTime.Now.AddDays(days));
                cmd.Parameters.AddWithValue("@MaintenanceListLimit", MaxMaintenanceListCount);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(MapMaintenanceRecord(reader));
                }
                return records;
            });
        }

        public async Task<int> CountUpcomingMaintenanceAsync(int days = 30)
        {
            if (days < 0)
                throw new ArgumentOutOfRangeException(nameof(days), "Days must be greater than or equal to 0.");

            return await Task.Run(() =>
            {
                var now = DateTime.Now;
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT COUNT(m.MaintenanceID)
                    FROM MaintenanceRecords m
                    JOIN Items i ON m.ItemID = i.ItemID
                    WHERE m.Status = 'Scheduled'
                    AND m.ScheduledDate >= @Now
                    AND m.ScheduledDate <= @FutureDate";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Now", now);
                cmd.Parameters.AddWithValue("@FutureDate", now.AddDays(days));
                return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            });
        }

        public async Task<MaintenanceRecord?> GetMaintenanceRecordByIdAsync(int maintenanceID)
        {
            if (maintenanceID < 1)
                throw new ArgumentOutOfRangeException(nameof(maintenanceID), "Maintenance ID must be greater than 0.");

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT m.*, i.ItemNumber, i.NameDescription as ItemName
                    FROM MaintenanceRecords m
                    JOIN Items i ON m.ItemID = i.ItemID
                    WHERE m.MaintenanceID = @MaintenanceID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@MaintenanceID", maintenanceID);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return MapMaintenanceRecord(reader);
                }
                return null;
            });
        }

        public async Task<int> CreateMaintenanceRecordAsync(MaintenanceRecord record)
        {
            if (record is null)
                throw new ArgumentNullException(nameof(record));
            if (record.ItemID < 1)
                throw new ArgumentOutOfRangeException(nameof(record.ItemID), "Item ID must be greater than 0.");

            NormalizeMaintenanceRecordForSave(record);

            var id = await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                EnsureItemExists(conn, record.ItemID);

                var sql = @"
                    INSERT INTO MaintenanceRecords 
                    (ItemID, ScheduledDate, CompletedDate, MaintenanceType, Description, 
                     PerformedBy, Cost, Status, Notes, UserID, CreatedAt)
                    VALUES 
                    (@ItemID, @ScheduledDate, @CompletedDate, @MaintenanceType, @Description, 
                     @PerformedBy, @Cost, @Status, @Notes, @UserID, @CreatedAt)";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ItemID", record.ItemID);
                cmd.Parameters.AddWithValue("@ScheduledDate", record.ScheduledDate);
                cmd.Parameters.AddWithValue("@CompletedDate", record.CompletedDate.HasValue ? (object)record.CompletedDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@MaintenanceType", record.MaintenanceType);
                cmd.Parameters.AddWithValue("@Description", record.Description);
                cmd.Parameters.AddWithValue("@PerformedBy", record.PerformedBy);
                cmd.Parameters.AddWithValue("@Cost", record.Cost);
                cmd.Parameters.AddWithValue("@Status", record.Status);
                cmd.Parameters.AddWithValue("@Notes", record.Notes);
                cmd.Parameters.AddWithValue("@UserID", _userContext.CurrentUser?.UserID ?? 0);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                var insertedRows = cmd.ExecuteNonQuery();
                EnsureMaintenanceCreateSucceeded(insertedRows);

                using var idCmd = new SqliteCommand("SELECT last_insert_rowid();", conn);
                var id = Convert.ToInt32(idCmd.ExecuteScalar());
                if (id < 1)
                    throw new InvalidOperationException("Unable to create maintenance record.");

                return id;
            });
            NotifyChanged(DomainDataScope.Maintenance | DomainDataScope.Reports, id);
            return id;
        }

        public async Task<bool> UpdateMaintenanceRecordAsync(MaintenanceRecord record)
        {
            if (record is null)
                throw new ArgumentNullException(nameof(record));
            if (record.MaintenanceID < 1)
                throw new ArgumentOutOfRangeException(nameof(record.MaintenanceID), "Maintenance ID must be greater than 0.");
            if (record.ItemID < 1)
                throw new ArgumentOutOfRangeException(nameof(record.ItemID), "Item ID must be greater than 0.");

            NormalizeMaintenanceRecordForSave(record);

            var updated = await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                EnsureMaintenanceRecordExists(conn, record.MaintenanceID);
                EnsureItemExists(conn, record.ItemID);

                var sql = @"
                    UPDATE MaintenanceRecords 
                    SET ItemID = @ItemID,
                        ScheduledDate = @ScheduledDate,
                        CompletedDate = @CompletedDate,
                        MaintenanceType = @MaintenanceType,
                        Description = @Description,
                        PerformedBy = @PerformedBy,
                        Cost = @Cost,
                        Status = @Status,
                        Notes = @Notes
                    WHERE MaintenanceID = @MaintenanceID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@MaintenanceID", record.MaintenanceID);
                cmd.Parameters.AddWithValue("@ItemID", record.ItemID);
                cmd.Parameters.AddWithValue("@ScheduledDate", record.ScheduledDate);
                cmd.Parameters.AddWithValue("@CompletedDate", record.CompletedDate.HasValue ? (object)record.CompletedDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@MaintenanceType", record.MaintenanceType);
                cmd.Parameters.AddWithValue("@Description", record.Description);
                cmd.Parameters.AddWithValue("@PerformedBy", record.PerformedBy);
                cmd.Parameters.AddWithValue("@Cost", record.Cost);
                cmd.Parameters.AddWithValue("@Status", record.Status);
                cmd.Parameters.AddWithValue("@Notes", record.Notes);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("Maintenance record not found.");

                return true;
            });
            NotifyChanged(DomainDataScope.Maintenance | DomainDataScope.Reports, record.MaintenanceID);
            return updated;
        }

        public async Task<bool> CompleteMaintenanceAsync(int maintenanceID, string performedBy, string notes = "")
        {
            if (maintenanceID < 1)
                throw new ArgumentOutOfRangeException(nameof(maintenanceID), "Maintenance ID must be greater than 0.");

            var normalizedPerformedBy = NormalizeOptionalText(performedBy);
            var normalizedNotes = NormalizeOptionalText(notes);

            var completed = await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                EnsureMaintenanceRecordExists(conn, maintenanceID);

                var sql = @"
                    UPDATE MaintenanceRecords 
                    SET Status = 'Completed',
                        CompletedDate = @CompletedDate,
                        PerformedBy = @PerformedBy,
                        Notes = @Notes
                    WHERE MaintenanceID = @MaintenanceID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@MaintenanceID", maintenanceID);
                cmd.Parameters.AddWithValue("@CompletedDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@PerformedBy", normalizedPerformedBy);
                cmd.Parameters.AddWithValue("@Notes", normalizedNotes);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("Maintenance record not found.");

                return true;
            });
            NotifyChanged(DomainDataScope.Maintenance | DomainDataScope.Reports, maintenanceID);
            return completed;
        }

        public async Task<bool> DeleteMaintenanceRecordAsync(int maintenanceID)
        {
            if (maintenanceID < 1)
                throw new ArgumentOutOfRangeException(nameof(maintenanceID), "Maintenance ID must be greater than 0.");

            var deleted = await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                EnsureMaintenanceRecordExists(conn, maintenanceID);

                var sql = "DELETE FROM MaintenanceRecords WHERE MaintenanceID = @MaintenanceID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@MaintenanceID", maintenanceID);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("Maintenance record not found.");

                return true;
            });
            NotifyChanged(DomainDataScope.Maintenance | DomainDataScope.Reports, maintenanceID);
            return deleted;
        }

        private MaintenanceRecord MapMaintenanceRecord(SqliteDataReader reader)
        {
            return new MaintenanceRecord
            {
                MaintenanceID = reader.GetInt32(reader.GetOrdinal("MaintenanceID")),
                ItemID = reader.GetInt32(reader.GetOrdinal("ItemID")),
                ItemNumber = reader.IsDBNull(reader.GetOrdinal("ItemNumber")) ? "" : reader.GetString(reader.GetOrdinal("ItemNumber")),
                ItemName = reader.IsDBNull(reader.GetOrdinal("ItemName")) ? "" : reader.GetString(reader.GetOrdinal("ItemName")),
                ScheduledDate = reader.GetDateTime(reader.GetOrdinal("ScheduledDate")),
                CompletedDate = reader.IsDBNull(reader.GetOrdinal("CompletedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("CompletedDate")),
                MaintenanceType = reader.GetString(reader.GetOrdinal("MaintenanceType")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString(reader.GetOrdinal("Description")),
                PerformedBy = reader.IsDBNull(reader.GetOrdinal("PerformedBy")) ? "" : reader.GetString(reader.GetOrdinal("PerformedBy")),
                Cost = reader.GetDecimal(reader.GetOrdinal("Cost")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? "" : reader.GetString(reader.GetOrdinal("Notes")),
                UserID = reader.IsDBNull(reader.GetOrdinal("UserID")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserID")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        private static void NormalizeMaintenanceRecordForSave(MaintenanceRecord record)
        {
            record.MaintenanceType = NormalizeOptionalText(record.MaintenanceType);
            record.Description = NormalizeOptionalText(record.Description);
            record.PerformedBy = NormalizeOptionalText(record.PerformedBy);
            record.Status = NormalizeMaintenanceStatus(record.Status);
            record.Notes = NormalizeOptionalText(record.Notes);
        }

        private static string NormalizeMaintenanceStatus(string? status)
        {
            var normalizedStatus = NormalizeOptionalText(status);
            return string.IsNullOrEmpty(normalizedStatus) ? "Scheduled" : normalizedStatus;
        }

        private static string NormalizeOptionalText(string? value) => value?.Trim() ?? string.Empty;

        private static void EnsureMaintenanceCreateSucceeded(int affectedRows)
        {
            if (affectedRows == 0)
                throw new InvalidOperationException("Unable to create maintenance record.");
        }

        private static void EnsureItemExists(SqliteConnection conn, int itemID)
        {
            using var itemCmd = new SqliteCommand("SELECT COUNT(*) FROM Items WHERE ItemID = @ItemID", conn);
            itemCmd.Parameters.AddWithValue("@ItemID", itemID);
            var itemCount = Convert.ToInt32(itemCmd.ExecuteScalar() ?? 0);
            if (itemCount < 1)
                throw new InvalidOperationException("Item not found.");
        }

        private static void EnsureMaintenanceRecordExists(SqliteConnection conn, int maintenanceID)
        {
            using var maintenanceCmd = new SqliteCommand("SELECT COUNT(*) FROM MaintenanceRecords WHERE MaintenanceID = @MaintenanceID", conn);
            maintenanceCmd.Parameters.AddWithValue("@MaintenanceID", maintenanceID);
            var maintenanceCount = Convert.ToInt32(maintenanceCmd.ExecuteScalar() ?? 0);
            if (maintenanceCount < 1)
                throw new InvalidOperationException("Maintenance record not found.");
        }

        static void NotifyChanged(DomainDataScope scope, int? entityId = null)
        {
            WeakReferenceMessenger.Default.Send(new DomainDataChangedMessage(scope, entityId));
        }
    }
}
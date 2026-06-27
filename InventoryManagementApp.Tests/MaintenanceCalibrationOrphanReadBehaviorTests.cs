using System;
using System.Threading.Tasks;
using Xunit;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Calibration;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Maintenance;
using Microsoft.Data.Sqlite;
using Moq;

namespace InventoryManagementApp.Tests
{
    public class MaintenanceCalibrationOrphanReadBehaviorTests
    {
        [Fact]
        public async Task MaintenanceReadModels_WithLegacyMissingItemReference_ShouldReturnValidRowsAndHideOrphanRecords()
        {
            using var databaseService = CreateDatabaseService("maintenance_orphan_reads");
            SeedRequiredData(databaseService);
            var maintenanceService = new MaintenanceService(databaseService, CreateUserContext());
            var now = DateTime.Now;
            var validOverdueId = InsertLegacyMaintenanceRecord(databaseService, 1, now.AddDays(-3), "Scheduled");
            var validUpcomingId = InsertLegacyMaintenanceRecord(databaseService, 1, now.AddDays(3), "Scheduled");
            var overdueOrphanId = InsertLegacyMaintenanceRecord(databaseService, 999, now.AddDays(-7), "Scheduled");
            var upcomingOrphanId = InsertLegacyMaintenanceRecord(databaseService, 999, now.AddDays(7), "Scheduled");

            var allRecords = await maintenanceService.GetAllMaintenanceRecordsAsync();
            var overdueRecords = await maintenanceService.GetOverdueMaintenanceAsync();
            var upcomingRecords = await maintenanceService.GetUpcomingMaintenanceAsync(30);
            var validById = await maintenanceService.GetMaintenanceRecordByIdAsync(validOverdueId);
            var overdueById = await maintenanceService.GetMaintenanceRecordByIdAsync(overdueOrphanId);
            var upcomingById = await maintenanceService.GetMaintenanceRecordByIdAsync(upcomingOrphanId);

            Assert.Contains(allRecords, record => record.MaintenanceID == validOverdueId);
            Assert.Contains(allRecords, record => record.MaintenanceID == validUpcomingId);
            Assert.Contains(overdueRecords, record => record.MaintenanceID == validOverdueId);
            Assert.Contains(upcomingRecords, record => record.MaintenanceID == validUpcomingId);
            Assert.NotNull(validById);
            Assert.Equal("ITEM-001", validById!.ItemNumber);
            Assert.DoesNotContain(allRecords, record => record.ItemID == 999);
            Assert.DoesNotContain(overdueRecords, record => record.ItemID == 999);
            Assert.DoesNotContain(upcomingRecords, record => record.ItemID == 999);
            Assert.Null(overdueById);
            Assert.Null(upcomingById);
        }

        [Fact]
        public async Task CalibrationReadModels_WithLegacyMissingItemReference_ShouldReturnValidRowsAndHideOrphanRecords()
        {
            using var databaseService = CreateDatabaseService("calibration_orphan_reads");
            SeedRequiredData(databaseService);
            var calibrationService = new CalibrationService(databaseService, CreateUserContext());
            var now = DateTime.Now;
            var validOverdueId = InsertLegacyCalibrationRecord(databaseService, 1, now.AddYears(-2), now.AddDays(-3));
            var validUpcomingId = InsertLegacyCalibrationRecord(databaseService, 1, now.AddDays(-7), now.AddDays(3));
            var overdueOrphanId = InsertLegacyCalibrationRecord(databaseService, 999, now.AddYears(-2), now.AddDays(-7));
            var upcomingOrphanId = InsertLegacyCalibrationRecord(databaseService, 999, now.AddDays(-7), now.AddDays(7));

            var allRecords = await calibrationService.GetAllCalibrationRecordsAsync();
            var overdueRecords = await calibrationService.GetOverdueCalibrationAsync();
            var upcomingRecords = await calibrationService.GetUpcomingCalibrationAsync(30);
            var validById = await calibrationService.GetCalibrationRecordByIdAsync(validOverdueId);
            var overdueById = await calibrationService.GetCalibrationRecordByIdAsync(overdueOrphanId);
            var upcomingById = await calibrationService.GetCalibrationRecordByIdAsync(upcomingOrphanId);

            Assert.Contains(allRecords, record => record.CalibrationID == validOverdueId);
            Assert.Contains(allRecords, record => record.CalibrationID == validUpcomingId);
            Assert.Contains(overdueRecords, record => record.CalibrationID == validOverdueId);
            Assert.Contains(upcomingRecords, record => record.CalibrationID == validUpcomingId);
            Assert.NotNull(validById);
            Assert.Equal("ITEM-001", validById!.ItemNumber);
            Assert.DoesNotContain(allRecords, record => record.ItemID == 999);
            Assert.DoesNotContain(overdueRecords, record => record.ItemID == 999);
            Assert.DoesNotContain(upcomingRecords, record => record.ItemID == 999);
            Assert.Null(overdueById);
            Assert.Null(upcomingById);
        }

        private static DatabaseService CreateDatabaseService(string prefix)
        {
            return new DatabaseService($"test_{prefix}_{Guid.NewGuid()}.db");
        }

        private static IUserContext CreateUserContext()
        {
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.CurrentUser).Returns(new User { UserID = 1, UserName = "TestUser" });
            return userContextMock.Object;
        }

        private static void SeedRequiredData(DatabaseService databaseService)
        {
            using var conn = databaseService.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Users (UserID, UserName, IsAdmin, IsActive) VALUES (1, 'TestUser', 0, 1);
                INSERT INTO Items (ItemID, ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, IsPowered) VALUES (1, 'ITEM-001', 'Seed Item', 1, 0, 0, 0);";
            cmd.ExecuteNonQuery();
        }

        private static int InsertLegacyMaintenanceRecord(DatabaseService databaseService, int itemID, DateTime scheduledDate, string status)
        {
            var builder = CreateLegacyConnectionStringBuilder(databaseService);

            using var conn = new SqliteConnection(builder.ToString());
            using var cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = @"
                INSERT INTO MaintenanceRecords
                    (ItemID, ScheduledDate, MaintenanceType, Status, Cost, UserID, CreatedAt)
                VALUES
                    (@ItemID, @ScheduledDate, @MaintenanceType, @Status, @Cost, @UserID, @CreatedAt);
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@ItemID", itemID);
            cmd.Parameters.AddWithValue("@ScheduledDate", scheduledDate);
            cmd.Parameters.AddWithValue("@MaintenanceType", "Legacy orphan");
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@Cost", 0m);
            cmd.Parameters.AddWithValue("@UserID", 1);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static int InsertLegacyCalibrationRecord(DatabaseService databaseService, int itemID, DateTime calibrationDate, DateTime nextCalibrationDue)
        {
            var builder = CreateLegacyConnectionStringBuilder(databaseService);

            using var conn = new SqliteConnection(builder.ToString());
            using var cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = @"
                INSERT INTO CalibrationRecords
                    (ItemID, CalibrationDate, NextCalibrationDue, Result, Cost, UserID, CreatedAt)
                VALUES
                    (@ItemID, @CalibrationDate, @NextCalibrationDue, @Result, @Cost, @UserID, @CreatedAt);
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@ItemID", itemID);
            cmd.Parameters.AddWithValue("@CalibrationDate", calibrationDate);
            cmd.Parameters.AddWithValue("@NextCalibrationDue", nextCalibrationDue);
            cmd.Parameters.AddWithValue("@Result", "Pass");
            cmd.Parameters.AddWithValue("@Cost", 0m);
            cmd.Parameters.AddWithValue("@UserID", 1);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static SqliteConnectionStringBuilder CreateLegacyConnectionStringBuilder(DatabaseService databaseService)
        {
            return new SqliteConnectionStringBuilder(databaseService.ConnectionString)
            {
                ForeignKeys = false
            };
        }
    }
}
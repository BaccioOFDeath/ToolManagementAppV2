using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using InventoryManagementApp.Services.Maintenance;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Interfaces;
using Moq;

namespace InventoryManagementApp.Tests
{
    public class MaintenanceServiceTests : IDisposable
    {
        private readonly DatabaseService _databaseService;
        private readonly MaintenanceService _maintenanceService;
        private readonly Mock<IUserContext> _userContextMock;

        public MaintenanceServiceTests()
        {
            var testDbPath = $"test_maintenance_{Guid.NewGuid()}.db";
            _databaseService = new DatabaseService(testDbPath);
            SeedRequiredData();
            _userContextMock = new Mock<IUserContext>();
            _userContextMock.Setup(x => x.CurrentUser).Returns(new User { UserID = 1, UserName = "TestUser" });
            _maintenanceService = new MaintenanceService(_databaseService, _userContextMock.Object);
        }

        public void Dispose()
        {
            _databaseService?.Dispose();
        }

        private void SeedRequiredData()
        {
            using var conn = _databaseService.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Users (UserID, UserName, IsAdmin, IsActive) VALUES (1, 'TestUser', 0, 1);
                INSERT INTO Items (ItemID, ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, IsPowered) VALUES (1, 'ITEM-001', 'Seed Item', 1, 0, 0, 0);";
            cmd.ExecuteNonQuery();
        }

        [Fact]
        public async Task CreateMaintenanceRecord_ShouldSucceed()
        {
            var record = new MaintenanceRecord
            {
                ItemID = 1,
                ScheduledDate = DateTime.Now.AddDays(7),
                MaintenanceType = "Routine",
                Description = "Test maintenance",
                Status = "Scheduled",
                Cost = 100.00m
            };

            var id = await _maintenanceService.CreateMaintenanceRecordAsync(record);

            Assert.True(id > 0);
        }

        [Fact]
        public async Task GetAllMaintenanceRecords_ShouldReturnList()
        {
            var record = new MaintenanceRecord
            {
                ItemID = 1,
                ScheduledDate = DateTime.Now.AddDays(7),
                MaintenanceType = "Routine",
                Status = "Scheduled"
            };
            await _maintenanceService.CreateMaintenanceRecordAsync(record);

            var records = await _maintenanceService.GetAllMaintenanceRecordsAsync();

            Assert.NotEmpty(records);
        }

        [Fact]
        public async Task GetOverdueMaintenance_ShouldReturnOverdueRecords()
        {
            var overdueRecord = new MaintenanceRecord
            {
                ItemID = 1,
                ScheduledDate = DateTime.Now.AddDays(-7),
                MaintenanceType = "Overdue Test",
                Status = "Scheduled"
            };
            await _maintenanceService.CreateMaintenanceRecordAsync(overdueRecord);

            var overdueRecords = await _maintenanceService.GetOverdueMaintenanceAsync();

            Assert.NotEmpty(overdueRecords);
        }

        [Fact]
        public async Task UpdateMaintenanceRecord_ShouldSucceed()
        {
            var record = new MaintenanceRecord
            {
                ItemID = 1,
                ScheduledDate = DateTime.Now.AddDays(7),
                MaintenanceType = "Routine",
                Status = "Scheduled"
            };
            var id = await _maintenanceService.CreateMaintenanceRecordAsync(record);
            record.MaintenanceID = id;
            record.MaintenanceType = "Updated Type";

            var result = await _maintenanceService.UpdateMaintenanceRecordAsync(record);

            Assert.True(result);
        }

        [Fact]
        public async Task CompleteMaintenanceAsync_ShouldMarkAsCompleted()
        {
            var record = new MaintenanceRecord
            {
                ItemID = 1,
                ScheduledDate = DateTime.Now,
                MaintenanceType = "Routine",
                Status = "Scheduled"
            };
            var id = await _maintenanceService.CreateMaintenanceRecordAsync(record);

            var result = await _maintenanceService.CompleteMaintenanceAsync(id, "John Doe", "Completed successfully");

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteMaintenanceRecord_ShouldSucceed()
        {
            var record = new MaintenanceRecord
            {
                ItemID = 1,
                ScheduledDate = DateTime.Now.AddDays(7),
                MaintenanceType = "Routine",
                Status = "Scheduled"
            };
            var id = await _maintenanceService.CreateMaintenanceRecordAsync(record);

            var result = await _maintenanceService.DeleteMaintenanceRecordAsync(id);

            Assert.True(result);
        }
    }
}

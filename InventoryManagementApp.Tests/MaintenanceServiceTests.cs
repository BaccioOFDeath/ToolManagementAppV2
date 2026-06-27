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
        public async Task CreateMaintenanceRecord_WithNullRecord_ShouldThrow()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => _maintenanceService.CreateMaintenanceRecordAsync(null));

            Assert.Equal("record", ex.ParamName);
        }

        [Fact]
        public async Task CreateMaintenanceRecord_WithMissingItem_ShouldThrow()
        {
            var record = new MaintenanceRecord
            {
                ItemID = 999,
                ScheduledDate = DateTime.Now.AddDays(7),
                MaintenanceType = "Routine",
                Description = "Missing item maintenance",
                Status = "Scheduled",
                Cost = 100.00m
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _maintenanceService.CreateMaintenanceRecordAsync(record));

            Assert.Equal("Item not found.", ex.Message);
        }

        [Fact]
        public async Task GetMaintenanceRecordsByItem_WithMissingItem_ShouldThrow()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _maintenanceService.GetMaintenanceRecordsByItemAsync(999));

            Assert.Equal("Item not found.", ex.Message);
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
        public async Task GetUpcomingMaintenance_WithNegativeDays_ShouldThrow()
        {
            var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _maintenanceService.GetUpcomingMaintenanceAsync(-1));

            Assert.Equal("days", ex.ParamName);
            Assert.Contains("Days must be greater than or equal to 0.", ex.Message);
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
        public async Task UpdateMaintenanceRecord_WithNullRecord_ShouldThrow()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => _maintenanceService.UpdateMaintenanceRecordAsync(null));

            Assert.Equal("record", ex.ParamName);
        }

        [Fact]
        public async Task UpdateMaintenanceRecord_WithMissingItem_ShouldThrow()
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
            record.ItemID = 999;

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _maintenanceService.UpdateMaintenanceRecordAsync(record));

            Assert.Equal("Item not found.", ex.Message);
        }

        [Fact]
        public async Task UpdateMaintenanceRecord_WithMissingRecord_ShouldThrow()
        {
            var record = new MaintenanceRecord
            {
                MaintenanceID = 999,
                ItemID = 1,
                ScheduledDate = DateTime.Now.AddDays(7),
                MaintenanceType = "Routine",
                Status = "Scheduled"
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _maintenanceService.UpdateMaintenanceRecordAsync(record));

            Assert.Equal("Maintenance record not found.", ex.Message);
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
        public async Task CompleteMaintenanceAsync_WithMissingRecord_ShouldThrow()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _maintenanceService.CompleteMaintenanceAsync(999, "John Doe"));

            Assert.Equal("Maintenance record not found.", ex.Message);
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

        [Fact]
        public async Task DeleteMaintenanceRecord_WithMissingRecord_ShouldThrow()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _maintenanceService.DeleteMaintenanceRecordAsync(999));

            Assert.Equal("Maintenance record not found.", ex.Message);
        }
    }
}
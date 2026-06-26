using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using InventoryManagementApp.Services.Calibration;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Interfaces;
using Moq;

namespace InventoryManagementApp.Tests
{
    public class CalibrationServiceTests : IDisposable
    {
        private readonly DatabaseService _databaseService;
        private readonly CalibrationService _calibrationService;
        private readonly Mock<IUserContext> _userContextMock;

        public CalibrationServiceTests()
        {
            var testDbPath = $"test_calibration_{Guid.NewGuid()}.db";
            _databaseService = new DatabaseService(testDbPath);
            SeedRequiredData();
            _userContextMock = new Mock<IUserContext>();
            _userContextMock.Setup(x => x.CurrentUser).Returns(new User { UserID = 1, UserName = "TestUser" });
            _calibrationService = new CalibrationService(_databaseService, _userContextMock.Object);
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
        public async Task CreateCalibrationRecord_ShouldSucceed()
        {
            var record = new CalibrationRecord
            {
                ItemID = 1,
                CalibrationDate = DateTime.Now,
                NextCalibrationDue = DateTime.Now.AddYears(1),
                CalibratedBy = "Test Lab",
                Result = "Pass",
                Cost = 150.00m
            };

            var id = await _calibrationService.CreateCalibrationRecordAsync(record);

            Assert.True(id > 0);
        }

        [Fact]
        public async Task CreateCalibrationRecord_WithMissingItem_ShouldThrow()
        {
            var record = new CalibrationRecord
            {
                ItemID = 999,
                CalibrationDate = DateTime.Now,
                NextCalibrationDue = DateTime.Now.AddYears(1),
                CalibratedBy = "Test Lab",
                Result = "Pass",
                Cost = 150.00m
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _calibrationService.CreateCalibrationRecordAsync(record));

            Assert.Equal("Item not found.", ex.Message);
        }

        [Fact]
        public async Task GetAllCalibrationRecords_ShouldReturnList()
        {
            var record = new CalibrationRecord
            {
                ItemID = 1,
                CalibrationDate = DateTime.Now,
                NextCalibrationDue = DateTime.Now.AddYears(1),
                Result = "Pass"
            };
            await _calibrationService.CreateCalibrationRecordAsync(record);

            var records = await _calibrationService.GetAllCalibrationRecordsAsync();

            Assert.NotEmpty(records);
        }

        [Fact]
        public async Task GetOverdueCalibration_ShouldReturnOverdueRecords()
        {
            var overdueRecord = new CalibrationRecord
            {
                ItemID = 1,
                CalibrationDate = DateTime.Now.AddYears(-2),
                NextCalibrationDue = DateTime.Now.AddDays(-1),
                Result = "Pass"
            };
            await _calibrationService.CreateCalibrationRecordAsync(overdueRecord);

            var overdueRecords = await _calibrationService.GetOverdueCalibrationAsync();

            Assert.NotEmpty(overdueRecords);
        }

        [Fact]
        public async Task GetUpcomingCalibration_WithNegativeDays_ShouldThrow()
        {
            var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _calibrationService.GetUpcomingCalibrationAsync(-1));

            Assert.Equal("days", ex.ParamName);
            Assert.Contains("Days must be greater than or equal to 0.", ex.Message);
        }

        [Fact]
        public async Task UpdateCalibrationRecord_ShouldSucceed()
        {
            var record = new CalibrationRecord
            {
                ItemID = 1,
                CalibrationDate = DateTime.Now,
                NextCalibrationDue = DateTime.Now.AddYears(1),
                Result = "Pass"
            };
            var id = await _calibrationService.CreateCalibrationRecordAsync(record);
            record.CalibrationID = id;
            record.CertificateNumber = "CERT-12345";

            var result = await _calibrationService.UpdateCalibrationRecordAsync(record);

            Assert.True(result);
        }

        [Fact]
        public async Task UpdateCalibrationRecord_WithMissingItem_ShouldThrow()
        {
            var record = new CalibrationRecord
            {
                ItemID = 1,
                CalibrationDate = DateTime.Now,
                NextCalibrationDue = DateTime.Now.AddYears(1),
                Result = "Pass"
            };
            var id = await _calibrationService.CreateCalibrationRecordAsync(record);
            record.CalibrationID = id;
            record.ItemID = 999;

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _calibrationService.UpdateCalibrationRecordAsync(record));

            Assert.Equal("Item not found.", ex.Message);
        }

        [Fact]
        public async Task UpdateCalibrationRecord_WithMissingRecord_ShouldThrow()
        {
            var record = new CalibrationRecord
            {
                CalibrationID = 999,
                ItemID = 1,
                CalibrationDate = DateTime.Now,
                NextCalibrationDue = DateTime.Now.AddYears(1),
                Result = "Pass"
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _calibrationService.UpdateCalibrationRecordAsync(record));

            Assert.Equal("Calibration record not found.", ex.Message);
        }

        [Fact]
        public async Task DeleteCalibrationRecord_ShouldSucceed()
        {
            var record = new CalibrationRecord
            {
                ItemID = 1,
                CalibrationDate = DateTime.Now,
                NextCalibrationDue = DateTime.Now.AddYears(1),
                Result = "Pass"
            };
            var id = await _calibrationService.CreateCalibrationRecordAsync(record);

            var result = await _calibrationService.DeleteCalibrationRecordAsync(id);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteCalibrationRecord_WithMissingRecord_ShouldThrow()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _calibrationService.DeleteCalibrationRecordAsync(999));

            Assert.Equal("Calibration record not found.", ex.Message);
        }
    }
}

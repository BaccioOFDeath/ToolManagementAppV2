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
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Vehicles;
using Moq;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class VehicleIntakeServiceTests : IDisposable
    {
        private readonly DatabaseService _databaseService;
        private readonly VehicleIntakeService _vehicleService;
        private readonly Mock<IUserContext> _userContextMock = new();

        public VehicleIntakeServiceTests()
        {
            var path = $"vehicle_test_{Guid.NewGuid():N}.db";
            _databaseService = new DatabaseService(path);
            _userContextMock.SetupGet(x => x.CurrentUser).Returns(new User { UserID = 99, UserName = "Dismantler" });
            _vehicleService = new VehicleIntakeService(_databaseService, _userContextMock.Object);
        }

        public void Dispose()
        {
            _databaseService?.Dispose();
        }

        [Fact]
        public async Task CreateVehicleAsync_ShouldPersistAndStampUser()
        {
            var vehicle = new VehicleIntake
            {
                Vin = "VIN-123",
                StockNumber = "STK-1",
                IntakeDate = DateTime.Today,
                Status = "Received",
                Location = "Yard A"
            };

            var id = await _vehicleService.CreateVehicleAsync(vehicle);
            var saved = await _vehicleService.GetVehicleByIdAsync(id);

            Assert.True(id > 0);
            Assert.NotNull(saved);
            Assert.Equal("VIN-123", saved!.Vin);
            Assert.Equal(99, saved.CreatedByUserID);
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldMoveVehicleBetweenStages()
        {
            var vehicle = new VehicleIntake { Vin = "VIN-234", IntakeDate = DateTime.Today, Status = "Received" };
            var id = await _vehicleService.CreateVehicleAsync(vehicle);

            var moved = await _vehicleService.UpdateStatusAsync(id, "Dismantling");
            var saved = await _vehicleService.GetVehicleByIdAsync(id);

            Assert.True(moved);
            Assert.NotNull(saved);
            Assert.Equal("Dismantling", saved!.Status);
        }

        [Fact]
        public async Task DismantlingTasks_ShouldTrackLifecycle()
        {
            var vehicle = new VehicleIntake { Vin = "VIN-345", IntakeDate = DateTime.Today, Status = "Received" };
            var id = await _vehicleService.CreateVehicleAsync(vehicle);
            var task = new DismantlingTask { VehicleID = id, PartName = "Front Bumper" };

            var taskId = await _vehicleService.CreateDismantlingTaskAsync(task);
            var started = await _vehicleService.StartTaskAsync(taskId, "Tech A");
            var completed = await _vehicleService.CompleteTaskAsync(taskId, "Grade A", "No cracks");
            var tasks = await _vehicleService.GetTasksForVehicleAsync(id);
            var saved = tasks.FirstOrDefault();

            Assert.True(started);
            Assert.True(completed);
            Assert.NotNull(saved);
            Assert.Equal("Completed", saved!.Status);
            Assert.Equal("Grade A", saved.ConditionGrade);
            Assert.Equal("Tech A", saved.Technician);
        }

        [Fact]
        public async Task GetPipelineSummaryAsync_ShouldCountStatuses()
        {
            var received = new VehicleIntake { Vin = "VIN-456", IntakeDate = DateTime.Today, Status = "Received" };
            var hold = new VehicleIntake { Vin = "VIN-457", IntakeDate = DateTime.Today, Status = "OnHold", ComplianceHoldReason = "Awaiting EPA" };
            var dismantling = new VehicleIntake { Vin = "VIN-458", IntakeDate = DateTime.Today, Status = "Dismantling" };
            var complete = new VehicleIntake { Vin = "VIN-459", IntakeDate = DateTime.Today, Status = "Completed" };

            await _vehicleService.CreateVehicleAsync(received);
            await _vehicleService.CreateVehicleAsync(hold);
            await _vehicleService.CreateVehicleAsync(dismantling);
            await _vehicleService.CreateVehicleAsync(complete);

            var summary = await _vehicleService.GetPipelineSummaryAsync();

            Assert.Equal(1, summary.Received);
            Assert.Equal(1, summary.OnHold);
            Assert.Equal(1, summary.Dismantling);
            Assert.Equal(1, summary.Completed);
        }
    }
}

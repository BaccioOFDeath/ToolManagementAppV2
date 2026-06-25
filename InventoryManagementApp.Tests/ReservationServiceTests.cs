using System;
using System.Threading.Tasks;
using Xunit;
using InventoryManagementApp.Services.Reservations;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Interfaces;
using Moq;

namespace InventoryManagementApp.Tests
{
    public class ReservationServiceTests : IDisposable
    {
        private readonly DatabaseService _databaseService;
        private readonly ReservationService _reservationService;
        private readonly Mock<IUserContext> _userContextMock;

        public ReservationServiceTests()
        {
            var testDbPath = $"test_reservation_{Guid.NewGuid()}.db";
            _databaseService = new DatabaseService(testDbPath);
            SeedRequiredData();
            _userContextMock = new Mock<IUserContext>();
            _userContextMock.Setup(x => x.CurrentUser).Returns(new User { UserID = 1, UserName = "TestUser" });
            _reservationService = new ReservationService(_databaseService, _userContextMock.Object);
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
                INSERT INTO Items (ItemID, ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, ImagePath, IsPowered) VALUES (1, 'ITEM-001', 'Seed Item', 1, 0, 0, 'Assets/ItemImages/ITEM-001.png', 0);
                INSERT INTO Customers (CustomerID, Company, Contact) VALUES (1, 'Seed Customer', 'Primary Contact');";
            cmd.ExecuteNonQuery();
        }

        [Fact]
        public async Task CreateReservation_ShouldSucceed()
        {
            var reservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Quantity = 1,
                Status = "Pending"
            };

            var id = await _reservationService.CreateReservationAsync(reservation);

            Assert.True(id > 0);
        }

        [Fact]
        public async Task GetAllReservations_ShouldReturnList()
        {
            var reservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Quantity = 1,
                Status = "Pending"
            };
            await _reservationService.CreateReservationAsync(reservation);

            var reservations = await _reservationService.GetAllReservationsAsync();

            Assert.NotEmpty(reservations);
        }

        [Fact]
        public async Task GetActiveReservations_ShouldReturnActiveOnly()
        {
            var activeReservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Quantity = 1,
                Status = "Confirmed"
            };
            await _reservationService.CreateReservationAsync(activeReservation);

            var activeReservations = await _reservationService.GetActiveReservationsAsync();

            Assert.NotEmpty(activeReservations);
        }

        [Fact]
        public async Task GetActiveReservations_IncludesItemImagePath()
        {
            var activeReservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Quantity = 1,
                Status = "Pending"
            };
            await _reservationService.CreateReservationAsync(activeReservation);

            var activeReservations = await _reservationService.GetActiveReservationsAsync();

            Assert.Contains(activeReservations, r => r.ImagePath == "Assets/ItemImages/ITEM-001.png");
        }

        [Fact]
        public async Task GetReservationsByCustomer_WithInvalidCustomerId_ShouldThrow()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _reservationService.GetReservationsByCustomerAsync(0));
        }

        [Fact]
        public async Task GetUpcomingReservations_WithNegativeDays_ShouldThrow()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _reservationService.GetUpcomingReservationsAsync(-1));
        }

        [Fact]
        public async Task GetReservationById_WithInvalidReservationId_ShouldThrow()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _reservationService.GetReservationByIdAsync(0));
        }

        [Fact]
        public async Task CreateReservation_WithMissingItem_ShouldThrow()
        {
            var reservation = new Reservation
            {
                ItemID = 999,
                CustomerID = 1,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(2),
                Quantity = 1,
                Status = "Pending"
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _reservationService.CreateReservationAsync(reservation));
        }

        [Fact]
        public async Task CreateReservation_WithMissingCustomer_ShouldThrow()
        {
            var reservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 999,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(2),
                Quantity = 1,
                Status = "Pending"
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _reservationService.CreateReservationAsync(reservation));
        }

        [Fact]
        public async Task UpdateReservation_WithMissingItem_ShouldThrow()
        {
            var reservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(2),
                Quantity = 1,
                Status = "Pending"
            };
            var id = await _reservationService.CreateReservationAsync(reservation);
            reservation.ReservationID = id;
            reservation.ItemID = 999;

            await Assert.ThrowsAsync<ArgumentException>(() => _reservationService.UpdateReservationAsync(reservation));
        }

        [Fact]
        public async Task UpdateReservation_WithMissingCustomer_ShouldThrow()
        {
            var reservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(2),
                Quantity = 1,
                Status = "Pending"
            };
            var id = await _reservationService.CreateReservationAsync(reservation);
            reservation.ReservationID = id;
            reservation.CustomerID = 999;

            await Assert.ThrowsAsync<ArgumentException>(() => _reservationService.UpdateReservationAsync(reservation));
        }

        [Fact]
        public async Task UpdateReservation_WithMissingRental_ShouldThrow()
        {
            var reservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(2),
                Quantity = 1,
                Status = "Pending"
            };
            var id = await _reservationService.CreateReservationAsync(reservation);
            reservation.ReservationID = id;
            reservation.RentalID = 999;

            await Assert.ThrowsAsync<ArgumentException>(() => _reservationService.UpdateReservationAsync(reservation));
        }

        [Fact]
        public async Task UpdateReservation_WithMissingReservation_ShouldThrow()
        {
            var reservation = new Reservation
            {
                ReservationID = 999,
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(2),
                Quantity = 1,
                Status = "Pending"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _reservationService.UpdateReservationAsync(reservation));
        }

        [Fact]
        public async Task UpdateReservation_ShouldSucceed()
        {
            var reservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Quantity = 1,
                Status = "Pending"
            };
            var id = await _reservationService.CreateReservationAsync(reservation);
            reservation.ReservationID = id;
            reservation.Quantity = 2;

            var result = await _reservationService.UpdateReservationAsync(reservation);

            Assert.True(result);
        }

        [Fact]
        public async Task ConfirmReservation_ShouldUpdateStatus()
        {
            var reservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Quantity = 1,
                Status = "Pending"
            };
            var id = await _reservationService.CreateReservationAsync(reservation);

            var result = await _reservationService.ConfirmReservationAsync(id);

            Assert.True(result);
        }

        [Fact]
        public async Task ConfirmReservation_WithMissingReservation_ShouldThrow()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reservationService.ConfirmReservationAsync(999));
        }

        [Fact]
        public async Task CancelReservation_ShouldUpdateStatus()
        {
            var reservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Quantity = 1,
                Status = "Pending"
            };
            var id = await _reservationService.CreateReservationAsync(reservation);

            var result = await _reservationService.CancelReservationAsync(id);

            Assert.True(result);
        }

        [Fact]
        public async Task CancelReservation_WithMissingReservation_ShouldThrow()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reservationService.CancelReservationAsync(999));
        }

        [Fact]
        public async Task FulfillReservation_WithMissingRental_ShouldThrow()
        {
            var reservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Quantity = 1,
                Status = "Confirmed"
            };
            var id = await _reservationService.CreateReservationAsync(reservation);

            await Assert.ThrowsAsync<ArgumentException>(() => _reservationService.FulfillReservationAsync(id, 999));
        }

        [Fact]
        public async Task FulfillReservation_WithMissingReservation_ShouldThrow()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reservationService.FulfillReservationAsync(999, 1));
        }

        [Fact]
        public async Task DeleteReservation_ShouldSucceed()
        {
            var reservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Quantity = 1,
                Status = "Pending"
            };
            var id = await _reservationService.CreateReservationAsync(reservation);

            var result = await _reservationService.DeleteReservationAsync(id);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteReservation_WithMissingReservation_ShouldThrow()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reservationService.DeleteReservationAsync(999));
        }

        [Fact]
        public async Task CreateReservation_WithEndDateBeforeStartDate_ShouldThrow()
        {
            var reservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Today.AddDays(3),
                EndDate = DateTime.Today.AddDays(2),
                Quantity = 1,
                Status = "Pending"
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _reservationService.CreateReservationAsync(reservation));
        }

        [Fact]
        public async Task CreateReservation_WithBlankStatus_ShouldDefaultToPending()
        {
            var reservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(2),
                Quantity = 1,
                Status = "  ",
                Notes = "  "
            };

            var id = await _reservationService.CreateReservationAsync(reservation);

            var saved = await _reservationService.GetReservationByIdAsync(id);
            Assert.NotNull(saved);
            Assert.Equal("Pending", saved.Status);
            Assert.Equal(string.Empty, saved.Notes);
        }

        [Fact]
        public async Task CreateReservation_WithUnknownStatus_ShouldThrow()
        {
            var reservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(2),
                Quantity = 1,
                Status = "Waiting"
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _reservationService.CreateReservationAsync(reservation));
        }

        [Fact]
        public async Task UpdateReservation_WithUnknownStatus_ShouldThrow()
        {
            var reservation = new Reservation
            {
                ItemID = 1,
                CustomerID = 1,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(2),
                Quantity = 1,
                Status = "Pending"
            };
            var id = await _reservationService.CreateReservationAsync(reservation);
            reservation.ReservationID = id;
            reservation.Status = "Waiting";

            await Assert.ThrowsAsync<ArgumentException>(() => _reservationService.UpdateReservationAsync(reservation));
        }

        [Fact]
        public async Task CheckAvailability_WithInvalidQuantity_ShouldThrow()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _reservationService.CheckAvailabilityAsync(1, DateTime.Today, DateTime.Today.AddDays(1), 0));
        }
    }
}

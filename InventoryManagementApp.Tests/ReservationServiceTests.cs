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
            _userContextMock = new Mock<IUserContext>();
            _userContextMock.Setup(x => x.CurrentUser).Returns(new User { UserID = 1, UserName = "TestUser" });
            _reservationService = new ReservationService(_databaseService, _userContextMock.Object);
        }

        public void Dispose()
        {
            _databaseService?.Dispose();
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
    }
}

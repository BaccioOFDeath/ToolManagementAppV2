using System;
using System.Threading.Tasks;
using Xunit;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Reservations;
using Microsoft.Data.Sqlite;
using Moq;

namespace InventoryManagementApp.Tests
{
    public class ReservationOrphanReadBehaviorTests
    {
        [Fact]
        public async Task ReservationReadModels_WithLegacyMissingReferences_ShouldReturnValidRowsAndHideOrphanRecords()
        {
            using var databaseService = CreateDatabaseService("reservation_orphan_reads");
            SeedRequiredData(databaseService);
            var reservationService = new ReservationService(databaseService, CreateUserContext());
            var now = DateTime.Now;
            var validPendingId = InsertLegacyReservation(databaseService, 1, 1, now.AddDays(1), now.AddDays(3), "Pending");
            var validConfirmedId = InsertLegacyReservation(databaseService, 1, 1, now.AddDays(2), now.AddDays(4), "Confirmed");
            var missingItemId = InsertLegacyReservation(databaseService, 999, 1, now.AddDays(1), now.AddDays(3), "Pending");
            var missingCustomerId = InsertLegacyReservation(databaseService, 1, 999, now.AddDays(2), now.AddDays(4), "Confirmed");

            var allReservations = await reservationService.GetAllReservationsAsync();
            var activeReservations = await reservationService.GetActiveReservationsAsync();
            var itemReservations = await reservationService.GetReservationsByItemAsync(1);
            var customerReservations = await reservationService.GetReservationsByCustomerAsync(1);
            var upcomingReservations = await reservationService.GetUpcomingReservationsAsync(30);
            var validById = await reservationService.GetReservationByIdAsync(validPendingId);
            var missingItemById = await reservationService.GetReservationByIdAsync(missingItemId);
            var missingCustomerById = await reservationService.GetReservationByIdAsync(missingCustomerId);

            Assert.Contains(allReservations, reservation => reservation.ReservationID == validPendingId);
            Assert.Contains(allReservations, reservation => reservation.ReservationID == validConfirmedId);
            Assert.Contains(activeReservations, reservation => reservation.ReservationID == validPendingId);
            Assert.Contains(activeReservations, reservation => reservation.ReservationID == validConfirmedId);
            Assert.Contains(itemReservations, reservation => reservation.ReservationID == validPendingId);
            Assert.Contains(customerReservations, reservation => reservation.ReservationID == validPendingId);
            Assert.Contains(upcomingReservations, reservation => reservation.ReservationID == validPendingId);
            Assert.Contains(upcomingReservations, reservation => reservation.ReservationID == validConfirmedId);
            Assert.NotNull(validById);
            Assert.Equal("ITEM-001", validById!.ItemNumber);
            Assert.Equal("Seed Customer", validById.CustomerName);

            Assert.DoesNotContain(allReservations, reservation => reservation.ReservationID == missingItemId || reservation.ReservationID == missingCustomerId);
            Assert.DoesNotContain(activeReservations, reservation => reservation.ReservationID == missingItemId || reservation.ReservationID == missingCustomerId);
            Assert.DoesNotContain(itemReservations, reservation => reservation.ReservationID == missingCustomerId);
            Assert.DoesNotContain(customerReservations, reservation => reservation.ReservationID == missingItemId);
            Assert.DoesNotContain(upcomingReservations, reservation => reservation.ReservationID == missingItemId || reservation.ReservationID == missingCustomerId);
            Assert.Null(missingItemById);
            Assert.Null(missingCustomerById);
        }

        private static DatabaseService CreateDatabaseService(string prefix)
        {
            return new DatabaseService($"test_{prefix}_{Guid.NewGuid()}.db");
        }

        private static IUserContext CreateUserContext()
        {
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.CurrentUser).Returns(new InventoryManagementApp.Models.Domain.User { UserID = 1, UserName = "TestUser" });
            return userContextMock.Object;
        }

        private static void SeedRequiredData(DatabaseService databaseService)
        {
            using var conn = databaseService.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Users (UserID, UserName, IsAdmin, IsActive) VALUES (1, 'TestUser', 0, 1);
                INSERT INTO Items (ItemID, ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, ImagePath, IsPowered) VALUES (1, 'ITEM-001', 'Seed Item', 1, 0, 0, 'Assets/ItemImages/ITEM-001.png', 0);
                INSERT INTO Customers (CustomerID, Company, Contact) VALUES (1, 'Seed Customer', 'Primary Contact');";
            cmd.ExecuteNonQuery();
        }

        private static int InsertLegacyReservation(
            DatabaseService databaseService,
            int itemID,
            int customerID,
            DateTime startDate,
            DateTime endDate,
            string status)
        {
            var builder = new SqliteConnectionStringBuilder(databaseService.ConnectionString)
            {
                ForeignKeys = false
            };

            using var conn = new SqliteConnection(builder.ToString());
            using var cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = @"
                INSERT INTO Reservations
                    (ItemID, CustomerID, ReservationDate, StartDate, EndDate, Quantity, Status, CreatedByUserID, CreatedAt)
                VALUES
                    (@ItemID, @CustomerID, @ReservationDate, @StartDate, @EndDate, @Quantity, @Status, @CreatedByUserID, @CreatedAt);
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@ItemID", itemID);
            cmd.Parameters.AddWithValue("@CustomerID", customerID);
            cmd.Parameters.AddWithValue("@ReservationDate", DateTime.Now);
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate", endDate);
            cmd.Parameters.AddWithValue("@Quantity", 1);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@CreatedByUserID", 1);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}

using System;
using System.Threading.Tasks;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Rentals;
using Microsoft.Data.Sqlite;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RentalOrphanReadBehaviorTests
    {
        [Fact]
        public async Task RentalReadModels_WithLegacyMissingReferences_ShouldReturnValidRowsAndHideOrphanRecords()
        {
            using var databaseService = CreateDatabaseService("rental_orphan_reads");
            SeedRequiredData(databaseService);
            var rentalService = new RentalService(databaseService);
            var overdueRentalDate = DateTime.Today.AddDays(-14);
            var overdueDueDate = DateTime.Today.AddDays(-1);
            var returnedRentalDate = DateTime.Today.AddDays(-21);
            var returnedDueDate = DateTime.Today.AddDays(-15);
            var futureRentalDate = DateTime.Today.AddDays(-2);
            var futureDueDate = DateTime.Today.AddDays(5);

            var validOverdueActiveId = InsertLegacyRental(databaseService, 1, 1, overdueRentalDate, overdueDueDate, "Rented");
            var validFutureActiveId = InsertLegacyRental(databaseService, 1, 1, futureRentalDate, futureDueDate, "Rented");
            var validReturnedId = InsertLegacyRental(databaseService, 1, 1, returnedRentalDate, returnedDueDate, "Returned");
            var missingItemId = InsertLegacyRental(databaseService, 999, 1, overdueRentalDate, overdueDueDate, "Rented");
            var missingCustomerId = InsertLegacyRental(databaseService, 1, 999, overdueRentalDate, overdueDueDate, "Rented");

            var allRentals = await rentalService.GetAllRentalsAsync();
            var activeRentals = await rentalService.GetActiveRentalsAsync();
            var overdueRentals = await rentalService.GetOverdueRentalsAsync();
            var itemHistory = await rentalService.GetRentalHistoryForItemAsync(1);
            var customerHistory = await rentalService.GetRentalHistoryForCustomerAsync(1);

            Assert.Contains(allRentals, rental => rental.RentalID == validOverdueActiveId);
            Assert.Contains(allRentals, rental => rental.RentalID == validFutureActiveId);
            Assert.Contains(allRentals, rental => rental.RentalID == validReturnedId);
            Assert.Contains(activeRentals, rental => rental.RentalID == validOverdueActiveId);
            Assert.Contains(activeRentals, rental => rental.RentalID == validFutureActiveId);
            Assert.DoesNotContain(activeRentals, rental => rental.RentalID == validReturnedId);
            Assert.Contains(overdueRentals, rental => rental.RentalID == validOverdueActiveId);
            Assert.DoesNotContain(overdueRentals, rental => rental.RentalID == validFutureActiveId || rental.RentalID == validReturnedId);
            Assert.Contains(itemHistory, rental => rental.RentalID == validOverdueActiveId);
            Assert.Contains(itemHistory, rental => rental.RentalID == validReturnedId);
            Assert.Contains(customerHistory, rental => rental.RentalID == validOverdueActiveId);
            Assert.Contains(customerHistory, rental => rental.RentalID == validReturnedId);

            Assert.DoesNotContain(allRentals, rental => rental.RentalID == missingItemId || rental.RentalID == missingCustomerId);
            Assert.DoesNotContain(activeRentals, rental => rental.RentalID == missingItemId || rental.RentalID == missingCustomerId);
            Assert.DoesNotContain(overdueRentals, rental => rental.RentalID == missingItemId || rental.RentalID == missingCustomerId);
            Assert.DoesNotContain(itemHistory, rental => rental.RentalID == missingCustomerId);
            Assert.DoesNotContain(customerHistory, rental => rental.RentalID == missingItemId);
        }

        private static DatabaseService CreateDatabaseService(string prefix)
        {
            return new DatabaseService($"test_{prefix}_{Guid.NewGuid()}.db");
        }

        private static void SeedRequiredData(DatabaseService databaseService)
        {
            using var conn = databaseService.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Items (ItemID, ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, ImagePath, IsPowered) VALUES (1, 'ITEM-001', 'Seed Item', 2, 0, 1, 'Assets/ItemImages/ITEM-001.png', 0);
                INSERT INTO Customers (CustomerID, Company, Contact) VALUES (1, 'Seed Customer', 'Primary Contact');";
            cmd.ExecuteNonQuery();
        }

        private static int InsertLegacyRental(
            DatabaseService databaseService,
            int itemID,
            int customerID,
            DateTime rentalDate,
            DateTime dueDate,
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
                INSERT INTO Rentals
                    (ItemID, CustomerID, RentalDate, DueDate, Status)
                VALUES
                    (@ItemID, @CustomerID, @RentalDate, @DueDate, @Status);
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@ItemID", itemID);
            cmd.Parameters.AddWithValue("@CustomerID", customerID);
            cmd.Parameters.AddWithValue("@RentalDate", rentalDate);
            cmd.Parameters.AddWithValue("@DueDate", dueDate);
            cmd.Parameters.AddWithValue("@Status", status);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}

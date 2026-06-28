using System;
using System.Threading.Tasks;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Rentals;
using Microsoft.Data.Sqlite;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RentalFrequencyOrphanBehaviorTests
    {
        [Fact]
        public async Task RentalFrequency_WithLegacyMissingReferences_ShouldCountOnlyVisibleCustomerBackedRentals()
        {
            using var databaseService = CreateDatabaseService("rental_frequency_orphan_reads");
            SeedRequiredData(databaseService);
            var rentalService = new RentalService(databaseService);
            var rentalDate = DateTime.Today.AddDays(-7);
            var dueDate = DateTime.Today.AddDays(-1);

            InsertLegacyRental(databaseService, 1, 1, rentalDate, dueDate, "Returned");
            InsertLegacyRental(databaseService, 1, 999, rentalDate, dueDate, "Returned");
            InsertLegacyRental(databaseService, 999, 1, rentalDate, dueDate, "Returned");

            var frequencies = await rentalService.GetRentalFrequencyAsync(10);

            var frequency = Assert.Single(frequencies);
            Assert.Equal(1, frequency.ItemID);
            Assert.Equal("ITEM-001", frequency.ItemNumber);
            Assert.Equal(1, frequency.RentalCount);
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
                INSERT INTO Items (ItemID, ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, ImagePath, IsPowered) VALUES (1, 'ITEM-001', 'Seed Item', 1, 0, 1, 'Assets/ItemImages/ITEM-001.png', 0);
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

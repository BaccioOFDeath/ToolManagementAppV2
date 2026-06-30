using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Rentals;
using Xunit;

namespace InventoryManagementApp.Tests;

public class RentalContactLogServiceTests
{
    [Fact]
    public async Task AddContactLogAsync_SavesRentalSpecificCommunication()
    {
        using var db = new DatabaseService(":memory:");
        SeedRental(db);
        var service = new RentalContactLogService(db);

        var result = await service.AddContactLogAsync(new RentalContactLog
        {
            RentalID = 1,
            Channel = "Email",
            Direction = "Outgoing",
            Recipient = "customer@example.com",
            Subject = "Rental T30",
            Message = "Please confirm return time.",
            CreatedBy = "TestUser"
        });

        var logs = await service.GetContactLogsForRentalAsync(1);

        Assert.True(result.Success);
        Assert.True(logs.Success);
        var log = Assert.Single(logs.Value!);
        Assert.Equal("Email", log.Channel);
        Assert.Equal("Outgoing", log.Direction);
        Assert.Equal("customer@example.com", log.Recipient);
        Assert.Equal("Rental T30", log.Subject);
        Assert.Equal("Please confirm return time.", log.Message);
        Assert.Equal("TestUser", log.CreatedBy);
    }

    [Fact]
    public async Task AddContactLogAsync_RejectsBlankMessage()
    {
        using var db = new DatabaseService(":memory:");
        SeedRental(db);
        var service = new RentalContactLogService(db);

        var result = await service.AddContactLogAsync(new RentalContactLog
        {
            RentalID = 1,
            Channel = "SMS",
            Direction = "Incoming",
            Message = " "
        });

        Assert.False(result.Success);
        Assert.Contains("Message is required", result.ErrorMessage);
    }

    static void SeedRental(DatabaseService db)
    {
        using var conn = db.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Items (ItemID, ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, IsPowered)
            VALUES (1, 'T30', 'Rental Item', 1, 0, 1, 0);
            INSERT INTO Customers (CustomerID, Company, Contact, Phone)
            VALUES (1, 'Pickerill Automotive And Tyres', 'Service', '078472255');
            INSERT INTO Rentals (RentalID, ItemID, CustomerID, RentalDate, DueDate, Status)
            VALUES (1, 1, 1, '2026-06-25 12:00:00', '2026-07-11 12:00:00', 'Rented');";
        cmd.ExecuteNonQuery();
    }
}

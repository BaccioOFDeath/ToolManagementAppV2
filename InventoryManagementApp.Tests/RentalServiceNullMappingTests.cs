using System;
using System.Data;
using System.Reflection;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Rentals;
using Xunit;

public class RentalServiceNullMappingTests
{
    [Fact]
    public void MapRental_CoalescesNullStrings()
    {
        var table = new DataTable();
        table.Columns.Add("RentalID", typeof(int));
        table.Columns.Add("ItemID", typeof(int));
        table.Columns.Add("CustomerID", typeof(int));
        table.Columns.Add("RentalDate", typeof(DateTime));
        table.Columns.Add("DueDate", typeof(DateTime));
        table.Columns.Add("ReturnDate", typeof(DateTime));
        table.Columns.Add("Status", typeof(string));
        table.Columns.Add("ItemNumber", typeof(string));
        table.Columns.Add("Company", typeof(string));
        table.Columns.Add("Contact", typeof(string));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("Phone", typeof(string));
        table.Columns.Add("Mobile", typeof(string));
        table.Columns.Add("Address", typeof(string));
        table.Columns.Add("ImagePath", typeof(string));
        table.Columns.Add("ItemLocation", typeof(string));

        table.Rows.Add(1, 2, 3, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), DBNull.Value,
            DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value,
            DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);

        using var reader = table.CreateDataReader();
        reader.Read();

        using var db = new DatabaseService(":memory:");
        var service = new RentalService(db);

        var method = typeof(RentalService).GetMethod("MapRental", BindingFlags.NonPublic | BindingFlags.Instance);
        var rental = (Rental?)method!.Invoke(service, new object[] { reader });

        Assert.NotNull(rental);
        Assert.Equal(string.Empty, rental!.Status);
        Assert.Equal(string.Empty, rental.ItemNumber);
        Assert.Equal(string.Empty, rental.CustomerName);
        Assert.Equal(string.Empty, rental.CustomerContact);
        Assert.Equal(string.Empty, rental.CustomerEmail);
        Assert.Equal(string.Empty, rental.CustomerPhone);
        Assert.Equal(string.Empty, rental.CustomerMobile);
        Assert.Equal(string.Empty, rental.CustomerAddress);
        Assert.Equal(string.Empty, rental.ImagePath);
        Assert.Equal(string.Empty, rental.ItemLocation);
    }
}


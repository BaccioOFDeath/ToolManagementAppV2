using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using Xunit;

public class ItemServiceUpdatedAtTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetItemsCheckedOutByAsync_HandlesEmptyUpdatedAt(string? updatedAt)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        using var db = new DatabaseService(dbPath);
        var factory = new SqliteConnectionFactory(db.ConnectionString);
        var repository = new ItemRepository(factory);
        var logger = new ListLogger<ItemService>();
        var service = new ItemService(db, repository, logger: logger);

        using (var conn = db.CreateConnection())
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO Items (ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, IsCheckedOut, CheckedOutBy, IsPowered, UpdatedAt) VALUES ('A1','Test',1,0,0,1,'User',0,@UpdatedAt);";
            var p = cmd.CreateParameter();
            p.ParameterName = "@UpdatedAt";
            p.Value = updatedAt ?? (object)DBNull.Value;
            cmd.Parameters.Add(p);
            cmd.ExecuteNonQuery();
        }

        var items = await service.GetItemsCheckedOutByAsync("User");

        Assert.Single(items);
        Assert.Equal(default, items[0].UpdatedAt);
        Assert.Empty(logger.Messages);

        File.Delete(dbPath);
    }
}


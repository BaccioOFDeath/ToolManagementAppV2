using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using InventoryManagementApp.Data;
using InventoryManagementApp.Models.Domain;
using Xunit;

public class ItemRepositorySearchTests
{
    private static SqliteConnectionFactory CreateFactory()
        => new("Data Source=:memory:");

    private static async Task SeedAsync(SqliteConnectionFactory factory)
    {
        using var conn = factory.Create();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"CREATE TABLE Items (
            ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
            ItemNumber TEXT,
            NameDescription TEXT,
            Location TEXT,
            Brand TEXT,
            PartNumber TEXT,
            Supplier TEXT,
            PurchasedDate TEXT,
            Notes TEXT,
            Keywords TEXT,
            AvailableQuantity INTEGER,
            RentedQuantity INTEGER,
            ImagePath TEXT,
            IsCheckedOut INTEGER,
            CheckedOutBy TEXT,
            CheckedOutTime TEXT,
            IsPowered INTEGER
        );";
        cmd.ExecuteNonQuery();
        await conn.ExecuteAsync(
            "INSERT INTO Items (ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsCheckedOut, IsPowered) VALUES (@ItemNumber,@Name,0,0,0,0)",
            new { ItemNumber = "ABC123", Name = "Hand Saw" });
    }

    [Fact]
    public async Task GetPageAsync_SearchItemNumber_IgnoresCase()
    {
        var factory = CreateFactory();
        await SeedAsync(factory);
        var repo = new ItemRepository(factory);
        var result = new List<ItemModel>();
        await foreach (var item in repo.GetPageAsync(new ItemFilter("abc"), new ItemPage(1, 10), CancellationToken.None))
            result.Add(item);
        Assert.Single(result);
        Assert.Equal("ABC123", result[0].ItemNumber);
    }

    [Fact]
    public async Task GetPageAsync_SearchName_IgnoresCase()
    {
        var factory = CreateFactory();
        await SeedAsync(factory);
        var repo = new ItemRepository(factory);
        var result = new List<ItemModel>();
        await foreach (var item in repo.GetPageAsync(new ItemFilter("saw"), new ItemPage(1, 10), CancellationToken.None))
            result.Add(item);
        Assert.Single(result);
        Assert.Equal("Hand Saw", result[0].Name);
    }
}

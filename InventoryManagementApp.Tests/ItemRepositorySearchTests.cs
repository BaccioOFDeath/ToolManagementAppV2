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
            IsRentalItem INTEGER,
            Price NUMERIC NOT NULL DEFAULT 0,
            ImagePath TEXT,
            IsCheckedOut INTEGER,
            CheckedOutBy TEXT,
            CheckedOutTime TEXT,
            IsPowered INTEGER,
            UpdatedAt TEXT
        );";
        cmd.ExecuteNonQuery();
        await conn.ExecuteAsync(
            "INSERT INTO Items (ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, IsCheckedOut, IsPowered, UpdatedAt) VALUES (@ItemNumber,@Name,0,0,0,0,0,@UpdatedAt)",
            new { ItemNumber = "ABC123", Name = "Hand Saw", UpdatedAt = System.DateTime.UtcNow });
        await conn.ExecuteAsync(
            "INSERT INTO Items (ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, IsCheckedOut, IsPowered, UpdatedAt) VALUES (@ItemNumber,@Name,0,0,1,0,0,@UpdatedAt)",
            new { ItemNumber = "CAFÉ1", Name = "Café Table", UpdatedAt = System.DateTime.UtcNow });
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

    [Fact]
    public async Task GetPageAsync_SearchName_AccentInsensitive()
    {
        var factory = CreateFactory();
        await SeedAsync(factory);
        var repo = new ItemRepository(factory);
        var result = new List<ItemModel>();
        await foreach (var item in repo.GetPageAsync(new ItemFilter("cafe"), new ItemPage(1, 10), CancellationToken.None))
            result.Add(item);
        Assert.Contains(result, i => i.Name == "Café Table");
    }

    [Fact]
    public async Task GetPageAsync_SearchItemNumber_AccentInsensitive()
    {
        var factory = CreateFactory();
        await SeedAsync(factory);
        var repo = new ItemRepository(factory);
        var result = new List<ItemModel>();
        await foreach (var item in repo.GetPageAsync(new ItemFilter("cafe"), new ItemPage(1, 10), CancellationToken.None))
            result.Add(item);
        Assert.Contains(result, i => i.ItemNumber == "CAFÉ1");
    }

    [Fact]
    public async Task GetPageAsync_FilterByIsRentalItem_True_ReturnsRentalItemsOnly()
    {
        var factory = CreateFactory();
        await SeedAsync(factory);
        var repo = new ItemRepository(factory);
        var result = new List<ItemModel>();
        await foreach (var item in repo.GetPageAsync(new ItemFilter(null, IsRentalItem: true), new ItemPage(1, 10), CancellationToken.None))
            result.Add(item);
        Assert.Single(result);
        Assert.True(result[0].IsRentalItem);
        Assert.Equal("Café Table", result[0].Name);
    }

    [Fact]
    public async Task GetPageAsync_FilterByIsRentalItem_False_ReturnsNonRentalItemsOnly()
    {
        var factory = CreateFactory();
        await SeedAsync(factory);
        var repo = new ItemRepository(factory);
        var result = new List<ItemModel>();
        await foreach (var item in repo.GetPageAsync(new ItemFilter(null, IsRentalItem: false), new ItemPage(1, 10), CancellationToken.None))
            result.Add(item);
        Assert.Single(result);
        Assert.False(result[0].IsRentalItem);
        Assert.Equal("Hand Saw", result[0].Name);
    }
}

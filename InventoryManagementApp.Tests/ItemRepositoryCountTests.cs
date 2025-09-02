using System.Threading;
using System.Threading.Tasks;
using Dapper;
using InventoryManagementApp.Data;
using InventoryManagementApp.Models.Domain;
using Xunit;

public class ItemRepositoryCountTests
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
            CheckedInBy TEXT,
            CheckedInTime TEXT,
            IsPowered INTEGER,
            UpdatedAt TEXT,
            DeviceId TEXT
        );";
        cmd.ExecuteNonQuery();
        await conn.ExecuteAsync(
            "INSERT INTO Items (ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, IsCheckedOut, IsPowered, UpdatedAt) VALUES (@ItemNumber,@Name,0,0,1,0,0,@UpdatedAt)",
            new { ItemNumber = "A1", Name = "Hand Saw", UpdatedAt = System.DateTime.UtcNow });
        await conn.ExecuteAsync(
            "INSERT INTO Items (ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, IsCheckedOut, IsPowered, UpdatedAt) VALUES (@ItemNumber,@Name,0,0,0,0,0,@UpdatedAt)",
            new { ItemNumber = "B2", Name = "Hammer", UpdatedAt = System.DateTime.UtcNow });
    }

    [Fact]
    public async Task CountAsync_NoSearch_ReturnsTotalCount()
    {
        var factory = CreateFactory();
        await SeedAsync(factory);
        var repo = new ItemRepository(factory);
        var count = await repo.CountAsync(new ItemFilter(null), CancellationToken.None);
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CountAsync_WithSearch_FiltersResults()
    {
        var factory = CreateFactory();
        await SeedAsync(factory);
        var repo = new ItemRepository(factory);
        var count = await repo.CountAsync(new ItemFilter("saw"), CancellationToken.None);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CountAsync_FilterByIsRentalItem()
    {
        var factory = CreateFactory();
        await SeedAsync(factory);
        var repo = new ItemRepository(factory);
        var count = await repo.CountAsync(new ItemFilter(null, IsRentalItem: true), CancellationToken.None);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CountAsync_Cancelled_Throws()
    {
        var factory = CreateFactory();
        await SeedAsync(factory);
        var repo = new ItemRepository(factory);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => repo.CountAsync(new ItemFilter(null), cts.Token));
    }
}

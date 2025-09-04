using System.Threading;
using System.Threading.Tasks;
using Dapper;
using InventoryManagementApp.Data;
using InventoryManagementApp.Models.Domain;
using Xunit;

public class ItemRepositoryGetByIdTests
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
            "INSERT INTO Items (ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, IsCheckedOut, IsPowered, UpdatedAt) VALUES (@ItemNumber,@Name,0,0,0,0,0,@UpdatedAt)",
            new { ItemNumber = "A1", Name = "Saw", UpdatedAt = System.DateTime.UtcNow });
    }

    [Fact]
    public async Task GetByIdAsync_FindsItem()
    {
        var factory = CreateFactory();
        await SeedAsync(factory);
        var repo = new ItemRepository(factory);
        var item = await repo.GetByIdAsync(1, CancellationToken.None);
        Assert.NotNull(item);
        Assert.Equal("A1", item!.ItemNumber);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMissing()
    {
        var factory = CreateFactory();
        await SeedAsync(factory);
        var repo = new ItemRepository(factory);
        var item = await repo.GetByIdAsync(42, CancellationToken.None);
        Assert.Null(item);
    }
}

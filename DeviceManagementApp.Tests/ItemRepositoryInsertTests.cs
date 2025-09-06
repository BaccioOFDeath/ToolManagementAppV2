using System.Threading;
using System.Threading.Tasks;
using Dapper;
using InventoryManagementApp.Data;
using InventoryManagementApp.Models.Domain;
using Xunit;

public class ItemRepositoryInsertTests
{
    private static SqliteConnectionFactory CreateFactory()
        => new("Data Source=:memory:");

    private static async Task CreateTableAsync(SqliteConnectionFactory factory)
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
            IsPowered INTEGER
        );";
        cmd.ExecuteNonQuery();
    }

    [Fact]
    public async Task InsertAsync_InsertsPrice()
    {
        var factory = CreateFactory();
        await CreateTableAsync(factory);
        var repo = new ItemRepository(factory);
        var item = new ItemModel { ItemNumber = "X1", Name = "Test", Price = 9.99m };

        var id = await repo.InsertAsync(item, CancellationToken.None);

        using var conn = factory.Create();
        var price = await conn.ExecuteScalarAsync<decimal>("SELECT Price FROM Items WHERE ItemID=@id", new { id });
        Assert.Equal(9.99m, price);
    }
}


using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using InventoryManagementApp.Data;
using InventoryManagementApp.Models.Domain;
using Xunit;

public class ItemRepositoryPaginationTests
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
        for (int i = 1; i <= 5; i++)
        {
            await conn.ExecuteAsync(
                "INSERT INTO Items (ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsCheckedOut, IsPowered) VALUES (@ItemNumber,@Name,0,0,0,0)",
                new { ItemNumber = $"I{i}", Name = $"Item {i}" });
        }
    }

    [Fact]
    public async Task GetPageAsync_ReturnsRequestedPage()
    {
        var factory = CreateFactory();
        await SeedAsync(factory);
        var repo = new ItemRepository(factory);
        var page = new ItemPage(2, 2);
        var result = new List<ItemModel>();
        await foreach (var item in repo.GetPageAsync(new ItemFilter(null), page, CancellationToken.None))
            result.Add(item);
        Assert.Collection(result,
            i => Assert.Equal("Item 3", i.Name),
            i => Assert.Equal("Item 4", i.Name));
    }

    [Fact]
    public async Task GetPageAsync_PartialEnumeration_DoesNotThrow()
    {
        var factory = CreateFactory();
        await SeedAsync(factory);
        var repo = new ItemRepository(factory);
        var page = new ItemPage(1, 5);
        var enumerable = repo.GetPageAsync(new ItemFilter(null), page, CancellationToken.None);
        await using var enumerator = enumerable.GetAsyncEnumerator();
        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal("Item 1", enumerator.Current.Name);
    }
}

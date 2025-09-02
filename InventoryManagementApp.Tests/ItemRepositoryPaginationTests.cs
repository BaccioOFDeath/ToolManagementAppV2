using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
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
        for (int i = 1; i <= 5; i++)
        {
            await conn.ExecuteAsync(
                "INSERT INTO Items (ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, IsCheckedOut, IsPowered, UpdatedAt) VALUES (@ItemNumber,@Name,0,0,@IsRental,0,0,@UpdatedAt)",
                new { ItemNumber = $"I{i}", Name = $"Item {i}", IsRental = i % 2, UpdatedAt = System.DateTime.UtcNow });
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
        await foreach (var item in repo.GetPageAsync(new ItemFilter(null), page, CancellationToken.None)
            .WithCancellation(CancellationToken.None))
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

    [Fact]
    public async Task GetPageAsync_Cancelled_Throws()
    {
        var factory = CreateFactory();
        await SeedAsync(factory);
        var repo = new ItemRepository(factory);
        var page = new ItemPage(1, 5);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var enumerable = repo.GetPageAsync(new ItemFilter(null), page, cts.Token);
        await using var enumerator = enumerable.GetAsyncEnumerator();
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await enumerator.MoveNextAsync());
    }
}

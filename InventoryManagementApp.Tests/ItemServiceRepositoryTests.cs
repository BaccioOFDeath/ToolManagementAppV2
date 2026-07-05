using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using Xunit;

public class ItemServiceRepositoryTests
{
    private sealed class RecordingRepository : IItemRepository
    {
        public int GetByUserCalls { get; private set; }
        public string? LastUser { get; private set; }
        public int GetCheckedOutCalls { get; private set; }
        public int UpdateImageCalls { get; private set; }
        public int? LastImageItemId { get; private set; }
        public string? LastImagePath { get; private set; }

        public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken ct)
        {
            GetByUserCalls++;
            LastUser = userName;
            return Task.FromResult(new List<ItemModel> { new ItemModel { ItemID = 1 } });
        }

        public Task<List<ItemModel>> GetCheckedOutItemsAsync(CancellationToken ct)
        {
            GetCheckedOutCalls++;
            return Task.FromResult(new List<ItemModel> { new ItemModel { ItemID = 2 } });
        }

        public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken ct)
        {
            UpdateImageCalls++;
            LastImageItemId = itemID;
            LastImagePath = imagePath;
            return Task.CompletedTask;
        }

        public IAsyncEnumerable<ItemModel> GetPageAsync(ItemFilter filter, ItemPage page, CancellationToken ct)
            => AsyncEnumerable.Empty<ItemModel>();
        public Task<int> CountAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(0);
        public Task<ItemModel?> GetByIdAsync(int id, CancellationToken ct) => Task.FromResult<ItemModel?>(null);
        public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct) => Task.CompletedTask;
        public Task<int> InsertAsync(ItemModel item, CancellationToken ct) => Task.FromResult(0);
        public Task UpdateAsync(ItemModel item, CancellationToken ct) => Task.CompletedTask;
        public Task DeleteAsync(int itemID, CancellationToken ct) => Task.CompletedTask;
        public Task<bool> ToggleCheckOutStatusAsync(int itemID, string currentUser, bool isAdmin, CancellationToken ct) => Task.FromResult(false);
        public Task<List<ItemModel>> GetMostCommonlyUsedItemsAsync(int limit, CancellationToken ct) => Task.FromResult(new List<ItemModel>());
        public Task<List<ItemModel>> GetIncompleteItemsAsync(CancellationToken ct) => Task.FromResult(new List<ItemModel>());
    }

    [Fact]
    public async Task GetItemsCheckedOutByAsync_DelegatesToRepository()
    {
        using var db = new DatabaseService(":memory:");
        var repo = new RecordingRepository();
        var service = new ItemService(db, repo);
        var result = await service.GetItemsCheckedOutByAsync("Alice");
        Assert.Single(result);
        Assert.Equal(1, repo.GetByUserCalls);
        Assert.Equal("Alice", repo.LastUser);
    }

    [Fact]
    public async Task GetCheckedOutItemsAsync_DelegatesToRepository()
    {
        using var db = new DatabaseService(":memory:");
        var repo = new RecordingRepository();
        var service = new ItemService(db, repo);
        var result = await service.GetCheckedOutItemsAsync();
        Assert.Single(result);
        Assert.Equal(1, repo.GetCheckedOutCalls);
    }

    [Fact]
    public async Task UpdateItemImageAsync_DelegatesToRepository()
    {
        using var db = new DatabaseService(":memory:");
        var repo = new RecordingRepository();
        var service = new ItemService(db, repo);
        await service.UpdateItemImageAsync(5, "img.png");
        Assert.Equal(1, repo.UpdateImageCalls);
        Assert.Equal(5, repo.LastImageItemId);
        Assert.Equal("img.png", repo.LastImagePath);
    }

    [Theory]
    [InlineData("bay-7")]
    [InlineData("dti")]
    [InlineData("kit-77")]
    [InlineData("supplier-alpha")]
    public async Task SearchItemsAsync_FindsRentalItemsByVisibleIdentifierFields(string searchTerm)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        await using var db = new DatabaseService(dbPath);
        var repository = new ItemRepository(new SqliteConnectionFactory(db.ConnectionString));
        var service = new ItemService(db, repository);

        await using (var connection = db.CreateConnection())
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                INSERT INTO Items (
                    ItemNumber,
                    NameDescription,
                    Location,
                    Brand,
                    PartNumber,
                    Supplier,
                    AvailableQuantity,
                    RentedQuantity,
                    IsRentalItem,
                    IsCheckedOut,
                    IsPowered)
                VALUES (
                    'A-100',
                    'Diagnostic interface',
                    'Bay-7',
                    'DTI',
                    'KIT-77',
                    'Supplier-Alpha',
                    1,
                    0,
                    1,
                    1,
                    0);";
            await command.ExecuteNonQueryAsync();
        }

        var results = new List<ItemModel>();
        await foreach (var item in service.SearchItemsAsync(searchTerm, new ItemPage(1, 20)))
            results.Add(item);

        var result = Assert.Single(results);
        Assert.Equal("A-100", result.ItemNumber);
    }
}

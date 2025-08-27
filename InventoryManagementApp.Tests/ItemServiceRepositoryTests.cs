using System.Collections.Generic;
using System.Linq;
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
}

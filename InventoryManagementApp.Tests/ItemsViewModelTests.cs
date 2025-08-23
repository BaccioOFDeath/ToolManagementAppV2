using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Models.ImportExport;
using InventoryManagementApp.Utilities;
using InventoryManagementApp.ViewModels;
using Xunit;
using System.Reflection;

namespace InventoryManagementApp.Tests
{
    public class ItemsViewModelTests
    {
        [Fact]
        public void CommandsExistAndExecute()
        {
            var service = new DummyItemService();
            var repository = new DummyItemRepository();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(service, repository, memoryBudget);

            Assert.NotNull(vm.EditItemCommand);
            Assert.True(vm.EditItemCommand.CanExecute(null));
            vm.EditItemCommand.Execute(null);

            Assert.NotNull(vm.ViewDetailsCommand);
            Assert.True(vm.ViewDetailsCommand.CanExecute(null));
            vm.ViewDetailsCommand.Execute(null);

            Assert.NotNull(vm.OpenRentalHistoryCommand);
            Assert.True(vm.OpenRentalHistoryCommand.CanExecute(null));
            vm.OpenRentalHistoryCommand.Execute(null);

            Assert.NotNull(vm.NewItemCommand);
            Assert.True(vm.NewItemCommand.CanExecute(null));
            vm.NewItemCommand.Execute(null);
        }

        [Fact]
        public async Task RapidFilterChangesOnlyLoadsLastRequest()
        {
            var data = new Dictionary<string, List<ItemModel>>
            {
                ["first"] = new() { new ItemModel { ItemID = 1 } },
                ["second"] = new() { new ItemModel { ItemID = 2 } },
                ["third"] = new() { new ItemModel { ItemID = 3 } }
            };
            var service = new RecordingItemService(data);
            var repository = new DummyItemRepository();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(service, repository, memoryBudget);

            vm.Filter = "first";
            await Task.Delay(100);
            vm.Filter = "second";
            await Task.Delay(100);
            vm.Filter = "third";

            await Task.Delay(600);

            Assert.Equal(new[] { "third" }, service.SearchRequests);
            Assert.Single(vm.Items);
            Assert.Equal(3, vm.Items[0].ItemID);
        }

        [Fact]
        public async Task ItemsResetAndReloadOnFilterChange()
        {
            var defaults = new List<ItemModel> { new ItemModel { ItemID = 1 } };
            var data = new Dictionary<string, List<ItemModel>>
            {
                ["new"] = new() { new ItemModel { ItemID = 2 } }
            };
            var service = new RecordingItemService(data, defaults);
            var repository = new DummyItemRepository();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(service, repository, memoryBudget);

            await vm.LoadMoreAsync();
            Assert.Single(vm.Items);
            Assert.Equal(1, vm.Items[0].ItemID);

            vm.Filter = "new";
            await Task.Delay(600);

            Assert.Equal(new[] { "new" }, service.SearchRequests);
            Assert.Equal(1, service.GetCalls);
            Assert.Single(vm.Items);
            Assert.Equal(2, vm.Items[0].ItemID);
            Assert.DoesNotContain(vm.Items, i => i.ItemID == 1);
        }

        [Fact]
        public async Task ConcurrentLoadMoreCallsDoNotDuplicateOrSkipPages()
        {
            var service = new PagingItemService();
            var repository = new DummyItemRepository();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(service, repository, memoryBudget);

            var tasks = new[]
            {
                vm.LoadMoreAsync(),
                vm.LoadMoreAsync(),
                vm.LoadMoreAsync()
            };
            await Task.WhenAll(tasks);

            const int pageSize = 200;
            Assert.Equal(pageSize * 3, vm.Items.Count);
            Assert.Equal(new[] { 1, 2, 3 }, service.Pages);
            Assert.Equal(vm.Items.Count, vm.Items.Select(i => i.ItemID).Distinct().Count());
        }

        [Fact]
        public void DisposeCanBeCalledMultipleTimesAndCancelsToken()
        {
            var service = new DummyItemService();
            var repository = new DummyItemRepository();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            var vm = new ItemsViewModel(service, repository, memoryBudget);
            vm.Items.Add(new ItemModel { ItemID = 1 });
            var ctsField = typeof(ItemsViewModel).GetField("_filterCts", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(ctsField);
            var cts = (CancellationTokenSource)ctsField!.GetValue(vm)!;
            var token = cts.Token;
            vm.Dispose();
            vm.Dispose();
            Assert.True(token.IsCancellationRequested);
            Assert.Empty(vm.Items);
        }

        [Fact]
        public async Task EditsAreQueued()
        {
            var item = new ItemModel { ItemID = 1, QuantityOnHand = 1, Location = "A", Price = 1m };
            var service = new StaticItemService(item);
            var repository = new RecordingItemRepository();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(service, repository, memoryBudget);
            await vm.LoadMoreAsync();
            var loaded = vm.Items[0];
            loaded.QuantityOnHand = 5;
            loaded.Price = 2m;
            Assert.Equal(1, vm.PendingEdits.Count);
            Assert.Empty(repository.Saved);
        }

        [Fact]
        public async Task SaveChangesPersistsQueuedEdits()
        {
            var item = new ItemModel { ItemID = 1, QuantityOnHand = 1, Location = "A", Price = 1m };
            var service = new StaticItemService(item);
            var repository = new RecordingItemRepository();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(service, repository, memoryBudget);
            await vm.LoadMoreAsync();
            var loaded = vm.Items[0];
            loaded.Location = "B";
            await vm.SaveChangesCommand.ExecuteAsync(null);
            Assert.Single(repository.Saved);
            Assert.Empty(vm.PendingEdits);
        }

        private sealed class PagingItemService : IItemService
        {
            private const int PageSize = 200;
            public List<int> Pages { get; } = new();

            public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => Task.FromResult<ItemModel?>(null);

            public IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, CancellationToken cancellationToken = default)
            {
                Pages.Add(page.Number);
                return EnumeratePageAsync(page.Number, cancellationToken);
            }

            public IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
            public Task<int> CountItemsAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(0);
            public Task<bool> ToggleItemCheckOutStatusAsync(int itemID, string currentUser, CancellationToken cancellationToken = default) => Task.FromResult(false);
            public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => Task.FromResult(new List<int>());
            public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
            public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, SqliteConnection? conn = null, SqliteTransaction? tx = null, CancellationToken cancellationToken = default) => Task.CompletedTask;

            private async IAsyncEnumerable<ItemModel> EnumeratePageAsync(int page, [EnumeratorCancellation] CancellationToken ct)
            {
                for (int i = 0; i < PageSize; i++)
                {
                    await Task.Yield();
                    ct.ThrowIfCancellationRequested();
                    yield return new ItemModel { ItemID = (page - 1) * PageSize + i + 1 };
                }
            }
        }

        private sealed class DummyItemService : IItemService
        {
            public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => Task.FromResult<ItemModel?>(null);
            public IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
            public IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
            public Task<int> CountItemsAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(0);
            public Task<bool> ToggleItemCheckOutStatusAsync(int itemID, string currentUser, CancellationToken cancellationToken = default) => Task.FromResult(false);
            public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => Task.FromResult(new List<int>());
            public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
            public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, SqliteConnection? conn = null, SqliteTransaction? tx = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private sealed class RecordingItemService : IItemService
        {
            private readonly Dictionary<string, List<ItemModel>> _searchData;
            private readonly List<ItemModel> _defaultItems;

            public List<string?> SearchRequests { get; } = new();
            public int GetCalls { get; private set; }

            public RecordingItemService(Dictionary<string, List<ItemModel>> searchData, List<ItemModel>? defaultItems = null)
            {
                _searchData = searchData;
                _defaultItems = defaultItems ?? new List<ItemModel>();
            }

            public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => Task.FromResult<ItemModel?>(null);

            public IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, CancellationToken cancellationToken = default)
            {
                GetCalls++;
                return EnumerateAsync(_defaultItems, cancellationToken);
            }

            public IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, CancellationToken cancellationToken = default)
            {
                SearchRequests.Add(searchText);
                _searchData.TryGetValue(searchText ?? string.Empty, out var list);
                list ??= new List<ItemModel>();
                return EnumerateAsync(list, cancellationToken);
            }

            public Task<int> CountItemsAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(0);
            public Task<bool> ToggleItemCheckOutStatusAsync(int itemID, string currentUser, CancellationToken cancellationToken = default) => Task.FromResult(false);
            public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => Task.FromResult(new List<int>());
            public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
            public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, SqliteConnection? conn = null, SqliteTransaction? tx = null, CancellationToken cancellationToken = default) => Task.CompletedTask;

            private async IAsyncEnumerable<ItemModel> EnumerateAsync(List<ItemModel> items, [EnumeratorCancellation] CancellationToken ct)
            {
                foreach (var item in items)
                {
                    await Task.Delay(10, ct);
                    yield return item;
                }
            }
        }

        private sealed class StaticItemService : IItemService
        {
            private readonly ItemModel _item;
            public StaticItemService(ItemModel item) => _item = item;
            public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => Task.FromResult<ItemModel?>(null);
            public IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, CancellationToken cancellationToken = default) => Enumerate(cancellationToken);
            public IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, CancellationToken cancellationToken = default) => Enumerate(cancellationToken);
            public Task<int> CountItemsAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(0);
            public Task<bool> ToggleItemCheckOutStatusAsync(int itemID, string currentUser, CancellationToken cancellationToken = default) => Task.FromResult(false);
            public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => Task.FromResult(new List<int>());
            public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
            public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, SqliteConnection? conn = null, SqliteTransaction? tx = null, CancellationToken cancellationToken = default) => Task.CompletedTask;

            private async IAsyncEnumerable<ItemModel> Enumerate([EnumeratorCancellation] CancellationToken ct)
            {
                await Task.Yield();
                yield return _item;
            }
        }

        private sealed class DummyItemRepository : IItemRepository
        {
            public IAsyncEnumerable<ItemModel> GetPageAsync(ItemFilter filter, ItemPage page, CancellationToken ct) => AsyncEnumerable.Empty<ItemModel>();
            public Task<int> CountAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(0);
            public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct) => Task.CompletedTask;
        }

        private sealed class RecordingItemRepository : IItemRepository
        {
            public List<ItemModel> Saved { get; } = new();
            public IAsyncEnumerable<ItemModel> GetPageAsync(ItemFilter filter, ItemPage page, CancellationToken ct) => AsyncEnumerable.Empty<ItemModel>();
            public Task<int> CountAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(0);
            public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct)
            {
                Saved.AddRange(changes);
                return Task.CompletedTask;
            }
        }
    }
}

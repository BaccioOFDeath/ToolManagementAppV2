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
using System.Windows.Documents;

namespace InventoryManagementApp.Tests
{
    public class ItemsViewModelTests
    {
        [Fact]
        public async Task CommandsExistAndExecute()
        {
            var service = new DummyItemService();
            var repository = new DummyItemRepository();
            var dialog = new DummyDialogService();
            var rental = new DummyRentalService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(service, repository, memoryBudget, dialog, rental);

            Assert.NotNull(vm.EditItemCommand);
            Assert.True(vm.EditItemCommand.CanExecute(null));
            await vm.EditItemCommand.ExecuteAsync(null);

            Assert.NotNull(vm.ViewDetailsCommand);
            Assert.True(vm.ViewDetailsCommand.CanExecute(null));
            vm.ViewDetailsCommand.Execute(null);

            Assert.NotNull(vm.OpenRentalHistoryCommand);
            Assert.True(vm.OpenRentalHistoryCommand.CanExecute(null));
            await vm.OpenRentalHistoryCommand.ExecuteAsync(null);

            Assert.NotNull(vm.NewItemCommand);
            Assert.True(vm.NewItemCommand.CanExecute(null));
            await vm.NewItemCommand.ExecuteAsync(null);
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
            var dialog = new DummyDialogService();
            var rental = new DummyRentalService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(service, repository, memoryBudget, dialog, rental);

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
            var dialog = new DummyDialogService();
            var rental = new DummyRentalService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(service, repository, memoryBudget, dialog, rental);

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
            var dialog = new DummyDialogService();
            var rental = new DummyRentalService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(service, repository, memoryBudget, dialog, rental);

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
            var dialog = new DummyDialogService();
            var rental = new DummyRentalService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            var vm = new ItemsViewModel(service, repository, memoryBudget, dialog, rental);
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
            var dialog = new DummyDialogService();
            var rental = new DummyRentalService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(service, repository, memoryBudget, dialog, rental);
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
            var dialog = new DummyDialogService();
            var rental = new DummyRentalService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(service, repository, memoryBudget, dialog, rental);
            await vm.LoadMoreAsync();
            var loaded = vm.Items[0];
            loaded.Location = "B";
            await vm.SaveChangesCommand.ExecuteAsync(null);
            Assert.Single(repository.Saved);
            Assert.Empty(vm.PendingEdits);
        }

        [Fact]
        public async Task EditItemCommand_InvokesDialogAndService()
        {
            var item = new ItemModel { ItemID = 1 };
            var dialog = new RecordingDialogService { EditItemDialogResult = new ItemModel { ItemID = 1 } };
            var itemService = new RecordingItemService2();
            var repository = new DummyItemRepository();
            var rental = new DummyRentalService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(itemService, repository, memoryBudget, dialog, rental);
            vm.SelectedItem = item;
            await vm.EditItemCommand.ExecuteAsync(null);
            Assert.True(dialog.EditItemDialogCalled);
            Assert.Single(itemService.Updated);
        }

        [Fact]
        public async Task EditItemCommand_HandlesCancellation()
        {
            var item = new ItemModel { ItemID = 1 };
            var dialog = new RecordingDialogService { EditItemDialogResult = new ItemModel { ItemID = 1 } };
            var itemService = new RecordingItemService2 { UpdateException = new OperationCanceledException() };
            var repository = new DummyItemRepository();
            var rental = new DummyRentalService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(itemService, repository, memoryBudget, dialog, rental);
            vm.SelectedItem = item;
            await vm.EditItemCommand.ExecuteAsync(null);
            Assert.True(dialog.EditItemDialogCalled);
            Assert.Single(itemService.Updated);
        }

        [Fact]
        public void ViewDetailsCommand_InvokesDialog()
        {
            var dialog = new RecordingDialogService();
            var itemService = new DummyItemService();
            var repository = new DummyItemRepository();
            var rental = new DummyRentalService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(itemService, repository, memoryBudget, dialog, rental);
            vm.SelectedItem = new ItemModel();
            vm.ViewDetailsCommand.Execute(null);
            Assert.True(dialog.ItemDetailsCalled);
        }

        [Fact]
        public void ViewDetailsCommand_HandlesException()
        {
            var dialog = new RecordingDialogService { DetailsException = new InvalidOperationException() };
            var itemService = new DummyItemService();
            var repository = new DummyItemRepository();
            var rental = new DummyRentalService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(itemService, repository, memoryBudget, dialog, rental);
            vm.SelectedItem = new ItemModel();
            vm.ViewDetailsCommand.Execute(null);
            Assert.True(dialog.ItemDetailsCalled);
        }

        [Fact]
        public async Task OpenRentalHistoryCommand_InvokesServices()
        {
            var item = new ItemModel { ItemID = 1 };
            var dialog = new RecordingDialogService();
            var rentalService = new RecordingRentalService();
            var itemService = new DummyItemService();
            var repository = new DummyItemRepository();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(itemService, repository, memoryBudget, dialog, rentalService);
            vm.SelectedItem = item;
            await vm.OpenRentalHistoryCommand.ExecuteAsync(null);
            Assert.True(rentalService.HistoryCalled);
            Assert.True(dialog.RentalHistoryCalled);
        }

        [Fact]
        public async Task OpenRentalHistoryCommand_HandlesCancellation()
        {
            var item = new ItemModel { ItemID = 1 };
            var dialog = new RecordingDialogService();
            var rentalService = new RecordingRentalService { HistoryException = new OperationCanceledException() };
            var itemService = new DummyItemService();
            var repository = new DummyItemRepository();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(itemService, repository, memoryBudget, dialog, rentalService);
            vm.SelectedItem = item;
            await vm.OpenRentalHistoryCommand.ExecuteAsync(null);
            Assert.True(rentalService.HistoryCalled);
            Assert.False(dialog.RentalHistoryCalled);
        }

        [Fact]
        public async Task NewItemCommand_InvokesDialogAndService()
        {
            var dialog = new RecordingDialogService { EditItemDialogResult = new ItemModel { ItemID = 2 } };
            var itemService = new RecordingItemService2();
            var repository = new DummyItemRepository();
            var rental = new DummyRentalService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(itemService, repository, memoryBudget, dialog, rental);
            await vm.NewItemCommand.ExecuteAsync(null);
            Assert.True(dialog.EditItemDialogCalled);
            Assert.Single(itemService.Added);
        }

        [Fact]
        public async Task NewItemCommand_HandlesCancellation()
        {
            var dialog = new RecordingDialogService { EditItemDialogResult = new ItemModel { ItemID = 2 } };
            var itemService = new RecordingItemService2 { AddException = new OperationCanceledException() };
            var repository = new DummyItemRepository();
            var rental = new DummyRentalService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(itemService, repository, memoryBudget, dialog, rental);
            await vm.NewItemCommand.ExecuteAsync(null);
            Assert.True(dialog.EditItemDialogCalled);
            Assert.Single(itemService.Added);
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

        private sealed class DummyDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => true;
            public ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public void ShowItemDetails(ItemModel item) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }

        private sealed class DummyRentalService : IRentalService
        {
            public Task RentItemAsync(int itemID, int customerID, DateTime rentalDate, DateTime dueDate) => Task.CompletedTask;
            public Task ReturnItemAsync(int rentalID, DateTime returnDate) => Task.CompletedTask;
            public Task ExtendRentalAsync(int rentalID, DateTime newDueDate) => Task.CompletedTask;
            public Task DeleteRentalAsync(int rentalID) => Task.CompletedTask;
            public Task<List<Rental>> GetActiveRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<int> CountActiveRentalsAsync() => Task.FromResult(0);
            public Task<List<Rental>> GetOverdueRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<List<Rental>> GetAllRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<List<Rental>> GetRentalHistoryForItemAsync(int itemID) => Task.FromResult(new List<Rental>());
            public Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID) => Task.FromResult(new List<Rental>());
        }

        private sealed class RecordingDialogService : IDialogService
        {
            public bool EditItemDialogCalled { get; private set; }
            public bool ItemDetailsCalled { get; private set; }
            public bool RentalHistoryCalled { get; private set; }
            public ItemModel? EditItemDialogResult { get; set; }
            public Exception? EditItemDialogException { get; set; }
            public Exception? DetailsException { get; set; }
            public Exception? RentalHistoryException { get; set; }
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => true;
            public ItemModel? ShowEditItemDialog(ItemModel item)
            {
                EditItemDialogCalled = true;
                if (EditItemDialogException != null) throw EditItemDialogException;
                return EditItemDialogResult;
            }
            public void ShowItemDetails(ItemModel item)
            {
                ItemDetailsCalled = true;
                if (DetailsException != null) throw DetailsException;
            }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history)
            {
                if (RentalHistoryException != null) throw RentalHistoryException;
                RentalHistoryCalled = true;
            }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }

        private sealed class RecordingRentalService : IRentalService
        {
            public bool HistoryCalled { get; private set; }
            public Exception? HistoryException { get; set; }
            public Task RentItemAsync(int itemID, int customerID, DateTime rentalDate, DateTime dueDate) => Task.CompletedTask;
            public Task ReturnItemAsync(int rentalID, DateTime returnDate) => Task.CompletedTask;
            public Task ExtendRentalAsync(int rentalID, DateTime newDueDate) => Task.CompletedTask;
            public Task DeleteRentalAsync(int rentalID) => Task.CompletedTask;
            public Task<List<Rental>> GetActiveRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<int> CountActiveRentalsAsync() => Task.FromResult(0);
            public Task<List<Rental>> GetOverdueRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<List<Rental>> GetAllRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<List<Rental>> GetRentalHistoryForItemAsync(int itemID)
            {
                HistoryCalled = true;
                if (HistoryException != null) throw HistoryException;
                return Task.FromResult(new List<Rental>());
            }
            public Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID) => Task.FromResult(new List<Rental>());
        }

        private sealed class RecordingItemService2 : IItemService
        {
            public List<ItemModel> Added { get; } = new();
            public List<ItemModel> Updated { get; } = new();
            public Exception? AddException { get; set; }
            public Exception? UpdateException { get; set; }
            public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default)
            {
                Added.Add(item);
                if (AddException != null) throw AddException;
                return Task.CompletedTask;
            }
            public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default)
            {
                Updated.Add(item);
                if (UpdateException != null) throw UpdateException;
                return Task.CompletedTask;
            }
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
    }
}

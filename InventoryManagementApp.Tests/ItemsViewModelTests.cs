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
using InventoryManagementApp.Models;
using InventoryManagementApp.Utilities;
using InventoryManagementApp.ViewModels;
using Xunit;
using System.Reflection;

namespace InventoryManagementApp.Tests
{
    public class ItemsViewModelTests
    {
        [Fact]
        public async Task EditItemCommandInvokesDialogServiceAndHandlesCancellationAndException()
        {
            var service = new DummyItemService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            var dialog = new RecordingDialogService();
            var rental = new DummyRentalService();
            using var vm = new ItemsViewModel(service, memoryBudget, dialog, rental);
            vm.SelectedItem = new ItemModel { ItemID = 1 };

            using var cts = new CancellationTokenSource();
            cts.Cancel();
            await vm.EditItemCommand.ExecuteAsync(null, cts.Token);
            Assert.Equal(0, dialog.EditItemCalls);

            dialog.ThrowOnEdit = true;
            await vm.EditItemCommand.ExecuteAsync(null);
            Assert.Equal(1, dialog.EditItemCalls);
        }

        [Fact]
        public void ViewDetailsCommandInvokesDialogServiceAndHandlesException()
        {
            var service = new DummyItemService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            var dialog = new RecordingDialogService { ThrowOnDetails = true };
            var rental = new DummyRentalService();
            using var vm = new ItemsViewModel(service, memoryBudget, dialog, rental);
            vm.SelectedItem = new ItemModel { ItemID = 2 };

            vm.ViewDetailsCommand.Execute(null);
            Assert.Equal(1, dialog.ViewDetailsCalls);
        }

        [Fact]
        public async Task OpenRentalHistoryCommandInvokesServicesAndHandlesCancellationAndException()
        {
            var service = new DummyItemService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            var dialog = new RecordingDialogService();
            var rental = new RecordingRentalService();
            using var vm = new ItemsViewModel(service, memoryBudget, dialog, rental);
            vm.SelectedItem = new ItemModel { ItemID = 5 };

            using var cts = new CancellationTokenSource();
            cts.Cancel();
            await vm.OpenRentalHistoryCommand.ExecuteAsync(null, cts.Token);
            Assert.Equal(0, rental.Calls);

            rental.ThrowOnGetHistory = true;
            await vm.OpenRentalHistoryCommand.ExecuteAsync(null);
            Assert.Equal(1, rental.Calls);
            Assert.Equal(1, dialog.RentalHistoryCalls);
        }

        [Fact]
        public async Task NewItemCommandInvokesDialogServiceAndHandlesCancellationAndException()
        {
            var service = new DummyItemService();
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            var dialog = new RecordingDialogService();
            var rental = new DummyRentalService();
            using var vm = new ItemsViewModel(service, memoryBudget, dialog, rental);

            using var cts = new CancellationTokenSource();
            cts.Cancel();
            await vm.NewItemCommand.ExecuteAsync(null, cts.Token);
            Assert.Equal(0, dialog.EditItemCalls);

            dialog.ThrowOnEdit = true;
            await vm.NewItemCommand.ExecuteAsync(null);
            Assert.Equal(1, dialog.EditItemCalls);
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
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(service, memoryBudget, new DummyDialogService(), new DummyRentalService());

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
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(service, memoryBudget, new DummyDialogService(), new DummyRentalService());

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
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            using var vm = new ItemsViewModel(service, memoryBudget, new DummyDialogService(), new DummyRentalService());

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
            using var memoryBudget = new MemoryBudget(TimeSpan.FromMinutes(1), long.MaxValue);
            var vm = new ItemsViewModel(service, memoryBudget, new DummyDialogService(), new DummyRentalService());
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

        private class DummyDialogService : IDialogService
        {
            public virtual void ShowInfo(string message, string title) { }
            public virtual bool ShowConfirmation(string message, string title) => false;
            public virtual ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public virtual void ShowItemDetails(ItemModel item) { }
            public virtual (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public virtual CustomerModel? ShowAddCustomerDialog() => null;
            public virtual void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public virtual void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public virtual Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public virtual Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public virtual void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public virtual void ShowPrintLabelDialog() { }
        }

        private sealed class RecordingDialogService : DummyDialogService
        {
            public int EditItemCalls { get; private set; }
            public int ViewDetailsCalls { get; private set; }
            public int RentalHistoryCalls { get; private set; }
            public bool ThrowOnEdit { get; set; }
            public bool ThrowOnDetails { get; set; }

            public override ItemModel? ShowEditItemDialog(ItemModel item)
            {
                EditItemCalls++;
                if (ThrowOnEdit) throw new InvalidOperationException();
                return item;
            }

            public override void ShowItemDetails(ItemModel item)
            {
                ViewDetailsCalls++;
                if (ThrowOnDetails) throw new InvalidOperationException();
            }

            public override void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history)
            {
                RentalHistoryCalls++;
            }
        }

        private class DummyRentalService : IRentalService
        {
            public virtual Task RentItemAsync(int itemID, int customerID, DateTime rentalDate, DateTime dueDate) => Task.CompletedTask;
            public virtual Task ReturnItemAsync(int rentalID, DateTime returnDate) => Task.CompletedTask;
            public virtual Task ExtendRentalAsync(int rentalID, DateTime newDueDate) => Task.CompletedTask;
            public virtual Task DeleteRentalAsync(int rentalID) => Task.CompletedTask;
            public virtual Task<List<Rental>> GetActiveRentalsAsync() => Task.FromResult(new List<Rental>());
            public virtual Task<int> CountActiveRentalsAsync() => Task.FromResult(0);
            public virtual Task<List<Rental>> GetOverdueRentalsAsync() => Task.FromResult(new List<Rental>());
            public virtual Task<List<Rental>> GetAllRentalsAsync() => Task.FromResult(new List<Rental>());
            public virtual Task<List<Rental>> GetRentalHistoryForItemAsync(int itemID) => Task.FromResult(new List<Rental>());
            public virtual Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID) => Task.FromResult(new List<Rental>());
        }

        private sealed class RecordingRentalService : DummyRentalService
        {
            public int Calls { get; private set; }
            public bool ThrowOnGetHistory { get; set; }

            public override Task<List<Rental>> GetRentalHistoryForItemAsync(int itemID)
            {
                Calls++;
                if (ThrowOnGetHistory) throw new InvalidOperationException();
                return Task.FromResult(new List<Rental>());
            }
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
    }
}

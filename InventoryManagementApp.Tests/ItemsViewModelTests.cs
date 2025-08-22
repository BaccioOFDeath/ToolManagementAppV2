using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using Xunit;

public class ItemsViewModelTests
{
    private sealed class FakeItemService : IItemService
    {
        public IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, CancellationToken cancellationToken = default)
            => CreateAsync(page, null, cancellationToken);

        public IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, CancellationToken cancellationToken = default)
            => CreateAsync(page, searchText, cancellationToken);

        private async IAsyncEnumerable<ItemModel> CreateAsync(ItemPage page, string? filter, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
        {
            var start = (page.Number - 1) * page.Size;
            for (int i = 0; i < page.Size; i++)
            {
                ct.ThrowIfCancellationRequested();
                var idx = start + i;
                var name = $"Item {idx}";
                if (string.IsNullOrWhiteSpace(filter) || name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    yield return new ItemModel { ItemID = idx, NameDescription = name };
                await Task.Yield();
            }
        }

        public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ToggleItemCheckOutStatusAsync(int itemID, string currentUser, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    [StaFact]
    public async Task FilterReloadsItemsAfterDelay()
    {
        var service = new FakeItemService();
        var vm = new ItemsViewModel(service);
        await vm.LoadMoreAsync();
        var first = vm.Items[0].NameDescription;
        vm.Filter = "5";
        await Task.Delay(400);
        var filteredFirst = vm.Items[0].NameDescription;
        Assert.Contains("5", filteredFirst);
        Assert.NotEqual(first, filteredFirst);
    }
}

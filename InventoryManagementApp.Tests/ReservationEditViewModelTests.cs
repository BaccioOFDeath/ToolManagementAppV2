using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.ImportExport;
using InventoryManagementApp.ViewModels;
using Microsoft.Data.Sqlite;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReservationEditViewModelTests
    {
        [Fact]
        public async Task SearchItemsCommand_LoadsMatchesAndAppliesSelectedItem()
        {
            var reservation = new Reservation
            {
                ItemNumber = "OLD",
                ItemName = "Old item"
            };
            var item = new ItemModel
            {
                ItemID = 8,
                ItemNumber = "TW-8",
                Name = "Torque wrench",
                Location = "Rack 4"
            };
            var vm = new ReservationEditViewModel(
                reservation,
                isNew: true,
                onSave: () => { },
                onCancel: () => { },
                new StubItemService(item));

            vm.ItemSearchText = "torque";
            await vm.SearchItemsCommand.ExecutionTask!;

            var match = Assert.Single(vm.ItemSearchResults);
            Assert.Same(item, match);
            Assert.Same(item, vm.SelectedSearchItem);

            vm.ApplySelectedItemCommand.Execute(null);

            Assert.Equal("TW-8", reservation.ItemNumber);
            Assert.Equal("Torque wrench", reservation.ItemName);
        }

        private sealed class StubItemService : IItemService
        {
            readonly ItemModel _item;

            public StubItemService(ItemModel item)
            {
                _item = item;
            }

            public async IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                await Task.Yield();
                yield return _item;
            }

            public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<int> CountItemsAsync(ItemFilter filter, CancellationToken ct) => throw new NotImplementedException();
            public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct) => throw new NotImplementedException();
            public Task<bool> ToggleItemCheckOutStatusAsync(int itemID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<ItemModel>> GetCheckedOutItemsAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<int>> ImportItemsAsync(string filePath, IDataImporter<ItemModel> importer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task ExportItemsAsync(string filePath, IDataExporter<ItemModel> exporter, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, SqliteConnection? conn = null, SqliteTransaction? tx = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<ItemModel>> GetMostCommonlyUsedItemsAsync(int limit, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<ItemModel>> GetIncompleteItemsAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        }
    }
}

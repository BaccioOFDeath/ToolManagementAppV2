using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.ImportExport;

namespace InventoryManagementApp.Interfaces
{
    public interface IItemService
    {
        Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default);
        Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default);
        Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default);
        Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default);
        IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, CancellationToken cancellationToken = default);
        IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, CancellationToken cancellationToken = default);
        Task<int> CountItemsAsync(ItemFilter filter, CancellationToken ct);
        Task<bool> ToggleItemCheckOutStatusAsync(int itemID, string currentUser, CancellationToken cancellationToken = default);
        Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default);
        Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default);
        Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken);
        Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default);
        Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default);
        Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default);
        Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental,
            SQLiteConnection? conn = null, SQLiteTransaction? tx = null, CancellationToken cancellationToken = default);
    }
}

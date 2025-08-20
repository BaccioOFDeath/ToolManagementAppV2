using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.ImportExport;

namespace ToolManagementAppV2.Interfaces
{
    public interface IItemService
    {
        Task AddToolAsync(ItemModel tool, CancellationToken cancellationToken = default);
        Task UpdateToolAsync(ItemModel tool, CancellationToken cancellationToken = default);
        Task DeleteToolAsync(int toolID, CancellationToken cancellationToken = default);
        Task<ItemModel?> GetToolByIDAsync(int toolID, CancellationToken cancellationToken = default);
        Task<List<ItemModel>> GetAllToolsAsync(CancellationToken cancellationToken = default);
        Task<List<ItemModel>> SearchToolsAsync(string? searchText, CancellationToken cancellationToken = default);
        Task<bool> ToggleToolCheckOutStatusAsync(int toolID, string currentUser, CancellationToken cancellationToken = default);
        Task<List<ItemModel>> GetToolsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default);
        Task UpdateToolImageAsync(int toolID, string imagePath, CancellationToken cancellationToken = default);
        Task<List<int>> ImportToolsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken);
        Task ExportToolsToCsvAsync(string filePath, CancellationToken cancellationToken = default);
        Task<ImageImportResult> ImportToolImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default);
        Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default);
        Task UpdateToolQuantitiesAsync(int toolID, int qtyChange, bool isRental,
            SQLiteConnection? conn = null, SQLiteTransaction? tx = null, CancellationToken cancellationToken = default);
    }
}

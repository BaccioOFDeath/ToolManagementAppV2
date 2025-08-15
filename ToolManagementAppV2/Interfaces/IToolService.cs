using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.ImportExport;

namespace ToolManagementAppV2.Interfaces
{
    public interface IToolService
    {
        Task AddToolAsync(ToolModel tool, CancellationToken cancellationToken = default);
        Task UpdateToolAsync(ToolModel tool, CancellationToken cancellationToken = default);
        Task DeleteToolAsync(int toolID, CancellationToken cancellationToken = default);
        Task<ToolModel?> GetToolByIDAsync(int toolID, CancellationToken cancellationToken = default);
        Task<List<ToolModel>> GetAllToolsAsync(CancellationToken cancellationToken = default);
        Task<List<ToolModel>> SearchToolsAsync(string? searchText, CancellationToken cancellationToken = default);
        Task<bool> ToggleToolCheckOutStatusAsync(int toolID, string currentUser, CancellationToken cancellationToken = default);
        Task<List<ToolModel>> GetToolsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default);
        Task UpdateToolImageAsync(int toolID, string imagePath, CancellationToken cancellationToken = default);
        Task<List<int>> ImportToolsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken);
        Task ExportToolsToCsvAsync(string filePath, CancellationToken cancellationToken = default);
        Task<ImageImportResult> ImportToolImagesAsync(string folderPath, Func<ToolModel, IEnumerable<string>> keySelector, CancellationToken cancellationToken = default);
        Task UpdateToolQuantitiesAsync(int toolID, int qtyChange, bool isRental,
            SQLiteConnection? conn = null, SQLiteTransaction? tx = null, CancellationToken cancellationToken = default);
    }
}

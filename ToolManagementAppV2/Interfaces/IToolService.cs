using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.ImportExport;

namespace ToolManagementAppV2.Interfaces
{
    public interface IToolService
    {
        void AddTool(ToolModel tool);
        Task AddToolAsync(ToolModel tool);
        void UpdateTool(ToolModel tool);
        Task UpdateToolAsync(ToolModel tool);
        void DeleteTool(int toolID);
        Task DeleteToolAsync(int toolID);
        ToolModel GetToolByID(int toolID);
        Task<ToolModel> GetToolByIDAsync(int toolID);
        List<ToolModel> GetAllTools();
        Task<List<ToolModel>> GetAllToolsAsync();
        List<ToolModel> SearchTools(string? searchText);
        Task<List<ToolModel>> SearchToolsAsync(string? searchText);
        void ToggleToolCheckOutStatus(int toolID, string currentUser);
        Task ToggleToolCheckOutStatusAsync(int toolID, string currentUser);
        List<ToolModel> GetToolsCheckedOutBy(string userName);
        Task<List<ToolModel>> GetToolsCheckedOutByAsync(string userName);
        void UpdateToolImage(int toolID, string imagePath);
        Task UpdateToolImageAsync(int toolID, string imagePath);
        List<int> ImportToolsFromCsv(string filePath, IDictionary<string, string> map);
        Task<List<int>> ImportToolsFromCsvAsync(string filePath, IDictionary<string, string> map);
        void ExportToolsToCsv(string filePath);
        Task ExportToolsToCsvAsync(string filePath);
        ImageImportResult ImportToolImages(string folderPath, Func<ToolModel, IEnumerable<string>> keySelector);
        Task<ImageImportResult> ImportToolImagesAsync(string folderPath, Func<ToolModel, IEnumerable<string>> keySelector);
        void UpdateToolQuantities(int toolID, int qtyChange, bool isRental,
            SQLiteConnection? conn = null, SQLiteTransaction? tx = null);
        Task UpdateToolQuantitiesAsync(int toolID, int qtyChange, bool isRental,
            SQLiteConnection? conn = null, SQLiteTransaction? tx = null);
    }
}


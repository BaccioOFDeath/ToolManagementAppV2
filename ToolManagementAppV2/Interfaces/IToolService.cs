using System;
using System.Collections.Generic;
using System.Data.SQLite;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.ImportExport;

namespace ToolManagementAppV2.Interfaces
{
    public interface IToolService
    {
        void AddTool(ToolModel tool);
        void UpdateTool(ToolModel tool);
        void DeleteTool(int toolID);
        ToolModel GetToolByID(int toolID);
        List<ToolModel> GetAllTools();
        List<ToolModel> SearchTools(string? searchText);
        void ToggleToolCheckOutStatus(int toolID, string currentUser);
        List<ToolModel> GetToolsCheckedOutBy(string userName);
        void UpdateToolImage(int toolID, string imagePath);
        List<int> ImportToolsFromCsv(string filePath, IDictionary<string, string> map);
        void ExportToolsToCsv(string filePath);
        ImageImportResult ImportToolImages(string folderPath, Func<ToolModel, IEnumerable<string>> keySelector);
        void UpdateToolQuantities(int toolID, int qtyChange, bool isRental,
            SQLiteConnection? conn = null, SQLiteTransaction? tx = null);
    }
}


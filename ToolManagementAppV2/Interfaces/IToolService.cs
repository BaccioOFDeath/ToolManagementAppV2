using System.Collections.Generic;
using ToolManagementAppV2.Models;


namespace ToolManagementAppV2.Interfaces
{
    internal interface IToolService
    {
        void AddTool(ToolModel tool);
        void UpdateTool(ToolModel tool);
        void DeleteTool(string toolID);
        ToolModel GetToolByID(string toolID);
        List<ToolModel> GetAllTools();
        List<ToolModel> SearchTools(string searchText);
        void ToggleToolCheckOutStatus(string toolID, string currentUser);
        List<ToolModel> GetToolsCheckedOutBy(string userName);
        void UpdateToolImage(string toolID, string imagePath);
        void ImportToolsFromCsv(string filePath, IDictionary<string, string> map);
        void ExportToolsToCsv(string filePath);
    }
}

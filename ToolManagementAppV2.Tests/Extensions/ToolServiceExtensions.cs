using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.ImportExport;
using ToolModel = ToolManagementAppV2.Models.Domain.Tool;

namespace ToolManagementAppV2.Tests.Extensions
{
    public static class ToolServiceExtensions
    {
        public static void AddTool(this IToolService service, ToolModel tool) =>
            service.AddToolAsync(tool).GetAwaiter().GetResult();

        public static void UpdateTool(this IToolService service, ToolModel tool) =>
            service.UpdateToolAsync(tool).GetAwaiter().GetResult();

        public static void DeleteTool(this IToolService service, int toolID) =>
            service.DeleteToolAsync(toolID).GetAwaiter().GetResult();

        public static ToolModel? GetToolByID(this IToolService service, int toolID) =>
            service.GetToolByIDAsync(toolID).GetAwaiter().GetResult();

        public static List<ToolModel> GetAllTools(this IToolService service) =>
            service.GetAllToolsAsync().GetAwaiter().GetResult();

        public static List<ToolModel> SearchTools(this IToolService service, string? searchText) =>
            service.SearchToolsAsync(searchText).GetAwaiter().GetResult();

        public static bool ToggleToolCheckOutStatus(this IToolService service, int toolID, string currentUser) =>
            service.ToggleToolCheckOutStatusAsync(toolID, currentUser).GetAwaiter().GetResult();

        public static List<ToolModel> GetToolsCheckedOutBy(this IToolService service, string userName) =>
            service.GetToolsCheckedOutByAsync(userName).GetAwaiter().GetResult();

        public static void UpdateToolImage(this IToolService service, int toolID, string imagePath) =>
            service.UpdateToolImageAsync(toolID, imagePath).GetAwaiter().GetResult();

        public static List<int> ImportToolsFromCsv(this IToolService service, string filePath, IDictionary<string, string> map) =>
            service.ImportToolsFromCsvAsync(filePath, map, CancellationToken.None).GetAwaiter().GetResult();

        public static void ExportToolsToCsv(this IToolService service, string filePath) =>
            service.ExportToolsToCsvAsync(filePath, CancellationToken.None).GetAwaiter().GetResult();

        public static ImageImportResult ImportToolImages(this IToolService service, string folderPath, Func<ToolModel, IEnumerable<string>> selector) =>
            service.ImportToolImagesAsync(folderPath, selector, CancellationToken.None).GetAwaiter().GetResult();

        public static void UpdateToolQuantities(this IToolService service, int toolID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null) =>
            service.UpdateToolQuantitiesAsync(toolID, qtyChange, isRental, conn, tx, CancellationToken.None).GetAwaiter().GetResult();
    }
}

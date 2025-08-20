using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.ImportExport;
using ItemModel = ToolManagementAppV2.Models.Domain.ItemModel;

namespace ToolManagementAppV2.Tests.Extensions
{
    public static class ItemServiceExtensions
    {
        public static void AddTool(this IItemService service, ItemModel tool) =>
            service.AddToolAsync(tool).GetAwaiter().GetResult();

        public static void UpdateTool(this IItemService service, ItemModel tool) =>
            service.UpdateToolAsync(tool).GetAwaiter().GetResult();

        public static void DeleteTool(this IItemService service, int toolID) =>
            service.DeleteToolAsync(toolID).GetAwaiter().GetResult();

        public static ItemModel? GetToolByID(this IItemService service, int toolID) =>
            service.GetToolByIDAsync(toolID).GetAwaiter().GetResult();

        public static List<ItemModel> GetAllTools(this IItemService service) =>
            service.GetAllToolsAsync().GetAwaiter().GetResult();

        public static List<ItemModel> SearchTools(this IItemService service, string? searchText) =>
            service.SearchToolsAsync(searchText).GetAwaiter().GetResult();
        public static string GenerateNextItemNumber(this IItemService service) =>
            service.GenerateNextItemNumberAsync().GetAwaiter().GetResult();

        public static bool ToggleToolCheckOutStatus(this IItemService service, int toolID, string currentUser) =>
            service.ToggleToolCheckOutStatusAsync(toolID, currentUser).GetAwaiter().GetResult();

        public static List<ItemModel> GetToolsCheckedOutBy(this IItemService service, string userName) =>
            service.GetToolsCheckedOutByAsync(userName).GetAwaiter().GetResult();

        public static void UpdateToolImage(this IItemService service, int toolID, string imagePath) =>
            service.UpdateToolImageAsync(toolID, imagePath).GetAwaiter().GetResult();

        public static List<int> ImportToolsFromCsv(this IItemService service, string filePath, IDictionary<string, string> map) =>
            service.ImportToolsFromCsvAsync(filePath, map, CancellationToken.None).GetAwaiter().GetResult();

        public static void ExportToolsToCsv(this IItemService service, string filePath) =>
            service.ExportToolsToCsvAsync(filePath, CancellationToken.None).GetAwaiter().GetResult();

        public static ImageImportResult ImportToolImages(this IItemService service, string folderPath, Func<ItemModel, IEnumerable<string>> selector) =>
            service.ImportToolImagesAsync(folderPath, selector, null, CancellationToken.None).GetAwaiter().GetResult();

        public static void UpdateToolQuantities(this IItemService service, int toolID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null) =>
            service.UpdateToolQuantitiesAsync(toolID, qtyChange, isRental, conn, tx, CancellationToken.None).GetAwaiter().GetResult();
    }
}

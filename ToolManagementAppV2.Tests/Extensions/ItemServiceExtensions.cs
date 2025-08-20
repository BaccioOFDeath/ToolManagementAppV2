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
        public static void AddItem(this IItemService service, ItemModel item) =>
            service.AddItemAsync(item).GetAwaiter().GetResult();

        public static void UpdateItem(this IItemService service, ItemModel item) =>
            service.UpdateItemAsync(item).GetAwaiter().GetResult();

        public static void DeleteItem(this IItemService service, int itemID) =>
            service.DeleteItemAsync(itemID).GetAwaiter().GetResult();

        public static ItemModel? GetItemByID(this IItemService service, int itemID) =>
            service.GetItemByIDAsync(itemID).GetAwaiter().GetResult();

        public static List<ItemModel> GetAllItems(this IItemService service) =>
            service.GetAllItemsAsync().GetAwaiter().GetResult();

        public static List<ItemModel> SearchItems(this IItemService service, string? searchText) =>
            service.SearchItemsAsync(searchText).GetAwaiter().GetResult();
        public static string GenerateNextItemNumber(this IItemService service) =>
            service.GenerateNextItemNumberAsync().GetAwaiter().GetResult();

        public static bool ToggleItemCheckOutStatus(this IItemService service, int itemID, string currentUser) =>
            service.ToggleItemCheckOutStatusAsync(itemID, currentUser).GetAwaiter().GetResult();

        public static List<ItemModel> GetItemsCheckedOutBy(this IItemService service, string userName) =>
            service.GetItemsCheckedOutByAsync(userName).GetAwaiter().GetResult();

        public static void UpdateItemImage(this IItemService service, int itemID, string imagePath) =>
            service.UpdateItemImageAsync(itemID, imagePath).GetAwaiter().GetResult();

        public static List<int> ImportItemsFromCsv(this IItemService service, string filePath, IDictionary<string, string> map) =>
            service.ImportItemsFromCsvAsync(filePath, map, CancellationToken.None).GetAwaiter().GetResult();

        public static void ExportItemsToCsv(this IItemService service, string filePath) =>
            service.ExportItemsToCsvAsync(filePath, CancellationToken.None).GetAwaiter().GetResult();

        public static ImageImportResult ImportItemImages(this IItemService service, string folderPath, Func<ItemModel, IEnumerable<string>> selector) =>
            service.ImportItemImagesAsync(folderPath, selector, null, CancellationToken.None).GetAwaiter().GetResult();

        public static void UpdateItemQuantities(this IItemService service, int itemID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null) =>
            service.UpdateItemQuantitiesAsync(itemID, qtyChange, isRental, conn, tx, CancellationToken.None).GetAwaiter().GetResult();
    }
}

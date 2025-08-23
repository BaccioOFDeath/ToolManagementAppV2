using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Utilities.IO;
using InventoryManagementApp.Models.ImportExport;
using InventoryManagementApp.Interfaces;
using System.Text;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Data;
using Microsoft.VisualBasic.FileIO;

namespace InventoryManagementApp.Services.Items
{
    public class ItemService : IItemService
    {
        readonly DatabaseService _dbService;
        readonly IItemRepository _repository;
        const string UpsertItemCsv = @"
            INSERT INTO Items
              (ItemNumber, NameDescription, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, Keywords, AvailableQuantity, RentedQuantity, IsRentalItem, ImagePath, IsCheckedOut, IsPowered)
            VALUES (@ItemNumber,@Desc,@Loc,@Brand,@PN,@Sup,@PD,@Notes,@Keywords,@Avail,@Rent,@Rental,@Img,0,@Powered);
            SELECT last_insert_rowid();";
        const int MaxQuantityOnHand = 10000;
    
        readonly ILogger<ItemService> _logger;
        readonly IAuthorizationService _auth;
        readonly ActivityLogService? _activityLog;
        readonly IUserContext? _context;

        public ItemService(DatabaseService dbService, IItemRepository repository, IAuthorizationService? authorizationService = null, ILogger<ItemService>? logger = null, ActivityLogService? activityLogService = null, IUserContext? userContext = null)
        {
            _dbService = dbService;
            _repository = repository;
            _auth = authorizationService ?? new NoOpAuthorizationService();
            _logger = logger ?? NullLogger<ItemService>.Instance;
            _activityLog = activityLogService;
            _context = userContext;
        }

        static void ValidateQuantity(int quantity)
        {
            if (quantity < 0 || quantity > MaxQuantityOnHand)
                throw new ArgumentOutOfRangeException(nameof(ItemModel.QuantityOnHand), $"QuantityOnHand must be between 0 and {MaxQuantityOnHand}.");
        }

        public async Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            await AddItemInternalAsync(item, cancellationToken).ConfigureAwait(false);
            if (_activityLog != null)
            {
                var user = _context?.CurrentUser;
                await _activityLog.LogActionAsync(user?.UserID ?? 0, user?.UserName ?? string.Empty, $"Added item {item.ItemNumber}", cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            await UpdateItemInternalAsync(item, cancellationToken).ConfigureAwait(false);
            if (_activityLog != null)
            {
                var user = _context?.CurrentUser;
                await _activityLog.LogActionAsync(user?.UserID ?? 0, user?.UserName ?? string.Empty, $"Updated item {item.ItemNumber}", cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            await DeleteItemInternalAsync(itemID, cancellationToken).ConfigureAwait(false);
            if (_activityLog != null)
            {
                var user = _context?.CurrentUser;
                await _activityLog.LogActionAsync(user?.UserID ?? 0, user?.UserName ?? string.Empty, $"Deleted item {itemID}", cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<bool> ToggleItemCheckOutStatusAsync(int itemID, string currentUser, CancellationToken cancellationToken = default)
        {
            if (_context?.CurrentUser == null)
                throw new InvalidOperationException("Current user is not available.");

            var caller = _context.UserName;
            var result = await ToggleItemCheckOutStatusInternalAsync(itemID, caller, cancellationToken).ConfigureAwait(false);
            if (result && _activityLog != null)
            {
                int userId = _context.CurrentUser?.UserID ?? 0;
                await _activityLog.LogActionAsync(userId, caller, $"Toggled item {itemID} check-out status", cancellationToken).ConfigureAwait(false);
            }
            return result;
        }

        public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default)
        {
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReaderAsync(conn,
                "SELECT * FROM Items WHERE CheckedOutBy=@User AND IsCheckedOut=1",
                MapItem,
                new[] { new SqliteParameter("@User", userName) }, cancellationToken);
        }

        public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            const string sql = "UPDATE Items SET ImagePath=@Img WHERE ItemID=@ID";
            var p = new[]
            {
                new SqliteParameter("@Img", imagePath),
                new SqliteParameter("@ID", itemID)
            };
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken);
        }

        public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken)
        {
            _auth.EnsureAdmin();
            return ImportItemsFromCsvInternalAsync(filePath, map, cancellationToken);
        }

        public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default)
            => ExportItemsToCsvInternalAsync(filePath, cancellationToken);

        public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            return ImportItemImagesInternalAsync(folderPath, keySelector, progress, cancellationToken);
        }

        public async Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT IFNULL(MAX(CAST(SUBSTR(ItemNumber, 2) AS INTEGER)), 0) FROM Items WHERE ItemNumber LIKE 'T%'";
            using var conn = _dbService.CreateConnection();
            var result = await SqliteHelper.ExecuteScalarAsync(conn, sql, null, cancellationToken);
            var max = result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            return $"T{max + 1}";
        }

        async Task InsertItemAsync(SqliteConnection conn, SqliteTransaction? tran, ItemModel item, CancellationToken cancellationToken)
        {
            ValidateQuantity(item.QuantityOnHand);
            var p = new[]
            {
                new SqliteParameter("@ItemNumber", item.ItemNumber),
                new SqliteParameter("@Desc", (object)item.Name ?? DBNull.Value),
                new SqliteParameter("@Loc", item.Location),
                new SqliteParameter("@Brand", item.Brand),
                new SqliteParameter("@PN", item.PartNumber),
                new SqliteParameter("@Sup", (object)item.Supplier ?? DBNull.Value),
                new SqliteParameter("@PD", (object)item.PurchasedDate ?? DBNull.Value),
                new SqliteParameter("@Notes", (object)item.Notes ?? DBNull.Value),
                new SqliteParameter("@Keywords", (object)item.Keywords ?? DBNull.Value),
                new SqliteParameter("@Avail", item.QuantityOnHand),
                new SqliteParameter("@Rent", item.RentedQuantity),
                new SqliteParameter("@Rental", item.IsRentalItem ? 1 : 0),
                new SqliteParameter("@Img", (object)item.ImagePath ?? DBNull.Value),
                new SqliteParameter("@Powered", item.IsPowered ? 1 : 0)
            };
            using var cmd = new SqliteCommand(UpsertItemCsv, conn, tran);
            cmd.Parameters.AddRange(p);
            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            if (result != null)
                item.ItemID = Convert.ToInt32(result);
        }
    
        private async Task<ImageImportResult> ImportItemImagesInternalAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress, CancellationToken cancellationToken)
        {
            var result = new ImageImportResult();
            if (string.IsNullOrWhiteSpace(folderPath) || keySelector == null)
                return result;

            cancellationToken.ThrowIfCancellationRequested();
            var items = new List<ItemModel>();
            await foreach (var item in GetItemsAsync(new ItemPage(1, int.MaxValue), SortField.Name, SortDirection.Ascending, cancellationToken: cancellationToken))
                items.Add(item);
            var groups = new Dictionary<string, List<ItemModel>>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var keys = keySelector(item);
                if (keys == null) continue;
                foreach (var key in keys)
                {
                    var k = (key ?? string.Empty).Trim().ToUpperInvariant();
                    if (string.IsNullOrEmpty(k))
                        continue;
                    if (!groups.TryGetValue(k, out var list))
                        groups[k] = list = new List<ItemModel>();
                    list.Add(item);
                }
            }

            var destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ItemImages");
            if (!Directory.Exists(destDir))
            {
                try
                {
                    Directory.CreateDirectory(destDir);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create image directory {Dir}", destDir);
                    return result;
                }
            }

            var supported = new HashSet<string>(new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" }, StringComparer.OrdinalIgnoreCase);

            var files = await Task.Run(() => Directory.EnumerateFiles(folderPath).ToList(), cancellationToken);
            var total = files.Count;
            var processed = 0;
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var ext = Path.GetExtension(file);
                if (!supported.Contains(ext))
                    continue;

                var name = Path.GetFileNameWithoutExtension(file).ToUpperInvariant();
                if (!groups.TryGetValue(name, out var list) || list.Count == 0)
                {
                    result.UnmatchedFiles.Add(file);
                    continue;
                }
                if (list.Count > 1)
                {
                    result.ConflictingFiles.Add(file);
                    continue;
                }
                var item = list[0];
                if (!string.IsNullOrEmpty(item.ImagePath))
                {
                    result.ConflictingFiles.Add(file);
                    continue;
                }
                var dest = Path.Combine(destDir, Path.GetFileName(file));
                if (!File.Exists(dest))
                {
                    try
                    {
                        await CopyFileAsync(file, dest, cancellationToken);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, "Failed to copy image from {Source} to {Destination}", file, dest);
                        result.ConflictingFiles.Add(file);
                        continue;
                    }
                }
                var relative = $"ItemImages/{Path.GetFileName(dest)}";
                await UpdateItemImageAsync(item.ItemID, relative, cancellationToken);
                result.ImportedCount++;
                processed++;
                progress?.Report(new ImageImportProgress { Processed = processed, Total = total });
            }

            return result;
        }

        protected virtual async Task CopyFileAsync(string sourceFileName, string destFileName, CancellationToken cancellationToken)
        {
            await using var source = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
            await using var destination = new FileStream(destFileName, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
            await source.CopyToAsync(destination, cancellationToken);
        }

        private async Task<bool> ItemExistsAsync(string itemNumber, int? exceptId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(itemNumber))
                return false;

            var sql = "SELECT COUNT(*) FROM Items WHERE ItemNumber = @TN";
            var parameters = new List<SqliteParameter>
            {
                new("@TN", itemNumber)
            };

            if (exceptId.HasValue)
            {
                sql += " AND ItemID <> @ID";
                parameters.Add(new SqliteParameter("@ID", exceptId.Value));
            }

            using var conn = _dbService.CreateConnection();
            var count = Convert.ToInt32(await SqliteHelper.ExecuteScalarAsync(conn, sql, parameters.ToArray(), cancellationToken));
            return count > 0;
        }
    
        ItemModel MapItem(IDataRecord r) => new()
        {
            ItemID = r["ItemID"] is DBNull ? 0 : Convert.ToInt32(r["ItemID"]),
            ItemNumber = r["ItemNumber"].ToString(),
            PartNumber = r["PartNumber"].ToString(),
            Name = r["NameDescription"].ToString(),
            Brand = r["Brand"].ToString(),
            Location = r["Location"].ToString(),
            QuantityOnHand = r["AvailableQuantity"] is DBNull ? 0 : Convert.ToInt32(r["AvailableQuantity"]),
            RentedQuantity = r["RentedQuantity"] is DBNull ? 0 : Convert.ToInt32(r["RentedQuantity"]),
            Supplier = r["Supplier"].ToString(),
            PurchasedDate = r["PurchasedDate"] is DBNull
                ? (DateTime?)null
                : ParseNullableDate(r["PurchasedDate"], "PurchasedDate"),
            Notes = r["Notes"].ToString(),
            IsCheckedOut = (r["IsCheckedOut"] is DBNull ? 0 : Convert.ToInt32(r["IsCheckedOut"])) == 1,
            CheckedOutBy = r["CheckedOutBy"].ToString(),
            CheckedOutTime = r["CheckedOutTime"] is DBNull
                ? (DateTime?)null
                : ParseNullableDate(r["CheckedOutTime"], "CheckedOutTime"),
            ImagePath = r["ImagePath"]?.ToString(),
            Keywords = r["Keywords"]?.ToString(),
            IsPowered = (r["IsPowered"] is DBNull ? 0 : Convert.ToInt32(r["IsPowered"])) == 1,
            IsRentalItem = (r["IsRentalItem"] is DBNull ? 0 : Convert.ToInt32(r["IsRentalItem"])) == 1,
            UpdatedAt = ParseNullableDate(r["UpdatedAt"], "UpdatedAt") ?? default
        };

        DateTime? ParseNullableDate(object? value, string field)
        {
            var text = value?.ToString();
            if (DateTime.TryParse(text, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToLocalTime();
            _logger.LogError("Failed to parse {Field}: {Value}", field, text);
            return null;
        }

        private async Task AddItemInternalAsync(ItemModel item, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(item?.ItemNumber))
                item.ItemNumber = await GenerateNextItemNumberAsync(cancellationToken);
            if (await ItemExistsAsync(itemNumber: item.ItemNumber, exceptId: null, cancellationToken: cancellationToken))
                throw new InvalidOperationException($"ItemModel {item.ItemNumber} already exists.");
            ValidateQuantity(item.QuantityOnHand);
            using var conn = _dbService.CreateConnection();
            await InsertItemAsync(conn, null, item, cancellationToken);
        }

        private async Task UpdateItemInternalAsync(ItemModel item, CancellationToken cancellationToken)
        {
            if (await ItemExistsAsync(itemNumber: item.ItemNumber, exceptId: item.ItemID, cancellationToken: cancellationToken))
                throw new InvalidOperationException($"ItemModel {item.ItemNumber} already exists.");
            using var conn = _dbService.CreateConnection();
            ValidateQuantity(item.QuantityOnHand);
            const string sql = @"
                UPDATE Items SET
                  ItemNumber = @ItemNumber,
                  NameDescription = @Desc,
                  Location = @Loc,
                  Brand = @Brand,
                  PartNumber = @PN,
                  Supplier = @Sup,
                  PurchasedDate = @PD,
                  Notes = @Notes,
                  Keywords = @Keywords,
                  AvailableQuantity = @Avail,
                  RentedQuantity = @Rent,
                  IsRentalItem = @Rental,
                  IsPowered = @Powered,
                  IsCheckedOut = @Out,
                  CheckedOutBy = @By,
                  CheckedOutTime = @Time,
                  ImagePath = @Img
                WHERE ItemID = @ID";
            var p = new[]
            {
                new SqliteParameter("@ID", item.ItemID),
                new SqliteParameter("@ItemNumber", item.ItemNumber),
                new SqliteParameter("@Desc", (object)item.Name ?? DBNull.Value),
                new SqliteParameter("@Loc", item.Location),
                new SqliteParameter("@Brand", item.Brand),
                new SqliteParameter("@PN", item.PartNumber),
                new SqliteParameter("@Sup", (object)item.Supplier ?? DBNull.Value),
                new SqliteParameter("@PD", (object)item.PurchasedDate ?? DBNull.Value),
                new SqliteParameter("@Notes", (object)item.Notes ?? DBNull.Value),
                new SqliteParameter("@Keywords", (object)item.Keywords ?? DBNull.Value),
                new SqliteParameter("@Avail", item.QuantityOnHand),
                new SqliteParameter("@Rent", item.RentedQuantity),
                new SqliteParameter("@Rental", item.IsRentalItem ? 1 : 0),
                new SqliteParameter("@Powered", item.IsPowered ? 1 : 0),
                new SqliteParameter("@Out", item.IsCheckedOut ? 1 : 0),
                new SqliteParameter("@By", (object)item.CheckedOutBy ?? DBNull.Value),
                new SqliteParameter("@Time", (object)item.CheckedOutTime ?? DBNull.Value),
                new SqliteParameter("@Img", (object)item.ImagePath ?? DBNull.Value)
            };
            try
            {
                await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken);
            }
            catch (SqliteException ex)
            {
                _logger.LogError(ex, "Failed to update item {ItemID}", item.ItemID);
                throw new InvalidOperationException($"Failed to update item {item.ItemID}.", ex);
            }
        }

        private async Task DeleteItemInternalAsync(int itemID, CancellationToken cancellationToken)
        {
            using var conn = _dbService.CreateConnection();
            try
            {
                await SqliteHelper.ExecuteNonQueryAsync(conn, "DELETE FROM Items WHERE ItemID=@ID",
                    new[] { new SqliteParameter("@ID", itemID) }, cancellationToken);
            }
            catch (SqliteException ex)
            {
                _logger.LogError(ex, "Failed to delete item {ItemID}", itemID);
                throw new InvalidOperationException($"Failed to delete item {itemID}.", ex);
            }
        }

        public async Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default)
        {
            using var conn = _dbService.CreateConnection();
            var list = await SqliteHelper.ExecuteReaderAsync(conn, "SELECT * FROM Items WHERE ItemID=@ItemID",
                MapItem,
                new[] { new SqliteParameter("@ItemID", itemID) }, cancellationToken);
            return list.FirstOrDefault();
        }

        public IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default)
            => _repository.GetPageAsync(new ItemFilter(null, sortField, sortDirection, isRentalItem), page, cancellationToken);

        public IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default)
            => _repository.GetPageAsync(new ItemFilter(searchText, sortField, sortDirection, isRentalItem), page, cancellationToken);

        public Task<int> CountItemsAsync(ItemFilter filter, CancellationToken ct)
            => _repository.CountAsync(filter, ct);

        private async Task<bool> ToggleItemCheckOutStatusInternalAsync(int itemID, string currentUser, CancellationToken cancellationToken)
        {
            using var conn = _dbService.CreateConnection();
            var record = (await SqliteHelper.ExecuteReaderAsync(conn,
                "SELECT IsCheckedOut, AvailableQuantity, CheckedOutBy FROM Items WHERE ItemID=@ID",
                r => new { Out = Convert.ToInt32(r["IsCheckedOut"]) == 1, Qty = Convert.ToInt32(r["AvailableQuantity"]), By = r["CheckedOutBy"]?.ToString() },
                new[] { new SqliteParameter("@ID", itemID) }, cancellationToken)).FirstOrDefault();

            if (record == null)
                throw new InvalidOperationException($"ItemModel {itemID} not found.");

            if (!record.Out)
            {
                if (record.Qty <= 0)
                    return false;
            }
            else if (!string.Equals(record.By, currentUser, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var newStatus = record.Out ? 0 : 1;
            var time = record.Out ? (object)DBNull.Value : DateTime.UtcNow;
            var by = record.Out ? (object)DBNull.Value : currentUser;
            var qtyChange = record.Out ? 1 : -1;

            var rows = await SqliteHelper.ExecuteNonQueryAsync(conn, @"
                UPDATE Items SET
                  IsCheckedOut = @Out,
                  CheckedOutBy = @By,
                  CheckedOutTime = @Time,
                  AvailableQuantity = AvailableQuantity + @Q
                WHERE ItemID = @ID", new[]
            {
                new SqliteParameter("@Out", newStatus),
                new SqliteParameter("@By", by),
                new SqliteParameter("@Time", time),
                new SqliteParameter("@Q", qtyChange),
                new SqliteParameter("@ID", itemID)
            }, cancellationToken);

            if (rows == 0)
                throw new InvalidOperationException("Check-out status update failed.");

            return true;
        }

        private async Task<List<int>> ImportItemsFromCsvInternalAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken)
        {
            if (map == null || !map.TryGetValue("ItemNumber", out var _) || string.IsNullOrWhiteSpace(map["ItemNumber"]))
                throw new ArgumentException("Mapping for required field 'ItemNumber' is missing.", nameof(map));

            var invalidRows = new List<int>();
            using var parser = new TextFieldParser(filePath);
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;
            if (parser.EndOfData)
                return invalidRows;
            var headers = parser.ReadFields();

            using var conn = _dbService.CreateConnection();
            var existingNumbers = new HashSet<string>(
                await SqliteHelper.ExecuteReaderAsync(conn,
                    "SELECT ItemNumber FROM Items",
                    r => r.GetString(0),
                    null, cancellationToken));

            using var tran = conn.BeginTransaction();
            var row = 1; // header already read
            try
            {
                while (!parser.EndOfData)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    row++;
                    var cols = parser.ReadFields();
                    var itemNumber = GetMapped(cols, headers, map, "ItemNumber");
                    if (string.IsNullOrWhiteSpace(itemNumber) || existingNumbers.Contains(itemNumber))
                    {
                        invalidRows.Add(row);
                        continue;
                    }

                    var item = new ItemModel
                    {
                        ItemNumber = itemNumber,
                        Name = GetMapped(cols, headers, map, nameof(ItemImportDto.Name)),
                        Location = GetMapped(cols, headers, map, "Location"),
                        Brand = GetMapped(cols, headers, map, "Brand"),
                        PartNumber = GetMapped(cols, headers, map, "PartNumber"),
                        Supplier = GetMapped(cols, headers, map, "Supplier"),
                        PurchasedDate = TryParseDate(GetMapped(cols, headers, map, "PurchasedDate")),
                        Notes = GetMapped(cols, headers, map, "Notes"),
                        Keywords = GetMapped(cols, headers, map, nameof(ItemImportDto.Keywords)),
                        QuantityOnHand = TryParseInt(GetMapped(cols, headers, map, "AvailableQuantity")),
                        IsPowered = TryParseBool(GetMapped(cols, headers, map, "IsPowered")),
                        IsRentalItem = TryParseBool(GetMapped(cols, headers, map, "IsRentalItem"))
                    };

                    await InsertItemAsync(conn, tran, item, cancellationToken);
                    existingNumbers.Add(itemNumber);
                }

                tran.Commit();
                return invalidRows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import items from CSV");
                tran.Rollback();
                throw;
            }

            static string? GetMapped(string[] row, string[] headers, IDictionary<string, string> map, string key)
            {
                if (!map.TryGetValue(key, out var column))
                    return null;
                var index = Array.FindIndex(headers, h => string.Equals(h, column, StringComparison.OrdinalIgnoreCase));
                return index >= 0 && index < row.Length ? row[index].Trim() : null;
            }

            static int TryParseInt(string? input) => int.TryParse(input, out var result) ? result : 0;

            static bool TryParseBool(string? input) => input != null && (input.Equals("1") || bool.TryParse(input, out var b) && b);

            static DateTime? TryParseDate(string? input) => DateTime.TryParse(input, out var result) ? result : null;
        }

        private async Task ExportItemsToCsvInternalAsync(string filePath, CancellationToken cancellationToken)
        {
            var items = new List<ItemModel>();
            await foreach (var item in GetItemsAsync(new ItemPage(1, int.MaxValue), SortField.Name, SortDirection.Ascending, cancellationToken: cancellationToken))
                items.Add(item);
            await CsvHelperUtil.ExportItemsToCsvAsync(filePath, items);
        }

        public async Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, SqliteConnection? conn = null, SqliteTransaction? tx = null, CancellationToken cancellationToken = default)
        {
            if (qtyChange <= 0) throw new ArgumentException("Quantity change must be positive.", nameof(qtyChange));
            var sql = isRental
                ? @"UPDATE Items SET AvailableQuantity = AvailableQuantity - @Q, RentedQuantity = RentedQuantity + @Q WHERE ItemID = @ID AND AvailableQuantity >= @Q"
                : @"UPDATE Items SET AvailableQuantity = AvailableQuantity + @Q, RentedQuantity = RentedQuantity - @Q WHERE ItemID = @ID AND RentedQuantity >= @Q";
            var p = new[]
            {
                new SqliteParameter("@ID", itemID),
                new SqliteParameter("@Q", qtyChange)
            };

            if (conn != null)
            {
                int rows = tx != null
                    ? await SqliteHelper.ExecuteNonQueryAsync(conn, tx, sql, p, cancellationToken)
                    : await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken);
                if (rows == 0)
                {
                    _logger.LogWarning("Quantity update affected 0 rows for ItemID {ItemID}", itemID);
                    throw new InvalidOperationException("Quantity update failed.");
                }
            }
            else
            {
                using var c = _dbService.CreateConnection();
                int rows = await SqliteHelper.ExecuteNonQueryAsync(c, sql, p, cancellationToken);
                if (rows == 0)
                {
                    _logger.LogWarning("Quantity update affected 0 rows for ItemID {ItemID}", itemID);
                    throw new InvalidOperationException("Quantity update failed.");
                }
            }
        }

        public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct)
        {
            _auth.EnsureAdmin();
            return _repository.SaveChangesAsync(changes, ct);
        }
    }
}

using System.Data.SQLite;
using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Utilities.IO;
using ToolManagementAppV2.Models.ImportExport;
using ToolManagementAppV2.Interfaces;
using System.Text;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ToolManagementAppV2.Services.Users;

namespace ToolManagementAppV2.Services.Items
{
    public class ItemService : IItemService
    {
        readonly DatabaseService _dbService;
        const string AllItemsSql = "SELECT * FROM Items";
        const string UpsertItemCsv = @"
            INSERT INTO Items
              (ItemNumber, NameDescription, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, Keywords, AvailableQuantity, RentedQuantity, ImagePath, IsCheckedOut, IsPowerTool)
            VALUES (@ItemNumber,@Desc,@Loc,@Brand,@PN,@Sup,@PD,@Notes,@Keywords,@Avail,@Rent,@Img,0,@Power);
            SELECT last_insert_rowid();";
        const int MaxQuantityOnHand = 10000;
        const int MaxSearchTerms = 10;
    
        readonly ILogger<ItemService> _logger;
        readonly IAuthorizationService _auth;
        readonly ActivityLogService? _activityLog;
        readonly IUserContext? _context;

        public ItemService(DatabaseService dbService, IAuthorizationService? authorizationService = null, ILogger<ItemService>? logger = null, ActivityLogService? activityLogService = null, IUserContext? userContext = null)
        {
            _dbService = dbService;
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
            _auth.EnsureAdmin();
            var result = await ToggleItemCheckOutStatusInternalAsync(itemID, currentUser, cancellationToken).ConfigureAwait(false);
            if (result && _activityLog != null)
            {
                int userId = _context?.CurrentUser?.UserID ?? 0;
                await _activityLog.LogActionAsync(userId, currentUser, $"Toggled item {itemID} check-out status", cancellationToken).ConfigureAwait(false);
            }
            return result;
        }

        public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default)
        {
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReaderAsync(conn,
                "SELECT * FROM Items WHERE CheckedOutBy=@User AND IsCheckedOut=1",
                new[] { new SQLiteParameter("@User", userName) }, MapItem, cancellationToken);
        }

        public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            const string sql = "UPDATE Items SET ImagePath=@Img WHERE ItemID=@ID";
            var p = new[]
            {
                new SQLiteParameter("@Img", imagePath),
                new SQLiteParameter("@ID", itemID)
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

        async Task InsertItemAsync(SQLiteConnection conn, SQLiteTransaction? tran, ItemModel item, CancellationToken cancellationToken)
        {
            ValidateQuantity(item.QuantityOnHand);
            var p = new[]
            {
                new SQLiteParameter("@ItemNumber", item.ItemNumber),
                new SQLiteParameter("@Desc", (object)item.NameDescription ?? DBNull.Value),
                new SQLiteParameter("@Loc", item.Location),
                new SQLiteParameter("@Brand", item.Brand),
                new SQLiteParameter("@PN", item.PartNumber),
                new SQLiteParameter("@Sup", (object)item.Supplier ?? DBNull.Value),
                new SQLiteParameter("@PD", (object)item.PurchasedDate ?? DBNull.Value),
                new SQLiteParameter("@Notes", (object)item.Notes ?? DBNull.Value),
                new SQLiteParameter("@Keywords", (object)item.Keywords ?? DBNull.Value),
                new SQLiteParameter("@Avail", item.QuantityOnHand),
                new SQLiteParameter("@Rent", item.RentedQuantity),
                new SQLiteParameter("@Img", (object)item.ImagePath ?? DBNull.Value),
                new SQLiteParameter("@Power", item.IsPowerTool ? 1 : 0)
            };
            using var cmd = new SQLiteCommand(UpsertItemCsv, conn, tran);
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
            var tools = await GetAllItemsAsync(cancellationToken);
            var groups = new Dictionary<string, List<ItemModel>>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in tools)
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

            var destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
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
                var relative = $"Images/{Path.GetFileName(dest)}";
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

        private async Task<bool> ItemExistsAsync(string toolNumber, int? exceptId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(toolNumber))
                return false;

            var sql = "SELECT COUNT(*) FROM Items WHERE ItemNumber = @TN";
            var parameters = new List<SQLiteParameter>
            {
                new("@TN", toolNumber)
            };

            if (exceptId.HasValue)
            {
                sql += " AND ItemID <> @ID";
                parameters.Add(new SQLiteParameter("@ID", exceptId.Value));
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
            NameDescription = r["NameDescription"].ToString(),
            Brand = r["Brand"].ToString(),
            Location = r["Location"].ToString(),
            QuantityOnHand = r["AvailableQuantity"] is DBNull ? 0 : Convert.ToInt32(r["AvailableQuantity"]),
            RentedQuantity = r["RentedQuantity"] is DBNull ? 0 : Convert.ToInt32(r["RentedQuantity"]),
            Supplier = r["Supplier"].ToString(),
            PurchasedDate = r["PurchasedDate"] is DBNull
                ? (DateTime?)null
                : DateTime.Parse(r["PurchasedDate"].ToString()!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
            Notes = r["Notes"].ToString(),
            IsCheckedOut = (r["IsCheckedOut"] is DBNull ? 0 : Convert.ToInt32(r["IsCheckedOut"])) == 1,
            CheckedOutBy = r["CheckedOutBy"].ToString(),
            CheckedOutTime = r["CheckedOutTime"] is DBNull
                ? (DateTime?)null
                : DateTime.Parse(r["CheckedOutTime"].ToString()!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
            ImagePath = r["ImagePath"]?.ToString(),
            Keywords = r["Keywords"]?.ToString(),
            IsPowerTool = (r["IsPowerTool"] is DBNull ? 0 : Convert.ToInt32(r["IsPowerTool"])) == 1
        };

        private async Task AddItemInternalAsync(ItemModel item, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(item?.ItemNumber))
                item.ItemNumber = await GenerateNextItemNumberAsync(cancellationToken);
            if (await ItemExistsAsync(item.ItemNumber, null, cancellationToken))
                throw new InvalidOperationException($"ItemModel {item.ItemNumber} already exists.");
            ValidateQuantity(item.QuantityOnHand);
            using var conn = _dbService.CreateConnection();
            await InsertItemAsync(conn, null, item, cancellationToken);
        }

        private async Task UpdateItemInternalAsync(ItemModel item, CancellationToken cancellationToken)
        {
            if (await ItemExistsAsync(item.ItemNumber, item.ItemID, cancellationToken))
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
                  IsPowerTool = @Power,
                  IsCheckedOut = @Out,
                  CheckedOutBy = @By,
                  CheckedOutTime = @Time,
                  ImagePath = @Img
                WHERE ItemID = @ID";
            var p = new[]
            {
                new SQLiteParameter("@ID", item.ItemID),
                new SQLiteParameter("@ItemNumber", item.ItemNumber),
                new SQLiteParameter("@Desc", (object)item.NameDescription ?? DBNull.Value),
                new SQLiteParameter("@Loc", item.Location),
                new SQLiteParameter("@Brand", item.Brand),
                new SQLiteParameter("@PN", item.PartNumber),
                new SQLiteParameter("@Sup", (object)item.Supplier ?? DBNull.Value),
                new SQLiteParameter("@PD", (object)item.PurchasedDate ?? DBNull.Value),
                new SQLiteParameter("@Notes", (object)item.Notes ?? DBNull.Value),
                new SQLiteParameter("@Keywords", (object)item.Keywords ?? DBNull.Value),
                new SQLiteParameter("@Avail", item.QuantityOnHand),
                new SQLiteParameter("@Rent", item.RentedQuantity),
                new SQLiteParameter("@Power", item.IsPowerTool ? 1 : 0),
                new SQLiteParameter("@Out", item.IsCheckedOut ? 1 : 0),
                new SQLiteParameter("@By", (object)item.CheckedOutBy ?? DBNull.Value),
                new SQLiteParameter("@Time", (object)item.CheckedOutTime ?? DBNull.Value),
                new SQLiteParameter("@Img", (object)item.ImagePath ?? DBNull.Value)
            };
            try
            {
                await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken);
            }
            catch (SQLiteException ex)
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
                    new[] { new SQLiteParameter("@ID", itemID) }, cancellationToken);
            }
            catch (SQLiteException ex)
            {
                _logger.LogError(ex, "Failed to delete item {ItemID}", itemID);
                throw new InvalidOperationException($"Failed to delete item {itemID}.", ex);
            }
        }

        public async Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default)
        {
            using var conn = _dbService.CreateConnection();
            var list = await SqliteHelper.ExecuteReaderAsync(conn, "SELECT * FROM Items WHERE ItemID=@ItemID",
                new[] { new SQLiteParameter("@ItemID", itemID) }, MapItem, cancellationToken);
            return list.FirstOrDefault();
        }

        public async Task<List<ItemModel>> GetAllItemsAsync(CancellationToken cancellationToken = default)
        {
            using var conn = _dbService.CreateConnection();
            return await SqliteHelper.ExecuteReaderAsync(conn, AllItemsSql, null, MapItem, cancellationToken);
        }

        public async Task<List<ItemModel>> SearchItemsAsync(string? searchText, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await GetAllItemsAsync(cancellationToken);
            }

            using var conn = _dbService.CreateConnection();
            var terms = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var originalCount = terms.Length;
            if (originalCount > MaxSearchTerms)
            {
                _logger.LogInformation(
                    "Search term limit exceeded; truncating from {OriginalCount} to {Max}",
                    originalCount, MaxSearchTerms);
                terms = terms.Take(MaxSearchTerms).ToArray();
            }
            var searchable = new[]
            {
                "ItemNumber",
                "NameDescription",
                "Brand",
                "PartNumber",
                "Supplier",
                "Location",
                "Notes",
                "Keywords"
            };

            var sb = new StringBuilder("SELECT * FROM Items WHERE ");
            var parameters = new List<SQLiteParameter>();
            for (int i = 0; i < terms.Length; i++)
            {
                if (i > 0) sb.Append(" AND ");
                var paramName = $"@p{i}";
                var likeClause = string.Join(" OR ", searchable.Select(col => $"{col} LIKE {paramName} COLLATE NOCASE"));
                sb.Append($"(CAST(ItemID AS TEXT) LIKE {paramName} COLLATE NOCASE OR {likeClause})");
                parameters.Add(new SQLiteParameter(paramName, $"%{terms[i]}%"));
            }

            cancellationToken.ThrowIfCancellationRequested();
            return await SqliteHelper.ExecuteReaderAsync(conn, sb.ToString(), parameters.ToArray(), MapItem, cancellationToken);
        }

        private async Task<bool> ToggleItemCheckOutStatusInternalAsync(int itemID, string currentUser, CancellationToken cancellationToken)
        {
            using var conn = _dbService.CreateConnection();
            var record = (await SqliteHelper.ExecuteReaderAsync(conn,
                "SELECT IsCheckedOut, AvailableQuantity FROM Items WHERE ItemID=@ID",
                new[] { new SQLiteParameter("@ID", itemID) },
                r => new { Out = Convert.ToInt32(r["IsCheckedOut"]) == 1, Qty = Convert.ToInt32(r["AvailableQuantity"]) }, cancellationToken)).FirstOrDefault();

            if (record == null)
                throw new InvalidOperationException($"ItemModel {itemID} not found.");

            if (!record.Out && record.Qty <= 0)
                return false;

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
                new SQLiteParameter("@Out", newStatus),
                new SQLiteParameter("@By", by),
                new SQLiteParameter("@Time", time),
                new SQLiteParameter("@Q", qtyChange),
                new SQLiteParameter("@ID", itemID)
            }, cancellationToken);

            if (rows == 0)
                throw new InvalidOperationException("Check-out status update failed.");

            return true;
        }

        private async Task<List<int>> ImportItemsFromCsvInternalAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken)
        {
            var (tools, invalidRows) = await CsvHelperUtil.LoadToolsFromCsvAsync(filePath, map, cancellationToken);
            using var conn = _dbService.CreateConnection();
            var existingNumbers = new HashSet<string>(
                await SqliteHelper.ExecuteReaderAsync(conn,
                    "SELECT ItemNumber FROM Items", null,
                    r => r.GetString(0), cancellationToken));

            using var tran = conn.BeginTransaction();
            try
            {
                foreach (var item in tools)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (string.IsNullOrWhiteSpace(item.ItemNumber) ||
                        existingNumbers.Contains(item.ItemNumber))
                        continue;
                    await InsertItemAsync(conn, tran, item, cancellationToken);
                    existingNumbers.Add(item.ItemNumber);
                }
                tran.Commit();
                return invalidRows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import tools from CSV");
                tran.Rollback();
                throw;
            }
        }

        private async Task ExportItemsToCsvInternalAsync(string filePath, CancellationToken cancellationToken)
        {
            var tools = await GetAllItemsAsync(cancellationToken);
            await CsvHelperUtil.ExportItemsToCsvAsync(filePath, tools);
        }

        public async Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null, CancellationToken cancellationToken = default)
        {
            if (qtyChange <= 0) throw new ArgumentException("Quantity change must be positive.", nameof(qtyChange));
            var sql = isRental
                ? @"UPDATE Items SET AvailableQuantity = AvailableQuantity - @Q, RentedQuantity = RentedQuantity + @Q WHERE ItemID = @ID AND AvailableQuantity >= @Q"
                : @"UPDATE Items SET AvailableQuantity = AvailableQuantity + @Q, RentedQuantity = RentedQuantity - @Q WHERE ItemID = @ID AND RentedQuantity >= @Q";
            var p = new[]
            {
                new SQLiteParameter("@ID", itemID),
                new SQLiteParameter("@Q", qtyChange)
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
    }
}

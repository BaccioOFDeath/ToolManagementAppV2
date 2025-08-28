using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Utilities.IO;
using InventoryManagementApp.Models.ImportExport;
using InventoryManagementApp.Interfaces;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Data;
using Microsoft.VisualBasic.FileIO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace InventoryManagementApp.Services.Items
{
    public class ItemService : IItemService
    {
        readonly DatabaseService _dbService;
        readonly IItemRepository _repository;
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

        public async Task<bool> ToggleItemCheckOutStatusAsync(int itemID, CancellationToken cancellationToken = default)
        {
            if (_context?.CurrentUser == null)
                throw new InvalidOperationException("Current user is not available.");

            var caller = _context.UserName;
            var result = await ToggleItemCheckOutStatusInternalAsync(itemID, caller, _auth.IsAdmin, cancellationToken).ConfigureAwait(false);
            if (!result)
                return false;

            if (_activityLog != null)
            {
                int userId = _context.CurrentUser?.UserID ?? 0;
                await _activityLog.LogActionAsync(userId, caller, $"Toggled item {itemID} check-out status", cancellationToken).ConfigureAwait(false);
            }
            return true;
        }

        public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default)
            => _repository.GetItemsCheckedOutByAsync(userName, cancellationToken);

        public Task<List<ItemModel>> GetCheckedOutItemsAsync(CancellationToken cancellationToken = default)
            => _repository.GetCheckedOutItemsAsync(cancellationToken);

        public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            return _repository.UpdateItemImageAsync(itemID, imagePath, cancellationToken);
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

        private async Task<ImageImportResult> ImportItemImagesInternalAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress, CancellationToken cancellationToken)
        {
            var result = new ImageImportResult();
            if (string.IsNullOrWhiteSpace(folderPath) || keySelector == null)
                return result;

            cancellationToken.ThrowIfCancellationRequested();
            var items = new List<ItemModel>();
            await foreach (var item in GetItemsAsync(new ItemPage(1, int.MaxValue), SortField.Name, SortDirection.Ascending, cancellationToken: cancellationToken)
                .WithCancellation(cancellationToken))
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

            var destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ItemImages");
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
                var fileName = Path.GetFileNameWithoutExtension(file) + ".jpg";
                var dest = Path.Combine(destDir, fileName);
                if (!File.Exists(dest))
                {
                    try
                    {
                        await CopyFileAsync(file, dest, 256, 256, cancellationToken);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, "Failed to copy image from {Source} to {Destination}", file, dest);
                        result.ConflictingFiles.Add(file);
                        continue;
                    }
                }
                var relative = $"Assets/ItemImages/{fileName}";
                await UpdateItemImageAsync(item.ItemID, relative, cancellationToken);
                result.ImportedCount++;
                processed++;
                progress?.Report(new ImageImportProgress { Processed = processed, Total = total });
            }

            return result;
        }

        protected virtual Task CopyFileAsync(string sourceFileName, string destFileName, int maxWidth, int maxHeight, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(sourceFileName);
            bitmap.EndInit();
            bitmap.Freeze();

            double scale = Math.Min((double)maxWidth / bitmap.PixelWidth, (double)maxHeight / bitmap.PixelHeight);
            if (scale > 1.0)
                scale = 1.0;

            BitmapSource source = bitmap;
            if (scale < 1.0)
            {
                source = new TransformedBitmap(bitmap, new ScaleTransform(scale, scale));
                source.Freeze();
            }

            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));

            using var stream = new FileStream(destFileName, FileMode.Create, FileAccess.Write, FileShare.None);
            encoder.Save(stream);
            return Task.CompletedTask;
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
    
        private async Task AddItemInternalAsync(ItemModel item, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(item?.ItemNumber))
                item.ItemNumber = await GenerateNextItemNumberAsync(cancellationToken);
            if (await ItemExistsAsync(itemNumber: item.ItemNumber, exceptId: null, cancellationToken: cancellationToken))
                throw new InvalidOperationException($"ItemModel {item.ItemNumber} already exists.");
            ValidateQuantity(item.QuantityOnHand);
            item.ItemID = await _repository.InsertAsync(item, cancellationToken);
        }

        private async Task UpdateItemInternalAsync(ItemModel item, CancellationToken cancellationToken)
        {
            if (await ItemExistsAsync(itemNumber: item.ItemNumber, exceptId: item.ItemID, cancellationToken: cancellationToken))
                throw new InvalidOperationException($"ItemModel {item.ItemNumber} already exists.");
            ValidateQuantity(item.QuantityOnHand);
            await _repository.UpdateAsync(item, cancellationToken);
        }

        private async Task DeleteItemInternalAsync(int itemID, CancellationToken cancellationToken)
        {
            await _repository.DeleteAsync(itemID, cancellationToken);
        }

        public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default)
            => _repository.GetByIdAsync(itemID, cancellationToken);

        public async IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in _repository
                .GetPageAsync(new ItemFilter(null, sortField, sortDirection, isRentalItem), page, cancellationToken)
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false))
            {
                yield return item;
            }
        }

        public async IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in _repository
                .GetPageAsync(new ItemFilter(searchText, sortField, sortDirection, isRentalItem), page, cancellationToken)
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false))
            {
                yield return item;
            }
        }

        public Task<int> CountItemsAsync(ItemFilter filter, CancellationToken ct)
            => _repository.CountAsync(filter, ct);

        private Task<bool> ToggleItemCheckOutStatusInternalAsync(int itemID, string currentUser, bool isAdmin, CancellationToken cancellationToken)
            => _repository.ToggleCheckOutStatusAsync(itemID, currentUser, isAdmin, cancellationToken);

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

                    await _repository.InsertAsync(item, cancellationToken);
                    existingNumbers.Add(itemNumber);
                }

                return invalidRows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import items from CSV");
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
            await foreach (var item in GetItemsAsync(new ItemPage(1, int.MaxValue), SortField.Name, SortDirection.Ascending, cancellationToken: cancellationToken)
                .WithCancellation(cancellationToken))
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

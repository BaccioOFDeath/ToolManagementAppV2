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
using InventoryManagementApp.Utilities.Helpers;
using Microsoft.VisualBasic.FileIO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace InventoryManagementApp.Services.Items
{
    /// <summary>
    /// Service for managing inventory items including CRUD operations, check-out/check-in, and image management.
    /// </summary>
    public class ItemService : IItemService
    {
        private readonly DatabaseService _dbService;
        private readonly IItemRepository _repository;
        private const int MaxQuantityOnHand = 10000;
    
        private readonly ILogger<ItemService> _logger;
        private readonly IAuthorizationService _auth;
        private readonly ActivityLogService? _activityLog;
        private readonly IUserContext? _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemService"/> class.
        /// </summary>
        /// <param name="dbService">Database service for data access.</param>
        /// <param name="repository">Repository for item data operations.</param>
        /// <param name="authorizationService">Optional authorization service for access control.</param>
        /// <param name="logger">Optional logger for diagnostic output.</param>
        /// <param name="activityLogService">Optional activity log service for audit trails.</param>
        /// <param name="userContext">Optional user context for tracking current user.</param>
        public ItemService(DatabaseService dbService, IItemRepository repository, IAuthorizationService? authorizationService = null, ILogger<ItemService>? logger = null, ActivityLogService? activityLogService = null, IUserContext? userContext = null)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _auth = authorizationService ?? new NoOpAuthorizationService();
            _logger = logger ?? NullLogger<ItemService>.Instance;
            _activityLog = activityLogService;
            _context = userContext;
        }

        /// <summary>
        /// Validates that a quantity value is within acceptable bounds.
        /// </summary>
        /// <param name="quantity">The quantity to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if quantity is negative or exceeds maximum.</exception>
        private static void ValidateQuantity(int quantity)
        {
            if (quantity < 0 || quantity > MaxQuantityOnHand)
                throw new ArgumentOutOfRangeException(nameof(ItemModel.QuantityOnHand), $"QuantityOnHand must be between 0 and {MaxQuantityOnHand}.");
        }

        /// <summary>
        /// Adds a new item to the inventory. Requires manage-items permission.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <exception cref="ArgumentNullException">Thrown if item is null.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if user lacks manage-items permission.</exception>
        public async Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            
            _auth.EnsurePermission(User.PermissionManageItems);
            await AddItemInternalAsync(item, cancellationToken).ConfigureAwait(false);
            if (_activityLog != null)
            {
                var user = _context?.CurrentUser;
                await _activityLog.LogActionAsync(user?.UserID ?? 0, user?.UserName ?? string.Empty, $"Added item {item.ItemNumber}", cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Updates an existing item in the inventory. Requires manage-items permission.
        /// </summary>
        /// <param name="item">The item with updated information.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <exception cref="ArgumentNullException">Thrown if item is null.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if user lacks manage-items permission.</exception>
        public async Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            
            _auth.EnsurePermission(User.PermissionManageItems);
            await UpdateItemInternalAsync(item, cancellationToken).ConfigureAwait(false);
            if (_activityLog != null)
            {
                var user = _context?.CurrentUser;
                await _activityLog.LogActionAsync(user?.UserID ?? 0, user?.UserName ?? string.Empty, $"Updated item {item.ItemNumber}", cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deletes an item from the inventory. Requires manage-items permission.
        /// </summary>
        /// <param name="itemID">The ID of the item to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if itemID is less than 1.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if user lacks manage-items permission.</exception>
        public async Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default)
        {
            if (itemID < 1)
                throw new ArgumentOutOfRangeException(nameof(itemID), "Item ID must be greater than 0.");
            
            _auth.EnsurePermission(User.PermissionManageItems);
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
            _auth.EnsureAnyPermission(User.PermissionManageItems, User.PermissionImportExport);
            return _repository.UpdateItemImageAsync(itemID, imagePath, cancellationToken);
        }

        public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken)
        {
            _auth.EnsurePermission(User.PermissionImportExport);
            return ImportItemsFromCsvInternalAsync(filePath, map, cancellationToken);
        }

        public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default)
            => ExportItemsToCsvInternalAsync(filePath, cancellationToken);

        public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            _auth.EnsurePermission(User.PermissionImportExport);
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
                    if (string.IsNullOrWhiteSpace(k))
                        continue;
                    if (!groups.TryGetValue(k, out var list))
                        groups[k] = list = new List<ItemModel>();
                    list.Add(item);
                }
            }

            string destDir;
            try
            {
                destDir = AppAssetHelper.EnsureAssetFolder(AppAssetHelper.ItemImagesFolder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create image directory for item assets");
                return result;
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
                if (!string.IsNullOrWhiteSpace(item.ImagePath))
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

            if (bitmap.PixelWidth == 0 || bitmap.PixelHeight == 0)
                throw new InvalidOperationException("Invalid image dimensions: image has zero width or height.");

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
            var count = Convert.ToInt32(await SqliteHelper.ExecuteScalarAsync(conn, sql, parameters.ToArray(), cancellationToken) ?? 0);
            return count > 0;
        }
    
        private async Task AddItemInternalAsync(ItemModel item, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(item);
            if (string.IsNullOrWhiteSpace(item.ItemNumber))
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
            if (headers == null || headers.Length == 0)
                throw new InvalidDataException("CSV header row is missing or empty.");

            using var conn = _dbService.CreateConnection();
            var existingNumbers = new HashSet<string>(
                await SqliteHelper.ExecuteReaderAsync(conn,
                    "SELECT ItemNumber FROM Items",
                    r => r.GetString(0),
                    null, cancellationToken),
                StringComparer.OrdinalIgnoreCase);
            using var transaction = conn.BeginTransaction();

            var row = 1; // header already read
            try
            {
                while (!parser.EndOfData)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    row++;
                    var cols = parser.ReadFields();
                    if (cols == null || cols.Length == 0 || headers == null)
                    {
                        invalidRows.Add(row);
                        continue;
                    }
                    var itemNumber = CsvHelperUtil.GetMapped(cols, headers, map, "ItemNumber");
                    var name = CsvHelperUtil.GetMapped(cols, headers, map, nameof(ItemImportDto.Name));
                    var location = CsvHelperUtil.GetMapped(cols, headers, map, "Location");
                    var brand = CsvHelperUtil.GetMapped(cols, headers, map, "Brand");
                    var partNumber = CsvHelperUtil.GetMapped(cols, headers, map, "PartNumber");
                    var supplier = CsvHelperUtil.GetMapped(cols, headers, map, "Supplier");
                    var purchased = CsvHelperUtil.GetMapped(cols, headers, map, "PurchasedDate");
                    var notes = CsvHelperUtil.GetMapped(cols, headers, map, "Notes");
                    var keywords = CsvHelperUtil.GetMapped(cols, headers, map, nameof(ItemImportDto.Keywords));
                    var quantity = CsvHelperUtil.GetMapped(cols, headers, map, "AvailableQuantity");
                    var powered = CsvHelperUtil.GetMapped(cols, headers, map, "IsPowered");
                    var rental = CsvHelperUtil.GetMapped(cols, headers, map, "IsRentalItem");

                    bool skip = false;
                    if (string.IsNullOrWhiteSpace(itemNumber))
                    {
                        _logger.LogWarning("Skipping row {Row}: ItemNumber is missing.", row);
                        skip = true;
                    }
                    else if (existingNumbers.Contains(itemNumber))
                    {
                        _logger.LogWarning("Skipping row {Row}: duplicate ItemNumber {ItemNumber}.", row, itemNumber);
                        skip = true;
                    }

                    if (!skip)
                    {
                        var requiredChecks = new List<(string Key, string? Value)>
                        {
                            (nameof(ItemImportDto.Name), name),
                            ("Location", location),
                            ("Brand", brand),
                            ("PartNumber", partNumber),
                            ("Supplier", supplier),
                            ("PurchasedDate", purchased),
                            ("Notes", notes),
                            (nameof(ItemImportDto.Keywords), keywords),
                            ("AvailableQuantity", quantity),
                            ("IsPowered", powered),
                            ("IsRentalItem", rental)
                        };

                        foreach (var (key, value) in requiredChecks)
                        {
                            if (map.ContainsKey(key) && value == null)
                            {
                                _logger.LogWarning("Skipping row {Row}: field {Field} is missing.", row, key);
                                skip = true;
                                break;
                            }
                        }
                    }

                    if (skip)
                    {
                        invalidRows.Add(row);
                        continue;
                    }

                    var item = new ItemModel
                    {
                        ItemNumber = itemNumber!,
                        Name = name ?? string.Empty,
                        Location = location ?? string.Empty,
                        Brand = brand ?? string.Empty,
                        PartNumber = partNumber ?? string.Empty,
                        Supplier = supplier ?? string.Empty,
                        PurchasedDate = TryParseDate(purchased),
                        Notes = notes ?? string.Empty,
                        Keywords = keywords ?? string.Empty,
                        QuantityOnHand = TryParseInt(quantity),
                        IsPowered = TryParseBool(powered),
                        IsRentalItem = TryParseBool(rental)
                    };

                    await InsertItemAsync(conn, transaction, item, cancellationToken).ConfigureAwait(false);
                    existingNumbers.Add(itemNumber!);
                }

                transaction.Commit();
                return invalidRows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import items from CSV");
                transaction.Rollback();
                throw;
            }

            static int TryParseInt(string? input) => int.TryParse(input, out var result) ? result : 0;

            static bool TryParseBool(string? input) => input != null && (input.Equals("1") || bool.TryParse(input, out var b) && b);

            static DateTime? TryParseDate(string? input) => DateTime.TryParse(input, out var result) ? result : null;
        }

        protected virtual async Task<int> InsertItemAsync(SqliteConnection conn, SqliteTransaction? transaction, ItemModel item, CancellationToken cancellationToken)
        {
            const string sql = @"INSERT INTO Items (ItemNumber, NameDescription, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, Keywords, AvailableQuantity, RentedQuantity, IsRentalItem, Price, ImagePath, IsCheckedOut, IsPowered, IsIncomplete, MissingComponentsNotes, IssuesNotes, CheckoutCount)
                                 VALUES (@ItemNumber,@Name,@Location,@Brand,@PartNumber,@Supplier,@PurchasedDate,@Notes,@Keywords,@QuantityOnHand,@RentedQuantity,@IsRentalItem,@Price,@ImagePath,0,@IsPowered,0,'','',0);
                                 SELECT last_insert_rowid();";

            var parameters = new[]
            {
                new SqliteParameter("@ItemNumber", item.ItemNumber),
                new SqliteParameter("@Name", item.Name),
                new SqliteParameter("@Location", item.Location),
                new SqliteParameter("@Brand", item.Brand),
                new SqliteParameter("@PartNumber", item.PartNumber),
                new SqliteParameter("@Supplier", item.Supplier),
                new SqliteParameter("@PurchasedDate", item.PurchasedDate is null ? DBNull.Value : item.PurchasedDate),
                new SqliteParameter("@Notes", item.Notes),
                new SqliteParameter("@Keywords", item.Keywords),
                new SqliteParameter("@QuantityOnHand", item.QuantityOnHand),
                new SqliteParameter("@RentedQuantity", item.RentedQuantity),
                new SqliteParameter("@IsRentalItem", item.IsRentalItem ? 1 : 0),
                new SqliteParameter("@Price", item.Price),
                new SqliteParameter("@ImagePath", string.IsNullOrWhiteSpace(item.ImagePath) ? DBNull.Value : item.ImagePath),
                new SqliteParameter("@IsPowered", item.IsPowered ? 1 : 0)
            };

            using var command = new SqliteCommand(sql, conn, transaction);
            command.Parameters.AddRange(parameters);
            var id = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return Convert.ToInt32(id);
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
            _auth.EnsurePermission(User.PermissionManageItems);
            return _repository.SaveChangesAsync(changes, ct);
        }

        public Task<List<ItemModel>> GetMostCommonlyUsedItemsAsync(int limit, CancellationToken cancellationToken = default)
        {
            return _repository.GetMostCommonlyUsedItemsAsync(limit, cancellationToken);
        }

        public Task<List<ItemModel>> GetIncompleteItemsAsync(CancellationToken cancellationToken = default)
        {
            return _repository.GetIncompleteItemsAsync(cancellationToken);
        }

        public async Task<List<int>> ImportItemsAsync(string filePath, IDataImporter<ItemModel> importer, CancellationToken cancellationToken = default)
        {
            _auth.EnsurePermission(User.PermissionImportExport);
            
            var (items, skippedRows) = await importer.ImportAsync(filePath, cancellationToken).ConfigureAwait(false);
            
            using var conn = _dbService.CreateConnection();
            var existingNumbers = new HashSet<string>(
                await SqliteHelper.ExecuteReaderAsync(conn,
                    "SELECT ItemNumber FROM Items",
                    r => r.GetString(0),
                    null, cancellationToken),
                StringComparer.OrdinalIgnoreCase);
            using var transaction = conn.BeginTransaction();

            try
            {
                foreach (var item in items)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (string.IsNullOrWhiteSpace(item.ItemNumber))
                        item.ItemNumber = GenerateNextImportedItemNumber(existingNumbers);

                    if (existingNumbers.Contains(item.ItemNumber))
                        continue;

                    ValidateQuantity(item.QuantityOnHand);
                    item.ItemID = await InsertItemAsync(conn, transaction, item, cancellationToken).ConfigureAwait(false);
                    existingNumbers.Add(item.ItemNumber);
                }

                transaction.Commit();
                return skippedRows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import items");
                transaction.Rollback();
                throw;
            }
        }

        private static string GenerateNextImportedItemNumber(ISet<string> existingNumbers)
        {
            var max = 0;
            foreach (var number in existingNumbers)
            {
                if (number.Length > 1 && number[0] == 'T' && int.TryParse(number[1..], out var parsed))
                    max = Math.Max(max, parsed);
            }

            string candidate;
            do
            {
                max++;
                candidate = $"T{max}";
            }
            while (existingNumbers.Contains(candidate));

            return candidate;
        }

        public async Task ExportItemsAsync(string filePath, IDataExporter<ItemModel> exporter, CancellationToken cancellationToken = default)
        {
            // Note: Using int.MaxValue as page size loads all items into memory.
            // For very large inventories (>10,000 items), consider implementing streaming export.
            // Current implementation matches existing CSV export behavior.
            var items = new List<ItemModel>();
            await foreach (var item in GetItemsAsync(new ItemPage(1, int.MaxValue), SortField.Name, SortDirection.Ascending, cancellationToken: cancellationToken)
                .WithCancellation(cancellationToken))
                items.Add(item);
            
            await exporter.ExportAsync(filePath, items, cancellationToken).ConfigureAwait(false);
        }
    }
}

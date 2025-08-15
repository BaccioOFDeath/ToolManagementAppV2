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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ToolManagementAppV2.Services.Tools
{
    public class ToolService : IToolService
    {
        readonly DatabaseService _dbService;
        const string AllToolsSql = "SELECT * FROM Tools";
        const string UpsertToolCsv = @"
            INSERT INTO Tools
              (ToolNumber, NameDescription, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, Keywords, AvailableQuantity, RentedQuantity, ToolImagePath, IsCheckedOut, IsPowerTool)
            VALUES (@ToolNumber,@Desc,@Loc,@Brand,@PN,@Sup,@PD,@Notes,@Keywords,@Avail,@Rent,@Img,0,@Power);
            SELECT last_insert_rowid();";
        const int MaxQuantityOnHand = 10000;
        const int MaxSearchTerms = 10;
    
        readonly ILogger<ToolService> _logger;

        public ToolService(DatabaseService dbService, ILogger<ToolService>? logger = null)
        {
            _dbService = dbService;
            _logger = logger ?? NullLogger<ToolService>.Instance;
        }

        static void RunSync(Func<Task> task) => task().GetAwaiter().GetResult();
        static T RunSync<T>(Func<Task<T>> task) => task().GetAwaiter().GetResult();
    
        public List<ToolModel> GetAllTools()
        {
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReader(conn, AllToolsSql, null, MapTool);
        }
    
        public ToolModel GetToolByID(int toolID)
        {
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReader(conn, "SELECT * FROM Tools WHERE ToolID=@ToolID",
                new[] { new SQLiteParameter("@ToolID", toolID) }, MapTool).FirstOrDefault();
        }
    
        public List<ToolModel> SearchTools(string? searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return GetAllTools();

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
                "ToolNumber",
                "NameDescription",
                "Brand",
                "PartNumber",
                "Supplier",
                "Location",
                "Notes",
                "Keywords"
            };

            var sb = new StringBuilder("SELECT * FROM Tools WHERE ");
            var parameters = new List<SQLiteParameter>();
            for (int i = 0; i < terms.Length; i++)
            {
                if (i > 0) sb.Append(" AND ");
                var paramName = $"@p{i}";
                var likeClause = string.Join(" OR ", searchable.Select(col => $"{col} LIKE {paramName} COLLATE NOCASE"));
                sb.Append($"(CAST(ToolID AS TEXT) LIKE {paramName} COLLATE NOCASE OR {likeClause})");
                parameters.Add(new SQLiteParameter(paramName, $"%{terms[i]}%"));
            }

            return SqliteHelper.ExecuteReader(conn, sb.ToString(), parameters.ToArray(), MapTool);
        }
    
        /// <summary>
        /// Adds a single tool to the database. ImportToolsFromCsv reuses the
        /// underlying InsertToolAsync method within a caller-managed transaction when
        /// performing bulk inserts.
        /// </summary>
        public void AddTool(ToolModel tool) =>
            RunSync(() => AddToolInternalAsync(tool));

        public Task AddToolAsync(ToolModel tool) => AddToolInternalAsync(tool);

        public void UpdateTool(ToolModel tool) =>
            RunSync(() => UpdateToolInternalAsync(tool));

        public Task UpdateToolAsync(ToolModel tool) => UpdateToolInternalAsync(tool);
    
        public void UpdateToolQuantities(int toolID, int qtyChange, bool isRental,
            SQLiteConnection? conn = null, SQLiteTransaction? tx = null)
        {
            if (qtyChange <= 0) throw new ArgumentException("Quantity change must be positive.", nameof(qtyChange));
            var sql = isRental
                ? @"UPDATE Tools SET AvailableQuantity = AvailableQuantity - @Q, RentedQuantity = RentedQuantity + @Q WHERE ToolID = @ID AND AvailableQuantity >= @Q"
                : @"UPDATE Tools SET AvailableQuantity = AvailableQuantity + @Q, RentedQuantity = RentedQuantity - @Q WHERE ToolID = @ID AND RentedQuantity >= @Q";
            var p = new[]
            {
                new SQLiteParameter("@ID", toolID),
                new SQLiteParameter("@Q", qtyChange)
            };

            if (conn != null)
            {
                int rows = tx != null
                    ? SqliteHelper.ExecuteNonQuery(conn, tx, sql, p)
                    : SqliteHelper.ExecuteNonQuery(conn, sql, p);
                if (rows == 0)
                {
                    _logger.LogWarning("Quantity update affected 0 rows for ToolID {ToolID}", toolID);
                    throw new InvalidOperationException("Quantity update failed.");
                }
            }
            else
            {
                using var c = _dbService.CreateConnection();
                int rows = SqliteHelper.ExecuteNonQuery(c, sql, p);
                if (rows == 0)
                {
                    _logger.LogWarning("Quantity update affected 0 rows for ToolID {ToolID}", toolID);
                    throw new InvalidOperationException("Quantity update failed.");
                }
            }
        }
    
        public void DeleteTool(int toolID) =>
            RunSync(() => DeleteToolInternalAsync(toolID));

        public Task DeleteToolAsync(int toolID) => DeleteToolInternalAsync(toolID);

        public void ToggleToolCheckOutStatus(int toolID, string currentUser) =>
            RunSync(() => ToggleToolCheckOutStatusInternalAsync(toolID, currentUser));

        public Task ToggleToolCheckOutStatusAsync(int toolID, string currentUser) =>
            ToggleToolCheckOutStatusInternalAsync(toolID, currentUser);
    
        public List<ToolModel> GetToolsCheckedOutBy(string userName)
        {
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReader(conn, "SELECT * FROM Tools WHERE CheckedOutBy=@User AND IsCheckedOut=1",
                new[] { new SQLiteParameter("@User", userName) }, MapTool);
        }
    
        public void UpdateToolImage(int toolID, string imagePath)
        {
            using var conn = _dbService.CreateConnection();
            SqliteHelper.ExecuteNonQuery(conn, "UPDATE Tools SET ToolImagePath=@Img WHERE ToolID=@ID",
                new[]
                {
                    new SQLiteParameter("@Img", imagePath),
                    new SQLiteParameter("@ID", toolID)
                });
        }
    
        public List<int> ImportToolsFromCsv(string filePath, IDictionary<string, string> map) =>
            RunSync(() => ImportToolsFromCsvInternalAsync(filePath, map, CancellationToken.None));

        public Task<List<int>> ImportToolsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) =>
            ImportToolsFromCsvInternalAsync(filePath, map, cancellationToken);

        static void ValidateQuantity(int quantity)
        {
            if (quantity < 0 || quantity > MaxQuantityOnHand)
                throw new ArgumentOutOfRangeException(nameof(Tool.QuantityOnHand), $"QuantityOnHand must be between 0 and {MaxQuantityOnHand}.");
        }

        async Task InsertToolAsync(SQLiteConnection conn, SQLiteTransaction? tran, ToolModel tool)
        {
            ValidateQuantity(tool.QuantityOnHand);
            var p = new[]
            {
                new SQLiteParameter("@ToolNumber", tool.ToolNumber),
                new SQLiteParameter("@Desc", (object)tool.NameDescription ?? DBNull.Value),
                new SQLiteParameter("@Loc", tool.Location),
                new SQLiteParameter("@Brand", tool.Brand),
                new SQLiteParameter("@PN", tool.PartNumber),
                new SQLiteParameter("@Sup", (object)tool.Supplier ?? DBNull.Value),
                new SQLiteParameter("@PD", (object)tool.PurchasedDate ?? DBNull.Value),
                new SQLiteParameter("@Notes", (object)tool.Notes ?? DBNull.Value),
                new SQLiteParameter("@Keywords", (object)tool.Keywords ?? DBNull.Value),
                new SQLiteParameter("@Avail", tool.QuantityOnHand),
                new SQLiteParameter("@Rent", tool.RentedQuantity),
                new SQLiteParameter("@Img", (object)tool.ToolImagePath ?? DBNull.Value),
                new SQLiteParameter("@Power", tool.IsPowerTool ? 1 : 0)
            };
            using var cmd = new SQLiteCommand(UpsertToolCsv, conn, tran);
            cmd.Parameters.AddRange(p);
            var result = await cmd.ExecuteScalarAsync();
            if (result != null)
                tool.ToolID = Convert.ToInt32(result);
        }
    
        public void ExportToolsToCsv(string filePath) =>
            RunSync(() => ExportToolsToCsvInternalAsync(filePath));

        public Task ExportToolsToCsvAsync(string filePath) =>
            ExportToolsToCsvInternalAsync(filePath);

        public virtual ImageImportResult ImportToolImages(string folderPath, Func<ToolModel, IEnumerable<string>> keySelector)
        {
            var result = new ImageImportResult();
            if (string.IsNullOrWhiteSpace(folderPath) || keySelector == null)
                return result;

            var tools = GetAllTools();
            var groups = new Dictionary<string, List<ToolModel>>(StringComparer.OrdinalIgnoreCase);
            foreach (var tool in tools)
            {
                var keys = keySelector(tool);
                if (keys == null) continue;
                foreach (var key in keys)
                {
                    var k = (key ?? string.Empty).Trim().ToUpperInvariant();
                    if (string.IsNullOrEmpty(k))
                        continue;
                    if (!groups.TryGetValue(k, out var list))
                        groups[k] = list = new List<ToolModel>();
                    list.Add(tool);
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

            foreach (var file in Directory.EnumerateFiles(folderPath))
            {
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
                var tool = list[0];
                if (!string.IsNullOrEmpty(tool.ToolImagePath))
                {
                    result.ConflictingFiles.Add(file);
                    continue;
                }
                var dest = Path.Combine(destDir, Path.GetFileName(file));
                if (!File.Exists(dest))
                {
                    try
                    {
                        CopyFile(file, dest);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, "Failed to copy image from {Source} to {Destination}", file, dest);
                        result.ConflictingFiles.Add(file);
                        continue;
                    }
                }
                var relative = $"Images/{Path.GetFileName(dest)}";
                UpdateToolImage(tool.ToolID, relative);
                result.ImportedCount++;
            }

            return result;
        }

        protected virtual void CopyFile(string sourceFileName, string destFileName)
            => File.Copy(sourceFileName, destFileName, true);

        private async Task<bool> ToolExistsAsync(string toolNumber, int? exceptId = null)
        {
            if (string.IsNullOrWhiteSpace(toolNumber))
                return false;

            var sql = "SELECT COUNT(*) FROM Tools WHERE ToolNumber = @TN";
            var parameters = new List<SQLiteParameter>
            {
                new("@TN", toolNumber)
            };

            if (exceptId.HasValue)
            {
                sql += " AND ToolID <> @ID";
                parameters.Add(new SQLiteParameter("@ID", exceptId.Value));
            }

            using var conn = _dbService.CreateConnection();
            var count = Convert.ToInt32(await SqliteHelper.ExecuteScalarAsync(conn, sql, parameters.ToArray()));
            return count > 0;
        }
    
        ToolModel MapTool(IDataRecord r) => new()
        {
            ToolID = r["ToolID"] is DBNull ? 0 : Convert.ToInt32(r["ToolID"]),
            ToolNumber = r["ToolNumber"].ToString(),
            PartNumber = r["PartNumber"].ToString(),
            NameDescription = r["NameDescription"].ToString(),
            Brand = r["Brand"].ToString(),
            Location = r["Location"].ToString(),
            QuantityOnHand = r["AvailableQuantity"] is DBNull ? 0 : Convert.ToInt32(r["AvailableQuantity"]),
            RentedQuantity = r["RentedQuantity"] is DBNull ? 0 : Convert.ToInt32(r["RentedQuantity"]),
            Supplier = r["Supplier"].ToString(),
            PurchasedDate = r["PurchasedDate"] is DBNull ? (DateTime?)null : Convert.ToDateTime(r["PurchasedDate"]),
            Notes = r["Notes"].ToString(),
            IsCheckedOut = (r["IsCheckedOut"] is DBNull ? 0 : Convert.ToInt32(r["IsCheckedOut"])) == 1,
            CheckedOutBy = r["CheckedOutBy"].ToString(),
            CheckedOutTime = r["CheckedOutTime"] is DBNull ? (DateTime?)null : Convert.ToDateTime(r["CheckedOutTime"]),
            ToolImagePath = r["ToolImagePath"]?.ToString(),
            Keywords = r["Keywords"]?.ToString(),
            IsPowerTool = (r["IsPowerTool"] is DBNull ? 0 : Convert.ToInt32(r["IsPowerTool"])) == 1
        };

        private async Task AddToolInternalAsync(ToolModel tool)
        {
            if (string.IsNullOrWhiteSpace(tool?.ToolNumber))
                throw new ArgumentException("ToolNumber is required.", nameof(tool));
            if (await ToolExistsAsync(tool.ToolNumber))
                throw new InvalidOperationException($"Tool {tool.ToolNumber} already exists.");
            ValidateQuantity(tool.QuantityOnHand);
            using var conn = _dbService.CreateConnection();
            await InsertToolAsync(conn, null, tool);
        }

        private async Task UpdateToolInternalAsync(ToolModel tool)
        {
            if (await ToolExistsAsync(tool.ToolNumber, tool.ToolID))
                throw new InvalidOperationException($"Tool {tool.ToolNumber} already exists.");
            using var conn = _dbService.CreateConnection();
            ValidateQuantity(tool.QuantityOnHand);
            const string sql = @"
                UPDATE Tools SET
                  ToolNumber = @ToolNumber,
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
                  ToolImagePath = @Img
                WHERE ToolID = @ID";
            var p = new[]
            {
                new SQLiteParameter("@ID", tool.ToolID),
                new SQLiteParameter("@ToolNumber", tool.ToolNumber),
                new SQLiteParameter("@Desc", (object)tool.NameDescription ?? DBNull.Value),
                new SQLiteParameter("@Loc", tool.Location),
                new SQLiteParameter("@Brand", tool.Brand),
                new SQLiteParameter("@PN", tool.PartNumber),
                new SQLiteParameter("@Sup", (object)tool.Supplier ?? DBNull.Value),
                new SQLiteParameter("@PD", (object)tool.PurchasedDate ?? DBNull.Value),
                new SQLiteParameter("@Notes", (object)tool.Notes ?? DBNull.Value),
                new SQLiteParameter("@Keywords", (object)tool.Keywords ?? DBNull.Value),
                new SQLiteParameter("@Avail", tool.QuantityOnHand),
                new SQLiteParameter("@Rent", tool.RentedQuantity),
                new SQLiteParameter("@Power", tool.IsPowerTool ? 1 : 0),
                new SQLiteParameter("@Out", tool.IsCheckedOut ? 1 : 0),
                new SQLiteParameter("@By", (object)tool.CheckedOutBy ?? DBNull.Value),
                new SQLiteParameter("@Time", (object)tool.CheckedOutTime ?? DBNull.Value),
                new SQLiteParameter("@Img", (object)tool.ToolImagePath ?? DBNull.Value)
            };
            try
            {
                await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p);
            }
            catch (SQLiteException ex)
            {
                _logger.LogError(ex, "Failed to update tool {ToolID}", tool.ToolID);
                throw new InvalidOperationException($"Failed to update tool {tool.ToolID}.", ex);
            }
        }

        private async Task DeleteToolInternalAsync(int toolID)
        {
            using var conn = _dbService.CreateConnection();
            try
            {
                await SqliteHelper.ExecuteNonQueryAsync(conn, "DELETE FROM Tools WHERE ToolID=@ID",
                    new[] { new SQLiteParameter("@ID", toolID) });
            }
            catch (SQLiteException ex)
            {
                _logger.LogError(ex, "Failed to delete tool {ToolID}", toolID);
                throw new InvalidOperationException($"Failed to delete tool {toolID}.", ex);
            }
        }

        public async Task<ToolModel> GetToolByIDAsync(int toolID)
        {
            using var conn = _dbService.CreateConnection();
            var list = await SqliteHelper.ExecuteReaderAsync(conn, "SELECT * FROM Tools WHERE ToolID=@ToolID",
                new[] { new SQLiteParameter("@ToolID", toolID) }, MapTool);
            return list.FirstOrDefault();
        }

        public async Task<List<ToolModel>> GetAllToolsAsync()
        {
            using var conn = _dbService.CreateConnection();
            return await SqliteHelper.ExecuteReaderAsync(conn, AllToolsSql, null, MapTool);
        }

        public async Task<List<ToolModel>> SearchToolsAsync(string? searchText, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await GetAllToolsAsync();
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
                "ToolNumber",
                "NameDescription",
                "Brand",
                "PartNumber",
                "Supplier",
                "Location",
                "Notes",
                "Keywords"
            };

            var sb = new StringBuilder("SELECT * FROM Tools WHERE ");
            var parameters = new List<SQLiteParameter>();
            for (int i = 0; i < terms.Length; i++)
            {
                if (i > 0) sb.Append(" AND ");
                var paramName = $"@p{i}";
                var likeClause = string.Join(" OR ", searchable.Select(col => $"{col} LIKE {paramName} COLLATE NOCASE"));
                sb.Append($"(CAST(ToolID AS TEXT) LIKE {paramName} COLLATE NOCASE OR {likeClause})");
                parameters.Add(new SQLiteParameter(paramName, $"%{terms[i]}%"));
            }

            cancellationToken.ThrowIfCancellationRequested();
            return await SqliteHelper.ExecuteReaderAsync(conn, sb.ToString(), parameters.ToArray(), MapTool);
        }

        private async Task ToggleToolCheckOutStatusInternalAsync(int toolID, string currentUser)
        {
            using var conn = _dbService.CreateConnection();
            var record = (await SqliteHelper.ExecuteReaderAsync(conn,
                "SELECT IsCheckedOut, AvailableQuantity FROM Tools WHERE ToolID=@ID",
                new[] { new SQLiteParameter("@ID", toolID) },
                r => new { Out = Convert.ToInt32(r["IsCheckedOut"]) == 1, Qty = Convert.ToInt32(r["AvailableQuantity"]) })).FirstOrDefault();

            if (record == null)
                throw new InvalidOperationException($"Tool {toolID} not found.");

            if (!record.Out && record.Qty <= 0)
                return;

            var newStatus = record.Out ? 0 : 1;
            var time = record.Out ? (object)DBNull.Value : DateTime.UtcNow;
            var by = record.Out ? (object)DBNull.Value : currentUser;
            var qtyChange = record.Out ? 1 : -1;

            await SqliteHelper.ExecuteNonQueryAsync(conn, @"
                UPDATE Tools SET
                  IsCheckedOut = @Out,
                  CheckedOutBy = @By,
                  CheckedOutTime = @Time,
                  AvailableQuantity = AvailableQuantity + @Q
                WHERE ToolID = @ID", new[]
            {
                new SQLiteParameter("@Out", newStatus),
                new SQLiteParameter("@By", by),
                new SQLiteParameter("@Time", time),
                new SQLiteParameter("@Q", qtyChange),
                new SQLiteParameter("@ID", toolID)
            });
        }

        public async Task<List<ToolModel>> GetToolsCheckedOutByAsync(string userName)
        {
            using var conn = _dbService.CreateConnection();
            return await SqliteHelper.ExecuteReaderAsync(conn,
                "SELECT * FROM Tools WHERE CheckedOutBy=@U", new[] { new SQLiteParameter("@U", userName) }, MapTool);
        }

        public async Task UpdateToolImageAsync(int toolID, string imagePath)
        {
            const string sql = "UPDATE Tools SET ToolImagePath=@Img WHERE ToolID=@ID";
            var p = new[]
            {
                new SQLiteParameter("@Img", imagePath),
                new SQLiteParameter("@ID", toolID)
            };
            using var conn = _dbService.CreateConnection();
            await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p);
        }

        private async Task<List<int>> ImportToolsFromCsvInternalAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken)
        {
            var tools = CsvHelperUtil.LoadToolsFromCsv(filePath, map, out var invalidRows);
            using var conn = _dbService.CreateConnection();
            var existingNumbers = new HashSet<string>(
                await SqliteHelper.ExecuteReaderAsync(conn,
                    "SELECT ToolNumber FROM Tools", null,
                    r => r.GetString(0)));

            using var tran = conn.BeginTransaction();
            try
            {
                foreach (var tool in tools)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (string.IsNullOrWhiteSpace(tool.ToolNumber) ||
                        existingNumbers.Contains(tool.ToolNumber))
                        continue;
                    await InsertToolAsync(conn, tran, tool);
                    existingNumbers.Add(tool.ToolNumber);
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

        private async Task ExportToolsToCsvInternalAsync(string filePath)
        {
            var tools = await GetAllToolsAsync();
            await CsvHelperUtil.ExportToolsToCsvAsync(filePath, tools);
        }

        public Task<ImageImportResult> ImportToolImagesAsync(string folderPath, Func<ToolModel, IEnumerable<string>> keySelector)
            => Task.FromResult(ImportToolImages(folderPath, keySelector));

        public async Task UpdateToolQuantitiesAsync(int toolID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null)
        {
            if (qtyChange <= 0) throw new ArgumentException("Quantity change must be positive.", nameof(qtyChange));
            var sql = isRental
                ? @"UPDATE Tools SET AvailableQuantity = AvailableQuantity - @Q, RentedQuantity = RentedQuantity + @Q WHERE ToolID = @ID AND AvailableQuantity >= @Q"
                : @"UPDATE Tools SET AvailableQuantity = AvailableQuantity + @Q, RentedQuantity = RentedQuantity - @Q WHERE ToolID = @ID AND RentedQuantity >= @Q";
            var p = new[]
            {
                new SQLiteParameter("@ID", toolID),
                new SQLiteParameter("@Q", qtyChange)
            };

            if (conn != null)
            {
                int rows = tx != null
                    ? await SqliteHelper.ExecuteNonQueryAsync(conn, tx, sql, p)
                    : await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p);
                if (rows == 0)
                {
                    _logger.LogWarning("Quantity update affected 0 rows for ToolID {ToolID}", toolID);
                    throw new InvalidOperationException("Quantity update failed.");
                }
            }
            else
            {
                using var c = _dbService.CreateConnection();
                int rows = await SqliteHelper.ExecuteNonQueryAsync(c, sql, p);
                if (rows == 0)
                {
                    _logger.LogWarning("Quantity update affected 0 rows for ToolID {ToolID}", toolID);
                    throw new InvalidOperationException("Quantity update failed.");
                }
            }
        }
    }
}

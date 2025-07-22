using System.Data.SQLite;
using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Utilities.IO;
using ToolManagementAppV2.Models.ImportExport;
using ToolManagementAppV2.Interfaces;
using System.Text;

namespace ToolManagementAppV2.Services.Tools
{
    public class ToolService : IToolService
    {
        readonly DatabaseService _dbService;
        List<ToolModel>? _cache;
        const string AllToolsSql = "SELECT * FROM Tools";
        const string UpsertToolCsv = @"
            INSERT INTO Tools
              (ToolNumber, NameDescription, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, Keywords, AvailableQuantity, RentedQuantity, IsCheckedOut)
            VALUES (@ToolNumber,@Desc,@Loc,@Brand,@PN,@Sup,@PD,@Notes,@Keywords,@Avail,@Rent,0);
            SELECT last_insert_rowid();";
    
        public ToolService(DatabaseService dbService)
        {
            _dbService = dbService;
        }
    
        public List<ToolModel> GetAllTools()
        {
            if (_cache != null)
                return _cache;

            using var conn = _dbService.CreateConnection();
            _cache = SqliteHelper.ExecuteReader(conn, AllToolsSql, null, MapTool);
            return _cache;
        }
    
        public ToolModel GetToolByID(string toolID)
        {
            var cached = GetAllTools().FirstOrDefault(t => t.ToolID == toolID);
            if (cached != null)
                return cached;

            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReader(conn, "SELECT * FROM Tools WHERE ToolID=@ToolID",
                new[] { new SQLiteParameter("@ToolID", toolID) }, MapTool).FirstOrDefault();
        }
    
        public List<ToolModel> SearchTools(string? searchText)
        {
            var all = GetAllTools();
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<ToolModel>(all);

            var terms = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return all.Where(t => terms.All(term =>
                (t.ToolID?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.ToolNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.NameDescription?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.Brand?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.PartNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.Supplier?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.Location?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.Notes?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.Keywords?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
            )).ToList();
        }
    
        public void AddTool(ToolModel tool)
        {
            if (ToolExists(tool.ToolNumber))
                throw new InvalidOperationException($"Tool {tool.ToolNumber} already exists.");
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
                new SQLiteParameter("@Rent", tool.RentedQuantity)
            };
            using var conn = _dbService.CreateConnection();
            var result = SqliteHelper.ExecuteScalar(conn, UpsertToolCsv, p);
            if (result != null)
                tool.ToolID = result.ToString();
            _cache = null;
        }
    
        public void UpdateTool(ToolModel tool)
        {
            var dup = GetAllTools().Any(t => t.ToolNumber == tool.ToolNumber && t.ToolID != tool.ToolID);
            if (dup)
                throw new InvalidOperationException($"Tool {tool.ToolNumber} already exists.");
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
                new SQLiteParameter("@Out", tool.IsCheckedOut ? 1 : 0),
                new SQLiteParameter("@By", (object)tool.CheckedOutBy ?? DBNull.Value),
                new SQLiteParameter("@Time", (object)tool.CheckedOutTime ?? DBNull.Value),
                new SQLiteParameter("@Img", (object)tool.ToolImagePath ?? DBNull.Value)
            };
            using var conn = _dbService.CreateConnection();
            SqliteHelper.ExecuteNonQuery(conn, sql, p);
            _cache = null;
        }
    
        public void UpdateToolQuantities(string toolID, int qtyChange, bool isRental)
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
            using var conn = _dbService.CreateConnection();
            if (SqliteHelper.ExecuteNonQuery(conn, sql, p) == 0)
                throw new InvalidOperationException("Quantity update failed.");
            _cache = null;
        }
    
        public void DeleteTool(string toolID)
        {
            using var conn = _dbService.CreateConnection();
            SqliteHelper.ExecuteNonQuery(conn, "DELETE FROM Tools WHERE ToolID=@ID",
                new[] { new SQLiteParameter("@ID", toolID) });
            _cache = null;
        }
    
        public void ToggleToolCheckOutStatus(string toolID, string currentUser)
        {
            using var conn = _dbService.CreateConnection();
            var record = SqliteHelper.ExecuteReader(conn,
                "SELECT IsCheckedOut, AvailableQuantity FROM Tools WHERE ToolID=@ID",
                new[] { new SQLiteParameter("@ID", toolID) },
                r => new { Out = Convert.ToInt32(r["IsCheckedOut"]) == 1, Qty = Convert.ToInt32(r["AvailableQuantity"]) }).FirstOrDefault();

            if (record == null)
                throw new InvalidOperationException($"Tool {toolID} not found.");

            if (!record.Out && record.Qty <= 0)
                return;

            var newStatus = record.Out ? 0 : 1;
            var time = record.Out ? (object)DBNull.Value : DateTime.Now;
            var by = record.Out ? (object)DBNull.Value : currentUser;
            var qtyChange = record.Out ? 1 : -1;

            SqliteHelper.ExecuteNonQuery(conn, @"
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
            _cache = null;
        }
    
        public List<ToolModel> GetToolsCheckedOutBy(string userName)
        {
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReader(conn, "SELECT * FROM Tools WHERE CheckedOutBy=@User AND IsCheckedOut=1",
                new[] { new SQLiteParameter("@User", userName) }, MapTool);
        }
    
        public void UpdateToolImage(string toolID, string imagePath)
        {
            using var conn = _dbService.CreateConnection();
            SqliteHelper.ExecuteNonQuery(conn, "UPDATE Tools SET ToolImagePath=@Img WHERE ToolID=@ID",
                new[]
                {
                    new SQLiteParameter("@Img", imagePath),
                    new SQLiteParameter("@ID", toolID)
                });
            _cache = null;
        }
    
        public List<int> ImportToolsFromCsv(string filePath, IDictionary<string, string> map)
        {
            var tools = CsvHelperUtil.LoadToolsFromCsv(filePath, map, out var invalidRows);
            foreach (var tool in tools)
            {
                if (!ToolExists(tool.ToolNumber))
                    AddTool(tool);
            }
            return invalidRows;
        }
    
        public void ExportToolsToCsv(string filePath)
        {
            var tools = GetAllTools();
            CsvHelperUtil.ExportToolsToCsv(filePath, tools);
        }

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
            Directory.CreateDirectory(destDir);

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
                    File.Copy(file, dest, true);
                var relative = $"Images/{Path.GetFileName(dest)}";
                UpdateToolImage(tool.ToolID, relative);
                result.ImportedCount++;
            }

            return result;
        }
    
        private bool ToolExists(string toolNumber)
        {
            const string sql = "SELECT COUNT(*) FROM Tools WHERE ToolNumber = @TN";
            using var conn = _dbService.CreateConnection();
            var count = Convert.ToInt32(SqliteHelper.ExecuteScalar(conn, sql, new[] {
                new SQLiteParameter("@TN", toolNumber)
            }));
            return count > 0;
        }
    
        ToolModel MapTool(IDataRecord r) => new()
        {
            ToolID = r["ToolID"].ToString(),
            ToolNumber = r["ToolNumber"].ToString(),
            PartNumber = r["PartNumber"].ToString(),
            NameDescription = r["NameDescription"].ToString(),
            Brand = r["Brand"].ToString(),
            Location = r["Location"].ToString(),
            QuantityOnHand = Convert.ToInt32(r["AvailableQuantity"]),
            RentedQuantity = Convert.ToInt32(r["RentedQuantity"]),
            Supplier = r["Supplier"].ToString(),
            PurchasedDate = r["PurchasedDate"] is DBNull ? (DateTime?)null : Convert.ToDateTime(r["PurchasedDate"]),
            Notes = r["Notes"].ToString(),
            IsCheckedOut = Convert.ToInt32(r["IsCheckedOut"]) == 1,
            CheckedOutBy = r["CheckedOutBy"].ToString(),
            CheckedOutTime = r["CheckedOutTime"] is DBNull ? (DateTime?)null : Convert.ToDateTime(r["CheckedOutTime"]),
            ToolImagePath = r["ToolImagePath"]?.ToString(),
            Keywords = r["Keywords"]?.ToString()
        };
    }
}

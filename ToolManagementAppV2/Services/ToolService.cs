using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Helpers;

namespace ToolManagementAppV2.Services
{
    public class ToolService
    {
        readonly string _connString;
        const string AllToolsSql = "SELECT * FROM Tools";
        const string UpsertToolCsv = @"
            INSERT INTO Tools 
              (Name, Description, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, AvailableQuantity, RentedQuantity, IsCheckedOut)
            VALUES (@Name,@Desc,@Loc,@Brand,@PN,@Sup,@PD,@Notes,@Avail,@Rent,0)";

        public ToolService(DatabaseService dbService) =>
            _connString = dbService.ConnectionString;

        public List<Tool> GetAllTools() =>
            SqliteHelper.ExecuteReader(_connString, AllToolsSql, null, MapTool);

        public Tool GetToolByID(string toolID) =>
            SqliteHelper.ExecuteReader(_connString, "SELECT * FROM Tools WHERE ToolID=@ToolID",
                new[] { new SQLiteParameter("@ToolID", toolID) }, MapTool).FirstOrDefault();

        public List<Tool> SearchTools(string searchText)
        {
            var terms = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder("SELECT * FROM Tools");
            if (terms.Any())
            {
                sb.Append(" WHERE ")
                  .Append(string.Join(" AND ", terms.Select((t, i) =>
                    "(ToolID LIKE @p" + i +
                    " OR Name LIKE @p" + i +
                    " OR Description LIKE @p" + i +
                    " OR Brand LIKE @p" + i +
                    " OR PartNumber LIKE @p" + i +
                    " OR Supplier LIKE @p" + i +
                    " OR Location LIKE @p" + i +
                    " OR Notes LIKE @p" + i + ")")));
            }
            var parameters = terms
                .Select((t, i) => new SQLiteParameter("@p" + i, $"%{t}%"))
                .ToArray();
            return SqliteHelper.ExecuteReader(_connString, sb.ToString(), parameters, MapTool);
        }

        public void AddTool(Tool tool)
        {
            var p = new[]
            {
                new SQLiteParameter("@Name",  tool.Name),
                new SQLiteParameter("@Desc",  (object)tool.Description ?? DBNull.Value),
                new SQLiteParameter("@Loc",   tool.Location),
                new SQLiteParameter("@Brand", tool.Brand),
                new SQLiteParameter("@PN",    tool.PartNumber),
                new SQLiteParameter("@Sup",   (object)tool.Supplier ?? DBNull.Value),
                new SQLiteParameter("@PD",    (object)tool.PurchasedDate ?? DBNull.Value),
                new SQLiteParameter("@Notes", (object)tool.Notes ?? DBNull.Value),
                new SQLiteParameter("@Avail", tool.QuantityOnHand),
                new SQLiteParameter("@Rent",  tool.RentedQuantity)
            };
            SqliteHelper.ExecuteNonQuery(_connString, UpsertToolCsv, p);
        }

        public void UpdateTool(Tool tool)
        {
            const string sql = @"
                UPDATE Tools SET
                  Name = @Name,
                  Description = @Desc,
                  Location = @Loc,
                  Brand = @Brand,
                  PartNumber = @PN,
                  Supplier = @Sup,
                  PurchasedDate = @PD,
                  Notes = @Notes,
                  AvailableQuantity = @Avail,
                  RentedQuantity = @Rent,
                  IsCheckedOut = @Out,
                  CheckedOutBy = @By,
                  CheckedOutTime = @Time,
                  ToolImagePath = @Img
                WHERE ToolID = @ID";
            var p = new[]
            {
                new SQLiteParameter("@ID",   tool.ToolID),
                new SQLiteParameter("@Name", tool.Name),
                new SQLiteParameter("@Desc",  (object)tool.Description ?? DBNull.Value),
                new SQLiteParameter("@Loc",   tool.Location),
                new SQLiteParameter("@Brand", tool.Brand),
                new SQLiteParameter("@PN",    tool.PartNumber),
                new SQLiteParameter("@Sup",   (object)tool.Supplier ?? DBNull.Value),
                new SQLiteParameter("@PD",    (object)tool.PurchasedDate ?? DBNull.Value),
                new SQLiteParameter("@Notes", (object)tool.Notes ?? DBNull.Value),
                new SQLiteParameter("@Avail", tool.QuantityOnHand),
                new SQLiteParameter("@Rent",  tool.RentedQuantity),
                new SQLiteParameter("@Out",   tool.IsCheckedOut ? 1 : 0),
                new SQLiteParameter("@By",    (object)tool.CheckedOutBy ?? DBNull.Value),
                new SQLiteParameter("@Time",  (object)tool.CheckedOutTime ?? DBNull.Value),
                new SQLiteParameter("@Img",   (object)tool.ToolImagePath ?? DBNull.Value)
            };
            SqliteHelper.ExecuteNonQuery(_connString, sql, p);
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
                new SQLiteParameter("@Q",  qtyChange)
            };
            if (SqliteHelper.ExecuteNonQuery(_connString, sql, p) == 0)
                throw new InvalidOperationException("Quantity update failed.");
        }

        public void DeleteTool(string toolID) =>
            SqliteHelper.ExecuteNonQuery(_connString, "DELETE FROM Tools WHERE ToolID=@ID",
                new[] { new SQLiteParameter("@ID", toolID) });

        public void ToggleToolCheckOutStatus(string toolID, string currentUser)
        {
            var isOut = Convert.ToInt32(SqliteHelper.ExecuteScalar(_connString,
                "SELECT IsCheckedOut FROM Tools WHERE ToolID=@ID",
                new[] { new SQLiteParameter("@ID", toolID) })) == 1;
            var newStatus = isOut ? 0 : 1;
            var time = isOut ? (object)DBNull.Value : DateTime.Now;
            var by = isOut ? (object)DBNull.Value : currentUser;
            SqliteHelper.ExecuteNonQuery(_connString, @"
                UPDATE Tools SET
                  IsCheckedOut = @Out,
                  CheckedOutBy = @By,
                  CheckedOutTime = @Time
                WHERE ToolID = @ID", new[]
            {
                new SQLiteParameter("@Out",  newStatus),
                new SQLiteParameter("@By",   by),
                new SQLiteParameter("@Time", time),
                new SQLiteParameter("@ID",   toolID)
            });
        }

        public List<Tool> GetToolsCheckedOutBy(string userName) =>
            SqliteHelper.ExecuteReader(_connString, "SELECT * FROM Tools WHERE CheckedOutBy=@User AND IsCheckedOut=1",
                new[] { new SQLiteParameter("@User", userName) }, MapTool);

        public void UpdateToolImage(string toolID, string imagePath) =>
            SqliteHelper.ExecuteNonQuery(_connString, "UPDATE Tools SET ToolImagePath=@Img WHERE ToolID=@ID",
                new[]
                {
                    new SQLiteParameter("@Img", imagePath),
                    new SQLiteParameter("@ID",  toolID)
                });

        public void ImportToolsFromCsv(string filePath, IDictionary<string, string> map)
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length < 2) return;

            var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
            var idx = map.ToDictionary(
                kv => kv.Key,
                kv => Array.IndexOf(headers, kv.Value.Trim()),
                StringComparer.OrdinalIgnoreCase
            );

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cols = line.Split(',');

                string get(string prop) =>
                    idx.TryGetValue(prop, out var i) && i >= 0 && i < cols.Length
                        ? cols[i].Trim()
                        : string.Empty;

                AddTool(new Tool
                {
                    Name = get("Name"),
                    Description = get("Description"),
                    Location = get("Location"),
                    Brand = get("Brand"),
                    PartNumber = get("PartNumber"),
                    Supplier = get("Supplier"),
                    PurchasedDate = DateTime.TryParse(get("PurchasedDate"), out var d) ? d : (DateTime?)null,
                    Notes = get("Notes"),
                    QuantityOnHand = int.TryParse(get("AvailableQuantity"), out var q) ? q : 0
                });
            }
        }

        public void ExportToolsToCsv(string filePath)
        {
            using var writer = new StreamWriter(filePath);
            writer.WriteLine("Name,Description,Location,Brand,PartNumber,Supplier,PurchasedDate,Notes,AvailableQuantity,RentedQuantity,IsCheckedOut");
            SqliteHelper.ExecuteReader(_connString, AllToolsSql, null, MapTool).ForEach(t =>
            {
                var vals = new[]
                {
                    t.Name, t.Description, t.Location, t.Brand, t.PartNumber,
                    t.Supplier, t.PurchasedDate?.ToString("s") ?? "",
                    t.Notes, t.QuantityOnHand.ToString(), t.RentedQuantity.ToString(),
                    t.IsCheckedOut ? "1" : "0"
                };
                writer.WriteLine(string.Join(",", vals));
            });
        }

        Tool MapTool(IDataRecord r) => new()
        {
            ToolID = r["ToolID"].ToString(),
            Name = r["Name"].ToString(),
            PartNumber = r["PartNumber"].ToString(),
            Description = r["Description"].ToString(),
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
            ToolImagePath = r["ToolImagePath"]?.ToString()
        };
    }
}

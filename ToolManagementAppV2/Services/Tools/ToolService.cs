using System.Data.SQLite;
using System.Data;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Utilities.IO;
using ToolManagementAppV2.Models.ImportExport;
using ToolManagementAppV2.Interfaces;
using System.Text;

public class ToolService : IToolService
{
    readonly string _connString;
    const string AllToolsSql = "SELECT * FROM Tools";
    const string UpsertToolCsv = @"
        INSERT INTO Tools 
          (ToolNumber, NameDescription, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, Keywords, AvailableQuantity, RentedQuantity, IsCheckedOut)
        VALUES (@ToolNumber,@Desc,@Loc,@Brand,@PN,@Sup,@PD,@Notes,@Keywords,@Avail,@Rent,0)";

    public ToolService(DatabaseService dbService) => _connString = dbService.ConnectionString;

    public List<ToolModel> GetAllTools() =>
        SqliteHelper.ExecuteReader(_connString, AllToolsSql, null, MapTool);

    public ToolModel GetToolByID(string toolID) =>
        SqliteHelper.ExecuteReader(_connString, "SELECT * FROM Tools WHERE ToolID=@ToolID",
            new[] { new SQLiteParameter("@ToolID", toolID) }, MapTool).FirstOrDefault();

    public List<ToolModel> SearchTools(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return GetAllTools();

        var terms = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder("SELECT * FROM Tools");
        if (terms.Any())
        {
            sb.Append(" WHERE ")
              .Append(string.Join(" AND ", terms.Select((t, i) =>
                "(ToolID LIKE @p" + i +
                " OR ToolNumber LIKE @p" + i +
                " OR NameDescription LIKE @p" + i +
                " OR Brand LIKE @p" + i +
                " OR PartNumber LIKE @p" + i +
                " OR Supplier LIKE @p" + i +
                " OR Location LIKE @p" + i +
                " OR Notes LIKE @p" + i +
                " OR Keywords LIKE @p" + i + ")")));
        }
        var parameters = terms
            .Select((t, i) => new SQLiteParameter("@p" + i, $"%{t}%"))
            .ToArray();
        return SqliteHelper.ExecuteReader(_connString, sb.ToString(), parameters, MapTool);
    }

    public void AddTool(ToolModel tool)
    {
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
        SqliteHelper.ExecuteNonQuery(_connString, UpsertToolCsv, p);
    }

    public void UpdateTool(ToolModel tool)
    {
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
            new SQLiteParameter("@Q", qtyChange)
        };
        if (SqliteHelper.ExecuteNonQuery(_connString, sql, p) == 0)
            throw new InvalidOperationException("Quantity update failed.");
    }

    public void DeleteTool(string toolID) =>
        SqliteHelper.ExecuteNonQuery(_connString, "DELETE FROM Tools WHERE ToolID=@ID",
            new[] { new SQLiteParameter("@ID", toolID) });

    public void ToggleToolCheckOutStatus(string toolID, string currentUser)
    {
        var result = SqliteHelper.ExecuteScalar(_connString,
            "SELECT IsCheckedOut FROM Tools WHERE ToolID=@ID",
               new[] { new SQLiteParameter("@ID", toolID) });

        if (result == null)
            throw new InvalidOperationException($"Tool {toolID} not found.");

        var isOut = Convert.ToInt32(result) == 1;
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
            new SQLiteParameter("@Out", newStatus),
            new SQLiteParameter("@By", by),
            new SQLiteParameter("@Time", time),
            new SQLiteParameter("@ID", toolID)
        });
    }

    public List<ToolModel> GetToolsCheckedOutBy(string userName) =>
        SqliteHelper.ExecuteReader(_connString, "SELECT * FROM Tools WHERE CheckedOutBy=@User AND IsCheckedOut=1",
            new[] { new SQLiteParameter("@User", userName) }, MapTool);

    public void UpdateToolImage(string toolID, string imagePath) =>
        SqliteHelper.ExecuteNonQuery(_connString, "UPDATE Tools SET ToolImagePath=@Img WHERE ToolID=@ID",
            new[]
            {
                new SQLiteParameter("@Img", imagePath),
                new SQLiteParameter("@ID", toolID)
            });

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

    private bool ToolExists(string toolNumber)
    {
        const string sql = "SELECT COUNT(*) FROM Tools WHERE ToolNumber = @TN";
        var count = Convert.ToInt32(SqliteHelper.ExecuteScalar(_connString, sql, new[] {
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

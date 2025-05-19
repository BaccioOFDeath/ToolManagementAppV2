// File: Services/ToolService.cs
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using ToolManagementAppV2.Models;

namespace ToolManagementAppV2.Services
{
    public class ToolService
    {
        private readonly DatabaseService _dbService;

        public ToolService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public List<Tool> GetAllTools()
        {
            var tools = new List<Tool>();
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "SELECT * FROM Tools";
            using var cmd = new SQLiteCommand(query, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tools.Add(new Tool
                {
                    ToolID = reader["ToolID"].ToString(),
                    Name = reader["Name"].ToString(),
                    PartNumber = reader["PartNumber"].ToString(),
                    Description = reader["Description"].ToString(),
                    Brand = reader["Brand"].ToString(),
                    Location = reader["Location"].ToString(),
                    QuantityOnHand = Convert.ToInt32(reader["AvailableQuantity"]),
                    RentedQuantity = Convert.ToInt32(reader["RentedQuantity"]),
                    Supplier = reader["Supplier"].ToString(),
                    PurchasedDate = reader["PurchasedDate"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["PurchasedDate"]),
                    Notes = reader["Notes"].ToString(),
                    IsCheckedOut = Convert.ToInt32(reader["IsCheckedOut"]) == 1,
                    CheckedOutBy = reader["CheckedOutBy"].ToString(),
                    CheckedOutTime = reader["CheckedOutTime"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["CheckedOutTime"]),
                    ToolImagePath = reader["ToolImagePath"]?.ToString()
                });
            }
            return tools;
        }

        public Tool GetToolByID(string toolID)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "SELECT * FROM Tools WHERE ToolID = @ToolID";
            using var cmd = new SQLiteCommand(query, connection);
            cmd.Parameters.AddWithValue("@ToolID", toolID);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Tool
                {
                    ToolID = reader["ToolID"].ToString(),
                    Name = reader["Name"].ToString(),
                    PartNumber = reader["PartNumber"].ToString(),
                    Description = reader["Description"].ToString(),
                    Brand = reader["Brand"].ToString(),
                    Location = reader["Location"].ToString(),
                    QuantityOnHand = Convert.ToInt32(reader["AvailableQuantity"]),
                    RentedQuantity = Convert.ToInt32(reader["RentedQuantity"]),
                    Supplier = reader["Supplier"].ToString(),
                    PurchasedDate = reader["PurchasedDate"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["PurchasedDate"]),
                    Notes = reader["Notes"].ToString(),
                    IsCheckedOut = Convert.ToInt32(reader["IsCheckedOut"]) == 1,
                    CheckedOutBy = reader["CheckedOutBy"].ToString(),
                    CheckedOutTime = reader["CheckedOutTime"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["CheckedOutTime"]),
                    ToolImagePath = reader["ToolImagePath"]?.ToString()
                };
            }
            return null;
        }

        public List<Tool> SearchTools(string searchText)
        {
            var tools = new List<Tool>();
            var terms = searchText
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder("SELECT * FROM Tools");
            if (terms.Length > 0)
            {
                sb.Append(" WHERE ");
                for (int i = 0; i < terms.Length; i++)
                {
                    var p = $"@p{i}";
                    sb.Append("(")
                      .Append("ToolID LIKE ").Append(p)
                      .Append(" OR Name LIKE ").Append(p)
                      .Append(" OR Description LIKE ").Append(p)
                      .Append(" OR Brand LIKE ").Append(p)
                      .Append(" OR PartNumber LIKE ").Append(p)
                      .Append(" OR Supplier LIKE ").Append(p)
                      .Append(" OR Location LIKE ").Append(p)
                      .Append(" OR Notes LIKE ").Append(p)
                      .Append(")");
                    if (i < terms.Length - 1)
                        sb.Append(" AND ");
                }
            }

            using var conn = new SQLiteConnection(_dbService.ConnectionString);
            conn.Open();
            using var cmd = new SQLiteCommand(sb.ToString(), conn);
            for (int i = 0; i < terms.Length; i++)
                cmd.Parameters.AddWithValue($"@p{i}", $"%{terms[i]}%");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tools.Add(new Tool
                {
                    ToolID = reader["ToolID"].ToString(),
                    Name = reader["Name"].ToString(),
                    PartNumber = reader["PartNumber"].ToString(),
                    Description = reader["Description"].ToString(),
                    Brand = reader["Brand"].ToString(),
                    Location = reader["Location"].ToString(),
                    QuantityOnHand = Convert.ToInt32(reader["AvailableQuantity"]),
                    RentedQuantity = Convert.ToInt32(reader["RentedQuantity"]),
                    Supplier = reader["Supplier"].ToString(),
                    PurchasedDate = reader["PurchasedDate"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["PurchasedDate"]),
                    Notes = reader["Notes"].ToString(),
                    IsCheckedOut = Convert.ToInt32(reader["IsCheckedOut"]) == 1,
                    CheckedOutBy = reader["CheckedOutBy"].ToString(),
                    CheckedOutTime = reader["CheckedOutTime"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["CheckedOutTime"]),
                    ToolImagePath = reader["ToolImagePath"]?.ToString()
                });
            }

            return tools;
        }
    
        public void AddTool(Tool tool)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = @"
                INSERT INTO Tools 
                  (Name, Description, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, AvailableQuantity, RentedQuantity, IsCheckedOut)
                VALUES 
                  (@Name, @Description, @Location, @Brand, @PartNumber, @Supplier, @PurchasedDate, @Notes, @AvailableQuantity, 0, 0)";
            using var cmd = new SQLiteCommand(query, connection);
            cmd.Parameters.AddWithValue("@Name", tool.Name);
            cmd.Parameters.AddWithValue("@Description", tool.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Location", tool.Location);
            cmd.Parameters.AddWithValue("@Brand", tool.Brand);
            cmd.Parameters.AddWithValue("@PartNumber", tool.PartNumber);
            cmd.Parameters.AddWithValue("@Supplier", tool.Supplier ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@PurchasedDate", tool.PurchasedDate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", tool.Notes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AvailableQuantity", tool.QuantityOnHand);
            cmd.ExecuteNonQuery();
        }

        public void UpdateTool(Tool tool)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = @"
                UPDATE Tools SET
                  Name = @Name,
                  Description = @Description,
                  Location = @Location,
                  Brand = @Brand,
                  PartNumber = @PartNumber,
                  Supplier = @Supplier,
                  PurchasedDate = @PurchasedDate,
                  Notes = @Notes,
                  AvailableQuantity = @AvailableQuantity,
                  IsCheckedOut = @IsCheckedOut,
                  CheckedOutBy = @CheckedOutBy,
                  CheckedOutTime = @CheckedOutTime,
                  ToolImagePath = @ToolImagePath
                WHERE ToolID = @ToolID";
            using var cmd = new SQLiteCommand(query, connection);
            cmd.Parameters.AddWithValue("@ToolID", tool.ToolID);
            cmd.Parameters.AddWithValue("@Name", tool.Name);
            cmd.Parameters.AddWithValue("@Description", tool.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Location", tool.Location);
            cmd.Parameters.AddWithValue("@Brand", tool.Brand);
            cmd.Parameters.AddWithValue("@PartNumber", tool.PartNumber);
            cmd.Parameters.AddWithValue("@Supplier", tool.Supplier ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@PurchasedDate", tool.PurchasedDate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", tool.Notes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AvailableQuantity", tool.QuantityOnHand);
            cmd.Parameters.AddWithValue("@IsCheckedOut", tool.IsCheckedOut ? 1 : 0);
            cmd.Parameters.AddWithValue("@CheckedOutBy", tool.CheckedOutBy ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CheckedOutTime", tool.CheckedOutTime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ToolImagePath", tool.ToolImagePath ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void UpdateToolQuantities(string toolID, int quantityChange, bool isRental)
        {
            if (quantityChange <= 0) throw new ArgumentException("Quantity change must be > 0");
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = isRental
                ? @"UPDATE Tools
                    SET AvailableQuantity = AvailableQuantity - @Q,
                        RentedQuantity   = RentedQuantity + @Q
                    WHERE ToolID = @ToolID AND AvailableQuantity >= @Q"
                : @"UPDATE Tools
                    SET AvailableQuantity = AvailableQuantity + @Q,
                        RentedQuantity   = RentedQuantity - @Q
                    WHERE ToolID = @ToolID AND RentedQuantity >= @Q";
            using var cmd = new SQLiteCommand(query, connection);
            cmd.Parameters.AddWithValue("@ToolID", toolID);
            cmd.Parameters.AddWithValue("@Q", quantityChange);
            if (cmd.ExecuteNonQuery() == 0)
                throw new InvalidOperationException("Quantity update failed.");
        }

        public void DeleteTool(string toolID)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "DELETE FROM Tools WHERE ToolID = @ToolID";
            using var cmd = new SQLiteCommand(query, connection);
            cmd.Parameters.AddWithValue("@ToolID", toolID);
            cmd.ExecuteNonQuery();
        }

        public void ToggleToolCheckOutStatus(string toolID, string currentUser)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var select = "SELECT IsCheckedOut FROM Tools WHERE ToolID = @ToolID";
            using var sCmd = new SQLiteCommand(select, connection);
            sCmd.Parameters.AddWithValue("@ToolID", toolID);
            bool isOut = Convert.ToInt32(sCmd.ExecuteScalar()) == 1;
            bool newStatus = !isOut;
            DateTime? time = newStatus ? DateTime.Now : (DateTime?)null;
            string by = newStatus ? currentUser : null;
            var update = @"
                UPDATE Tools SET
                  IsCheckedOut = @O,
                  CheckedOutBy = @B,
                  CheckedOutTime = @T
                WHERE ToolID = @ToolID";
            using var uCmd = new SQLiteCommand(update, connection);
            uCmd.Parameters.AddWithValue("@O", newStatus ? 1 : 0);
            uCmd.Parameters.AddWithValue("@B", by ?? (object)DBNull.Value);
            uCmd.Parameters.AddWithValue("@T", time ?? (object)DBNull.Value);
            uCmd.Parameters.AddWithValue("@ToolID", toolID);
            uCmd.ExecuteNonQuery();
        }

        public List<Tool> GetToolsCheckedOutBy(string userName)
        {
            var tools = new List<Tool>();
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "SELECT * FROM Tools WHERE CheckedOutBy = @User AND IsCheckedOut = 1";
            using var cmd = new SQLiteCommand(query, connection);
            cmd.Parameters.AddWithValue("@User", userName);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tools.Add(new Tool
                {
                    ToolID = reader["ToolID"].ToString(),
                    Name = reader["Name"].ToString(),
                    PartNumber = reader["PartNumber"].ToString(),
                    Description = reader["Description"].ToString(),
                    Brand = reader["Brand"].ToString(),
                    Location = reader["Location"].ToString(),
                    QuantityOnHand = Convert.ToInt32(reader["AvailableQuantity"]),
                    RentedQuantity = Convert.ToInt32(reader["RentedQuantity"]),
                    Supplier = reader["Supplier"].ToString(),
                    PurchasedDate = reader["PurchasedDate"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["PurchasedDate"]),
                    Notes = reader["Notes"].ToString(),
                    IsCheckedOut = true,
                    CheckedOutBy = reader["CheckedOutBy"].ToString(),
                    CheckedOutTime = reader["CheckedOutTime"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["CheckedOutTime"]),
                    ToolImagePath = reader["ToolImagePath"]?.ToString()
                });
            }
            return tools;
        }

        public void UpdateToolImage(string toolID, string imagePath)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "UPDATE Tools SET ToolImagePath = @P WHERE ToolID = @ID";
            using var cmd = new SQLiteCommand(query, connection);
            cmd.Parameters.AddWithValue("@P", imagePath);
            cmd.Parameters.AddWithValue("@ID", toolID);
            cmd.ExecuteNonQuery();
        }

        public void ImportToolsFromCsv(string filePath)
        {
            var lines = File.ReadAllLines(filePath).Skip(1);
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            foreach (var line in lines)
            {
                var c = line.Split(',');
                var query = @"
                    INSERT INTO Tools 
                      (Name, Description, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, AvailableQuantity, RentedQuantity, IsCheckedOut)
                    VALUES 
                      (@Name,@Desc,@Loc,@Brand,@PN,@Sup,@PD,@Notes,@Avail,@Rent,0)";
                using var cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Name", c[0]);
                cmd.Parameters.AddWithValue("@Desc", c[1]);
                cmd.Parameters.AddWithValue("@Loc", c[2]);
                cmd.Parameters.AddWithValue("@Brand", c[3]);
                cmd.Parameters.AddWithValue("@PN", c[4]);
                cmd.Parameters.AddWithValue("@Sup", c[5]);
                cmd.Parameters.AddWithValue("@PD", DateTime.TryParse(c[6], out var d) ? (object)d : DBNull.Value);
                cmd.Parameters.AddWithValue("@Notes", c[7]);
                cmd.Parameters.AddWithValue("@Avail", int.TryParse(c[8], out var q) ? q : 0);
                cmd.Parameters.AddWithValue("@Rent", int.TryParse(c[9], out var r) ? r : 0);
                cmd.ExecuteNonQuery();
            }
        }

        public void ExportToolsToCsv(string filePath)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "SELECT * FROM Tools";
            using var cmd = new SQLiteCommand(query, connection);
            using var reader = cmd.ExecuteReader();
            using var wr = new StreamWriter(filePath);
            wr.WriteLine("Name,Description,Location,Brand,PartNumber,Supplier,PurchasedDate,Notes,AvailableQuantity,RentedQuantity,IsCheckedOut");
            while (reader.Read())
            {
                var vals = new[]
                {
                    reader["Name"],
                    reader["Description"],
                    reader["Location"],
                    reader["Brand"],
                    reader["PartNumber"],
                    reader["Supplier"],
                    reader["PurchasedDate"],
                    reader["Notes"],
                    reader["AvailableQuantity"],
                    reader["RentedQuantity"],
                    reader["IsCheckedOut"]
                };
                wr.WriteLine(string.Join(",", vals));
            }
        }

        public void ImportToolsFromCsv(string filePath, IDictionary<string, string> map)
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length < 2) return;
            var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
            var idx = map.ToDictionary(kv => kv.Key,
                                      kv => System.Array.IndexOf(headers, kv.Value));

            using var conn = new SQLiteConnection(_dbService.ConnectionString);
            conn.Open();
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cols = line.Split(',');

                string get(string prop)
                    => idx[prop] >= 0 && idx[prop] < cols.Length
                        ? cols[idx[prop]].Trim()
                        : "";

                DateTime? pd = DateTime.TryParse(get("PurchasedDate"), out var d) ? d : (DateTime?)null;
                int aq = int.TryParse(get("AvailableQuantity"), out var q) ? q : 0;

                var tool = new Tool
                {
                    Name = get("Name"),
                    Description = get("Description"),
                    Location = get("Location"),
                    Brand = get("Brand"),
                    PartNumber = get("PartNumber"),
                    Supplier = get("Supplier"),
                    PurchasedDate = pd,
                    Notes = get("Notes"),
                    QuantityOnHand = aq
                };

                AddTool(tool);
            }
        }
    }
}

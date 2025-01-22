using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
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
            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                tools.Add(new Tool
                {
                    ToolID = reader["ToolID"].ToString(),
                    Description = reader["Description"].ToString(),
                    Location = reader["Location"].ToString(),
                    Brand = reader["Brand"].ToString(),
                    IsCheckedOut = reader["IsCheckedOut"].ToString() == "1",
                    CheckedOutBy = reader["CheckedOutBy"].ToString(),
                    CheckedOutTime = reader["CheckedOutTime"] is DBNull ? null : Convert.ToDateTime(reader["CheckedOutTime"])
                });
            }

            return tools;
        }

        public void AddTool(Tool tool)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = @"
        INSERT INTO Tools (Name, Description, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, AvailableQuantity, RentedQuantity, IsCheckedOut)
        VALUES (@Name, @Description, @Location, @Brand, @PartNumber, @Supplier, @PurchasedDate, @Notes, @AvailableQuantity, @RentedQuantity, @IsCheckedOut)";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Name", tool.Name);
            command.Parameters.AddWithValue("@Description", tool.Description);
            command.Parameters.AddWithValue("@Location", tool.Location);
            command.Parameters.AddWithValue("@Brand", tool.Brand);
            command.Parameters.AddWithValue("@PartNumber", tool.PartNumber);
            command.Parameters.AddWithValue("@Supplier", tool.Supplier ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PurchasedDate", tool.PurchasedDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Notes", tool.Notes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@AvailableQuantity", tool.QuantityOnHand);
            command.Parameters.AddWithValue("@RentedQuantity", 0); // New tools start with no rentals
            command.Parameters.AddWithValue("@IsCheckedOut", 0);  // Default to not checked out
            command.ExecuteNonQuery();
        }

        public void UpdateTool(Tool tool)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = @"
            UPDATE Tools 
            SET Description = @Description, Location = @Location, Brand = @Brand,
                IsCheckedOut = @IsCheckedOut, CheckedOutBy = @CheckedOutBy, CheckedOutTime = @CheckedOutTime
            WHERE ToolID = @ToolID";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@ToolID", tool.ToolID);
            command.Parameters.AddWithValue("@Description", tool.Description);
            command.Parameters.AddWithValue("@Location", tool.Location);
            command.Parameters.AddWithValue("@Brand", tool.Brand);
            command.Parameters.AddWithValue("@IsCheckedOut", tool.IsCheckedOut ? 1 : 0);
            command.Parameters.AddWithValue("@CheckedOutBy", tool.CheckedOutBy ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CheckedOutTime", tool.CheckedOutTime ?? (object)DBNull.Value);
            command.ExecuteNonQuery();
        }

        public void UpdateToolQuantities(string toolID, int quantityChange, bool isRental)
        {
            if (quantityChange <= 0) throw new ArgumentException("Quantity change must be greater than 0.");

            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = isRental
                ? "UPDATE Tools SET AvailableQuantity = AvailableQuantity - @QuantityChange, RentedQuantity = RentedQuantity + @QuantityChange WHERE ToolID = @ToolID AND AvailableQuantity >= @QuantityChange"
                : "UPDATE Tools SET AvailableQuantity = AvailableQuantity + @QuantityChange, RentedQuantity = RentedQuantity - @QuantityChange WHERE ToolID = @ToolID AND RentedQuantity >= @QuantityChange";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@ToolID", toolID);
            command.Parameters.AddWithValue("@QuantityChange", quantityChange);
            var rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
                throw new InvalidOperationException("Quantity update failed. Ensure sufficient stock or valid tool ID.");
        }

        public void DeleteTool(string toolID)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = "DELETE FROM Tools WHERE ToolID = @ToolID";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@ToolID", toolID);
            command.ExecuteNonQuery();
        }

        public List<Tool> SearchTools(string searchTerm)
        {
            var tools = new List<Tool>();
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = @"
            SELECT * FROM Tools
            WHERE ToolID LIKE @SearchTerm OR Description LIKE @SearchTerm OR Location LIKE @SearchTerm";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                tools.Add(new Tool
                {
                    ToolID = reader["ToolID"].ToString(),
                    Description = reader["Description"].ToString(),
                    Location = reader["Location"].ToString(),
                    Brand = reader["Brand"].ToString(),
                    IsCheckedOut = reader["IsCheckedOut"].ToString() == "1",
                    CheckedOutBy = reader["CheckedOutBy"].ToString(),
                    CheckedOutTime = reader["CheckedOutTime"] is DBNull ? null : Convert.ToDateTime(reader["CheckedOutTime"])
                });
            }

            return tools;
        }

        public void ImportToolsFromCsv(string filePath)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines.Skip(1)) // Skip header row
            {
                var columns = line.Split(',');
                var query = @"
        INSERT INTO Tools (Name, Description, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, AvailableQuantity, RentedQuantity, IsCheckedOut)
        VALUES (@Name, @Description, @Location, @Brand, @PartNumber, @Supplier, @PurchasedDate, @Notes, @AvailableQuantity, @RentedQuantity, @IsCheckedOut)";
                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@Name", columns[0]);
                command.Parameters.AddWithValue("@Description", columns[1]);
                command.Parameters.AddWithValue("@Location", columns[2]);
                command.Parameters.AddWithValue("@Brand", columns[3]);
                command.Parameters.AddWithValue("@PartNumber", columns[4]);
                command.Parameters.AddWithValue("@Supplier", columns[5]);
                command.Parameters.AddWithValue("@PurchasedDate", DateTime.Parse(columns[6]));
                command.Parameters.AddWithValue("@Notes", columns[7]);
                command.Parameters.AddWithValue("@AvailableQuantity", int.Parse(columns[8]));
                command.Parameters.AddWithValue("@RentedQuantity", int.Parse(columns[9]));
                command.Parameters.AddWithValue("@IsCheckedOut", int.Parse(columns[10]));
                command.ExecuteNonQuery();
            }
        }

        public void ExportToolsToCsv(string filePath)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = "SELECT * FROM Tools";
            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            using var writer = new StreamWriter(filePath);
            writer.WriteLine("Name,Description,Location,Brand,PartNumber,Supplier,PurchasedDate,Notes,AvailableQuantity,RentedQuantity,IsCheckedOut");
            while (reader.Read())
            {
                var line = string.Join(",",
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
                    reader["IsCheckedOut"]);
                writer.WriteLine(line);
            }
        }

    }
}

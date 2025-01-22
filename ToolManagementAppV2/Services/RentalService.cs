using System;
using System.Collections.Generic;
using System.Data.SQLite;
using ToolManagementAppV2.Models;

namespace ToolManagementAppV2.Services
{
    public class RentalService
    {
        private readonly DatabaseService _dbService;

        public RentalService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public void RentTool(string toolID, int customerID, DateTime rentalDate, DateTime dueDate)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var rentalQuery = @"
        INSERT INTO Rentals (ToolID, CustomerID, RentalDate, DueDate)
        VALUES (@ToolID, @CustomerID, @RentalDate, @DueDate);
        
        UPDATE Tools
        SET AvailableQuantity = AvailableQuantity - 1, RentedQuantity = RentedQuantity + 1
        WHERE ToolID = @ToolID AND AvailableQuantity > 0";

            using var command = new SQLiteCommand(rentalQuery, connection);
            command.Parameters.AddWithValue("@ToolID", toolID);
            command.Parameters.AddWithValue("@CustomerID", customerID);
            command.Parameters.AddWithValue("@RentalDate", rentalDate);
            command.Parameters.AddWithValue("@DueDate", dueDate);
            command.ExecuteNonQuery();
        }


        public void ReturnTool(int rentalID, DateTime returnDate)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var returnQuery = @"
            UPDATE Rentals
            SET ReturnDate = @ReturnDate, Status = 'Returned'
            WHERE RentalID = @RentalID;

            UPDATE Tools
            SET AvailableQuantity = AvailableQuantity + 1, RentedQuantity = RentedQuantity - 1
            WHERE ToolID = (SELECT ToolID FROM Rentals WHERE RentalID = @RentalID)";

            using var command = new SQLiteCommand(returnQuery, connection);
            command.Parameters.AddWithValue("@RentalID", rentalID);
            command.Parameters.AddWithValue("@ReturnDate", returnDate);
            command.ExecuteNonQuery();
        }

        public List<Rental> GetActiveRentals()
        {
            var rentals = new List<Rental>();
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = "SELECT * FROM Rentals WHERE Status = 'Rented'";
            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                rentals.Add(new Rental
                {
                    RentalID = Convert.ToInt32(reader["RentalID"]),
                    ToolID = Convert.ToInt32(reader["ToolID"]),
                    CustomerID = Convert.ToInt32(reader["CustomerID"]),
                    RentalDate = Convert.ToDateTime(reader["RentalDate"]),
                    DueDate = Convert.ToDateTime(reader["DueDate"]),
                    ReturnDate = reader["ReturnDate"] as DateTime?,
                    Status = reader["Status"].ToString()
                });
            }

            return rentals;
        }

        public void UpdateRentalStatus(int rentalID, string status)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = "UPDATE Rentals SET Status = @Status WHERE RentalID = @RentalID";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Status", status);
            command.Parameters.AddWithValue("@RentalID", rentalID);
            command.ExecuteNonQuery();
        }
    }
}

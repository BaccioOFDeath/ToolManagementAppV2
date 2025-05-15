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
        INSERT INTO Rentals (ToolID, CustomerID, RentalDate, DueDate, Status)
        VALUES (@ToolID, @CustomerID, @RentalDate, @DueDate, 'Rented');
        UPDATE Tools
           SET AvailableQuantity = AvailableQuantity - 1,
               RentedQuantity   = RentedQuantity + 1
         WHERE ToolID = @ToolID AND AvailableQuantity > 0";

            using var command = new SQLiteCommand(rentalQuery, connection);
            command.Parameters.AddWithValue("@ToolID", toolID);
            command.Parameters.AddWithValue("@CustomerID", customerID);
            command.Parameters.AddWithValue("@RentalDate", rentalDate);
            command.Parameters.AddWithValue("@DueDate", dueDate);
            command.ExecuteNonQuery();
        }

        public void RentToolWithTransaction(string toolID, int customerID, DateTime rentalDate, DateTime dueDate)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                // Verify available quantity
                var checkQuery = "SELECT AvailableQuantity FROM Tools WHERE ToolID = @ToolID";
                int availableQuantity = 0;
                using var checkCmd = new SQLiteCommand(checkQuery, connection, transaction);
                checkCmd.Parameters.AddWithValue("@ToolID", toolID);
                var result = checkCmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    availableQuantity = Convert.ToInt32(result);
                if (availableQuantity < 1)
                    throw new InvalidOperationException("Insufficient quantity available.");

                // Insert rental record
                var rentalQuery = @"
            INSERT INTO Rentals (ToolID, CustomerID, RentalDate, DueDate, Status)
            VALUES (@ToolID, @CustomerID, @RentalDate, @DueDate, 'Rented')";
                using var rentalCmd = new SQLiteCommand(rentalQuery, connection, transaction);
                rentalCmd.Parameters.AddWithValue("@ToolID", toolID);
                rentalCmd.Parameters.AddWithValue("@CustomerID", customerID);
                rentalCmd.Parameters.AddWithValue("@RentalDate", rentalDate);
                rentalCmd.Parameters.AddWithValue("@DueDate", dueDate);
                rentalCmd.ExecuteNonQuery();

                // Update tool quantities
                var updateQuery = @"
            UPDATE Tools
               SET AvailableQuantity = AvailableQuantity - 1,
                   RentedQuantity   = RentedQuantity + 1
             WHERE ToolID = @ToolID";
                using var updateCmd = new SQLiteCommand(updateQuery, connection, transaction);
                updateCmd.Parameters.AddWithValue("@ToolID", toolID);
                updateCmd.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Rental> GetOverdueRentals()
        {
            var rentals = new List<Rental>();
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = @"
        SELECT * FROM Rentals
         WHERE Status = 'Rented'
           AND DueDate < @Today";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Today", DateTime.Today);
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
                    ReturnDate = reader["ReturnDate"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["ReturnDate"]),
                    Status = reader["Status"].ToString()
                });
            }

            return rentals;
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

        
        public void ReturnToolWithTransaction(int rentalID, DateTime returnDate)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                // Retrieve the associated toolID and ensure the rental is active
                int toolID = 0;
                var selectQuery = "SELECT ToolID FROM Rentals WHERE RentalID = @RentalID AND Status = 'Rented'";
                using (var selectCmd = new SQLiteCommand(selectQuery, connection, transaction))
                {
                    selectCmd.Parameters.AddWithValue("@RentalID", rentalID);
                    var result = selectCmd.ExecuteScalar();
                    if (result == null)
                        throw new InvalidOperationException("Rental not found or already returned.");
                    toolID = Convert.ToInt32(result);
                }

                // Update the rental record
                var rentalUpdateQuery = "UPDATE Rentals SET ReturnDate = @ReturnDate, Status = 'Returned' WHERE RentalID = @RentalID";
                using (var rentalUpdateCmd = new SQLiteCommand(rentalUpdateQuery, connection, transaction))
                {
                    rentalUpdateCmd.Parameters.AddWithValue("@ReturnDate", returnDate);
                    rentalUpdateCmd.Parameters.AddWithValue("@RentalID", rentalID);
                    rentalUpdateCmd.ExecuteNonQuery();
                }

                // Update tool quantities
                var updateQuery = "UPDATE Tools SET AvailableQuantity = AvailableQuantity + 1, RentedQuantity = RentedQuantity - 1 WHERE ToolID = @ToolID";
                using (var updateCmd = new SQLiteCommand(updateQuery, connection, transaction))
                {
                    updateCmd.Parameters.AddWithValue("@ToolID", toolID);
                    updateCmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Rental> GetRentalHistoryForTool(int toolID)
        {
            var rentals = new List<Rental>();
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "SELECT * FROM Rentals WHERE ToolID = @ToolID ORDER BY RentalDate DESC";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@ToolID", toolID);
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
                    ReturnDate = reader["ReturnDate"] is DBNull ? null : Convert.ToDateTime(reader["ReturnDate"]),
                    Status = reader["Status"].ToString()
                });
            }
            return rentals;
        }

        

        public void ExtendRental(int rentalID, DateTime newDueDate)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "UPDATE Rentals SET DueDate = @NewDueDate WHERE RentalID = @RentalID AND Status = 'Rented'";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@NewDueDate", newDueDate);
            command.Parameters.AddWithValue("@RentalID", rentalID);
            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected == 0)
                throw new InvalidOperationException("Unable to extend rental. Rental not found or already returned.");
        }

        public List<Rental> GetAllRentals()
        {
            var rentals = new List<Rental>();
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "SELECT * FROM Rentals";
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
                    ReturnDate = reader["ReturnDate"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["ReturnDate"]),
                    Status = reader["Status"].ToString()
                });
            }
            return rentals;
        }

    }
}

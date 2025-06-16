using System.Data;
using System.Data.SQLite;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.Services.Rentals
{
    public class RentalService : IRentalService
    {
        readonly DatabaseService _dbService;

        public RentalService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        // toolID is passed as a string even though the underlying column is INTEGER
        // to keep consistency with ToolModel.ToolID
        public void RentTool(string toolID, int customerID, DateTime rentalDate, DateTime dueDate)
        {
            using var conn = _dbService.CreateConnection();
            using var tx = conn.BeginTransaction();

            try
            {
                var availCmd = new SQLiteCommand(
                    "SELECT AvailableQuantity FROM Tools WHERE ToolID=@ToolID",
                    conn, tx);
                availCmd.Parameters.AddWithValue("@ToolID", toolID);
                int avail = Convert.ToInt32(availCmd.ExecuteScalar() ?? 0);
                if (avail < 1)
                    throw new InvalidOperationException("Insufficient quantity.");

                SqliteHelper.ExecuteNonQuery(conn, tx,
                    "INSERT INTO Rentals (ToolID, CustomerID, RentalDate, DueDate, Status) " +
                    "VALUES (@ToolID, @CustomerID, @RentalDate, @DueDate, 'Rented')",
                    new[]
                    {
                        new SQLiteParameter("@ToolID", toolID),
                        new SQLiteParameter("@CustomerID", customerID),
                        new SQLiteParameter("@RentalDate", rentalDate),
                        new SQLiteParameter("@DueDate", dueDate)
                    });

                SqliteHelper.ExecuteNonQuery(conn, tx,
                    "UPDATE Tools SET AvailableQuantity = AvailableQuantity - 1, " +
                    "RentedQuantity = RentedQuantity + 1 WHERE ToolID = @ToolID",
                    new[] { new SQLiteParameter("@ToolID", toolID) });

                tx.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                tx.Rollback();
                return;
            }
        }

        public void RentToolWithTransaction(string toolID, int customerID, DateTime rentalDate, DateTime dueDate)
        {
            using var conn = _dbService.CreateConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                var availCmd = new SQLiteCommand(
                    "SELECT AvailableQuantity FROM Tools WHERE ToolID=@ToolID", conn, tx);
                availCmd.Parameters.AddWithValue("@ToolID", toolID);
                int avail = Convert.ToInt32(availCmd.ExecuteScalar() ?? 0);
                if (avail < 1) throw new InvalidOperationException("Insufficient quantity.");

                SqliteHelper.ExecuteNonQuery(conn, tx,
                    "INSERT INTO Rentals (ToolID,CustomerID,RentalDate,DueDate,Status) VALUES(@ToolID,@CustomerID,@RentalDate,@DueDate,'Rented')",
                    new[]
                    {
                        new SQLiteParameter("@ToolID", toolID),
                        new SQLiteParameter("@CustomerID", customerID),
                        new SQLiteParameter("@RentalDate", rentalDate),
                        new SQLiteParameter("@DueDate", dueDate)
                    });

                SqliteHelper.ExecuteNonQuery(conn, tx,
                    "UPDATE Tools SET AvailableQuantity = AvailableQuantity - 1, RentedQuantity = RentedQuantity + 1 WHERE ToolID = @ToolID",
                    new[] { new SQLiteParameter("@ToolID", toolID) });

                tx.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                tx.Rollback();
                return;
            }
        }

        public void ReturnTool(int rentalID, DateTime returnDate)
        {
            using var conn = _dbService.CreateConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                var rentalRows = SqliteHelper.ExecuteNonQuery(conn, tx,
                    "UPDATE Rentals SET ReturnDate=@ReturnDate, Status='Returned' WHERE RentalID=@RentalID",
                    new[]
                    {
                        new SQLiteParameter("@ReturnDate", returnDate),
                        new SQLiteParameter("@RentalID", rentalID)
                    });

                var toolRows = SqliteHelper.ExecuteNonQuery(conn, tx,
                    "UPDATE Tools SET AvailableQuantity=AvailableQuantity+1, RentedQuantity=RentedQuantity-1 WHERE ToolID=(SELECT ToolID FROM Rentals WHERE RentalID=@RentalID)",
                    new[] { new SQLiteParameter("@RentalID", rentalID) });

                if (rentalRows == 0 || toolRows == 0)
                    throw new InvalidOperationException("Return operation failed.");

                tx.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                tx.Rollback();
                return;
            }
        }

        public void ReturnToolWithTransaction(int rentalID, DateTime returnDate)
        {
            using var conn = _dbService.CreateConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                var selCmd = new SQLiteCommand(
                    "SELECT ToolID FROM Rentals WHERE RentalID=@RentalID AND Status='Rented'", conn, tx);
                selCmd.Parameters.AddWithValue("@RentalID", rentalID);
                var result = selCmd.ExecuteScalar();
                if (result == null) throw new InvalidOperationException("Rental not found or already returned.");
                string toolID = result.ToString();

                SqliteHelper.ExecuteNonQuery(conn, tx,
                    "UPDATE Rentals SET ReturnDate=@ReturnDate,Status='Returned' WHERE RentalID=@RentalID",
                    new[]
                    {
                        new SQLiteParameter("@ReturnDate", returnDate),
                        new SQLiteParameter("@RentalID", rentalID)
                    });

                SqliteHelper.ExecuteNonQuery(conn, tx,
                    "UPDATE Tools SET AvailableQuantity=AvailableQuantity+1,RentedQuantity=RentedQuantity-1 WHERE ToolID=@ToolID",
                    new[] { new SQLiteParameter("@ToolID", toolID) });

                tx.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                tx.Rollback();
                return;
            }
        }

        public void ExtendRental(int rentalID, DateTime newDueDate)
        {
            const string sql = @"
                UPDATE Rentals
                   SET DueDate = @NewDueDate
                 WHERE RentalID = @RentalID AND Status = 'Rented'";
            var p = new[]
            {
                new SQLiteParameter("@NewDueDate", newDueDate),
                new SQLiteParameter("@RentalID", rentalID)
            };
            using var conn = _dbService.CreateConnection();
            if (SqliteHelper.ExecuteNonQuery(conn, sql, p) == 0)
                throw new InvalidOperationException("Unable to extend rental. Rental not found or already returned.");
        }

        const string BaseSelect = @"SELECT r.*, t.ToolNumber, t.NameDescription, c.Company FROM Rentals r
                                    JOIN Tools t ON r.ToolID = t.ToolID
                                    JOIN Customers c ON r.CustomerID = c.CustomerID";

        public List<Rental> GetActiveRentals()
        {
            using var conn = _dbService.CreateConnection();
            var sql = BaseSelect + " WHERE r.Status='Rented'";
            return SqliteHelper.ExecuteReader(conn, sql, null, MapRental);
        }

        public List<Rental> GetOverdueRentals()
        {
            const string sql = BaseSelect + @" WHERE r.Status = 'Rented' AND r.DueDate < @Today";
            var p = new[] { new SQLiteParameter("@Today", DateTime.Today) };
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReader(conn, sql, p, MapRental);
        }

        public List<Rental> GetAllRentals()
        {
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReader(conn, BaseSelect, null, MapRental);
        }

        public List<Rental> GetRentalHistoryForTool(string toolID)
        {
            const string sql = BaseSelect + @" WHERE r.ToolID = @ToolID ORDER BY r.RentalDate DESC";
            var p = new[] { new SQLiteParameter("@ToolID", toolID) };
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReader(conn, sql, p, MapRental);
        }

        public List<Rental> GetRentalHistoryForCustomer(int customerID)
        {
            const string sql = BaseSelect + @" WHERE r.CustomerID = @CustomerID ORDER BY r.RentalDate DESC";
            var p = new[] { new SQLiteParameter("@CustomerID", customerID) };
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReader(conn, sql, p, MapRental);
        }

        Rental MapRental(IDataRecord r) => new()
        {
            RentalID = Convert.ToInt32(r["RentalID"]),
            ToolID = r["ToolID"].ToString(),
            CustomerID = Convert.ToInt32(r["CustomerID"]),
            RentalDate = Convert.ToDateTime(r["RentalDate"]),
            DueDate = Convert.ToDateTime(r["DueDate"]),
            ReturnDate = r["ReturnDate"] is DBNull ? null : Convert.ToDateTime(r["ReturnDate"]),
            Status = r["Status"].ToString(),
            ToolNumber = r["ToolNumber"].ToString(),
            CustomerName = r["Company"].ToString()
        };
    }
}

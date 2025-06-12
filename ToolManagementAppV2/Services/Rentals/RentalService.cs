using System.Data;
using System.Data.SQLite;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;

namespace ToolManagementAppV2.Services.Rentals
{
    public class RentalService
    {
        readonly string _connString;

        public RentalService(DatabaseService dbService) =>
            _connString = dbService.ConnectionString;

        // toolID is passed as a string even though the underlying column is INTEGER
        // to keep consistency with ToolModel.ToolID
        public void RentTool(string toolID, int customerID, DateTime rentalDate, DateTime dueDate)
        {
            const string sql = @"
                INSERT INTO Rentals (ToolID, CustomerID, RentalDate, DueDate, Status)
                VALUES (@ToolID, @CustomerID, @RentalDate, @DueDate, 'Rented');
                UPDATE Tools
                   SET AvailableQuantity = AvailableQuantity - 1,
                       RentedQuantity   = RentedQuantity + 1
                 WHERE ToolID = @ToolID AND AvailableQuantity > 0";
            var p = new[]
            {
                new SQLiteParameter("@ToolID", toolID),
                new SQLiteParameter("@CustomerID", customerID),
                new SQLiteParameter("@RentalDate", rentalDate),
                new SQLiteParameter("@DueDate", dueDate)
            };
            SqliteHelper.ExecuteNonQuery(_connString, sql, p);
        }

        public void RentToolWithTransaction(string toolID, int customerID, DateTime rentalDate, DateTime dueDate)
        {
            using var conn = new SQLiteConnection(_connString);
            conn.Open();
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
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void ReturnTool(int rentalID, DateTime returnDate)
        {
            const string sql = @"
                UPDATE Rentals
                   SET ReturnDate = @ReturnDate, Status = 'Returned'
                 WHERE RentalID = @RentalID;
                UPDATE Tools
                   SET AvailableQuantity = AvailableQuantity + 1,
                       RentedQuantity   = RentedQuantity - 1
                 WHERE ToolID =
                   (SELECT ToolID FROM Rentals WHERE RentalID=@RentalID)";
            var p = new[]
            {
                new SQLiteParameter("@RentalID", rentalID),
                new SQLiteParameter("@ReturnDate", returnDate)
            };
            SqliteHelper.ExecuteNonQuery(_connString, sql, p);
        }

        public void ReturnToolWithTransaction(int rentalID, DateTime returnDate)
        {
            using var conn = new SQLiteConnection(_connString);
            conn.Open();
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
            catch
            {
                tx.Rollback();
                throw;
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
            if (SqliteHelper.ExecuteNonQuery(_connString, sql, p) == 0)
                throw new InvalidOperationException("Unable to extend rental. Rental not found or already returned.");
        }

        public List<Rental> GetActiveRentals() =>
            SqliteHelper.ExecuteReader(_connString, "SELECT * FROM Rentals WHERE Status='Rented'", null, MapRental);

        public List<Rental> GetOverdueRentals()
        {
            const string sql = @"
                SELECT * FROM Rentals
                 WHERE Status = 'Rented'
                   AND DueDate < @Today";
            var p = new[] { new SQLiteParameter("@Today", DateTime.Today) };
            return SqliteHelper.ExecuteReader(_connString, sql, p, MapRental);
        }

        public List<Rental> GetAllRentals() =>
            SqliteHelper.ExecuteReader(_connString, "SELECT * FROM Rentals", null, MapRental);

        public List<Rental> GetRentalHistoryForTool(string toolID)
        {
            const string sql = @"
                SELECT * FROM Rentals
                 WHERE ToolID = @ToolID
              ORDER BY RentalDate DESC";
            var p = new[] { new SQLiteParameter("@ToolID", toolID) };
            return SqliteHelper.ExecuteReader(_connString, sql, p, MapRental);
        }

        Rental MapRental(IDataRecord r) => new()
        {
            RentalID = Convert.ToInt32(r["RentalID"]),
            ToolID = r["ToolID"].ToString(),
            CustomerID = Convert.ToInt32(r["CustomerID"]),
            RentalDate = Convert.ToDateTime(r["RentalDate"]),
            DueDate = Convert.ToDateTime(r["DueDate"]),
            ReturnDate = r["ReturnDate"] is DBNull ? null : Convert.ToDateTime(r["ReturnDate"]),
            Status = r["Status"].ToString()
        };
    }
}

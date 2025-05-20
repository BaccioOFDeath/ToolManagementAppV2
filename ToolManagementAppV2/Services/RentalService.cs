using System.Data;
using System.Data.SQLite;
using ToolManagementAppV2.Models;

namespace ToolManagementAppV2.Services
{
    public class RentalService
    {
        readonly string _connString;

        public RentalService(DatabaseService dbService) =>
            _connString = dbService.ConnectionString;

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
                new SQLiteParameter("@ToolID",       toolID),
                new SQLiteParameter("@CustomerID",   customerID),
                new SQLiteParameter("@RentalDate",   rentalDate),
                new SQLiteParameter("@DueDate",      dueDate)
            };
            ExecuteNonQuery(sql, p);
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

                var insertCmd = new SQLiteCommand(@"
                    INSERT INTO Rentals (ToolID,CustomerID,RentalDate,DueDate,Status)
                    VALUES(@ToolID,@CustomerID,@RentalDate,@DueDate,'Rented')", conn, tx);
                insertCmd.Parameters.AddWithValue("@ToolID", toolID);
                insertCmd.Parameters.AddWithValue("@CustomerID", customerID);
                insertCmd.Parameters.AddWithValue("@RentalDate", rentalDate);
                insertCmd.Parameters.AddWithValue("@DueDate", dueDate);
                insertCmd.ExecuteNonQuery();

                var updateCmd = new SQLiteCommand(@"
                    UPDATE Tools
                       SET AvailableQuantity = AvailableQuantity - 1,
                           RentedQuantity   = RentedQuantity + 1
                     WHERE ToolID = @ToolID", conn, tx);
                updateCmd.Parameters.AddWithValue("@ToolID", toolID);
                updateCmd.ExecuteNonQuery();

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
                new SQLiteParameter("@RentalID",   rentalID),
                new SQLiteParameter("@ReturnDate", returnDate)
            };
            ExecuteNonQuery(sql, p);
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
                int toolID = Convert.ToInt32(result);

                var rentCmd = new SQLiteCommand(
                    "UPDATE Rentals SET ReturnDate=@ReturnDate,Status='Returned' WHERE RentalID=@RentalID",
                    conn, tx);
                rentCmd.Parameters.AddWithValue("@ReturnDate", returnDate);
                rentCmd.Parameters.AddWithValue("@RentalID", rentalID);
                rentCmd.ExecuteNonQuery();

                var updCmd = new SQLiteCommand(
                    "UPDATE Tools SET AvailableQuantity=AvailableQuantity+1,RentedQuantity=RentedQuantity-1 WHERE ToolID=@ToolID",
                    conn, tx);
                updCmd.Parameters.AddWithValue("@ToolID", toolID);
                updCmd.ExecuteNonQuery();

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
                new SQLiteParameter("@RentalID",   rentalID)
            };
            if (ExecuteNonQuery(sql, p) == 0)
                throw new InvalidOperationException("Unable to extend rental. Rental not found or already returned.");
        }

        public List<Rental> GetActiveRentals() =>
            ExecuteReader("SELECT * FROM Rentals WHERE Status='Rented'", null);

        public List<Rental> GetOverdueRentals()
        {
            const string sql = @"
                SELECT * FROM Rentals
                 WHERE Status = 'Rented'
                   AND DueDate < @Today";
            var p = new[] { new SQLiteParameter("@Today", DateTime.Today) };
            return ExecuteReader(sql, p);
        }

        public List<Rental> GetAllRentals() =>
            ExecuteReader("SELECT * FROM Rentals", null);

        public List<Rental> GetRentalHistoryForTool(int toolID)
        {
            const string sql = @"
                SELECT * FROM Rentals
                 WHERE ToolID = @ToolID
              ORDER BY RentalDate DESC";
            var p = new[] { new SQLiteParameter("@ToolID", toolID) };
            return ExecuteReader(sql, p);
        }

        // --- Helpers ---
        List<Rental> ExecuteReader(string sql, SQLiteParameter[] parameters)
        {
            var list = new List<Rental>();
            using var conn = new SQLiteConnection(_connString);
            conn.Open();
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                list.Add(MapRental(rdr));
            return list;
        }

        int ExecuteNonQuery(string sql, SQLiteParameter[] parameters)
        {
            using var conn = new SQLiteConnection(_connString);
            conn.Open();
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        Rental MapRental(IDataRecord r) => new()
        {
            RentalID = Convert.ToInt32(r["RentalID"]),
            ToolID = Convert.ToInt32(r["ToolID"]),  
            CustomerID = Convert.ToInt32(r["CustomerID"]),
            RentalDate = Convert.ToDateTime(r["RentalDate"]),
            DueDate = Convert.ToDateTime(r["DueDate"]),
            ReturnDate = r["ReturnDate"] is DBNull
                         ? (DateTime?)null
                         : Convert.ToDateTime(r["ReturnDate"]),
            Status = r["Status"].ToString()
        };
    }
}

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
        readonly IToolService _toolService;

        public RentalService(DatabaseService dbService, IToolService toolService)
        {
            _dbService = dbService;
            _toolService = toolService;
        }

        // toolID is passed as a string even though the underlying column is INTEGER
        // to keep consistency with ToolModel.ToolID
        /// <summary>
        /// Rents a tool to a customer within a transaction and updates inventory counts.
        /// </summary>
        /// <param name="toolID">Identifier of the tool to rent.</param>
        /// <param name="customerID">Identifier of the customer renting the tool.</param>
        /// <param name="rentalDate">Date the rental begins.</param>
        /// <param name="dueDate">Date the rental is due.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when insufficient quantity is available. The exception is propagated to the caller.
        /// </exception>
        /// <exception cref="SQLiteException">
        /// Thrown when a database operation fails. The exception is propagated to the caller.
        /// </exception>
        public void RentTool(string toolID, int customerID, DateTime rentalDate, DateTime dueDate)
        {
            ExecuteWithTransaction((conn, tx) =>
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
            },
            () =>
            {
                var tool = _toolService.GetToolByID(toolID);
                if (tool != null)
                {
                    tool.QuantityOnHand--;
                    _toolService.UpdateTool(tool);
                }
            });
        }

        /// <summary>
        /// Marks a rental as returned and restores inventory counts within a transaction.
        /// </summary>
        /// <param name="rentalID">Identifier of the rental to return.</param>
        /// <param name="returnDate">Date the tool was returned.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the rental cannot be found or has already been returned. The exception is propagated to the caller.
        /// </exception>
        /// <exception cref="SQLiteException">
        /// Thrown when a database operation fails. The exception is propagated to the caller.
        /// </exception>
        public void ReturnTool(int rentalID, DateTime returnDate)
        {
            string? toolID = null;
            ExecuteWithTransaction((conn, tx) =>
            {
                var selCmd = new SQLiteCommand(
                    "SELECT ToolID FROM Rentals WHERE RentalID=@RentalID AND Status='Rented'", conn, tx);
                selCmd.Parameters.AddWithValue("@RentalID", rentalID);
                var result = selCmd.ExecuteScalar();
                if (result == null) throw new InvalidOperationException("Rental not found or already returned.");
                toolID = result.ToString();

                SqliteHelper.ExecuteNonQuery(conn, tx,
                    "UPDATE Rentals SET ReturnDate=@ReturnDate,Status='Returned' WHERE RentalID=@RentalID",
                    new[]
                    {
                        new SQLiteParameter("@ReturnDate", returnDate),
                        new SQLiteParameter("@RentalID", rentalID)
                    });
            },
            () =>
            {
                var tool = _toolService.GetToolByID(toolID);
                if (tool != null)
                {
                    tool.QuantityOnHand++;
                    _toolService.UpdateTool(tool);
                }
            });
        }

        /// <summary>
        /// Executes the specified action within a database transaction and rolls back on error.
        /// </summary>
        /// <param name="action">Database operations to execute.</param>
        /// <param name="postCommitAction">Optional action invoked after a successful commit.</param>
        /// <exception cref="SQLiteException">
        /// Thrown when a database operation fails. The original exception is rethrown after rollback.
        /// </exception>
        /// <exception cref="Exception">
        /// Any exception thrown by <paramref name="action"/> is rethrown after rollback.
        /// </exception>
        void ExecuteWithTransaction(Action<SQLiteConnection, SQLiteTransaction> action, Action? postCommitAction = null)
        {
            using var conn = _dbService.CreateConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                action(conn, tx);
                tx.Commit();
                postCommitAction?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
            using var conn = _dbService.CreateConnection();
            if (SqliteHelper.ExecuteNonQuery(conn, sql, p) == 0)
                throw new InvalidOperationException("Unable to extend rental. Rental not found or already returned.");
        }

        const string BaseSelect = @"SELECT r.*,
                                    t.ToolNumber,
                                    t.NameDescription,
                                    t.Location AS ToolLocation,
                                    t.ToolImagePath,
                                    c.Company,
                                    c.Contact,
                                    c.Email,
                                    c.Phone,
                                    c.Mobile,
                                    c.Address
                                 FROM Rentals r
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
            CustomerName = r["Company"].ToString(),
            CustomerContact = r["Contact"].ToString(),
            CustomerEmail = r["Email"].ToString(),
            CustomerPhone = r["Phone"].ToString(),
            CustomerMobile = r["Mobile"].ToString(),
            CustomerAddress = r["Address"].ToString(),
            ToolImagePath = r["ToolImagePath"].ToString(),
            ToolLocation = r["ToolLocation"].ToString()
        };
    }
}

// Services/Rentals/RentalService.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ToolManagementAppV2.Services.Users;

namespace ToolManagementAppV2.Services.Rentals
{
    public class RentalService : IRentalService
    {
        private readonly DatabaseService _dbService;
        private readonly IToolService? _toolService;
        private readonly ILogger<RentalService> _logger;
        private readonly IAuthorizationService _auth;
        private readonly ActivityLogService? _activityLog;
        private readonly IUserContext? _context;

        public RentalService(DatabaseService dbService, IAuthorizationService? authorizationService = null, IToolService? toolService = null, ILogger<RentalService>? logger = null, ActivityLogService? activityLogService = null, IUserContext? userContext = null)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _auth = authorizationService ?? new NoOpAuthorizationService();
            _toolService = toolService; // may be null if inventory sync not desired
            _logger = logger ?? NullLogger<RentalService>.Instance;
            _activityLog = activityLogService;
            _context = userContext;
        }

        async Task ExecuteWithTransactionAsync(Func<SQLiteConnection, SQLiteTransaction, Task> action, Func<Task>? postCommitAction = null)
        {
            using var conn = _dbService.CreateConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                await action(conn, tx);
                tx.Commit();
                if (postCommitAction != null)
                    await postCommitAction();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database transaction failed");
                tx.Rollback();
                throw;
            }
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

        // Synchronous rental operations removed; use async equivalents instead.

        Rental MapRental(IDataRecord r) => new()
        {
            RentalID = Convert.ToInt32(r["RentalID"]),
            ToolID = Convert.ToInt32(r["ToolID"]),
            CustomerID = Convert.ToInt32(r["CustomerID"]),
            RentalDate = DateTime.Parse(r["RentalDate"].ToString()!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
            DueDate = DateTime.Parse(r["DueDate"].ToString()!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
            ReturnDate = r["ReturnDate"] is DBNull ? null : DateTime.Parse(r["ReturnDate"].ToString()!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
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

        public async Task RentToolAsync(int toolID, int customerID, DateTime rentalDate, DateTime dueDate)
        {
            _auth.EnsureAdmin();
            await ExecuteWithTransactionAsync(async (conn, tx) =>
            {
                var availCmd = new SQLiteCommand(
                    "SELECT AvailableQuantity FROM Tools WHERE ToolID=@ToolID",
                    conn, tx);
                availCmd.Parameters.AddWithValue("@ToolID", toolID);
                int avail = Convert.ToInt32(await availCmd.ExecuteScalarAsync() ?? 0);
                if (avail < 1)
                    throw new InvalidOperationException("Insufficient quantity.");

                await SqliteHelper.ExecuteNonQueryAsync(conn, tx,
                    "INSERT INTO Rentals (ToolID, CustomerID, RentalDate, DueDate, Status) " +
                    "VALUES (@ToolID, @CustomerID, @RentalDate, @DueDate, 'Rented')",
                    new[]
                    {
                        new SQLiteParameter("@ToolID", toolID),
                        new SQLiteParameter("@CustomerID", customerID),
                        new SQLiteParameter("@RentalDate", rentalDate),
                        new SQLiteParameter("@DueDate", dueDate)
                    });

                if (_toolService != null)
                    await _toolService.UpdateToolQuantitiesAsync(toolID, 1, true, conn, tx);
            });
            if (_activityLog != null)
            {
                var user = _context?.CurrentUser;
                await _activityLog.LogActionAsync(user?.UserID ?? 0, user?.UserName ?? string.Empty, $"Rented tool {toolID} to customer {customerID}").ConfigureAwait(false);
            }
        }

        public async Task ReturnToolAsync(int rentalID, DateTime returnDate)
        {
            _auth.EnsureAdmin();
            await ExecuteWithTransactionAsync(async (conn, tx) =>
            {
                var selCmd = new SQLiteCommand(
                    "SELECT ToolID FROM Rentals WHERE RentalID=@RentalID AND Status='Rented'", conn, tx);
                selCmd.Parameters.AddWithValue("@RentalID", rentalID);
                var result = await selCmd.ExecuteScalarAsync();
                if (result == null) throw new InvalidOperationException("Rental not found or already returned.");
                var toolID = Convert.ToInt32(result);

                await SqliteHelper.ExecuteNonQueryAsync(conn, tx,
                    "UPDATE Rentals SET ReturnDate=@ReturnDate,Status='Returned' WHERE RentalID=@RentalID",
                    new[]
                    {
                        new SQLiteParameter("@ReturnDate", returnDate),
                        new SQLiteParameter("@RentalID", rentalID)
                    });

                if (_toolService != null)
                    await _toolService.UpdateToolQuantitiesAsync(toolID, 1, false, conn, tx);
            });
            if (_activityLog != null)
            {
                var user = _context?.CurrentUser;
                await _activityLog.LogActionAsync(user?.UserID ?? 0, user?.UserName ?? string.Empty, $"Returned rental {rentalID}").ConfigureAwait(false);
            }
        }

        public async Task ExtendRentalAsync(int rentalID, DateTime newDueDate)
        {
            _auth.EnsureAdmin();
            await ExecuteWithTransactionAsync(async (conn, tx) =>
            {
                var selectCmd = new SQLiteCommand(
                    "SELECT ToolID, DueDate FROM Rentals WHERE RentalID=@RentalID AND Status='Rented'",
                    conn, tx);
                selectCmd.Parameters.AddWithValue("@RentalID", rentalID);
                using var reader = await selectCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    throw new InvalidOperationException("Unable to extend rental. Rental not found or already returned.");

                int toolID = Convert.ToInt32(reader["ToolID"]);
                DateTime oldDueDate = DateTime.Parse(reader["DueDate"].ToString()!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

                var updateCmd = new SQLiteCommand(
                    "UPDATE Rentals SET DueDate=@NewDueDate WHERE RentalID=@RentalID AND Status='Rented'",
                    conn, tx);
                updateCmd.Parameters.AddWithValue("@NewDueDate", newDueDate);
                updateCmd.Parameters.AddWithValue("@RentalID", rentalID);
                if (await updateCmd.ExecuteNonQueryAsync() == 0)
                    throw new InvalidOperationException("Unable to extend rental. Rental not found or already returned.");

                if (_toolService != null)
                {
                    if (oldDueDate <= DateTime.Today && newDueDate > DateTime.Today)
                        await _toolService.UpdateToolQuantitiesAsync(toolID, 1, true, conn, tx);
                    else if (oldDueDate > DateTime.Today && newDueDate <= DateTime.Today)
                        await _toolService.UpdateToolQuantitiesAsync(toolID, 1, false, conn, tx);
                }
            });
            if (_activityLog != null)
            {
                var user = _context?.CurrentUser;
                await _activityLog.LogActionAsync(user?.UserID ?? 0, user?.UserName ?? string.Empty, $"Extended rental {rentalID}").ConfigureAwait(false);
            }
        }

        public async Task DeleteRentalAsync(int rentalID)
        {
            _auth.EnsureAdmin();
            const string sql = "DELETE FROM Rentals WHERE RentalID = @RentalID";
            var p = new[] { new SQLiteParameter("@RentalID", rentalID) };
            using var conn = _dbService.CreateConnection();
            if (await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p) == 0)
                throw new InvalidOperationException("Rental not found.");
            if (_activityLog != null)
            {
                var user = _context?.CurrentUser;
                await _activityLog.LogActionAsync(user?.UserID ?? 0, user?.UserName ?? string.Empty, $"Deleted rental {rentalID}").ConfigureAwait(false);
            }
        }

        public async Task<List<Rental>> GetActiveRentalsAsync()
        {
            using var conn = _dbService.CreateConnection();
            var sql = BaseSelect + " WHERE r.Status='Rented'";
            return await SqliteHelper.ExecuteReaderAsync(conn, sql, null, MapRental);
        }

        public async Task<List<Rental>> GetOverdueRentalsAsync()
        {
            const string sql = BaseSelect + @" WHERE r.Status = 'Rented' AND r.DueDate < @Today";
            var p = new[] { new SQLiteParameter("@Today", DateTime.Today) };
            using var conn = _dbService.CreateConnection();
            return await SqliteHelper.ExecuteReaderAsync(conn, sql, p, MapRental);
        }

        public async Task<List<Rental>> GetAllRentalsAsync()
        {
            using var conn = _dbService.CreateConnection();
            return await SqliteHelper.ExecuteReaderAsync(conn, BaseSelect, null, MapRental);
        }

        public async Task<List<Rental>> GetRentalHistoryForToolAsync(int toolID)
        {
            const string sql = BaseSelect + @" WHERE r.ToolID = @ToolID ORDER BY r.RentalDate DESC";
            var p = new[] { new SQLiteParameter("@ToolID", toolID) };
            using var conn = _dbService.CreateConnection();
            return await SqliteHelper.ExecuteReaderAsync(conn, sql, p, MapRental);
        }

        public async Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID)
        {
            const string sql = BaseSelect + @" WHERE r.CustomerID = @CustomerID ORDER BY r.RentalDate DESC";
            var p = new[] { new SQLiteParameter("@CustomerID", customerID) };
            using var conn = _dbService.CreateConnection();
            return await SqliteHelper.ExecuteReaderAsync(conn, sql, p, MapRental);
        }
    }
}

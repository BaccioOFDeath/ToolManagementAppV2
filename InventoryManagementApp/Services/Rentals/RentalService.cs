// Services/Rentals/RentalService.cs
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Services.Users;

namespace InventoryManagementApp.Services.Rentals
{
    public class RentalService : IRentalService
    {
        private readonly DatabaseService _dbService;
        private readonly IItemService? _itemService;
        private readonly ILogger<RentalService> _logger;
        private readonly IAuthorizationService _auth;
        private readonly ActivityLogService? _activityLog;
        private readonly IUserContext? _context;

        public RentalService(DatabaseService dbService, IAuthorizationService? authorizationService = null, IItemService? itemService = null, ILogger<RentalService>? logger = null, ActivityLogService? activityLogService = null, IUserContext? userContext = null)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _auth = authorizationService ?? new NoOpAuthorizationService();
            _itemService = itemService; // may be null if inventory sync not desired
            _logger = logger ?? NullLogger<RentalService>.Instance;
            _activityLog = activityLogService;
            _context = userContext;
        }

        async Task ExecuteWithTransactionAsync(Func<SqliteConnection, SqliteTransaction, Task> action, Func<Task>? postCommitAction = null)
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
                                    t.ItemNumber,
                                    t.NameDescription AS Name,
                                    t.Location AS ItemLocation,
                                    t.ImagePath,
                                    c.Company,
                                    c.Contact,
                                    c.Email,
                                    c.Phone,
                                    c.Mobile,
                                    c.Address
                                 FROM Rentals r
                                 JOIN Items t ON r.ItemID = t.ItemID
                                 JOIN Customers c ON r.CustomerID = c.CustomerID";

        // Synchronous rental operations removed; use async equivalents instead.

        Rental? MapRental(IDataRecord r)
        {
            try
            {
                return new Rental
                {
                    RentalID = Convert.ToInt32(r["RentalID"]),
                    ItemID = Convert.ToInt32(r["ItemID"]),
                    CustomerID = Convert.ToInt32(r["CustomerID"]),
                    RentalDate = ParseDateOrDefault(r["RentalDate"], "RentalDate"),
                    DueDate = ParseDateOrDefault(r["DueDate"], "DueDate"),
                    ReturnDate = r["ReturnDate"] is DBNull ? null : ParseNullableDate(r["ReturnDate"], "ReturnDate"),
                    Status = r["Status"].ToString(),
                    ItemNumber = r["ItemNumber"].ToString(),
                    CustomerName = r["Company"].ToString(),
                    CustomerContact = r["Contact"].ToString(),
                    CustomerEmail = r["Email"].ToString(),
                    CustomerPhone = r["Phone"].ToString(),
                    CustomerMobile = r["Mobile"].ToString(),
                    CustomerAddress = r["Address"].ToString(),
                    ImagePath = r["ImagePath"].ToString(),
                    ItemLocation = r["ItemLocation"].ToString()
                };
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Skipping rental with invalid date");
                return null;
            }
        }

        DateTime ParseDateOrDefault(object? value, string field)
        {
            var text = value?.ToString();
            if (DateTime.TryParse(text, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToLocalTime();
            _logger.LogError("Failed to parse {Field}: {Value}", field, text);
            throw new FormatException($"Invalid date value for {field}: {text}");
        }

        DateTime? ParseNullableDate(object? value, string field)
        {
            var text = value?.ToString();
            if (DateTime.TryParse(text, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToLocalTime();
            _logger.LogError("Failed to parse {Field}: {Value}", field, text);
            return null;
        }

        public async Task RentItemAsync(int itemID, int customerID, DateTime rentalDate, DateTime dueDate)
        {
            _auth.EnsureAdmin();
            await ExecuteWithTransactionAsync(async (conn, tx) =>
            {
                var availCmd = new SqliteCommand(
                    "SELECT AvailableQuantity FROM Items WHERE ItemID=@ItemID",
                    conn, tx);
                availCmd.Parameters.AddWithValue("@ItemID", itemID);
                int avail = Convert.ToInt32(await availCmd.ExecuteScalarAsync() ?? 0);
                if (avail < 1)
                    throw new InvalidOperationException("Insufficient quantity.");

                await SqliteHelper.ExecuteNonQueryAsync(conn, tx,
                    "INSERT INTO Rentals (ItemID, CustomerID, RentalDate, DueDate, Status) " +
                    "VALUES (@ItemID, @CustomerID, @RentalDate, @DueDate, 'Rented')",
                    new[]
                    {
                        new SqliteParameter("@ItemID", itemID),
                        new SqliteParameter("@CustomerID", customerID),
                        new SqliteParameter("@RentalDate", rentalDate),
                        new SqliteParameter("@DueDate", dueDate)
                    });

                if (_itemService != null)
                    await _itemService.UpdateItemQuantitiesAsync(itemID, 1, true, conn, tx);
            });
            if (_activityLog != null)
            {
                var user = _context?.CurrentUser;
                await _activityLog.LogActionAsync(user?.UserID ?? 0, user?.UserName ?? string.Empty, $"Rented item {itemID} to customer {customerID}").ConfigureAwait(false);
            }
        }

        public async Task ReturnItemAsync(int rentalID, DateTime returnDate)
        {
            _auth.EnsureAdmin();
            await ExecuteWithTransactionAsync(async (conn, tx) =>
            {
                var selCmd = new SqliteCommand(
                    "SELECT ItemID FROM Rentals WHERE RentalID=@RentalID AND Status='Rented'", conn, tx);
                selCmd.Parameters.AddWithValue("@RentalID", rentalID);
                var result = await selCmd.ExecuteScalarAsync();
                if (result == null) throw new InvalidOperationException("Rental not found or already returned.");
                var itemID = Convert.ToInt32(result);

                await SqliteHelper.ExecuteNonQueryAsync(conn, tx,
                    "UPDATE Rentals SET ReturnDate=@ReturnDate,Status='Returned' WHERE RentalID=@RentalID",
                    new[]
                    {
                        new SqliteParameter("@ReturnDate", returnDate),
                        new SqliteParameter("@RentalID", rentalID)
                    });

                if (_itemService != null)
                    await _itemService.UpdateItemQuantitiesAsync(itemID, 1, false, conn, tx);
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
                var selectCmd = new SqliteCommand(
                    "SELECT ItemID, DueDate FROM Rentals WHERE RentalID=@RentalID AND Status='Rented'",
                    conn, tx);
                selectCmd.Parameters.AddWithValue("@RentalID", rentalID);
                using var reader = await selectCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    throw new InvalidOperationException("Unable to extend rental. Rental not found or already returned.");

                int itemID = Convert.ToInt32(reader["ItemID"]);
                var dueText = reader["DueDate"].ToString();
                DateTime oldDueDate;
                if (!DateTime.TryParse(dueText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out oldDueDate))
                {
                    _logger.LogError("Failed to parse DueDate: {Value}", dueText);
                    oldDueDate = default;
                }

                var updateCmd = new SqliteCommand(
                    "UPDATE Rentals SET DueDate=@NewDueDate WHERE RentalID=@RentalID AND Status='Rented'",
                    conn, tx);
                updateCmd.Parameters.AddWithValue("@NewDueDate", newDueDate);
                updateCmd.Parameters.AddWithValue("@RentalID", rentalID);
                if (await updateCmd.ExecuteNonQueryAsync() == 0)
                    throw new InvalidOperationException("Unable to extend rental. Rental not found or already returned.");

                if (_itemService != null)
                {
                    if (oldDueDate <= DateTime.Today && newDueDate > DateTime.Today)
                        await _itemService.UpdateItemQuantitiesAsync(itemID, 1, true, conn, tx);
                    else if (oldDueDate > DateTime.Today && newDueDate <= DateTime.Today)
                        await _itemService.UpdateItemQuantitiesAsync(itemID, 1, false, conn, tx);
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
            var p = new[] { new SqliteParameter("@RentalID", rentalID) };
            using var conn = _dbService.CreateConnection();
            if (await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p) == 0)
                throw new InvalidOperationException("Rental not found.");
            if (_activityLog != null)
            {
                var user = _context?.CurrentUser;
                await _activityLog.LogActionAsync(user?.UserID ?? 0, user?.UserName ?? string.Empty, $"Deleted rental {rentalID}").ConfigureAwait(false);
            }
        }

        public async Task<int> CountActiveRentalsAsync()
        {
            using var conn = _dbService.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM Rentals WHERE Status='Rented'";
            return Convert.ToInt32(await SqliteHelper.ExecuteScalarAsync(conn, sql).ConfigureAwait(false));
        }

        public async Task<List<Rental>> GetActiveRentalsAsync()
        {
            using var conn = _dbService.CreateConnection();
            var sql = BaseSelect + " WHERE r.Status='Rented'";
            var list = await SqliteHelper.ExecuteReaderAsync(conn, sql, MapRental);
            return list.Where(r => r != null).Select(r => r!).ToList();
        }

        public async Task<List<Rental>> GetOverdueRentalsAsync()
        {
            const string sql = BaseSelect + @" WHERE r.Status = 'Rented' AND r.DueDate < @Today";
            var p = new[] { new SqliteParameter("@Today", DateTime.Today) };
            using var conn = _dbService.CreateConnection();
            var list = await SqliteHelper.ExecuteReaderAsync(conn, sql, MapRental, p);
            return list.Where(r => r != null).Select(r => r!).ToList();
        }

        public async Task<List<Rental>> GetAllRentalsAsync()
        {
            using var conn = _dbService.CreateConnection();
            var list = await SqliteHelper.ExecuteReaderAsync(conn, BaseSelect, MapRental);
            return list.Where(r => r != null).Select(r => r!).ToList();
        }

        public async Task<List<Rental>> GetRentalHistoryForItemAsync(int itemID)
        {
            const string sql = BaseSelect + @" WHERE r.ItemID = @ItemID ORDER BY r.RentalDate DESC";
            var p = new[] { new SqliteParameter("@ItemID", itemID) };
            using var conn = _dbService.CreateConnection();
            var list = await SqliteHelper.ExecuteReaderAsync(conn, sql, MapRental, p);
            return list.Where(r => r != null).Select(r => r!).ToList();
        }

        public async Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID)
        {
            const string sql = BaseSelect + @" WHERE r.CustomerID = @CustomerID ORDER BY r.RentalDate DESC";
            var p = new[] { new SqliteParameter("@CustomerID", customerID) };
            using var conn = _dbService.CreateConnection();
            var list = await SqliteHelper.ExecuteReaderAsync(conn, sql, MapRental, p);
            return list.Where(r => r != null).Select(r => r!).ToList();
        }
    }
}

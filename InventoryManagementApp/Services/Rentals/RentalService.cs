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
using InventoryManagementApp.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace InventoryManagementApp.Services.Rentals
{
    /// <summary>
    /// Service for managing rental operations including renting items, returns, extensions, and rental history.
    /// </summary>
    public class RentalService : IRentalService
    {
        private const int MaxRentalListCount = 500;
        private const int MaxRentalHistoryCount = 500;
        private const int MaxRentalFrequencyCount = 100;

        private readonly DatabaseService _dbService;
        private readonly IItemService? _itemService;
        private readonly ILogger<RentalService> _logger;
        private readonly IAuthorizationService _auth;
        private readonly ActivityLogService? _activityLog;
        private readonly IUserContext? _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="RentalService"/> class.
        /// </summary>
        /// <param name="dbService">Database service for data access.</param>
        /// <param name="authorizationService">Optional authorization service for access control.</param>
        /// <param name="itemService">Optional item service for inventory synchronization.</param>
        /// <param name="logger">Optional logger for diagnostic output.</param>
        /// <param name="activityLogService">Optional activity log service for audit trails.</param>
        /// <param name="userContext">Optional user context for tracking current user.</param>
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
                    Status = ValidateString(r["Status"], "Status"),
                    ItemNumber = ValidateString(r["ItemNumber"], "ItemNumber"),
                    CustomerName = ValidateString(r["Company"], "Company"),
                    CustomerContact = ValidateString(r["Contact"], "Contact"),
                    CustomerEmail = ValidateString(r["Email"], "Email"),
                    CustomerPhone = ValidateString(r["Phone"], "Phone"),
                    CustomerMobile = ValidateString(r["Mobile"], "Mobile"),
                    CustomerAddress = ValidateString(r["Address"], "Address"),
                    ImagePath = ValidateString(r["ImagePath"], "ImagePath"),
                    ItemLocation = ValidateString(r["ItemLocation"], "ItemLocation")
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
            if (value is DateTime dateTime)
                return dateTime.Kind == DateTimeKind.Utc ? dateTime.ToLocalTime() : dateTime;

            var text = value?.ToString();
            if (DateTime.TryParse(text, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToLocalTime();
            _logger.LogError("Failed to parse {Field}: {Value}", field, text);
            throw new FormatException($"Invalid date value for {field}: {text}");
        }

        DateTime? ParseNullableDate(object? value, string field)
        {
            if (value is DateTime dateTime)
                return dateTime.Kind == DateTimeKind.Utc ? dateTime.ToLocalTime() : dateTime;

            var text = value?.ToString();
            if (DateTime.TryParse(text, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToLocalTime();
            _logger.LogError("Failed to parse {Field}: {Value}", field, text);
            return null;
        }

        string ValidateString(object? value, string field)
        {
            var text = value?.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("{Field} was null or empty while mapping rental", field);
                return string.Empty;
            }
            return text;
        }

        /// <summary>
        /// Creates a new rental transaction for an item. Requires admin privileges.
        /// </summary>
        /// <param name="itemID">The ID of the item to rent.</param>
        /// <param name="customerID">The ID of the customer renting the item.</param>
        /// <param name="rentalDate">The start date of the rental.</param>
        /// <param name="dueDate">The expected return date.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if itemID or customerID is less than 1.</exception>
        /// <exception cref="ArgumentException">Thrown if dueDate is before rentalDate.</exception>
        /// <exception cref="InvalidOperationException">Thrown if item is not available for rental.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if user lacks admin privileges.</exception>
        public async Task RentItemAsync(int itemID, int customerID, DateTime rentalDate, DateTime dueDate)
        {
            if (itemID < 1)
                throw new ArgumentOutOfRangeException(nameof(itemID), "Item ID must be greater than 0.");
            if (customerID < 1)
                throw new ArgumentOutOfRangeException(nameof(customerID), "Customer ID must be greater than 0.");
            if (dueDate < rentalDate)
                throw new ArgumentException("Due date cannot be before rental date.", nameof(dueDate));
            
            _auth.EnsureAdmin();
            await ExecuteWithTransactionAsync(async (conn, tx) =>
            {
                var avail = await GetAvailableQuantityForExistingItemAsync(conn, tx, itemID);
                await EnsureCustomerExistsAsync(conn, tx, customerID);

                if (avail < 1)
                    throw new InvalidOperationException("Insufficient quantity.");

                var insertedRows = await SqliteHelper.ExecuteNonQueryAsync(conn, tx,
                    "INSERT INTO Rentals (ItemID, CustomerID, RentalDate, DueDate, Status) " +
                    "VALUES (@ItemID, @CustomerID, @RentalDate, @DueDate, 'Rented')",
                    new[]
                    {
                        new SqliteParameter("@ItemID", itemID),
                        new SqliteParameter("@CustomerID", customerID),
                        new SqliteParameter("@RentalDate", rentalDate),
                        new SqliteParameter("@DueDate", dueDate)
                    });
                if (insertedRows == 0)
                    throw new InvalidOperationException("Unable to create rental.");

                if (_itemService != null)
                    await _itemService.UpdateItemQuantitiesAsync(itemID, 1, true, conn, tx);
            });
            if (_activityLog != null)
            {
                var user = _context?.CurrentUser;
                await _activityLog.LogActionAsync(user?.UserID ?? 0, user?.UserName ?? string.Empty, $"Rented item {itemID} to customer {customerID}").ConfigureAwait(false);
            }
            NotifyChanged(DomainDataScope.Rentals | DomainDataScope.Items | DomainDataScope.Reservations | DomainDataScope.ActivityLogs | DomainDataScope.Reports, itemID);
        }

        /// <summary>
        /// Processes the return of a rented item. Requires admin privileges.
        /// </summary>
        /// <param name="rentalID">The ID of the rental to return.</param>
        /// <param name="returnDate">The date the item was returned.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if rentalID is less than 1.</exception>
        /// <exception cref="InvalidOperationException">Thrown if rental not found or already returned.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if user lacks admin privileges.</exception>
        public async Task ReturnItemAsync(int rentalID, DateTime returnDate)
        {
            if (rentalID < 1)
                throw new ArgumentOutOfRangeException(nameof(rentalID), "Rental ID must be greater than 0.");
            
            _auth.EnsureAdmin();
            var changedItemID = 0;
            await ExecuteWithTransactionAsync(async (conn, tx) =>
            {
                var selCmd = new SqliteCommand(
                    "SELECT ItemID FROM Rentals WHERE RentalID=@RentalID AND Status='Rented'", conn, tx);
                selCmd.Parameters.AddWithValue("@RentalID", rentalID);
                var result = await selCmd.ExecuteScalarAsync();
                if (result == null) throw new InvalidOperationException("Rental not found or already returned.");
                var itemID = Convert.ToInt32(result);
                changedItemID = itemID;

                var returnedRows = await SqliteHelper.ExecuteNonQueryAsync(conn, tx,
                    "UPDATE Rentals SET ReturnDate=@ReturnDate,Status='Returned' WHERE RentalID=@RentalID AND Status='Rented'",
                    new[]
                    {
                        new SqliteParameter("@ReturnDate", returnDate),
                        new SqliteParameter("@RentalID", rentalID)
                    });
                if (returnedRows == 0)
                    throw new InvalidOperationException("Rental not found or already returned.");

                if (_itemService != null)
                    await _itemService.UpdateItemQuantitiesAsync(itemID, 1, false, conn, tx);
            });
            if (_activityLog != null)
            {
                var user = _context?.CurrentUser;
                await _activityLog.LogActionAsync(user?.UserID ?? 0, user?.UserName ?? string.Empty, $"Returned rental {rentalID}").ConfigureAwait(false);
            }
            NotifyChanged(DomainDataScope.Rentals | DomainDataScope.Items | DomainDataScope.Reservations | DomainDataScope.ActivityLogs | DomainDataScope.Reports, changedItemID > 0 ? changedItemID : rentalID);
        }

        /// <summary>
        /// Extends the due date of an active rental. Requires admin privileges.
        /// </summary>
        /// <param name="rentalID">The ID of the rental to extend.</param>
        /// <param name="newDueDate">The new due date for the rental.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if rentalID is less than 1.</exception>
        /// <exception cref="InvalidOperationException">Thrown if rental not found or not active.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if user lacks admin privileges.</exception>
        public async Task ExtendRentalAsync(int rentalID, DateTime newDueDate)
        {
            if (rentalID < 1)
                throw new ArgumentOutOfRangeException(nameof(rentalID), "Rental ID must be greater than 0.");

            _auth.EnsureAdmin();
            await ExecuteWithTransactionAsync(async (conn, tx) =>
            {
                var selectCmd = new SqliteCommand(
                    "SELECT 1 FROM Rentals WHERE RentalID=@RentalID AND Status='Rented'",
                    conn, tx);
                selectCmd.Parameters.AddWithValue("@RentalID", rentalID);
                var activeRental = await selectCmd.ExecuteScalarAsync();
                if (activeRental == null)
                    throw new InvalidOperationException("Unable to extend rental. Rental not found or already returned.");

                var updateCmd = new SqliteCommand(
                    "UPDATE Rentals SET DueDate=@NewDueDate WHERE RentalID=@RentalID AND Status='Rented'",
                    conn, tx);
                updateCmd.Parameters.AddWithValue("@NewDueDate", newDueDate);
                updateCmd.Parameters.AddWithValue("@RentalID", rentalID);
                if (await updateCmd.ExecuteNonQueryAsync() == 0)
                    throw new InvalidOperationException("Unable to extend rental. Rental not found or already returned.");
            });
            if (_activityLog != null)
            {
                var user = _context?.CurrentUser;
                await _activityLog.LogActionAsync(user?.UserID ?? 0, user?.UserName ?? string.Empty, $"Extended rental {rentalID}").ConfigureAwait(false);
            }
            NotifyChanged(DomainDataScope.Rentals | DomainDataScope.ActivityLogs | DomainDataScope.Reports, rentalID);
        }

        public async Task SwapRentalItemAsync(int rentalID, int newItemID)
        {
            if (rentalID < 1)
                throw new ArgumentOutOfRangeException(nameof(rentalID), "Rental ID must be greater than 0.");
            if (newItemID < 1)
                throw new ArgumentOutOfRangeException(nameof(newItemID), "Item ID must be greater than 0.");
            if (_itemService == null)
                throw new InvalidOperationException("Rental item swaps require inventory synchronization.");

            _auth.EnsureAdmin();
            var oldItemID = 0;
            await ExecuteWithTransactionAsync(async (conn, tx) =>
            {
                var selectCmd = new SqliteCommand(
                    "SELECT ItemID FROM Rentals WHERE RentalID=@RentalID AND Status='Rented'",
                    conn, tx);
                selectCmd.Parameters.AddWithValue("@RentalID", rentalID);
                var result = await selectCmd.ExecuteScalarAsync();
                if (result == null || result is DBNull)
                    throw new InvalidOperationException("Unable to swap item. Rental not found or already returned.");

                oldItemID = Convert.ToInt32(result);
                if (oldItemID == newItemID)
                    return;

                var avail = await GetAvailableQuantityForExistingItemAsync(conn, tx, newItemID);
                if (avail < 1)
                    throw new InvalidOperationException("Replacement item has no available stock.");

                var updateCmd = new SqliteCommand(
                    "UPDATE Rentals SET ItemID=@NewItemID WHERE RentalID=@RentalID AND Status='Rented'",
                    conn, tx);
                updateCmd.Parameters.AddWithValue("@NewItemID", newItemID);
                updateCmd.Parameters.AddWithValue("@RentalID", rentalID);
                if (await updateCmd.ExecuteNonQueryAsync() == 0)
                    throw new InvalidOperationException("Unable to swap item. Rental not found or already returned.");

                await _itemService.UpdateItemQuantitiesAsync(oldItemID, 1, false, conn, tx);
                await _itemService.UpdateItemQuantitiesAsync(newItemID, 1, true, conn, tx);
            });
            if (_activityLog != null)
            {
                var user = _context?.CurrentUser;
                await _activityLog.LogActionAsync(user?.UserID ?? 0, user?.UserName ?? string.Empty, $"Swapped rental {rentalID} item from {oldItemID} to {newItemID}").ConfigureAwait(false);
            }
            NotifyChanged(DomainDataScope.Rentals | DomainDataScope.Items | DomainDataScope.ActivityLogs | DomainDataScope.Reports, newItemID);
        }

        public async Task DeleteRentalAsync(int rentalID)
        {
            if (rentalID < 1)
                throw new ArgumentOutOfRangeException(nameof(rentalID), "Rental ID must be greater than 0.");

            _auth.EnsureAdmin();
            var changedItemID = 0;
            await ExecuteWithTransactionAsync(async (conn, tx) =>
            {
                var selectCmd = new SqliteCommand(
                    "SELECT ItemID, Status, ReturnDate FROM Rentals WHERE RentalID=@RentalID",
                    conn, tx);
                selectCmd.Parameters.AddWithValue("@RentalID", rentalID);
                using var reader = await selectCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    throw new InvalidOperationException("Rental not found.");

                var itemID = Convert.ToInt32(reader["ItemID"]);
                changedItemID = itemID;
                var status = reader["Status"]?.ToString();
                var isActive = string.Equals(status, "Rented", StringComparison.OrdinalIgnoreCase) && reader["ReturnDate"] is DBNull;
                await reader.DisposeAsync();

                var deleteCmd = new SqliteCommand(
                    "DELETE FROM Rentals WHERE RentalID=@RentalID",
                    conn, tx);
                deleteCmd.Parameters.AddWithValue("@RentalID", rentalID);
                var deletedRows = await deleteCmd.ExecuteNonQueryAsync();
                if (deletedRows == 0)
                    throw new InvalidOperationException("Rental not found.");

                if (isActive && _itemService != null)
                    await _itemService.UpdateItemQuantitiesAsync(itemID, 1, false, conn, tx);
            });
            if (_activityLog != null)
            {
                var user = _context?.CurrentUser;
                await _activityLog.LogActionAsync(user?.UserID ?? 0, user?.UserName ?? string.Empty, $"Deleted rental {rentalID}").ConfigureAwait(false);
            }
            NotifyChanged(DomainDataScope.Rentals | DomainDataScope.Items | DomainDataScope.ActivityLogs | DomainDataScope.Reports, changedItemID > 0 ? changedItemID : rentalID);
        }

        static void NotifyChanged(DomainDataScope scope, int? entityId = null)
        {
            WeakReferenceMessenger.Default.Send(new DomainDataChangedMessage(scope, entityId));
        }

        public async Task<int> CountRentalsAsync()
        {
            using var conn = _dbService.CreateConnection();
            const string sql = @"
                SELECT COUNT(r.RentalID)
                FROM Rentals r
                JOIN Items t ON r.ItemID = t.ItemID
                JOIN Customers c ON r.CustomerID = c.CustomerID";
            return Convert.ToInt32(await SqliteHelper.ExecuteScalarAsync(conn, sql).ConfigureAwait(false) ?? 0);
        }

        public async Task<int> CountActiveRentalsAsync()
        {
            using var conn = _dbService.CreateConnection();
            const string sql = @"
                SELECT COUNT(r.RentalID)
                FROM Rentals r
                JOIN Items t ON r.ItemID = t.ItemID
                JOIN Customers c ON r.CustomerID = c.CustomerID
                WHERE r.Status='Rented'";
            return Convert.ToInt32(await SqliteHelper.ExecuteScalarAsync(conn, sql).ConfigureAwait(false) ?? 0);
        }

        public async Task<List<Rental>> GetActiveRentalsAsync()
        {
            const string sql = BaseSelect + " WHERE r.Status='Rented' ORDER BY r.DueDate ASC LIMIT @RentalListLimit";
            var p = new[] { new SqliteParameter("@RentalListLimit", MaxRentalListCount) };
            using var conn = _dbService.CreateConnection();
            var list = await SqliteHelper.ExecuteReaderAsync(conn, sql, MapRental, p);
            return list.Where(r => r != null).Select(r => r!).ToList();
        }

        public async Task<List<Rental>> GetOverdueRentalsAsync()
        {
            const string sql = BaseSelect + @" WHERE r.Status = 'Rented' AND r.DueDate < @Today ORDER BY r.DueDate ASC LIMIT @RentalListLimit";
            var p = new[]
            {
                new SqliteParameter("@Today", DateTime.Today),
                new SqliteParameter("@RentalListLimit", MaxRentalListCount)
            };
            using var conn = _dbService.CreateConnection();
            var list = await SqliteHelper.ExecuteReaderAsync(conn, sql, MapRental, p);
            return list.Where(r => r != null).Select(r => r!).ToList();
        }

        public async Task<List<Rental>> GetAllRentalsAsync()
        {
            const string sql = BaseSelect + " ORDER BY r.RentalDate DESC LIMIT @RentalListLimit";
            var p = new[] { new SqliteParameter("@RentalListLimit", MaxRentalListCount) };
            using var conn = _dbService.CreateConnection();
            var list = await SqliteHelper.ExecuteReaderAsync(conn, sql, MapRental, p);
            return list.Where(r => r != null).Select(r => r!).ToList();
        }

        public async Task<List<Rental>> GetRentalHistoryForItemAsync(int itemID)
        {
            if (itemID < 1)
                throw new ArgumentOutOfRangeException(nameof(itemID), "Item ID must be greater than 0.");

            using var conn = _dbService.CreateConnection();
            await EnsureItemExistsAsync(conn, itemID).ConfigureAwait(false);

            const string sql = BaseSelect + @" WHERE r.ItemID = @ItemID ORDER BY r.RentalDate DESC LIMIT @RentalHistoryLimit";
            var p = new[]
            {
                new SqliteParameter("@ItemID", itemID),
                new SqliteParameter("@RentalHistoryLimit", MaxRentalHistoryCount)
            };
            var list = await SqliteHelper.ExecuteReaderAsync(conn, sql, MapRental, p);
            return list.Where(r => r != null).Select(r => r!).ToList();
        }

        public async Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID)
        {
            if (customerID < 1)
                throw new ArgumentOutOfRangeException(nameof(customerID), "Customer ID must be greater than 0.");

            using var conn = _dbService.CreateConnection();
            await EnsureCustomerExistsAsync(conn, customerID).ConfigureAwait(false);

            const string sql = BaseSelect + @" WHERE r.CustomerID = @CustomerID ORDER BY r.RentalDate DESC LIMIT @RentalHistoryLimit";
            var p = new[]
            {
                new SqliteParameter("@CustomerID", customerID),
                new SqliteParameter("@RentalHistoryLimit", MaxRentalHistoryCount)
            };
            var list = await SqliteHelper.ExecuteReaderAsync(conn, sql, MapRental, p);
            return list.Where(r => r != null).Select(r => r!).ToList();
        }

        public async Task<List<ItemRentalFrequency>> GetRentalFrequencyAsync(int topN = 10)
        {
            if (topN < 1)
                throw new ArgumentOutOfRangeException(nameof(topN), "Top rental frequency count must be greater than 0.");
            if (topN > MaxRentalFrequencyCount)
                throw new ArgumentOutOfRangeException(nameof(topN), $"Top rental frequency count cannot exceed {MaxRentalFrequencyCount}.");

            const string sql = @"
                SELECT t.ItemID, t.ItemNumber, t.NameDescription, COUNT(r.RentalID) AS RentalCount
                FROM Items t
                LEFT JOIN Rentals r ON t.ItemID = r.ItemID
                    AND EXISTS (SELECT 1 FROM Customers c WHERE c.CustomerID = r.CustomerID)
                GROUP BY t.ItemID, t.ItemNumber, t.NameDescription
                HAVING RentalCount > 0
                ORDER BY RentalCount DESC
                LIMIT @TopN";
            
            var p = new[] { new SqliteParameter("@TopN", topN) };
            using var conn = _dbService.CreateConnection();
            
            var frequencies = new List<ItemRentalFrequency>();
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddRange(p);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                frequencies.Add(new ItemRentalFrequency
                {
                    ItemID = Convert.ToInt32(reader["ItemID"]),
                    ItemNumber = reader["ItemNumber"]?.ToString() ?? string.Empty,
                    ItemName = reader["NameDescription"]?.ToString() ?? string.Empty,
                    RentalCount = Convert.ToInt32(reader["RentalCount"])
                });
            }
            
            return frequencies;
        }

        private static async Task<int> GetAvailableQuantityForExistingItemAsync(SqliteConnection conn, SqliteTransaction tx, int itemID)
        {
            var availCmd = new SqliteCommand(
                "SELECT AvailableQuantity FROM Items WHERE ItemID=@ItemID",
                conn, tx);
            availCmd.Parameters.AddWithValue("@ItemID", itemID);
            var result = await availCmd.ExecuteScalarAsync();
            if (result == null || result is DBNull)
                throw new InvalidOperationException("Item not found.");

            return Convert.ToInt32(result);
        }

        private static async Task EnsureItemExistsAsync(SqliteConnection conn, int itemID)
        {
            var itemCmd = new SqliteCommand(
                "SELECT COUNT(*) FROM Items WHERE ItemID=@ItemID",
                conn);
            itemCmd.Parameters.AddWithValue("@ItemID", itemID);
            var itemCount = Convert.ToInt32(await itemCmd.ExecuteScalarAsync() ?? 0);
            if (itemCount < 1)
                throw new InvalidOperationException("Item not found.");
        }

        private static async Task EnsureCustomerExistsAsync(SqliteConnection conn, int customerID)
        {
            var customerCmd = new SqliteCommand(
                "SELECT COUNT(*) FROM Customers WHERE CustomerID=@CustomerID",
                conn);
            customerCmd.Parameters.AddWithValue("@CustomerID", customerID);
            var customerCount = Convert.ToInt32(await customerCmd.ExecuteScalarAsync() ?? 0);
            if (customerCount < 1)
                throw new InvalidOperationException("Customer not found.");
        }

        private static async Task EnsureCustomerExistsAsync(SqliteConnection conn, SqliteTransaction tx, int customerID)
        {
            var customerCmd = new SqliteCommand(
                "SELECT COUNT(*) FROM Customers WHERE CustomerID=@CustomerID",
                conn, tx);
            customerCmd.Parameters.AddWithValue("@CustomerID", customerID);
            var customerCount = Convert.ToInt32(await customerCmd.ExecuteScalarAsync() ?? 0);
            if (customerCount < 1)
                throw new InvalidOperationException("Customer not found.");
        }
    }
}

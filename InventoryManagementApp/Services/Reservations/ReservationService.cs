using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Services.Reservations
{
    /// <summary>
    /// Service for managing item reservations, allowing customers to reserve items for future use.
    /// </summary>
    public class ReservationService
    {
        private readonly DatabaseService _databaseService;
        private readonly IUserContext _userContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationService"/> class.
        /// </summary>
        /// <param name="databaseService">Database service for data access.</param>
        /// <param name="userContext">User context for tracking current user.</param>
        public ReservationService(DatabaseService databaseService, IUserContext userContext)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        /// <summary>
        /// Retrieves all reservations from the database, ordered by start date descending.
        /// </summary>
        /// <returns>A list of all reservations.</returns>
        public async Task<List<Reservation>> GetAllReservationsAsync()
        {
            return await Task.Run(() =>
            {
                var reservations = new List<Reservation>();
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT r.*, i.ItemNumber, i.NameDescription as ItemName, i.ImagePath, c.Company as CustomerName
                    FROM Reservations r
                    LEFT JOIN Items i ON r.ItemID = i.ItemID
                    LEFT JOIN Customers c ON r.CustomerID = c.CustomerID
                    ORDER BY r.StartDate DESC";
                using var cmd = new SqliteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reservations.Add(MapReservation(reader));
                }
                return reservations;
            });
        }

        /// <summary>
        /// Retrieves all active reservations (pending or confirmed status), ordered by start date.
        /// </summary>
        /// <returns>A list of active reservations.</returns>
        public async Task<List<Reservation>> GetActiveReservationsAsync()
        {
            return await Task.Run(() =>
            {
                var reservations = new List<Reservation>();
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT r.*, i.ItemNumber, i.NameDescription as ItemName, i.ImagePath, c.Company as CustomerName
                    FROM Reservations r
                    LEFT JOIN Items i ON r.ItemID = i.ItemID
                    LEFT JOIN Customers c ON r.CustomerID = c.CustomerID
                    WHERE r.Status IN ('Pending', 'Confirmed')
                    ORDER BY r.StartDate ASC";
                using var cmd = new SqliteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reservations.Add(MapReservation(reader));
                }
                return reservations;
            });
        }

        /// <summary>
        /// Retrieves all reservations for a specific item.
        /// </summary>
        /// <param name="itemID">The ID of the item.</param>
        /// <returns>A list of reservations for the specified item.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if itemID is less than 1.</exception>
        public async Task<List<Reservation>> GetReservationsByItemAsync(int itemID)
        {
            if (itemID < 1)
                throw new ArgumentOutOfRangeException(nameof(itemID), "Item ID must be greater than 0.");
            return await Task.Run(() =>
            {
                var reservations = new List<Reservation>();
                using var conn = _databaseService.CreateConnection();
                EnsureReservationItemExists(conn, itemID);
                var sql = @"
                    SELECT r.*, i.ItemNumber, i.NameDescription as ItemName, i.ImagePath, c.Company as CustomerName
                    FROM Reservations r
                    LEFT JOIN Items i ON r.ItemID = i.ItemID
                    LEFT JOIN Customers c ON r.CustomerID = c.CustomerID
                    WHERE r.ItemID = @ItemID
                    ORDER BY r.StartDate DESC";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ItemID", itemID);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reservations.Add(MapReservation(reader));
                }
                return reservations;
            });
        }

        public async Task<List<Reservation>> GetReservationsByCustomerAsync(int customerID)
        {
            if (customerID < 1)
                throw new ArgumentOutOfRangeException(nameof(customerID), "Customer ID must be greater than 0.");

            return await Task.Run(() =>
            {
                var reservations = new List<Reservation>();
                using var conn = _databaseService.CreateConnection();
                EnsureReservationCustomerExists(conn, customerID);
                var sql = @"
                    SELECT r.*, i.ItemNumber, i.NameDescription as ItemName, i.ImagePath, c.Company as CustomerName
                    FROM Reservations r
                    LEFT JOIN Items i ON r.ItemID = i.ItemID
                    LEFT JOIN Customers c ON r.CustomerID = c.CustomerID
                    WHERE r.CustomerID = @CustomerID
                    ORDER BY r.StartDate DESC";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CustomerID", customerID);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reservations.Add(MapReservation(reader));
                }
                return reservations;
            });
        }

        public async Task<List<Reservation>> GetUpcomingReservationsAsync(int days = 7)
        {
            if (days < 0)
                throw new ArgumentOutOfRangeException(nameof(days), "Days must be greater than or equal to 0.");

            return await Task.Run(() =>
            {
                var reservations = new List<Reservation>();
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT r.*, i.ItemNumber, i.NameDescription as ItemName, i.ImagePath, c.Company as CustomerName
                    FROM Reservations r
                    LEFT JOIN Items i ON r.ItemID = i.ItemID
                    LEFT JOIN Customers c ON r.CustomerID = c.CustomerID
                    WHERE r.Status IN ('Pending', 'Confirmed')
                    AND r.StartDate <= @FutureDate
                    ORDER BY r.StartDate ASC";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FutureDate", DateTime.Now.AddDays(days));
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reservations.Add(MapReservation(reader));
                }
                return reservations;
            });
        }

        public async Task<Reservation?> GetReservationByIdAsync(int reservationID)
        {
            if (reservationID < 1)
                throw new ArgumentOutOfRangeException(nameof(reservationID), "Reservation ID must be greater than 0.");

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT r.*, i.ItemNumber, i.NameDescription as ItemName, i.ImagePath, c.Company as CustomerName
                    FROM Reservations r
                    LEFT JOIN Items i ON r.ItemID = i.ItemID
                    LEFT JOIN Customers c ON r.CustomerID = c.CustomerID
                    WHERE r.ReservationID = @ReservationID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ReservationID", reservationID);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return MapReservation(reader);
                }
                return null;
            });
        }

        public async Task<int> CreateReservationAsync(Reservation reservation)
        {
            ValidateReservation(reservation, requireExistingId: false);

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                EnsureReservationReferencesExist(conn, reservation);

                var sql = @"
                    INSERT INTO Reservations 
                    (ItemID, CustomerID, ReservationDate, StartDate, EndDate, 
                     Quantity, Status, Notes, CreatedByUserID, CreatedAt)
                    VALUES 
                    (@ItemID, @CustomerID, @ReservationDate, @StartDate, @EndDate, 
                     @Quantity, @Status, @Notes, @CreatedByUserID, @CreatedAt);
                    SELECT last_insert_rowid();";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ItemID", reservation.ItemID);
                cmd.Parameters.AddWithValue("@CustomerID", reservation.CustomerID);
                cmd.Parameters.AddWithValue("@ReservationDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@StartDate", reservation.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", reservation.EndDate);
                cmd.Parameters.AddWithValue("@Quantity", reservation.Quantity);
                cmd.Parameters.AddWithValue("@Status", NormalizeStatus(reservation.Status));
                cmd.Parameters.AddWithValue("@Notes", ToDbNullableText(reservation.Notes));
                cmd.Parameters.AddWithValue("@CreatedByUserID", _userContext.CurrentUser?.UserID ?? 0);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                var id = Convert.ToInt32(cmd.ExecuteScalar());
                return id;
            });
        }

        public async Task<bool> UpdateReservationAsync(Reservation reservation)
        {
            ValidateReservation(reservation, requireExistingId: true);

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                EnsureReservationExists(conn, reservation.ReservationID);
                EnsureReservationReferencesExist(conn, reservation);
                EnsureReservationRentalReferenceExists(conn, reservation);

                var sql = @"
                    UPDATE Reservations 
                    SET ItemID = @ItemID,
                        CustomerID = @CustomerID,
                        StartDate = @StartDate,
                        EndDate = @EndDate,
                        Quantity = @Quantity,
                        Status = @Status,
                        Notes = @Notes,
                        RentalID = @RentalID
                    WHERE ReservationID = @ReservationID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ReservationID", reservation.ReservationID);
                cmd.Parameters.AddWithValue("@ItemID", reservation.ItemID);
                cmd.Parameters.AddWithValue("@CustomerID", reservation.CustomerID);
                cmd.Parameters.AddWithValue("@StartDate", reservation.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", reservation.EndDate);
                cmd.Parameters.AddWithValue("@Quantity", reservation.Quantity);
                cmd.Parameters.AddWithValue("@Status", NormalizeStatus(reservation.Status));
                cmd.Parameters.AddWithValue("@Notes", ToDbNullableText(reservation.Notes));
                cmd.Parameters.AddWithValue("@RentalID", reservation.RentalID.HasValue ? (object)reservation.RentalID.Value : DBNull.Value);
                var updatedRows = cmd.ExecuteNonQuery();
                EnsureReservationWriteSucceeded(updatedRows);
                return true;
            });
        }

        public async Task<bool> ConfirmReservationAsync(int reservationID)
        {
            if (reservationID < 1)
                throw new ArgumentOutOfRangeException(nameof(reservationID), "Reservation ID must be greater than 0.");

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                EnsureReservationExists(conn, reservationID);

                var sql = "UPDATE Reservations SET Status = 'Confirmed' WHERE ReservationID = @ReservationID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ReservationID", reservationID);
                var confirmedRows = cmd.ExecuteNonQuery();
                EnsureReservationWriteSucceeded(confirmedRows);
                return true;
            });
        }

        public async Task<bool> CancelReservationAsync(int reservationID)
        {
            if (reservationID < 1)
                throw new ArgumentOutOfRangeException(nameof(reservationID), "Reservation ID must be greater than 0.");

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                EnsureReservationExists(conn, reservationID);

                var sql = "UPDATE Reservations SET Status = 'Cancelled' WHERE ReservationID = @ReservationID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ReservationID", reservationID);
                var cancelledRows = cmd.ExecuteNonQuery();
                EnsureReservationWriteSucceeded(cancelledRows);
                return true;
            });
        }

        public async Task<bool> FulfillReservationAsync(int reservationID, int rentalID)
        {
            if (reservationID < 1)
                throw new ArgumentOutOfRangeException(nameof(reservationID), "Reservation ID must be greater than 0.");
            if (rentalID < 1)
                throw new ArgumentOutOfRangeException(nameof(rentalID), "Rental ID must be greater than 0.");

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                EnsureReservationExists(conn, reservationID);
                if (!RecordExists(conn, "SELECT COUNT(*) FROM Rentals WHERE RentalID = @ID", rentalID))
                    throw new ArgumentException("Reservation fulfillment must reference an existing rental.", nameof(rentalID));

                var sql = "UPDATE Reservations SET Status = 'Fulfilled', RentalID = @RentalID WHERE ReservationID = @ReservationID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ReservationID", reservationID);
                cmd.Parameters.AddWithValue("@RentalID", rentalID);
                var fulfilledRows = cmd.ExecuteNonQuery();
                EnsureReservationWriteSucceeded(fulfilledRows);
                return true;
            });
        }

        public async Task<bool> DeleteReservationAsync(int reservationID)
        {
            if (reservationID < 1)
                throw new ArgumentOutOfRangeException(nameof(reservationID), "Reservation ID must be greater than 0.");

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                EnsureReservationExists(conn, reservationID);

                var sql = "DELETE FROM Reservations WHERE ReservationID = @ReservationID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ReservationID", reservationID);
                var deletedRows = cmd.ExecuteNonQuery();
                EnsureReservationWriteSucceeded(deletedRows);
                return true;
            });
        }

        public async Task<bool> CheckAvailabilityAsync(int itemID, DateTime startDate, DateTime endDate, int quantity)
        {
            if (itemID < 1)
                throw new ArgumentOutOfRangeException(nameof(itemID), "Item ID must be greater than 0.");
            if (quantity < 1)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0.");
            if (endDate < startDate)
                throw new ArgumentException("End date must be on or after start date.", nameof(endDate));

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT i.AvailableQuantity,
                    COALESCE(SUM(r.Quantity), 0) as ReservedQuantity
                    FROM Items i
                    LEFT JOIN Reservations r ON i.ItemID = r.ItemID
                        AND r.Status IN ('Pending', 'Confirmed')
                        AND ((r.StartDate <= @EndDate AND r.EndDate >= @StartDate))
                    WHERE i.ItemID = @ItemID
                    GROUP BY i.ItemID, i.AvailableQuantity";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ItemID", itemID);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var availableQuantity = reader.GetInt32(0);
                    var reservedQuantity = reader.GetInt32(1);
                    return (availableQuantity - reservedQuantity) >= quantity;
                }
                return false;
            });
        }

        private Reservation MapReservation(SqliteDataReader reader)
        {
            return new Reservation
            {
                ReservationID = reader.GetInt32(reader.GetOrdinal("ReservationID")),
                ItemID = reader.GetInt32(reader.GetOrdinal("ItemID")),
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                ItemNumber = reader.IsDBNull(reader.GetOrdinal("ItemNumber")) ? "" : reader.GetString(reader.GetOrdinal("ItemNumber")),
                ItemName = reader.IsDBNull(reader.GetOrdinal("ItemName")) ? "" : reader.GetString(reader.GetOrdinal("ItemName")),
                CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? "" : reader.GetString(reader.GetOrdinal("CustomerName")),
                ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? "" : reader.GetString(reader.GetOrdinal("ImagePath")),
                ReservationDate = reader.GetDateTime(reader.GetOrdinal("ReservationDate")),
                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? "" : reader.GetString(reader.GetOrdinal("Notes")),
                CreatedByUserID = reader.GetInt32(reader.GetOrdinal("CreatedByUserID")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                RentalID = reader.IsDBNull(reader.GetOrdinal("RentalID")) ? null : reader.GetInt32(reader.GetOrdinal("RentalID"))
            };
        }

        private static void ValidateReservation(Reservation reservation, bool requireExistingId)
        {
            if (reservation == null)
                throw new ArgumentNullException(nameof(reservation));
            if (requireExistingId && reservation.ReservationID < 1)
                throw new ArgumentOutOfRangeException(nameof(reservation.ReservationID), "Reservation ID must be greater than 0.");
            if (reservation.ItemID < 1)
                throw new ArgumentOutOfRangeException(nameof(reservation.ItemID), "Item ID must be greater than 0.");
            if (reservation.CustomerID < 1)
                throw new ArgumentOutOfRangeException(nameof(reservation.CustomerID), "Customer ID must be greater than 0.");
            if (reservation.Quantity < 1)
                throw new ArgumentOutOfRangeException(nameof(reservation.Quantity), "Quantity must be greater than 0.");
            if (reservation.EndDate < reservation.StartDate)
                throw new ArgumentException("End date must be on or after start date.", nameof(reservation.EndDate));
        }

        private static void EnsureReservationExists(SqliteConnection conn, int reservationID)
        {
            if (!RecordExists(conn, "SELECT COUNT(*) FROM Reservations WHERE ReservationID = @ID", reservationID))
                throw new InvalidOperationException("Reservation not found.");
        }

        private static void EnsureReservationWriteSucceeded(int affectedRows)
        {
            if (affectedRows == 0)
                throw new InvalidOperationException("Reservation not found.");
        }

        private static void EnsureReservationItemExists(SqliteConnection conn, int itemID)
        {
            if (!RecordExists(conn, "SELECT COUNT(*) FROM Items WHERE ItemID = @ID", itemID))
                throw new InvalidOperationException("Item not found.");
        }

        private static void EnsureReservationCustomerExists(SqliteConnection conn, int customerID)
        {
            if (!RecordExists(conn, "SELECT COUNT(*) FROM Customers WHERE CustomerID = @ID", customerID))
                throw new InvalidOperationException("Customer not found.");
        }

        private static void EnsureReservationReferencesExist(SqliteConnection conn, Reservation reservation)
        {
            if (!RecordExists(conn, "SELECT COUNT(*) FROM Items WHERE ItemID = @ID", reservation.ItemID))
                throw new ArgumentException("Reservation must reference an existing item.", nameof(reservation.ItemID));
            if (!RecordExists(conn, "SELECT COUNT(*) FROM Customers WHERE CustomerID = @ID", reservation.CustomerID))
                throw new ArgumentException("Reservation must reference an existing customer.", nameof(reservation.CustomerID));
        }

        private static void EnsureReservationRentalReferenceExists(SqliteConnection conn, Reservation reservation)
        {
            if (reservation.RentalID.HasValue && !RecordExists(conn, "SELECT COUNT(*) FROM Rentals WHERE RentalID = @ID", reservation.RentalID.Value))
                throw new ArgumentException("Reservation must reference an existing rental.", nameof(reservation.RentalID));
        }

        private static bool RecordExists(SqliteConnection conn, string sql, int id)
        {
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ID", id);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private static string NormalizeStatus(string? status)
        {
            var normalizedStatus = string.IsNullOrWhiteSpace(status) ? "Pending" : status.Trim();
            return normalizedStatus switch
            {
                "Pending" or "Confirmed" or "Cancelled" or "Fulfilled" => normalizedStatus,
                _ => throw new ArgumentException("Reservation status must be Pending, Confirmed, Cancelled, or Fulfilled.", nameof(status))
            };
        }

        private static object ToDbNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
        }
    }
}

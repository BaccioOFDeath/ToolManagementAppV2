using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Services.Kits
{
    /// <summary>
    /// Service for managing equipment kits, which are collections of related items grouped together.
    /// </summary>
    public class KitService
    {
        private readonly DatabaseService _databaseService;
        private readonly IUserContext _userContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="KitService"/> class.
        /// </summary>
        /// <param name="databaseService">Database service for data access.</param>
        /// <param name="userContext">User context for tracking current user.</param>
        public KitService(DatabaseService databaseService, IUserContext userContext)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        /// <summary>
        /// Retrieves all kits from the database, ordered by name.
        /// </summary>
        /// <returns>A list of all kits.</returns>
        public async Task<List<Kit>> GetAllKitsAsync()
        {
            return await Task.Run(() =>
            {
                var kits = new List<Kit>();
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT * FROM Kits
                    ORDER BY Name ASC";
                using var cmd = new SqliteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    kits.Add(MapKit(reader));
                }
                return kits;
            });
        }

        /// <summary>
        /// Retrieves all active kits from the database, ordered by name.
        /// </summary>
        /// <returns>A list of active kits.</returns>
        public async Task<List<Kit>> GetActiveKitsAsync()
        {
            return await Task.Run(() =>
            {
                var kits = new List<Kit>();
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT * FROM Kits
                    WHERE IsActive = 1
                    ORDER BY Name ASC";
                using var cmd = new SqliteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    kits.Add(MapKit(reader));
                }
                return kits;
            });
        }

        /// <summary>
        /// Retrieves a specific kit by its ID.
        /// </summary>
        /// <param name="kitID">The ID of the kit to retrieve.</param>
        /// <returns>The kit if found; otherwise, null.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if kitID is less than 1.</exception>
        public async Task<Kit?> GetKitByIdAsync(int kitID)
        {
            if (kitID < 1)
                throw new ArgumentOutOfRangeException(nameof(kitID), "Kit ID must be greater than 0.");
            
            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = "SELECT * FROM Kits WHERE KitID = @KitID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@KitID", kitID);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return MapKit(reader);
                }
                return null;
            });
        }

        /// <summary>
        /// Retrieves all items that belong to a specific kit.
        /// </summary>
        /// <param name="kitID">The ID of the kit.</param>
        /// <returns>A list of items in the kit.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if kitID is less than 1.</exception>
        public async Task<List<KitItem>> GetKitItemsAsync(int kitID)
        {
            if (kitID < 1)
                throw new ArgumentOutOfRangeException(nameof(kitID), "Kit ID must be greater than 0.");
            return await Task.Run(() =>
            {
                var items = new List<KitItem>();
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT ki.*, i.ItemNumber, i.NameDescription as ItemName
                    FROM KitItems ki
                    LEFT JOIN Items i ON ki.ItemID = i.ItemID
                    WHERE ki.KitID = @KitID
                    ORDER BY i.ItemNumber";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@KitID", kitID);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    items.Add(MapKitItem(reader));
                }
                return items;
            });
        }

        public async Task<int> CreateKitAsync(Kit kit)
        {
            ValidateKit(kit, requireExistingId: false);

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    INSERT INTO Kits 
                    (KitNumber, Name, Description, Category, IsActive, CreatedByUserID, CreatedAt, UpdatedAt)
                    VALUES 
                    (@KitNumber, @Name, @Description, @Category, @IsActive, @CreatedByUserID, @CreatedAt, @UpdatedAt);
                    SELECT last_insert_rowid();";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@KitNumber", kit.KitNumber.Trim());
                cmd.Parameters.AddWithValue("@Name", kit.Name.Trim());
                cmd.Parameters.AddWithValue("@Description", ToDbNullableText(kit.Description));
                cmd.Parameters.AddWithValue("@Category", ToDbNullableText(kit.Category));
                cmd.Parameters.AddWithValue("@IsActive", kit.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@CreatedByUserID", _userContext.CurrentUser?.UserID ?? 0);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                var id = Convert.ToInt32(cmd.ExecuteScalar());
                return id;
            });
        }

        public async Task<bool> UpdateKitAsync(Kit kit)
        {
            ValidateKit(kit, requireExistingId: true);

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    UPDATE Kits 
                    SET KitNumber = @KitNumber,
                        Name = @Name,
                        Description = @Description,
                        Category = @Category,
                        IsActive = @IsActive,
                        UpdatedAt = @UpdatedAt
                    WHERE KitID = @KitID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@KitID", kit.KitID);
                cmd.Parameters.AddWithValue("@KitNumber", kit.KitNumber.Trim());
                cmd.Parameters.AddWithValue("@Name", kit.Name.Trim());
                cmd.Parameters.AddWithValue("@Description", ToDbNullableText(kit.Description));
                cmd.Parameters.AddWithValue("@Category", ToDbNullableText(kit.Category));
                cmd.Parameters.AddWithValue("@IsActive", kit.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                return cmd.ExecuteNonQuery() > 0;
            });
        }

        public async Task<bool> DeleteKitAsync(int kitID)
        {
            if (kitID < 1)
                throw new ArgumentOutOfRangeException(nameof(kitID), "Kit ID must be greater than 0.");

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                using var transaction = conn.BeginTransaction();
                try
                {
                    var deleteItemsSql = "DELETE FROM KitItems WHERE KitID = @KitID";
                    using var deleteItemsCmd = new SqliteCommand(deleteItemsSql, conn, transaction);
                    deleteItemsCmd.Parameters.AddWithValue("@KitID", kitID);
                    deleteItemsCmd.ExecuteNonQuery();

                    var deleteKitSql = "DELETE FROM Kits WHERE KitID = @KitID";
                    using var deleteKitCmd = new SqliteCommand(deleteKitSql, conn, transaction);
                    deleteKitCmd.Parameters.AddWithValue("@KitID", kitID);
                    var result = deleteKitCmd.ExecuteNonQuery() > 0;

                    transaction.Commit();
                    return result;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });
        }

        public async Task<int> AddKitItemAsync(KitItem kitItem)
        {
            ValidateKitItem(kitItem, requireExistingId: false);

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                EnsureKitItemReferencesExist(conn, kitItem);

                var sql = @"
                    INSERT INTO KitItems 
                    (KitID, ItemID, Quantity, IsOptional)
                    VALUES 
                    (@KitID, @ItemID, @Quantity, @IsOptional);
                    SELECT last_insert_rowid();";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@KitID", kitItem.KitID);
                cmd.Parameters.AddWithValue("@ItemID", kitItem.ItemID);
                cmd.Parameters.AddWithValue("@Quantity", kitItem.Quantity);
                cmd.Parameters.AddWithValue("@IsOptional", kitItem.IsOptional ? 1 : 0);
                var id = Convert.ToInt32(cmd.ExecuteScalar());
                return id;
            });
        }

        public async Task<bool> UpdateKitItemAsync(KitItem kitItem)
        {
            ValidateKitItem(kitItem, requireExistingId: true);

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                EnsureKitItemReferencesExist(conn, kitItem);

                var sql = @"
                    UPDATE KitItems 
                    SET ItemID = @ItemID,
                        Quantity = @Quantity,
                        IsOptional = @IsOptional
                    WHERE KitItemID = @KitItemID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@KitItemID", kitItem.KitItemID);
                cmd.Parameters.AddWithValue("@ItemID", kitItem.ItemID);
                cmd.Parameters.AddWithValue("@Quantity", kitItem.Quantity);
                cmd.Parameters.AddWithValue("@IsOptional", kitItem.IsOptional ? 1 : 0);
                return cmd.ExecuteNonQuery() > 0;
            });
        }

        public async Task<bool> RemoveKitItemAsync(int kitItemID)
        {
            if (kitItemID < 1)
                throw new ArgumentOutOfRangeException(nameof(kitItemID), "Kit item ID must be greater than 0.");

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = "DELETE FROM KitItems WHERE KitItemID = @KitItemID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@KitItemID", kitItemID);
                return cmd.ExecuteNonQuery() > 0;
            });
        }

        public async Task<bool> CheckKitAvailabilityAsync(int kitID)
        {
            if (kitID < 1)
                throw new ArgumentOutOfRangeException(nameof(kitID), "Kit ID must be greater than 0.");

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    SELECT COUNT(*) as MissingItems
                    FROM KitItems ki
                    LEFT JOIN Items i ON ki.ItemID = i.ItemID
                    WHERE ki.KitID = @KitID
                    AND ki.IsOptional = 0
                    AND (i.ItemID IS NULL OR i.AvailableQuantity < ki.Quantity)";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@KitID", kitID);
                var missingItems = Convert.ToInt32(cmd.ExecuteScalar());
                return missingItems == 0;
            });
        }

        private Kit MapKit(SqliteDataReader reader)
        {
            return new Kit
            {
                KitID = reader.GetInt32(reader.GetOrdinal("KitID")),
                KitNumber = reader.GetString(reader.GetOrdinal("KitNumber")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString(reader.GetOrdinal("Description")),
                Category = reader.IsDBNull(reader.GetOrdinal("Category")) ? "" : reader.GetString(reader.GetOrdinal("Category")),
                IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1,
                CreatedByUserID = reader.GetInt32(reader.GetOrdinal("CreatedByUserID")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        private KitItem MapKitItem(SqliteDataReader reader)
        {
            return new KitItem
            {
                KitItemID = reader.GetInt32(reader.GetOrdinal("KitItemID")),
                KitID = reader.GetInt32(reader.GetOrdinal("KitID")),
                ItemID = reader.GetInt32(reader.GetOrdinal("ItemID")),
                ItemNumber = reader.IsDBNull(reader.GetOrdinal("ItemNumber")) ? "" : reader.GetString(reader.GetOrdinal("ItemNumber")),
                ItemName = reader.IsDBNull(reader.GetOrdinal("ItemName")) ? "" : reader.GetString(reader.GetOrdinal("ItemName")),
                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                IsOptional = reader.GetInt32(reader.GetOrdinal("IsOptional")) == 1
            };
        }

        private static void ValidateKit(Kit kit, bool requireExistingId)
        {
            if (kit == null)
                throw new ArgumentNullException(nameof(kit));
            if (requireExistingId && kit.KitID < 1)
                throw new ArgumentOutOfRangeException(nameof(kit.KitID), "Kit ID must be greater than 0.");
            if (string.IsNullOrWhiteSpace(kit.KitNumber))
                throw new ArgumentException("Kit number is required.", nameof(kit.KitNumber));
            if (string.IsNullOrWhiteSpace(kit.Name))
                throw new ArgumentException("Kit name is required.", nameof(kit.Name));
        }

        private static void ValidateKitItem(KitItem kitItem, bool requireExistingId)
        {
            if (kitItem == null)
                throw new ArgumentNullException(nameof(kitItem));
            if (requireExistingId && kitItem.KitItemID < 1)
                throw new ArgumentOutOfRangeException(nameof(kitItem.KitItemID), "Kit item ID must be greater than 0.");
            if (kitItem.KitID < 1)
                throw new ArgumentOutOfRangeException(nameof(kitItem.KitID), "Kit ID must be greater than 0.");
            if (kitItem.ItemID < 1)
                throw new ArgumentOutOfRangeException(nameof(kitItem.ItemID), "Item ID must be greater than 0.");
            if (kitItem.Quantity < 1)
                throw new ArgumentOutOfRangeException(nameof(kitItem.Quantity), "Quantity must be greater than 0.");
        }

        private static void EnsureKitItemReferencesExist(SqliteConnection conn, KitItem kitItem)
        {
            if (!RecordExists(conn, "SELECT COUNT(*) FROM Kits WHERE KitID = @ID", kitItem.KitID))
                throw new ArgumentException("Kit item must reference an existing kit.", nameof(kitItem.KitID));
            if (!RecordExists(conn, "SELECT COUNT(*) FROM Items WHERE ItemID = @ID", kitItem.ItemID))
                throw new ArgumentException("Kit item must reference an existing item.", nameof(kitItem.ItemID));
        }

        private static bool RecordExists(SqliteConnection conn, string sql, int id)
        {
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ID", id);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private static object ToDbNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
        }
    }
}
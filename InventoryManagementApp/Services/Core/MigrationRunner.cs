using System;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Services.Core
{
    /// <summary>
    /// Executes database schema migrations in a versioned manner.
    /// </summary>
    public class MigrationRunner
    {
        private readonly DatabaseService _db;
        private readonly ILogger<MigrationRunner> _logger;

        public MigrationRunner(DatabaseService db, ILogger<MigrationRunner>? logger = null)
        {
            _db = db;
            _logger = logger ?? NullLogger<MigrationRunner>.Instance;
        }

        /// <summary>
        /// Applies any pending migrations to bring the database schema up to date.
        /// </summary>
        public void Migrate()
        {
            using var conn = _db.CreateConnection();

            // Ensure migration tracking table exists
            using (var cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS SchemaInfo (Version INTEGER PRIMARY KEY);", conn))
            {
                cmd.ExecuteNonQuery();
            }

            int currentVersion;
            using (var cmd = new SqliteCommand("SELECT IFNULL(MAX(Version),0) FROM SchemaInfo;", conn))
            {
                currentVersion = Convert.ToInt32(cmd.ExecuteScalar());
            }

            if (currentVersion < 1)
            {
                ApplyV1(conn);
                using var insert = new SqliteCommand("INSERT INTO SchemaInfo (Version) VALUES (1);", conn);
                insert.ExecuteNonQuery();
            }
        }

        void ApplyV1(SqliteConnection conn)
        {
            // Columns for Items table
            _db.EnsureColumn("Items", "ItemNumber", "TEXT");
            _db.EnsureColumn("Items", "NameDescription", "TEXT");
            _db.EnsureColumn("Items", "ImagePath", "TEXT");
            _db.EnsureColumn("Items", "CheckedOutBy", "TEXT");
            _db.EnsureColumn("Items", "CheckedOutTime", "DATETIME");
            _db.EnsureColumn("Items", "CheckedInBy", "TEXT");
            _db.EnsureColumn("Items", "CheckedInTime", "DATETIME");
            _db.EnsureColumn("Items", "Keywords", "TEXT");
            _db.EnsureColumn("Items", "IsPowered", "INTEGER", "0");
            _db.EnsureColumn("Items", "IsCheckedOut", "INTEGER", "0");
            _db.EnsureColumn("Items", "IsRentalItem", "INTEGER", "0");
            _db.EnsureColumn("Items", "Price", "NUMERIC", "0");
            _db.EnsureColumn("Items", "UpdatedAt", "DATETIME");

            // Indexes on Items
            _db.EnsureIndex(conn, "Items", "AvailableQuantity");
            _db.EnsureIndex(conn, "Items", "Keywords");
            _db.EnsureIndex(conn, "Items", "UpdatedAt");
            _db.EnsureIndex(conn, "Items", "IsRentalItem");

            // Columns for Users table
            _db.EnsureColumn("Users", "PasswordHash", "TEXT");
            _db.EnsureColumn("Users", "PasswordSalt", "TEXT");
            _db.EnsureColumn("Users", "Email", "TEXT");
            _db.EnsureColumn("Users", "Phone", "TEXT");
            _db.EnsureColumn("Users", "Mobile", "TEXT");
            _db.EnsureColumn("Users", "Address", "TEXT");
            _db.EnsureColumn("Users", "Role", "TEXT");
            _db.EnsureColumn("Users", "IsActive", "INTEGER", "1");
            _db.EnsureColumn("Users", "CreatedAt", "DATETIME");
            _db.EnsureColumn("Users", "PasswordExpired", "INTEGER", "0");
        }
    }
}

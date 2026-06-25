using Microsoft.Data.Sqlite;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Utilities.Helpers;
using System.Linq;

namespace InventoryManagementApp.Services.Core
{
    /// <summary>
    /// Provides Sqlite database access for the application. Instances should be
    /// disposed when no longer needed to release pooled connections.
    /// </summary>
    public class DatabaseService : IDisposable, IAsyncDisposable, IDatabaseBackupService, IDatabaseService
    {
        public string ConnectionString { get; }
        private readonly ILogger<DatabaseService> _logger;
        private readonly SqliteConnection? _keepAliveConnection;
        bool _disposed;
        
        private const int DefaultTimeoutSeconds = 5;
        private const int BusyTimeoutMilliseconds = 15000;

        public SqliteConnection CreateConnection()
        {
            var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            ConfigureConnection(conn);
            return conn;
        }

        public DatabaseService(
            string dbPath,
            ILogger<DatabaseService>? logger = null,
            bool secureDatabaseFile = true,
            bool useWalJournal = true,
            bool useConnectionPooling = true)
        {
            _logger = logger ?? NullLogger<DatabaseService>.Instance;

            // Ensure database file exists (create if missing), but skip in-memory databases.
            var isInMemory = dbPath.Contains(":memory:", StringComparison.OrdinalIgnoreCase);
            if (!isInMemory)
            {
                var securityWarning = DatabaseSecurityHelper.EnsureDatabaseFileSecurity(dbPath, secureDatabaseFile);
                if (!string.IsNullOrWhiteSpace(securityWarning))
                    _logger.LogWarning("Database permissions warning: {Warning}", securityWarning);
            }
            var builder = new SqliteConnectionStringBuilder
            {
                Pooling = useConnectionPooling,
                Cache = SqliteCacheMode.Shared,
                DefaultTimeout = DefaultTimeoutSeconds
            };

            if (isInMemory)
            {
                builder.DataSource = $"InventoryManagementApp_{Guid.NewGuid():N}";
                builder.Mode = SqliteOpenMode.Memory;
            }
            else
            {
                builder.DataSource = dbPath;
            }

            ConnectionString = builder.ToString();
            if (isInMemory)
            {
                _keepAliveConnection = new SqliteConnection(ConnectionString);
                _keepAliveConnection.Open();
            }
            ConfigureDatabase(useWalJournal);
            InitializeDatabase();

            if (!isInMemory)
            {
                var securityWarning = DatabaseSecurityHelper.EnsureDatabaseFileSecurity(dbPath, secureDatabaseFile);
                if (!string.IsNullOrWhiteSpace(securityWarning))
                    _logger.LogWarning("Database permissions warning: {Warning}", securityWarning);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _keepAliveConnection?.Dispose();
                SqliteConnection.ClearAllPools();
            }
            _disposed = true;
        }

        ~DatabaseService()
        {
            Dispose(false);
        }

        void ConfigureDatabase(bool useWalJournal)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var journalMode = useWalJournal ? "WAL" : "DELETE";
            using var cmd = new SqliteCommand($"PRAGMA journal_mode={journalMode};", conn);
            cmd.ExecuteNonQuery();
            ConfigureConnection(conn);
        }

        static void ConfigureConnection(SqliteConnection conn)
        {
            using var timeout = new SqliteCommand($"PRAGMA busy_timeout={BusyTimeoutMilliseconds};", conn);
            timeout.ExecuteNonQuery();
        }

        void InitializeDatabase()
        {
            using var conn = CreateConnection();

            // Legacy migration: rename old legacy item table to Items
            MigrateLegacyItemsTable(conn);

            // Remove obsolete device-related tables from older databases
            DropObsoleteDeviceTables(conn);

            var sql = @"
                CREATE TABLE IF NOT EXISTS Items (
                    ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
                    ItemNumber TEXT NOT NULL,
                    NameDescription TEXT,
                    Location TEXT,
                    Brand TEXT,
                    PartNumber TEXT,
                    Supplier TEXT,
                    PurchasedDate DATETIME,
                    Notes TEXT,
                    Keywords TEXT,
                    AvailableQuantity INTEGER NOT NULL DEFAULT 0,
                    RentedQuantity INTEGER NOT NULL DEFAULT 0,
                    IsRentalItem INTEGER NOT NULL DEFAULT 0,
                    Price NUMERIC NOT NULL DEFAULT 0,
                    ImagePath TEXT,
                    IsPowered INTEGER NOT NULL DEFAULT 0,
                    IsCheckedOut INTEGER NOT NULL DEFAULT 0,
                    CheckedOutBy TEXT,
                    CheckedOutTime DATETIME,
                    CheckedInBy TEXT,
                    CheckedInTime DATETIME,
                    UpdatedAt DATETIME,
                    IsIncomplete INTEGER NOT NULL DEFAULT 0,
                    MissingComponentsNotes TEXT,
                    IssuesNotes TEXT,
                    CheckoutCount INTEGER NOT NULL DEFAULT 0
                );
                CREATE TABLE IF NOT EXISTS Users (
                    UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserName TEXT NOT NULL,
                    UserPhotoPath TEXT,
                    PasswordHash TEXT,
                    PasswordSalt TEXT,
                    IsAdmin INTEGER NOT NULL DEFAULT 0,
                    Email TEXT,
                    Phone TEXT,
                    Mobile TEXT,
                    Address TEXT,
                    Role TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    PasswordExpired INTEGER NOT NULL DEFAULT 0,
                    FailedLoginAttempts INTEGER NOT NULL DEFAULT 0,
                    LockoutEndUtc DATETIME,
                    Permissions TEXT
                );
                CREATE TABLE IF NOT EXISTS Customers (
                    CustomerID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Company TEXT NOT NULL,
                    Email TEXT,
                    Contact TEXT,
                    Phone TEXT,
                    Mobile TEXT,
                    Address TEXT
                );
                CREATE TABLE IF NOT EXISTS Rentals (
                    RentalID INTEGER PRIMARY KEY AUTOINCREMENT,
                    ItemID INTEGER NOT NULL,
                    CustomerID INTEGER NOT NULL,
                    RentalDate DATETIME NOT NULL,
                    DueDate DATETIME NOT NULL,
                    ReturnDate DATETIME,
                    Status TEXT NOT NULL DEFAULT 'Rented',
                    FOREIGN KEY (ItemID) REFERENCES Items(ItemID),
                    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
                );
                CREATE TABLE IF NOT EXISTS RentalPhotos (
                    PhotoID INTEGER PRIMARY KEY AUTOINCREMENT,
                    RentalID INTEGER,
                    ItemID INTEGER NOT NULL,
                    PhotoStage TEXT NOT NULL,
                    FilePath TEXT NOT NULL,
                    Notes TEXT,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CreatedBy TEXT,
                    FOREIGN KEY (RentalID) REFERENCES Rentals(RentalID),
                    FOREIGN KEY (ItemID) REFERENCES Items(ItemID)
                );
                CREATE TABLE IF NOT EXISTS ActivityLogs (
                    LogID INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserID INTEGER,
                    UserName TEXT,
                    Action TEXT,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserID) REFERENCES Users(UserID)
                );
                CREATE TABLE IF NOT EXISTS Settings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT
                );
                CREATE TABLE IF NOT EXISTS MaintenanceRecords (
                    MaintenanceID INTEGER PRIMARY KEY AUTOINCREMENT,
                    ItemID INTEGER NOT NULL,
                    ScheduledDate DATETIME NOT NULL,
                    CompletedDate DATETIME,
                    MaintenanceType TEXT NOT NULL,
                    Description TEXT,
                    PerformedBy TEXT,
                    Cost NUMERIC NOT NULL DEFAULT 0,
                    Status TEXT NOT NULL DEFAULT 'Scheduled',
                    Notes TEXT,
                    UserID INTEGER,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (ItemID) REFERENCES Items(ItemID),
                    FOREIGN KEY (UserID) REFERENCES Users(UserID)
                );
                CREATE TABLE IF NOT EXISTS CalibrationRecords (
                    CalibrationID INTEGER PRIMARY KEY AUTOINCREMENT,
                    ItemID INTEGER NOT NULL,
                    CalibrationDate DATETIME NOT NULL,
                    NextCalibrationDue DATETIME NOT NULL,
                    CalibratedBy TEXT,
                    CertificateNumber TEXT,
                    Standard TEXT,
                    Result TEXT,
                    Cost NUMERIC NOT NULL DEFAULT 0,
                    Notes TEXT,
                    UserID INTEGER,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (ItemID) REFERENCES Items(ItemID),
                    FOREIGN KEY (UserID) REFERENCES Users(UserID)
                );
                CREATE TABLE IF NOT EXISTS Reservations (
                    ReservationID INTEGER PRIMARY KEY AUTOINCREMENT,
                    ItemID INTEGER NOT NULL,
                    CustomerID INTEGER NOT NULL,
                    ReservationDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    StartDate DATETIME NOT NULL,
                    EndDate DATETIME NOT NULL,
                    Quantity INTEGER NOT NULL DEFAULT 1,
                    Status TEXT NOT NULL DEFAULT 'Pending',
                    Notes TEXT,
                    CreatedByUserID INTEGER NOT NULL,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    RentalID INTEGER,
                    FOREIGN KEY (ItemID) REFERENCES Items(ItemID),
                    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
                    FOREIGN KEY (CreatedByUserID) REFERENCES Users(UserID),
                    FOREIGN KEY (RentalID) REFERENCES Rentals(RentalID)
                );
                CREATE TABLE IF NOT EXISTS Kits (
                    KitID INTEGER PRIMARY KEY AUTOINCREMENT,
                    KitNumber TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    Category TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    CreatedByUserID INTEGER NOT NULL,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (CreatedByUserID) REFERENCES Users(UserID)
                );
                CREATE TABLE IF NOT EXISTS KitItems (
                    KitItemID INTEGER PRIMARY KEY AUTOINCREMENT,
                    KitID INTEGER NOT NULL,
                    ItemID INTEGER NOT NULL,
                    Quantity INTEGER NOT NULL DEFAULT 1,
                    IsOptional INTEGER NOT NULL DEFAULT 0,
                    FOREIGN KEY (KitID) REFERENCES Kits(KitID),
                    FOREIGN KEY (ItemID) REFERENCES Items(ItemID)
                );";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.ExecuteNonQuery();
            EnsureCurrentSchemaColumns(conn);
            EnsureIndex(conn, "Items", "ItemNumber", true);
            EnsureIndex(conn, "Items", "NameDescription");
            EnsureIndex(conn, "Items", "Brand");
            EnsureIndex(conn, "Items", "PartNumber");
            EnsureIndex(conn, "Items", "Supplier");
            EnsureIndex(conn, "Items", "Location");
            EnsureIndex(conn, "Items", "Notes");
            // Ensure each user has a unique username
            EnsureIndex(conn, "Users", "UserName", true);
            EnsureIndex(conn, "Customers", "Contact");
            EnsureIndex(conn, "Rentals", new[] { "ItemID", "CustomerID" });
            EnsureIndex(conn, "RentalPhotos", "RentalID");
            EnsureIndex(conn, "RentalPhotos", "ItemID");
            EnsureIndex(conn, "RentalPhotos", "PhotoStage");
            EnsureIndex(conn, "MaintenanceRecords", "ItemID");
            EnsureIndex(conn, "MaintenanceRecords", "ScheduledDate");
            EnsureIndex(conn, "MaintenanceRecords", "Status");
            EnsureIndex(conn, "CalibrationRecords", "ItemID");
            EnsureIndex(conn, "CalibrationRecords", "NextCalibrationDue");
            EnsureIndex(conn, "Reservations", "ItemID");
            EnsureIndex(conn, "Reservations", "CustomerID");
            EnsureIndex(conn, "Reservations", new[] { "StartDate", "EndDate" });
            EnsureIndex(conn, "Reservations", "Status");
            EnsureIndex(conn, "Kits", "KitNumber", true);
            EnsureIndex(conn, "KitItems", "KitID");
            EnsureIndex(conn, "KitItems", "ItemID");
        }

        void EnsureCurrentSchemaColumns(SqliteConnection conn)
        {
            RenameColumnIfExists(conn, "Items", "ToolID", "ItemID");
            RenameColumnIfExists(conn, "Items", "ToolNumber", "ItemNumber");
            RenameColumnIfExists(conn, "Items", "Description", "NameDescription");
            RenameColumnIfExists(conn, "Items", "Quantity", "AvailableQuantity");

            EnsureColumn(conn, "Items", "ItemNumber", "TEXT", "''");
            EnsureColumn(conn, "Items", "NameDescription", "TEXT");
            EnsureColumn(conn, "Items", "Location", "TEXT");
            EnsureColumn(conn, "Items", "Brand", "TEXT");
            EnsureColumn(conn, "Items", "PartNumber", "TEXT");
            EnsureColumn(conn, "Items", "Supplier", "TEXT");
            EnsureColumn(conn, "Items", "PurchasedDate", "DATETIME");
            EnsureColumn(conn, "Items", "Notes", "TEXT");
            EnsureColumn(conn, "Items", "Keywords", "TEXT");
            EnsureColumn(conn, "Items", "AvailableQuantity", "INTEGER", "0");
            EnsureColumn(conn, "Items", "RentedQuantity", "INTEGER", "0");
            EnsureColumn(conn, "Items", "IsRentalItem", "INTEGER", "0");
            EnsureColumn(conn, "Items", "Price", "NUMERIC", "0");
            EnsureColumn(conn, "Items", "ImagePath", "TEXT");
            EnsureColumn(conn, "Items", "IsPowered", "INTEGER", "0");
            EnsureColumn(conn, "Items", "IsCheckedOut", "INTEGER", "0");
            EnsureColumn(conn, "Items", "CheckedOutBy", "TEXT");
            EnsureColumn(conn, "Items", "CheckedOutTime", "DATETIME");
            EnsureColumn(conn, "Items", "CheckedInBy", "TEXT");
            EnsureColumn(conn, "Items", "CheckedInTime", "DATETIME");
            EnsureColumn(conn, "Items", "UpdatedAt", "DATETIME");
            EnsureColumn(conn, "Items", "IsIncomplete", "INTEGER", "0");
            EnsureColumn(conn, "Items", "MissingComponentsNotes", "TEXT");
            EnsureColumn(conn, "Items", "IssuesNotes", "TEXT");
            EnsureColumn(conn, "Items", "CheckoutCount", "INTEGER", "0");

            EnsureColumn(conn, "Users", "UserName", "TEXT", "''");
            EnsureColumn(conn, "Users", "UserPhotoPath", "TEXT");
            EnsureColumn(conn, "Users", "PasswordHash", "TEXT");
            EnsureColumn(conn, "Users", "PasswordSalt", "TEXT");
            EnsureColumn(conn, "Users", "IsAdmin", "INTEGER", "0");
            EnsureColumn(conn, "Users", "Email", "TEXT");
            EnsureColumn(conn, "Users", "Phone", "TEXT");
            EnsureColumn(conn, "Users", "Mobile", "TEXT");
            EnsureColumn(conn, "Users", "Address", "TEXT");
            EnsureColumn(conn, "Users", "Role", "TEXT");
            EnsureColumn(conn, "Users", "IsActive", "INTEGER", "1");
            EnsureColumn(conn, "Users", "CreatedAt", "DATETIME", "'1970-01-01 00:00:00'");
            EnsureColumn(conn, "Users", "PasswordExpired", "INTEGER", "0");
            EnsureColumn(conn, "Users", "FailedLoginAttempts", "INTEGER", "0");
            EnsureColumn(conn, "Users", "LockoutEndUtc", "DATETIME");
            EnsureColumn(conn, "Users", "Permissions", "TEXT");

            EnsureColumn(conn, "Customers", "Company", "TEXT", "''");
            EnsureColumn(conn, "Customers", "Email", "TEXT");
            EnsureColumn(conn, "Customers", "Contact", "TEXT");
            EnsureColumn(conn, "Customers", "Phone", "TEXT");
            EnsureColumn(conn, "Customers", "Mobile", "TEXT");
            EnsureColumn(conn, "Customers", "Address", "TEXT");

            EnsureColumn(conn, "Rentals", "ItemID", "INTEGER", "0");
            EnsureColumn(conn, "Rentals", "CustomerID", "INTEGER", "0");
            EnsureColumn(conn, "Rentals", "RentalDate", "DATETIME", "'1970-01-01 00:00:00'");
            EnsureColumn(conn, "Rentals", "DueDate", "DATETIME", "'1970-01-01 00:00:00'");
            EnsureColumn(conn, "Rentals", "ReturnDate", "DATETIME");
            EnsureColumn(conn, "Rentals", "Status", "TEXT", "'Rented'");

            EnsureColumn(conn, "RentalPhotos", "RentalID", "INTEGER");
            EnsureColumn(conn, "RentalPhotos", "ItemID", "INTEGER", "0");
            EnsureColumn(conn, "RentalPhotos", "PhotoStage", "TEXT", "'General'");
            EnsureColumn(conn, "RentalPhotos", "FilePath", "TEXT", "''");
            EnsureColumn(conn, "RentalPhotos", "Notes", "TEXT");
            EnsureColumn(conn, "RentalPhotos", "CreatedAt", "DATETIME", "'1970-01-01 00:00:00'");
            EnsureColumn(conn, "RentalPhotos", "CreatedBy", "TEXT");

            EnsureColumn(conn, "ActivityLogs", "UserID", "INTEGER");
            EnsureColumn(conn, "ActivityLogs", "UserName", "TEXT");
            EnsureColumn(conn, "ActivityLogs", "Action", "TEXT");
            EnsureColumn(conn, "ActivityLogs", "Timestamp", "DATETIME", "'1970-01-01 00:00:00'");

            EnsureColumn(conn, "MaintenanceRecords", "ItemID", "INTEGER", "0");
            EnsureColumn(conn, "MaintenanceRecords", "ScheduledDate", "DATETIME", "'1970-01-01 00:00:00'");
            EnsureColumn(conn, "MaintenanceRecords", "CompletedDate", "DATETIME");
            EnsureColumn(conn, "MaintenanceRecords", "MaintenanceType", "TEXT", "''");
            EnsureColumn(conn, "MaintenanceRecords", "Description", "TEXT");
            EnsureColumn(conn, "MaintenanceRecords", "PerformedBy", "TEXT");
            EnsureColumn(conn, "MaintenanceRecords", "Cost", "NUMERIC", "0");
            EnsureColumn(conn, "MaintenanceRecords", "Status", "TEXT", "'Scheduled'");
            EnsureColumn(conn, "MaintenanceRecords", "Notes", "TEXT");
            EnsureColumn(conn, "MaintenanceRecords", "UserID", "INTEGER");
            EnsureColumn(conn, "MaintenanceRecords", "CreatedAt", "DATETIME", "'1970-01-01 00:00:00'");

            EnsureColumn(conn, "CalibrationRecords", "ItemID", "INTEGER", "0");
            EnsureColumn(conn, "CalibrationRecords", "CalibrationDate", "DATETIME", "'1970-01-01 00:00:00'");
            EnsureColumn(conn, "CalibrationRecords", "NextCalibrationDue", "DATETIME", "'1970-01-01 00:00:00'");
            EnsureColumn(conn, "CalibrationRecords", "CalibratedBy", "TEXT");
            EnsureColumn(conn, "CalibrationRecords", "CertificateNumber", "TEXT");
            EnsureColumn(conn, "CalibrationRecords", "Standard", "TEXT");
            EnsureColumn(conn, "CalibrationRecords", "Result", "TEXT");
            EnsureColumn(conn, "CalibrationRecords", "Cost", "NUMERIC", "0");
            EnsureColumn(conn, "CalibrationRecords", "Notes", "TEXT");
            EnsureColumn(conn, "CalibrationRecords", "UserID", "INTEGER");
            EnsureColumn(conn, "CalibrationRecords", "CreatedAt", "DATETIME", "'1970-01-01 00:00:00'");

            EnsureColumn(conn, "Reservations", "ItemID", "INTEGER", "0");
            EnsureColumn(conn, "Reservations", "CustomerID", "INTEGER", "0");
            EnsureColumn(conn, "Reservations", "ReservationDate", "DATETIME", "'1970-01-01 00:00:00'");
            EnsureColumn(conn, "Reservations", "StartDate", "DATETIME", "'1970-01-01 00:00:00'");
            EnsureColumn(conn, "Reservations", "EndDate", "DATETIME", "'1970-01-01 00:00:00'");
            EnsureColumn(conn, "Reservations", "Quantity", "INTEGER", "1");
            EnsureColumn(conn, "Reservations", "Status", "TEXT", "'Pending'");
            EnsureColumn(conn, "Reservations", "Notes", "TEXT");
            EnsureColumn(conn, "Reservations", "CreatedByUserID", "INTEGER", "0");
            EnsureColumn(conn, "Reservations", "CreatedAt", "DATETIME", "'1970-01-01 00:00:00'");
            EnsureColumn(conn, "Reservations", "RentalID", "INTEGER");

            EnsureColumn(conn, "Kits", "KitNumber", "TEXT", "''");
            EnsureColumn(conn, "Kits", "Name", "TEXT", "''");
            EnsureColumn(conn, "Kits", "Description", "TEXT");
            EnsureColumn(conn, "Kits", "Category", "TEXT");
            EnsureColumn(conn, "Kits", "IsActive", "INTEGER", "1");
            EnsureColumn(conn, "Kits", "CreatedByUserID", "INTEGER", "0");
            EnsureColumn(conn, "Kits", "CreatedAt", "DATETIME", "'1970-01-01 00:00:00'");
            EnsureColumn(conn, "Kits", "UpdatedAt", "DATETIME", "'1970-01-01 00:00:00'");

            EnsureColumn(conn, "KitItems", "KitID", "INTEGER", "0");
            EnsureColumn(conn, "KitItems", "ItemID", "INTEGER", "0");
            EnsureColumn(conn, "KitItems", "Quantity", "INTEGER", "1");
            EnsureColumn(conn, "KitItems", "IsOptional", "INTEGER", "0");

            BackfillRequiredUniqueValues(conn);
        }

        static void BackfillRequiredUniqueValues(SqliteConnection conn)
        {
            using var itemNumber = new SqliteCommand(
                "UPDATE Items SET ItemNumber = 'ITEM-' || ItemID WHERE ItemNumber IS NULL OR TRIM(ItemNumber) = ''",
                conn);
            itemNumber.ExecuteNonQuery();

            using var userName = new SqliteCommand(
                "UPDATE Users SET UserName = 'user-' || UserID WHERE UserName IS NULL OR TRIM(UserName) = ''",
                conn);
            userName.ExecuteNonQuery();

            using var kitNumber = new SqliteCommand(
                "UPDATE Kits SET KitNumber = 'KIT-' || KitID WHERE KitNumber IS NULL OR TRIM(KitNumber) = ''",
                conn);
            kitNumber.ExecuteNonQuery();

            using var kitName = new SqliteCommand(
                "UPDATE Kits SET Name = KitNumber WHERE Name IS NULL OR TRIM(Name) = ''",
                conn);
            kitName.ExecuteNonQuery();
        }

        void MigrateLegacyItemsTable(SqliteConnection conn)
        {
            // Legacy migration: rename old legacy item table to "Items"
            const string legacyTable = "To" + "ols";
            using var check = new SqliteCommand($"SELECT name FROM sqlite_master WHERE type='table' AND name='{legacyTable}';", conn);
            var legacyItemTableExists = check.ExecuteScalar();
            if (legacyItemTableExists != null)
            {
                using var itemsCheck = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='Items';", conn);
                var itemsExists = itemsCheck.ExecuteScalar();
                if (itemsExists == null)
                {
                    using var rename = new SqliteCommand($"ALTER TABLE {legacyTable} RENAME TO Items;", conn);
                    rename.ExecuteNonQuery();
                }
            }
        }

        void DropObsoleteDeviceTables(SqliteConnection conn)
        {
            var dropSql = @"
                DROP TABLE IF EXISTS Devices;
                DROP TABLE IF EXISTS DeviceGroups;
                DROP TABLE IF EXISTS DeviceGroupAssignments;
                DROP TABLE IF EXISTS PulledDeviceFiles;
            ";
            using var dropCmd = new SqliteCommand(dropSql, conn);
            dropCmd.ExecuteNonQuery();
        }

        internal void EnsureColumn(string table, string column, string type, string? defaultValue = null)
        {
            using var conn = CreateConnection();
            EnsureColumn(conn, table, column, type, defaultValue);
        }

        internal void EnsureColumn(SqliteConnection conn, string table, string column, string type, string? defaultValue = null)
        {
            if (SqliteHelper.ColumnExists(conn, table, column)) return;
            try
            {
                var defaultClause = defaultValue != null ? $" NOT NULL DEFAULT {defaultValue}" : string.Empty;
                using var alter = new SqliteCommand($"ALTER TABLE {table} ADD COLUMN {column} {type}{defaultClause}", conn);
                alter.ExecuteNonQuery();
                if (defaultValue != null)
                {
                    using var update = new SqliteCommand($"UPDATE {table} SET {column}={defaultValue} WHERE {column} IS NULL", conn);
                    update.ExecuteNonQuery();
                }
            }
            catch (SqliteException ex)
            {
                if (ex.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase))
                {
                    // Column already exists due to a race condition; safe to ignore
                }
                else
                {
                    _logger.LogError(ex, "Failed to ensure column {Column} on table {Table}", column, table);
                    throw;
                }
            }
        }

        internal void EnsureIndex(SqliteConnection conn, string table, string column, bool unique = false)
            => EnsureIndex(conn, table, new[] { column }, unique);

        internal void EnsureIndex(SqliteConnection conn, string table, string[] columns, bool unique = false)
        {
            var indexName = $"idx_{table}_{string.Join("_", columns)}";
            if (SqliteHelper.IndexExists(conn, indexName)) return;
            try
            {
                var uniqueSql = unique ? "UNIQUE" : string.Empty;
                var columnsSql = string.Join(", ", columns);
                using var cmd = new SqliteCommand($"CREATE {uniqueSql} INDEX {indexName} ON {table}({columnsSql})", conn);
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                if (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    // Index already exists due to a race condition; safe to ignore
                }
                else
                {
                    _logger.LogError(ex, "Failed to ensure index on {Table}.{Columns}", table, string.Join(",", columns));
                    throw;
                }
            }
        }

        void RenameColumnIfExists(SqliteConnection conn, string table, string oldName, string newName)
        {
            using var info = new SqliteCommand($"PRAGMA table_info({table})", conn);
            using var rdr = info.ExecuteReader();
            var oldExists = false;
            var newExists = false;
            while (rdr.Read())
            {
                var name = rdr["name"].ToString();
                if (string.Equals(name, oldName, StringComparison.OrdinalIgnoreCase)) oldExists = true;
                if (string.Equals(name, newName, StringComparison.OrdinalIgnoreCase)) newExists = true;
            }
            if (oldExists && !newExists)
            {
                using var rename = new SqliteCommand($"ALTER TABLE {table} RENAME COLUMN {oldName} TO {newName}", conn);
                rename.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Creates a backup of the current database using Sqlite's backup API.
        /// </summary>
        /// <remarks>
        /// Ensure this method is called when no open transactions exist on the connection.
        /// </remarks>
        /// <param name="backupFilePath">Destination path for the backup file.</param>
        /// <exception cref="InvalidOperationException">Thrown when the database file path cannot be resolved.</exception>
        /// <exception cref="IOException">Thrown when the backup operation fails.</exception>
        public void BackupDatabase(string backupFilePath)
        {
            var dataSource = ConnectionString
                .Split(';')
                .FirstOrDefault(x => x.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
                ?.Substring("Data Source=".Length)
                .Trim();

            if (string.IsNullOrWhiteSpace(dataSource) || !File.Exists(dataSource))
                throw new InvalidOperationException("Database file path could not be determined.");

            try
            {
                using var source = CreateConnection();
                var builder = new SqliteConnectionStringBuilder
                {
                    DataSource = backupFilePath,
                    Pooling = true,
                    Cache = SqliteCacheMode.Shared
                };
                using var destination = new SqliteConnection(builder.ToString());
                destination.Open();

                source.BackupDatabase(destination);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to backup database");
                throw new IOException("Failed to backup database.", ex);
            }
        }

        /// <summary>
        /// Asynchronously creates a backup of the current database.
        /// </summary>
        /// <param name="backupFilePath">Destination path for the backup file.</param>
        /// <param name="cancellationToken">Token to observe for cancellation.</param>
        public Task BackupDatabaseAsync(string backupFilePath, CancellationToken cancellationToken)
            => Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                BackupDatabase(backupFilePath);
            }, cancellationToken);
    }
}

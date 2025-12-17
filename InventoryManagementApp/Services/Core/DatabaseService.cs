using Microsoft.Data.Sqlite;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Services.Core
{
    /// <summary>
    /// Provides Sqlite database access for the application. Instances should be
    /// disposed when no longer needed to release pooled connections.
    /// </summary>
    public class DatabaseService : IDisposable, IDatabaseBackupService, IDatabaseService
    {
        public string ConnectionString { get; }
        private readonly ILogger<DatabaseService> _logger;
        bool _disposed;
        
        private const int DefaultTimeoutSeconds = 5;
        private const int BusyTimeoutMilliseconds = 5000;

        public SqliteConnection CreateConnection()
        {
            var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public DatabaseService(string dbPath, ILogger<DatabaseService>? logger = null)
        {
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Pooling = true,
                Cache = SqliteCacheMode.Shared,
                DefaultTimeout = DefaultTimeoutSeconds
            };
            ConnectionString = builder.ToString();
            _logger = logger ?? NullLogger<DatabaseService>.Instance;
            ConfigureDatabase();
            InitializeDatabase();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                SqliteConnection.ClearAllPools();
            }
            _disposed = true;
        }

        ~DatabaseService()
        {
            Dispose(false);
        }

        void ConfigureDatabase()
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = new SqliteCommand("PRAGMA journal_mode=WAL;", conn);
            cmd.ExecuteNonQuery();
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
                    AvailableQuantity INTEGER NOT NULL DEFAULT 0,
                    RentedQuantity INTEGER NOT NULL DEFAULT 0,
                    IsRentalItem INTEGER NOT NULL DEFAULT 0,
                    Price NUMERIC NOT NULL DEFAULT 0,
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
                    PasswordExpired INTEGER NOT NULL DEFAULT 0
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
                );
                CREATE TABLE IF NOT EXISTS Vehicles (
                    VehicleID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Vin TEXT NOT NULL,
                    StockNumber TEXT,
                    Year INTEGER,
                    Make TEXT,
                    Model TEXT,
                    Trim TEXT,
                    IntakeDate DATETIME NOT NULL,
                    Status TEXT NOT NULL DEFAULT 'Received',
                    Location TEXT,
                    Mileage INTEGER,
                    FuelType TEXT,
                    DriveTrain TEXT,
                    Disposition TEXT,
                    Notes TEXT,
                    ComplianceHoldReason TEXT,
                    CreatedByUserID INTEGER,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (CreatedByUserID) REFERENCES Users(UserID)
                );
                CREATE TABLE IF NOT EXISTS DismantlingTasks (
                    TaskID INTEGER PRIMARY KEY AUTOINCREMENT,
                    VehicleID INTEGER NOT NULL,
                    PartName TEXT NOT NULL,
                    PartTag TEXT,
                    ConditionGrade TEXT,
                    Technician TEXT,
                    StartedAt DATETIME,
                    CompletedAt DATETIME,
                    Status TEXT NOT NULL DEFAULT 'Pending',
                    Notes TEXT,
                    ContainsHazmat INTEGER NOT NULL DEFAULT 0,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (VehicleID) REFERENCES Vehicles(VehicleID)
                );";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.ExecuteNonQuery();
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
            EnsureIndex(conn, "Vehicles", "Vin", true);
            EnsureIndex(conn, "Vehicles", "Status");
            EnsureIndex(conn, "Vehicles", "StockNumber");
            EnsureIndex(conn, "DismantlingTasks", "VehicleID");
            EnsureIndex(conn, "DismantlingTasks", "Status");
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
            if (SqliteHelper.ColumnExists(ConnectionString, table, column)) return;
            try
            {
                using var conn = CreateConnection();
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

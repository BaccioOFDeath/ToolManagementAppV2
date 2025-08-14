using System.Data.SQLite;
using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.Services.Core
{
    /// <summary>
    /// Provides SQLite database access for the application. Instances should be
    /// disposed when no longer needed to release pooled connections.
    /// </summary>
    public class DatabaseService : IDisposable, IDatabaseBackupService
    {
        public string ConnectionString { get; }
        private readonly ILogger<DatabaseService> _logger;
        bool _disposed;

        public SQLiteConnection CreateConnection()
        {
            var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public DatabaseService(string dbPath, ILogger<DatabaseService>? logger = null)
        {
            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = dbPath,
                Version = 3,
                Pooling = true,
                BusyTimeout = 5000,
                JournalMode = SQLiteJournalModeEnum.Wal
            };
            builder["Cache"] = "Shared";
            ConnectionString = builder.ToString();
            _logger = logger ?? NullLogger<DatabaseService>.Instance;
            ConfigureDatabase();
            InitializeDatabase();
            EnsureColumn("Tools", "ToolNumber", "TEXT");
            EnsureColumn("Tools", "NameDescription", "TEXT");
            EnsureColumn("Tools", "ToolImagePath", "TEXT");
            EnsureColumn("Tools", "CheckedOutBy", "TEXT");
            EnsureColumn("Tools", "CheckedOutTime", "DATETIME");
            EnsureColumn("Tools", "Keywords", "TEXT");
            EnsureColumn("Tools", "IsPowerTool", "INTEGER", "0");
            EnsureColumn("Tools", "IsCheckedOut", "INTEGER", "0");
            EnsureColumn("Users", "Password", "TEXT");
            EnsureColumn("Users", "Salt", "TEXT");
            EnsureColumn("Users", "Email", "TEXT");
            EnsureColumn("Users", "Phone", "TEXT");
            EnsureColumn("Users", "Mobile", "TEXT");
            EnsureColumn("Users", "Address", "TEXT");
            EnsureColumn("Users", "Role", "TEXT");
            EnsureColumn("Users", "IsActive", "INTEGER", "1");
            EnsureColumn("Users", "CreatedAt", "DATETIME");
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
                SQLiteConnection.ClearAllPools();
            }
            _disposed = true;
        }

        ~DatabaseService()
        {
            Dispose(false);
        }

        void ConfigureDatabase()
        {
            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
            using var cmd = new SQLiteCommand("PRAGMA journal_mode=WAL;", conn);
            cmd.ExecuteNonQuery();
            using var timeout = new SQLiteCommand("PRAGMA busy_timeout=5000;", conn);
            timeout.ExecuteNonQuery();
        }

        void InitializeDatabase()
        {
            using var conn = CreateConnection();
            var sql = @"
                CREATE TABLE IF NOT EXISTS Tools (
                    ToolID INTEGER PRIMARY KEY AUTOINCREMENT,
                    ToolNumber TEXT NOT NULL,
                    NameDescription TEXT,
                    Location TEXT,
                    Brand TEXT,
                    PartNumber TEXT,
                    Supplier TEXT,
                    PurchasedDate DATETIME,
                    Notes TEXT,
                    AvailableQuantity INTEGER NOT NULL DEFAULT 0,
                    RentedQuantity INTEGER NOT NULL DEFAULT 0,
                    IsPowerTool INTEGER NOT NULL DEFAULT 0,
                    IsCheckedOut INTEGER NOT NULL DEFAULT 0,
                    CheckedOutBy TEXT,
                    CheckedOutTime DATETIME
                );
                CREATE TABLE IF NOT EXISTS Users (
                    UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserName TEXT NOT NULL,
                    UserPhotoPath TEXT,
                    Password TEXT,
                    Salt TEXT,
                    IsAdmin INTEGER NOT NULL DEFAULT 0,
                    Email TEXT,
                    Phone TEXT,
                    Mobile TEXT,
                    Address TEXT,
                    Role TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
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
                    ToolID INTEGER NOT NULL,
                    CustomerID INTEGER NOT NULL,
                    RentalDate DATETIME NOT NULL,
                    DueDate DATETIME NOT NULL,
                    ReturnDate DATETIME,
                    Status TEXT NOT NULL DEFAULT 'Rented',
                    FOREIGN KEY (ToolID) REFERENCES Tools(ToolID),
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
                );";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.ExecuteNonQuery();

            EnsureIndex(conn, "Tools", "ToolNumber", true);
            EnsureIndex(conn, "Tools", "NameDescription");
            EnsureIndex(conn, "Tools", "Brand");
            EnsureIndex(conn, "Tools", "PartNumber");
            EnsureIndex(conn, "Tools", "Supplier");
            EnsureIndex(conn, "Tools", "Location");
            EnsureIndex(conn, "Tools", "Notes");
            EnsureIndex(conn, "Tools", "Keywords");
            EnsureIndex(conn, "Users", "UserName");
        }

        void EnsureColumn(string table, string column, string type, string? defaultValue = null)
        {
            if (SqliteHelper.ColumnExists(ConnectionString, table, column)) return;
            try
            {
                using var conn = CreateConnection();
                var defaultClause = defaultValue != null ? $" NOT NULL DEFAULT {defaultValue}" : string.Empty;
                using var alter = new SQLiteCommand($"ALTER TABLE {table} ADD COLUMN {column} {type}{defaultClause}", conn);
                alter.ExecuteNonQuery();
                if (defaultValue != null)
                {
                    using var update = new SQLiteCommand($"UPDATE {table} SET {column}={defaultValue} WHERE {column} IS NULL", conn);
                    update.ExecuteNonQuery();
                }
            }
            catch (SQLiteException ex)
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

        void EnsureIndex(SQLiteConnection conn, string table, string column, bool unique = false)
        {
            var indexName = $"idx_{table}_{column}";
            if (SqliteHelper.IndexExists(conn, indexName)) return;
            try
            {
                var uniqueSql = unique ? "UNIQUE" : string.Empty;
                using var cmd = new SQLiteCommand($"CREATE {uniqueSql} INDEX {indexName} ON {table}({column})", conn);
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                if (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    // Index already exists due to a race condition; safe to ignore
                }
                else
                {
                    _logger.LogError(ex, "Failed to ensure index on {Table}.{Column}", table, column);
                    throw;
                }
            }
        }

        /// <summary>
        /// Creates a backup of the current database using SQLite's backup API.
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

            if (string.IsNullOrEmpty(dataSource) || !File.Exists(dataSource))
                throw new InvalidOperationException("Database file path could not be determined.");

            try
            {
                using var source = CreateConnection();
                using var destination = new SQLiteConnection($"Data Source={backupFilePath};Version=3;Pooling=True;Cache=Shared;");
                destination.Open();

                source.BackupDatabase(destination, "main", "main", -1, null, 0);
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
        public Task BackupDatabaseAsync(string backupFilePath)
            => Task.Run(() => BackupDatabase(backupFilePath));
    }
}

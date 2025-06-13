using System.Data.SQLite;
using System.IO;

namespace ToolManagementAppV2.Services.Core
{
    public class DatabaseService
    {
        public string ConnectionString { get; }

        public DatabaseService(string dbPath)
        {
            ConnectionString = $"Data Source={dbPath};Version=3;";
            InitializeDatabase();
            EnsureColumn("Tools", "ToolNumber", "TEXT");
            EnsureColumn("Tools", "NameDescription", "TEXT");
            EnsureColumn("Tools", "ToolImagePath", "TEXT");
            EnsureColumn("Tools", "CheckedOutBy", "TEXT");
            EnsureColumn("Tools", "CheckedOutTime", "DATETIME");
            EnsureColumn("Tools", "Keywords", "TEXT");
            EnsureColumn("Users", "Password", "TEXT");
            EnsureColumn("Users", "Email", "TEXT");
            EnsureColumn("Users", "Phone", "TEXT");
            EnsureColumn("Users", "Mobile", "TEXT");
            EnsureColumn("Users", "Address", "TEXT");
            EnsureColumn("Users", "Role", "TEXT");
        }

        void InitializeDatabase()
        {
            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
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
                    IsCheckedOut INTEGER NOT NULL DEFAULT 0,
                    CheckedOutBy TEXT,
                    CheckedOutTime DATETIME
                );
                CREATE TABLE IF NOT EXISTS Users (
                    UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserName TEXT NOT NULL,
                    UserPhotoPath TEXT,
                    IsAdmin INTEGER NOT NULL DEFAULT 0
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
        }

        void EnsureColumn(string table, string column, string type)
        {
            if (SqliteHelper.ColumnExists(ConnectionString, table, column)) return;
            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
            using var alter = new SQLiteCommand($"ALTER TABLE {table} ADD COLUMN {column} {type}", conn);
            alter.ExecuteNonQuery();
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
                using var source = new SQLiteConnection(ConnectionString);
                using var destination = new SQLiteConnection($"Data Source={backupFilePath};Version=3;");
                source.Open();
                destination.Open();

                source.BackupDatabase(destination, "main", "main", -1, null, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw new IOException("Failed to backup database.", ex);
            }
        }
    }
}

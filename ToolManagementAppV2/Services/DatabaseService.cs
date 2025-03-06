using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace ToolManagementAppV2.Services
{
    public class DatabaseService
    {
        public string ConnectionString { get; }

        public DatabaseService(string dbPath)
        {
            ConnectionString = $"Data Source={dbPath};Version=3;";
            InitializeDatabase();
            UpdateDatabaseSchema();
        }

        private void InitializeDatabase()
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            var createTables = @"
                -- Tools Table
                CREATE TABLE IF NOT EXISTS Tools (
                    ToolID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT,
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

                -- Users Table
                CREATE TABLE IF NOT EXISTS Users (
                    UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserName TEXT NOT NULL,
                    UserPhotoPath TEXT,
                    IsAdmin INTEGER NOT NULL DEFAULT 0
                );

                -- Customers Table
                CREATE TABLE IF NOT EXISTS Customers (
                    CustomerID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Email TEXT,
                    Contact TEXT,
                    Phone TEXT,
                    Address TEXT
                );

                -- Rentals Table
                CREATE TABLE IF NOT EXISTS Rentals (
                    RentalID INTEGER PRIMARY KEY AUTOINCREMENT,
                    ToolID INTEGER NOT NULL,
                    CustomerID INTEGER NOT NULL,
                    RentalDate DATETIME NOT NULL,
                    DueDate DATETIME NOT NULL,
                    ReturnDate DATETIME,
                    Status TEXT NOT NULL DEFAULT 'Rented',
                    FOREIGN KEY (ToolID) REFERENCES Tools (ToolID),
                    FOREIGN KEY (CustomerID) REFERENCES Customers (CustomerID)
                );

                -- Activity Logs Table
                CREATE TABLE IF NOT EXISTS ActivityLogs (
                    LogID INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserID INTEGER,
                    UserName TEXT,
                    Action TEXT,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserID) REFERENCES Users (UserID)
                );

                -- Settings Table for configuration options
                CREATE TABLE IF NOT EXISTS Settings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT
                );
            ";

            using var command = new SQLiteCommand(createTables, connection);
            command.ExecuteNonQuery();
        }

        private void UpdateDatabaseSchema()
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            bool columnExists = false;
            using (var command = new SQLiteCommand("PRAGMA table_info(Tools)", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader["name"].ToString().Equals("ToolImagePath", StringComparison.OrdinalIgnoreCase))
                    {
                        columnExists = true;
                        break;
                    }
                }
            }

            if (!columnExists)
            {
                using var command = new SQLiteCommand("ALTER TABLE Tools ADD COLUMN ToolImagePath TEXT", connection);
                command.ExecuteNonQuery();
            }
        }

        public void BackupDatabase(string backupFilePath)
        {
            var dataSourcePart = ConnectionString.Split(';').FirstOrDefault(s => s.StartsWith("Data Source="));
            if (dataSourcePart != null)
            {
                string dbFilePath = dataSourcePart.Replace("Data Source=", "").Trim();
                File.Copy(dbFilePath, backupFilePath, true);
            }
            else
            {
                throw new InvalidOperationException("Database file path could not be determined.");
            }
        }
    }
}

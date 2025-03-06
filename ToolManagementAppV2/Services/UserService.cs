using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows.Media.Imaging;
using ToolManagementAppV2.Models;

namespace ToolManagementAppV2.Services
{
    public class UserService
    {
        private readonly DatabaseService _dbService;

        public UserService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        // Retrieve all users from the database
        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = "SELECT * FROM Users";
            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var user = new User
                {
                    UserID = Convert.ToInt32(reader["UserID"]),
                    UserName = reader["UserName"].ToString(),
                    UserPhotoPath = reader["UserPhotoPath"].ToString(),
                    IsAdmin = reader["IsAdmin"].ToString() == "1"
                };

                // Load the bitmap image from the photo path
                if (!string.IsNullOrEmpty(user.UserPhotoPath) && File.Exists(user.UserPhotoPath))
                {
                    user.PhotoBitmap = new BitmapImage(new Uri(user.UserPhotoPath));
                }

                users.Add(user);
            }

            return users;
        }


        // Add a new user to the database
        public void AddUser(User user)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = @"
            INSERT INTO Users (UserName, UserPhotoPath, IsAdmin)
            VALUES (@UserName, @UserPhotoPath, @IsAdmin)";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@UserName", user.UserName);
            command.Parameters.AddWithValue("@UserPhotoPath", user.UserPhotoPath ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsAdmin", user.IsAdmin ? 1 : 0); // SQLite uses 1/0 for booleans
            command.ExecuteNonQuery();
        }

        // Update an existing user's information
        public void UpdateUser(User user)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = @"
            UPDATE Users 
            SET UserName = @UserName, UserPhotoPath = @UserPhotoPath, IsAdmin = @IsAdmin
            WHERE UserID = @UserID";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@UserID", user.UserID);
            command.Parameters.AddWithValue("@UserName", user.UserName);
            command.Parameters.AddWithValue("@UserPhotoPath", user.UserPhotoPath ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsAdmin", user.IsAdmin ? 1 : 0);
            command.ExecuteNonQuery();
        }

        // Delete a user from the database
        public void DeleteUser(int userID)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = "DELETE FROM Users WHERE UserID = @UserID";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@UserID", userID);
            command.ExecuteNonQuery();
        }

        public User GetCurrentUser()
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = "SELECT * FROM Users LIMIT 1"; // Update this query based on your logic
            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new User
                {
                    UserID = Convert.ToInt32(reader["UserID"]),
                    UserName = reader["UserName"].ToString(),
                    UserPhotoPath = reader["UserPhotoPath"].ToString(),
                    IsAdmin = reader["IsAdmin"].ToString() == "1"
                };
            }

            return null; // Return null if no user is found
        }

        public User GetUserByName(string userName)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "SELECT * FROM Users WHERE UserName = @UserName";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@UserName", userName);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    UserID = Convert.ToInt32(reader["UserID"]),
                    UserName = reader["UserName"].ToString(),
                    UserPhotoPath = reader["UserPhotoPath"].ToString(),
                    IsAdmin = Convert.ToInt32(reader["IsAdmin"]) == 1
                };
            }
            return null;
        }

        public User GetUserByID(int userID)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "SELECT * FROM Users WHERE UserID = @UserID";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@UserID", userID);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    UserID = Convert.ToInt32(reader["UserID"]),
                    UserName = reader["UserName"].ToString(),
                    UserPhotoPath = reader["UserPhotoPath"].ToString(),
                    IsAdmin = Convert.ToInt32(reader["IsAdmin"]) == 1
                };
            }
            return null;
        }

        public List<Rental> GetRentalsForCustomer(int customerID)
        {
            var rentals = new List<Rental>();
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "SELECT * FROM Rentals WHERE CustomerID = @CustomerID ORDER BY RentalDate DESC";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerID", customerID);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                rentals.Add(new Rental
                {
                    RentalID = Convert.ToInt32(reader["RentalID"]),
                    ToolID = Convert.ToInt32(reader["ToolID"]),
                    CustomerID = Convert.ToInt32(reader["CustomerID"]),
                    RentalDate = Convert.ToDateTime(reader["RentalDate"]),
                    DueDate = Convert.ToDateTime(reader["DueDate"]),
                    ReturnDate = reader["ReturnDate"] is DBNull ? null : Convert.ToDateTime(reader["ReturnDate"]),
                    Status = reader["Status"].ToString()
                });
            }
            return rentals;
        }

        public void ExtendRental(int rentalID, DateTime newDueDate)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "UPDATE Rentals SET DueDate = @NewDueDate WHERE RentalID = @RentalID AND Status = 'Rented'";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@NewDueDate", newDueDate);
            command.Parameters.AddWithValue("@RentalID", rentalID);
            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected == 0)
                throw new InvalidOperationException("Unable to extend rental. Rental not found or already returned.");
        }

    }
}

// File: Services/UserService.cs
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
                    Password = reader["Password"].ToString(),
                    UserPhotoPath = reader["UserPhotoPath"].ToString(),
                    IsAdmin = Convert.ToInt32(reader["IsAdmin"]) == 1
                };

                if (!string.IsNullOrEmpty(user.UserPhotoPath) && File.Exists(user.UserPhotoPath))
                {
                    user.PhotoBitmap = new BitmapImage(new Uri(user.UserPhotoPath));
                }
                users.Add(user);
            }

            return users;
        }

        public void AddUser(User user)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = @"
            INSERT INTO Users (UserName, Password, UserPhotoPath, IsAdmin)
            VALUES (@UserName, @Password, @UserPhotoPath, @IsAdmin)";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@UserName", user.UserName);
            command.Parameters.AddWithValue("@Password", user.Password ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@UserPhotoPath", user.UserPhotoPath ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsAdmin", user.IsAdmin ? 1 : 0);
            command.ExecuteNonQuery();
        }

        public void UpdateUser(User user)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = @"
            UPDATE Users 
            SET UserName = @UserName, Password = @Password, UserPhotoPath = @UserPhotoPath, IsAdmin = @IsAdmin
            WHERE UserID = @UserID";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@UserID", user.UserID);
            command.Parameters.AddWithValue("@UserName", user.UserName);
            command.Parameters.AddWithValue("@Password", user.Password ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@UserPhotoPath", user.UserPhotoPath ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsAdmin", user.IsAdmin ? 1 : 0);
            command.ExecuteNonQuery();
        }

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

            var query = "SELECT * FROM Users LIMIT 1";
            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new User
                {
                    UserID = Convert.ToInt32(reader["UserID"]),
                    UserName = reader["UserName"].ToString(),
                    Password = reader["Password"].ToString(),
                    UserPhotoPath = reader["UserPhotoPath"].ToString(),
                    IsAdmin = Convert.ToInt32(reader["IsAdmin"]) == 1
                };
            }
            return null;
        }

        public User AuthenticateUser(string userName, string password)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = "SELECT * FROM Users WHERE UserName = @UserName AND Password = @Password";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@UserName", userName);
            command.Parameters.AddWithValue("@Password", password);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new User
                {
                    UserID = Convert.ToInt32(reader["UserID"]),
                    UserName = reader["UserName"].ToString(),
                    Password = reader["Password"].ToString(),
                    UserPhotoPath = reader["UserPhotoPath"].ToString(),
                    IsAdmin = Convert.ToInt32(reader["IsAdmin"]) == 1
                };
            }
            return null;
        }

        public void ChangeUserPassword(int userID, string newPassword)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = "UPDATE Users SET Password = @Password WHERE UserID = @UserID";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Password", newPassword);
            command.Parameters.AddWithValue("@UserID", userID);
            command.ExecuteNonQuery();
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
                    Password = reader["Password"].ToString(),
                    UserPhotoPath = reader["UserPhotoPath"].ToString(),
                    IsAdmin = Convert.ToInt32(reader["IsAdmin"]) == 1
                };
            }
            return null;
        }
    }
}

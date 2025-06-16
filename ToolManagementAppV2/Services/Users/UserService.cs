using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Media.Imaging;
using System.Data;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.Services.Users
{
    public class UserService : IUserService
    {
        readonly DatabaseService _dbService;

        public UserService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public List<User> GetAllUsers()
        {
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReader(conn, "SELECT * FROM Users", null, MapUser);
        }

        public User GetUserByID(int userID)
        {
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReader(conn, "SELECT * FROM Users WHERE UserID=@ID",
                new[] { new SQLiteParameter("@ID", userID) }, MapUser).FirstOrDefault();
        }

        public User AuthenticateUser(string userName, string password)
        {
            using var conn = _dbService.CreateConnection();
            var users = SqliteHelper.ExecuteReader(conn,
                "SELECT * FROM Users WHERE UserName=@UserName",
                new[] { new SQLiteParameter("@UserName", userName) }, MapUser);
            var u = users.FirstOrDefault();
            if (u == null) return null;
            var hashed = SecurityHelper.ComputeSha256Hash(password ?? string.Empty);
            return u.Password == hashed ? u : null;
        }

        public User GetCurrentUser()
        {
            if (System.Windows.Application.Current.Properties["CurrentUser"] is User u)
                return GetUserByID(u.UserID);
            return null;
        }

        public void AddUser(User user)
        {
            const string sql = @"
                INSERT INTO Users
                  (UserName, Password, UserPhotoPath, IsAdmin, Email, Phone, Address, Role)
                VALUES
                  (@UserName,@Password,@Photo,@Admin,@Email,@Phone,@Address,@Role);
                SELECT last_insert_rowid();";

            using var conn = _dbService.CreateConnection();
            using var cmd = new SQLiteCommand(sql, conn);

            var hashed = string.IsNullOrWhiteSpace(user.Password)
                ? string.Empty
                : SecurityHelper.IsSha256Hash(user.Password)
                    ? user.Password
                    : SecurityHelper.ComputeSha256Hash(user.Password);

            cmd.Parameters.AddRange(new[]
            {
                new SQLiteParameter("@UserName", user.UserName),
                new SQLiteParameter("@Password", hashed),
                new SQLiteParameter("@Photo",    (object)user.UserPhotoPath ?? DBNull.Value),
                new SQLiteParameter("@Admin",    user.IsAdmin ? 1 : 0),
                new SQLiteParameter("@Email",    (object)user.Email ?? DBNull.Value),
                new SQLiteParameter("@Phone",    (object)user.Phone ?? DBNull.Value),
                new SQLiteParameter("@Address",  (object)user.Address ?? DBNull.Value),
                new SQLiteParameter("@Role",     (object)user.Role ?? DBNull.Value)
            });
            user.UserID = Convert.ToInt32(cmd.ExecuteScalar());
            user.Password = hashed;
        }

        public void UpdateUser(User user)
        {
            const string sql = @"
                UPDATE Users SET
                  UserName      = @UserName,
                  Password      = @Password,
                  UserPhotoPath = @Photo,
                  IsAdmin       = @Admin,
                  Email         = @Email,
                  Phone         = @Phone,
                  Address       = @Address,
                  Role          = @Role
                WHERE UserID = @UserID";

            var hashed = string.IsNullOrWhiteSpace(user.Password)
                ? string.Empty
                : SecurityHelper.IsSha256Hash(user.Password)
                    ? user.Password
                    : SecurityHelper.ComputeSha256Hash(user.Password);

            var p = new[]
            {
                new SQLiteParameter("@UserID",   user.UserID),
                new SQLiteParameter("@UserName", user.UserName),
                new SQLiteParameter("@Password", hashed),
                new SQLiteParameter("@Photo",    (object)user.UserPhotoPath ?? DBNull.Value),
                new SQLiteParameter("@Admin",    user.IsAdmin ? 1 : 0),
                new SQLiteParameter("@Email",    (object)user.Email ?? DBNull.Value),
                new SQLiteParameter("@Phone",    (object)user.Phone ?? DBNull.Value),
                new SQLiteParameter("@Address",  (object)user.Address ?? DBNull.Value),
                new SQLiteParameter("@Role",     (object)user.Role ?? DBNull.Value)
            };

            using var conn = _dbService.CreateConnection();
            SqliteHelper.ExecuteNonQuery(conn, sql, p);
            user.Password = hashed;
        }

        public void ChangeUserPassword(int userID, string newPassword)
        {
            var sql = "UPDATE Users SET Password=@Pwd WHERE UserID=@ID";
            var hashed = string.IsNullOrWhiteSpace(newPassword)
                ? string.Empty
                : SecurityHelper.IsSha256Hash(newPassword)
                    ? newPassword
                    : SecurityHelper.ComputeSha256Hash(newPassword);

            var p = new[]
            {
                new SQLiteParameter("@Pwd", hashed),
                new SQLiteParameter("@ID",  userID)
            };
            using var conn = _dbService.CreateConnection();
            SqliteHelper.ExecuteNonQuery(conn, sql, p);
        }

        public bool TryDeleteUser(int userID)
        {
            var user = GetUserByID(userID);
            if (user == null) return false;
            if (user.IsAdmin)
            {
                var adminCount = GetAllUsers().Count(u => u.IsAdmin);
                if (adminCount <= 1) return false;
            }
            DeleteUserInternal(userID);
            return true;
        }

        public bool DeleteUser(int userID) => TryDeleteUser(userID);

        void DeleteUserInternal(int userID)
        {
            var sql = "DELETE FROM Users WHERE UserID=@ID";
            var p = new[] { new SQLiteParameter("@ID", userID) };
            using var conn = _dbService.CreateConnection();
            SqliteHelper.ExecuteNonQuery(conn, sql, p);
        }

        User MapUser(IDataRecord rdr)
        {
            var u = new User
            {
                UserID = Convert.ToInt32(rdr["UserID"]),
                UserName = rdr["UserName"].ToString(),
                Password = rdr["Password"].ToString(),
                UserPhotoPath = rdr["UserPhotoPath"]?.ToString(),
                IsAdmin = Convert.ToInt32(rdr["IsAdmin"]) == 1,
                Email = rdr["Email"]?.ToString(),
                Phone = rdr["Phone"]?.ToString(),
                Address = rdr["Address"]?.ToString(),
                Role = rdr["Role"]?.ToString()
            };

            try
            {
                if (!string.IsNullOrWhiteSpace(u.UserPhotoPath))
                {
                    Uri uri;
                    if (u.UserPhotoPath.StartsWith("pack://"))
                        uri = new Uri(u.UserPhotoPath, UriKind.Absolute);
                    else
                    {
                        var full = PathHelper.GetAbsolutePath(u.UserPhotoPath);
                        if (string.IsNullOrEmpty(full) || !File.Exists(full))
                        {
                            u.PhotoBitmap = null;
                            return u;
                        }
                        uri = new Uri(full, UriKind.Absolute);
                    }

                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = uri;
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bmp.EndInit();
                    u.PhotoBitmap = bmp;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                u.PhotoBitmap = null;
            }

            return u;
        }
    }
}

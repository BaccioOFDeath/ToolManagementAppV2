using System.Data.SQLite;
using System.IO;
using System.Windows.Media.Imaging;
using System.Data;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;

namespace ToolManagementAppV2.Services.Users
{
    public class UserService
    {
        readonly string _connString;

        public UserService(DatabaseService dbService) =>
            _connString = dbService.ConnectionString;

        public List<User> GetAllUsers() =>
            SqliteHelper.ExecuteReader(_connString, "SELECT * FROM Users", null, MapUser);

        public User GetUserByID(int userID) =>
            SqliteHelper.ExecuteReader(_connString, "SELECT * FROM Users WHERE UserID=@ID",
                new[] { new SQLiteParameter("@ID", userID) }, MapUser).FirstOrDefault();

        public User AuthenticateUser(string userName, string password)
        {
            var users = SqliteHelper.ExecuteReader(_connString,
                "SELECT * FROM Users WHERE UserName=@UserName",
                new[] { new SQLiteParameter("@UserName", userName) }, MapUser);
            var u = users.FirstOrDefault();
            return u != null && u.Password == password ? u : null;
        }

        public User GetCurrentUser() =>
            SqliteHelper.ExecuteReader(_connString, "SELECT * FROM Users LIMIT 1", null, MapUser).FirstOrDefault();

        public void AddUser(User user)
        {
            const string sql = @"
                INSERT INTO Users
                  (UserName, Password, UserPhotoPath, IsAdmin, Email, Phone, Address, Role)
                VALUES
                  (@UserName,@Password,@Photo,@Admin,@Email,@Phone,@Address,@Role);
                SELECT last_insert_rowid();";

            using var conn = new SQLiteConnection(_connString);
            conn.Open();
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddRange(new[]
            {
                new SQLiteParameter("@UserName", user.UserName),
                new SQLiteParameter("@Password", user.Password ?? string.Empty),
                new SQLiteParameter("@Photo",    (object)user.UserPhotoPath ?? DBNull.Value),
                new SQLiteParameter("@Admin",    user.IsAdmin ? 1 : 0),
                new SQLiteParameter("@Email",    (object)user.Email ?? DBNull.Value),
                new SQLiteParameter("@Phone",    (object)user.Phone ?? DBNull.Value),
                new SQLiteParameter("@Address",  (object)user.Address ?? DBNull.Value),
                new SQLiteParameter("@Role",     (object)user.Role ?? DBNull.Value)
            });
            user.UserID = Convert.ToInt32(cmd.ExecuteScalar());
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

            var p = new[]
            {
                new SQLiteParameter("@UserID",   user.UserID),
                new SQLiteParameter("@UserName", user.UserName),
                new SQLiteParameter("@Password", user.Password ?? string.Empty),
                new SQLiteParameter("@Photo",    (object)user.UserPhotoPath ?? DBNull.Value),
                new SQLiteParameter("@Admin",    user.IsAdmin ? 1 : 0),
                new SQLiteParameter("@Email",    (object)user.Email ?? DBNull.Value),
                new SQLiteParameter("@Phone",    (object)user.Phone ?? DBNull.Value),
                new SQLiteParameter("@Address",  (object)user.Address ?? DBNull.Value),
                new SQLiteParameter("@Role",     (object)user.Role ?? DBNull.Value)
            };
            SqliteHelper.ExecuteNonQuery(_connString, sql, p);
        }

        public void ChangeUserPassword(int userID, string newPassword)
        {
            var sql = "UPDATE Users SET Password=@Pwd WHERE UserID=@ID";
            var p = new[]
            {
                new SQLiteParameter("@Pwd", newPassword),
                new SQLiteParameter("@ID",  userID)
            };
            SqliteHelper.ExecuteNonQuery(_connString, sql, p);
        }

        public void DeleteUser(int userID)
        {
            var sql = "DELETE FROM Users WHERE UserID=@ID";
            var p = new[] { new SQLiteParameter("@ID", userID) };
            SqliteHelper.ExecuteNonQuery(_connString, sql, p);
        }

        User MapUser(IDataRecord rdr)
        {
            var u = new User
            {
                UserID = Convert.ToInt32(rdr["UserID"]),
                UserName = rdr["UserName"].ToString(),
                Password = rdr["Password"].ToString(),
                UserPhotoPath = rdr["UserPhotoPath"].ToString(),
                IsAdmin = Convert.ToInt32(rdr["IsAdmin"]) == 1,
                Email = rdr["Email"]?.ToString(),
                Phone = rdr["Phone"]?.ToString(),
                Address = rdr["Address"]?.ToString(),
                Role = rdr["Role"]?.ToString()
            };
            if (!string.IsNullOrEmpty(u.UserPhotoPath) && File.Exists(u.UserPhotoPath))
                u.PhotoBitmap = new BitmapImage(new Uri(u.UserPhotoPath));
            return u;
        }
    }
}

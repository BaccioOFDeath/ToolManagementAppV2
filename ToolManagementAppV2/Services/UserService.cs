using System.Data.SQLite;
using System.IO;
using System.Windows.Media.Imaging;
using ToolManagementAppV2.Models;

namespace ToolManagementAppV2.Services
{
    public class UserService
    {
        readonly string _connString;

        public UserService(DatabaseService dbService) =>
            _connString = dbService.ConnectionString;

        public List<User> GetAllUsers() =>
            ExecuteReader("SELECT * FROM Users", null);

        public User GetUserByID(int userID) =>
            ExecuteReader("SELECT * FROM Users WHERE UserID=@ID",
                new[] { new SQLiteParameter("@ID", userID) })
            .FirstOrDefault();

        public User AuthenticateUser(string userName, string password)
        {
            var users = ExecuteReader(
                "SELECT * FROM Users WHERE UserName=@UserName",
                new[] { new SQLiteParameter("@UserName", userName) }
            );
            var u = users.FirstOrDefault();
            return u != null && u.Password == password ? u : null;
        }

        public User GetCurrentUser() =>
            ExecuteReader("SELECT * FROM Users LIMIT 1", null)
            .FirstOrDefault();

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

            ExecuteNonQuery(sql, new[]
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
            });
        }

        public void ChangeUserPassword(int userID, string newPassword) =>
            ExecuteNonQuery(
                "UPDATE Users SET Password=@Pwd WHERE UserID=@ID",
                new[]
                {
                    new SQLiteParameter("@Pwd", newPassword),
                    new SQLiteParameter("@ID",  userID)
                });

        public void DeleteUser(int userID) =>
            ExecuteNonQuery(
                "DELETE FROM Users WHERE UserID=@ID",
                new[] { new SQLiteParameter("@ID", userID) });

        // --- Helpers ---
        List<User> ExecuteReader(string sql, SQLiteParameter[] parameters)
        {
            var list = new List<User>();
            using var conn = new SQLiteConnection(_connString);
            conn.Open();
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
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
                list.Add(u);
            }
            return list;
        }

        int ExecuteNonQuery(string sql, SQLiteParameter[] parameters)
        {
            using var conn = new SQLiteConnection(_connString);
            conn.Open();
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }
    }
}

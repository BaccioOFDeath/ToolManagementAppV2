using System;
using System.Data.SQLite;
using System.Data;
using System.Threading.Tasks;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Interfaces;
using System.Linq;

namespace ToolManagementAppV2.Services.Users
{
    public class UserService : IUserService
    {
        readonly DatabaseService _dbService;
        readonly IUserContext _context;

        public UserService(DatabaseService dbService, IUserContext context)
        {
            _dbService = dbService;
            _context = context;
        }

        public List<User> GetAllUsers()
        {
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReader(conn, "SELECT * FROM Users", null, MapUser);
        }

        public User? GetUserByID(int userID)
        {
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReader(conn, "SELECT * FROM Users WHERE UserID=@ID",
                new[] { new SQLiteParameter("@ID", userID) }, MapUser).FirstOrDefault();
        }

        public User? AuthenticateUser(string userName, string password)
        {
            using var conn = _dbService.CreateConnection();
            var users = SqliteHelper.ExecuteReader(conn,
                "SELECT * FROM Users WHERE UserName=@UserName",
                new[] { new SQLiteParameter("@UserName", userName) }, MapUser);
            var u = users.FirstOrDefault();
            if (u == null) return null;
            if (u.LockoutUntil.HasValue && u.LockoutUntil > DateTime.UtcNow)
                return null;
            if (u.LockoutUntil.HasValue && u.LockoutUntil <= DateTime.UtcNow)
            {
                var reset = new[]
                {
                    new SQLiteParameter("@Attempts", 0),
                    new SQLiteParameter("@Lockout", DBNull.Value),
                    new SQLiteParameter("@ID", u.UserID)
                };
                SqliteHelper.ExecuteNonQuery(conn, "UPDATE Users SET FailedAttempts=@Attempts, LockoutUntil=@Lockout WHERE UserID=@ID", reset);
                u.FailedAttempts = 0;
                u.LockoutUntil = null;
            }

            bool success;
            if (string.IsNullOrWhiteSpace(u.Salt) && SecurityHelper.IsSha256Hash(u.Password))
            {
                var legacy = SecurityHelper.ComputeSha256HashLegacy(password ?? string.Empty);
                success = u.Password == legacy;
                if (success)
                {
                    var upgraded = SecurityHelper.HashPassword(password ?? string.Empty, out var salt);
                    var p = new[]
                    {
                        new SQLiteParameter("@Pwd", upgraded),
                        new SQLiteParameter("@Salt", salt),
                        new SQLiteParameter("@ID", u.UserID)
                    };
                    SqliteHelper.ExecuteNonQuery(conn, "UPDATE Users SET Password=@Pwd, Salt=@Salt WHERE UserID=@ID", p);
                    u.Password = upgraded;
                    u.Salt = salt;
                }
            }
            else
            {
                success = SecurityHelper.VerifyPassword(password ?? string.Empty, u.Salt, u.Password);
            }

            if (success)
            {
                var reset = new[]
                {
                    new SQLiteParameter("@Attempts", 0),
                    new SQLiteParameter("@Lockout", DBNull.Value),
                    new SQLiteParameter("@ID", u.UserID)
                };
                SqliteHelper.ExecuteNonQuery(conn, "UPDATE Users SET FailedAttempts=@Attempts, LockoutUntil=@Lockout WHERE UserID=@ID", reset);
                u.FailedAttempts = 0;
                u.LockoutUntil = null;
                return u;
            }

            u.FailedAttempts++;
            DateTime? lockout = null;
            if (u.FailedAttempts >= 3)
                lockout = DateTime.UtcNow.AddMinutes(15);
            var update = new[]
            {
                new SQLiteParameter("@Attempts", u.FailedAttempts),
                new SQLiteParameter("@Lockout", (object?)lockout ?? DBNull.Value),
                new SQLiteParameter("@ID", u.UserID)
            };
            SqliteHelper.ExecuteNonQuery(conn, "UPDATE Users SET FailedAttempts=@Attempts, LockoutUntil=@Lockout WHERE UserID=@ID", update);
            u.LockoutUntil = lockout;
            return null;
        }

        public User? GetCurrentUser()
        {
            if (_context.CurrentUser is User u)
                return GetUserByID(u.UserID);
            return null;
        }

        public void AddUser(User user)
        {
            const string sql = @"
                INSERT INTO Users
                  (UserName, Password, Salt, UserPhotoPath, IsAdmin, Email, Phone, Mobile, Address, Role, IsActive, CreatedAt)
                VALUES
                  (@UserName,@Password,@Salt,@Photo,@Admin,@Email,@Phone,@Mobile,@Address,@Role,@IsActive,@CreatedAt);
                SELECT last_insert_rowid();";

            using var conn = _dbService.CreateConnection();
            using var cmd = new SQLiteCommand(sql, conn);

            string hashed = string.Empty;
            string salt = string.Empty;
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                if (!string.IsNullOrWhiteSpace(user.Salt))
                {
                    hashed = user.Password;
                    salt = user.Salt;
                }
                else
                {
                    hashed = SecurityHelper.HashPassword(user.Password, out salt);
                }
            }

            if (user.CreatedAt == default)
                user.CreatedAt = DateTime.UtcNow;
            cmd.Parameters.AddRange(new[]
            {
                new SQLiteParameter("@UserName", user.UserName),
                new SQLiteParameter("@Password", hashed),
                new SQLiteParameter("@Salt",     salt),
                new SQLiteParameter("@Photo",    (object)user.UserPhotoPath ?? DBNull.Value),
                new SQLiteParameter("@Admin",    user.IsAdmin ? 1 : 0),
                new SQLiteParameter("@Email",    (object)user.Email ?? DBNull.Value),
                new SQLiteParameter("@Phone",    (object)user.Phone ?? DBNull.Value),
                new SQLiteParameter("@Mobile",   (object)user.Mobile ?? DBNull.Value),
                new SQLiteParameter("@Address",  (object)user.Address ?? DBNull.Value),
                new SQLiteParameter("@Role",     (object)user.Role ?? DBNull.Value),
                new SQLiteParameter("@IsActive", user.IsActive ? 1 : 0),
                new SQLiteParameter("@CreatedAt", user.CreatedAt)
            });
            user.UserID = Convert.ToInt32(cmd.ExecuteScalar());
            user.Password = hashed;
            user.Salt = salt;
        }

        public void UpdateUser(User user)
        {
            const string sql = @"
                UPDATE Users SET
                  UserName      = @UserName,
                  Password      = @Password,
                  Salt          = @Salt,
                  UserPhotoPath = @Photo,
                  IsAdmin       = @Admin,
                  Email         = @Email,
                  Phone         = @Phone,
                  Mobile        = @Mobile,
                  Address       = @Address,
                  Role          = @Role,
                  IsActive      = @IsActive
                WHERE UserID = @UserID";

            string hashed = user.Password;
            string salt = user.Salt;
            if (!string.IsNullOrWhiteSpace(user.Password) && string.IsNullOrWhiteSpace(user.Salt))
            {
                hashed = SecurityHelper.HashPassword(user.Password, out salt);
            }

            var p = new[]
            {
                new SQLiteParameter("@UserID",   user.UserID),
                new SQLiteParameter("@UserName", user.UserName),
                new SQLiteParameter("@Password", hashed),
                new SQLiteParameter("@Salt",     salt),
                new SQLiteParameter("@Photo",    (object)user.UserPhotoPath ?? DBNull.Value),
                new SQLiteParameter("@Admin",    user.IsAdmin ? 1 : 0),
                new SQLiteParameter("@Email",    (object)user.Email ?? DBNull.Value),
                new SQLiteParameter("@Phone",    (object)user.Phone ?? DBNull.Value),
                new SQLiteParameter("@Mobile",   (object)user.Mobile ?? DBNull.Value),
                new SQLiteParameter("@Address",  (object)user.Address ?? DBNull.Value),
                new SQLiteParameter("@Role",     (object)user.Role ?? DBNull.Value),
                new SQLiteParameter("@IsActive", user.IsActive ? 1 : 0)
            };

            using var conn = _dbService.CreateConnection();
            SqliteHelper.ExecuteNonQuery(conn, sql, p);
            user.Password = hashed;
            user.Salt = salt;
        }

        public void ChangeUserPassword(int userID, string newPassword)
        {
            var sql = "UPDATE Users SET Password=@Pwd, Salt=@Salt WHERE UserID=@ID";
            string hashed = string.Empty;
            string salt = string.Empty;
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                hashed = SecurityHelper.HashPassword(newPassword, out salt);
            }

            var p = new[]
            {
                new SQLiteParameter("@Pwd", hashed),
                new SQLiteParameter("@Salt", salt),
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
                const string sql = "SELECT COUNT(*) FROM Users WHERE IsAdmin = 1";
                using var conn = _dbService.CreateConnection();
                var adminCount = Convert.ToInt32(SqliteHelper.ExecuteScalar(conn, sql));
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
            return new User
            {
                UserID = rdr["UserID"] != DBNull.Value ? Convert.ToInt32(rdr["UserID"]) : 0,
                UserName = rdr["UserName"].ToString(),
                Password = rdr["Password"].ToString(),
                Salt = rdr["Salt"]?.ToString(),
                UserPhotoPath = rdr["UserPhotoPath"]?.ToString(),
                IsAdmin = rdr["IsAdmin"] != DBNull.Value && Convert.ToInt32(rdr["IsAdmin"]) == 1,
                Email = rdr["Email"]?.ToString(),
                Phone = rdr["Phone"]?.ToString(),
                Mobile = rdr["Mobile"]?.ToString(),
                Address = rdr["Address"]?.ToString(),
                Role = rdr["Role"]?.ToString(),
                IsActive = rdr["IsActive"] != DBNull.Value && Convert.ToInt32(rdr["IsActive"]) == 1,
                CreatedAt = rdr["CreatedAt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["CreatedAt"]),
                FailedAttempts = rdr["FailedAttempts"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["FailedAttempts"]),
                LockoutUntil = rdr["LockoutUntil"] == DBNull.Value ? null : Convert.ToDateTime(rdr["LockoutUntil"])
            };
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            using var conn = _dbService.CreateConnection();
            return await SqliteHelper.ExecuteReaderAsync(conn, "SELECT * FROM Users", null, MapUser);
        }

        public async Task<User?> GetUserByIDAsync(int userID)
        {
            using var conn = _dbService.CreateConnection();
            var list = await SqliteHelper.ExecuteReaderAsync(conn, "SELECT * FROM Users WHERE UserID=@ID",
                new[] { new SQLiteParameter("@ID", userID) }, MapUser);
            return list.FirstOrDefault();
        }

        public async Task<User?> AuthenticateUserAsync(string userName, string password)
        {
            using var conn = _dbService.CreateConnection();
            var users = await SqliteHelper.ExecuteReaderAsync(conn,
                "SELECT * FROM Users WHERE UserName=@UserName",
                new[] { new SQLiteParameter("@UserName", userName) }, MapUser);
            var u = users.FirstOrDefault();
            if (u == null) return null;
            if (u.LockoutUntil.HasValue && u.LockoutUntil > DateTime.UtcNow)
                return null;
            if (u.LockoutUntil.HasValue && u.LockoutUntil <= DateTime.UtcNow)
            {
                var reset = new[]
                {
                    new SQLiteParameter("@Attempts", 0),
                    new SQLiteParameter("@Lockout", DBNull.Value),
                    new SQLiteParameter("@ID", u.UserID)
                };
                await SqliteHelper.ExecuteNonQueryAsync(conn, "UPDATE Users SET FailedAttempts=@Attempts, LockoutUntil=@Lockout WHERE UserID=@ID", reset);
                u.FailedAttempts = 0;
                u.LockoutUntil = null;
            }

            bool success;
            if (string.IsNullOrWhiteSpace(u.Salt) && SecurityHelper.IsSha256Hash(u.Password))
            {
                var legacy = SecurityHelper.ComputeSha256HashLegacy(password ?? string.Empty);
                success = u.Password == legacy;
                if (success)
                {
                    var upgraded = SecurityHelper.HashPassword(password ?? string.Empty, out var salt);
                    var p = new[]
                    {
                        new SQLiteParameter("@Pwd", upgraded),
                        new SQLiteParameter("@Salt", salt),
                        new SQLiteParameter("@ID", u.UserID)
                    };
                    await SqliteHelper.ExecuteNonQueryAsync(conn, "UPDATE Users SET Password=@Pwd, Salt=@Salt WHERE UserID=@ID", p);
                    u.Password = upgraded;
                    u.Salt = salt;
                }
            }
            else
            {
                success = SecurityHelper.VerifyPassword(password ?? string.Empty, u.Salt, u.Password);
            }

            if (success)
            {
                var reset = new[]
                {
                    new SQLiteParameter("@Attempts", 0),
                    new SQLiteParameter("@Lockout", DBNull.Value),
                    new SQLiteParameter("@ID", u.UserID)
                };
                await SqliteHelper.ExecuteNonQueryAsync(conn, "UPDATE Users SET FailedAttempts=@Attempts, LockoutUntil=@Lockout WHERE UserID=@ID", reset);
                u.FailedAttempts = 0;
                u.LockoutUntil = null;
                return u;
            }

            u.FailedAttempts++;
            DateTime? lockout = null;
            if (u.FailedAttempts >= 3)
                lockout = DateTime.UtcNow.AddMinutes(15);
            var update = new[]
            {
                new SQLiteParameter("@Attempts", u.FailedAttempts),
                new SQLiteParameter("@Lockout", (object?)lockout ?? DBNull.Value),
                new SQLiteParameter("@ID", u.UserID)
            };
            await SqliteHelper.ExecuteNonQueryAsync(conn, "UPDATE Users SET FailedAttempts=@Attempts, LockoutUntil=@Lockout WHERE UserID=@ID", update);
            u.LockoutUntil = lockout;
            return null;
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            if (_context.CurrentUser is User u)
                return await GetUserByIDAsync(u.UserID);
            return null;
        }

        public async Task AddUserAsync(User user)
        {
            const string sql = @"
                INSERT INTO Users
                  (UserName, Password, Salt, UserPhotoPath, IsAdmin, Email, Phone, Mobile, Address, Role, IsActive, CreatedAt)
                VALUES
                  (@UserName,@Password,@Salt,@Photo,@Admin,@Email,@Phone,@Mobile,@Address,@Role,@IsActive,@CreatedAt);
                SELECT last_insert_rowid();";

            using var conn = _dbService.CreateConnection();
            using var cmd = new SQLiteCommand(sql, conn);

            string hashed = string.Empty;
            string salt = string.Empty;
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                if (!string.IsNullOrWhiteSpace(user.Salt))
                {
                    hashed = user.Password;
                    salt = user.Salt;
                }
                else
                {
                    hashed = SecurityHelper.HashPassword(user.Password, out salt);
                }
            }

            if (user.CreatedAt == default)
                user.CreatedAt = DateTime.UtcNow;
            cmd.Parameters.AddRange(new[]
            {
                new SQLiteParameter("@UserName", user.UserName),
                new SQLiteParameter("@Password", hashed),
                new SQLiteParameter("@Salt",     salt),
                new SQLiteParameter("@Photo",    (object)user.UserPhotoPath ?? DBNull.Value),
                new SQLiteParameter("@Admin",    user.IsAdmin ? 1 : 0),
                new SQLiteParameter("@Email",    (object)user.Email ?? DBNull.Value),
                new SQLiteParameter("@Phone",    (object)user.Phone ?? DBNull.Value),
                new SQLiteParameter("@Mobile",   (object)user.Mobile ?? DBNull.Value),
                new SQLiteParameter("@Address",  (object)user.Address ?? DBNull.Value),
                new SQLiteParameter("@Role",     (object)user.Role ?? DBNull.Value),
                new SQLiteParameter("@IsActive", user.IsActive ? 1 : 0),
                new SQLiteParameter("@CreatedAt", user.CreatedAt)
            });
            user.UserID = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            user.Password = hashed;
            user.Salt = salt;
        }

        public async Task UpdateUserAsync(User user)
        {
            const string sql = @"
                UPDATE Users SET
                  UserName      = @UserName,
                  Password      = @Password,
                  Salt          = @Salt,
                  UserPhotoPath = @Photo,
                  IsAdmin       = @Admin,
                  Email         = @Email,
                  Phone         = @Phone,
                  Mobile        = @Mobile,
                  Address       = @Address,
                  Role          = @Role,
                  IsActive      = @IsActive
                WHERE UserID = @UserID";

            string hashed = user.Password;
            string salt = user.Salt;
            if (!string.IsNullOrWhiteSpace(user.Password) && string.IsNullOrWhiteSpace(user.Salt))
            {
                hashed = SecurityHelper.HashPassword(user.Password, out salt);
            }

            var p = new[]
            {
                new SQLiteParameter("@UserID",   user.UserID),
                new SQLiteParameter("@UserName", user.UserName),
                new SQLiteParameter("@Password", hashed),
                new SQLiteParameter("@Salt",     salt),
                new SQLiteParameter("@Photo",    (object)user.UserPhotoPath ?? DBNull.Value),
                new SQLiteParameter("@Admin",    user.IsAdmin ? 1 : 0),
                new SQLiteParameter("@Email",    (object)user.Email ?? DBNull.Value),
                new SQLiteParameter("@Phone",    (object)user.Phone ?? DBNull.Value),
                new SQLiteParameter("@Mobile",   (object)user.Mobile ?? DBNull.Value),
                new SQLiteParameter("@Address",  (object)user.Address ?? DBNull.Value),
                new SQLiteParameter("@Role",     (object)user.Role ?? DBNull.Value),
                new SQLiteParameter("@IsActive", user.IsActive ? 1 : 0)
            };

            using var conn = _dbService.CreateConnection();
            await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p);
            user.Password = hashed;
            user.Salt = salt;
        }

        public async Task ChangeUserPasswordAsync(int userID, string newPassword)
        {
            var sql = "UPDATE Users SET Password=@Pwd, Salt=@Salt WHERE UserID=@ID";
            string hashed = string.Empty;
            string salt = string.Empty;
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                hashed = SecurityHelper.HashPassword(newPassword, out salt);
            }

            var p = new[]
            {
                new SQLiteParameter("@Pwd", hashed),
                new SQLiteParameter("@Salt", salt),
                new SQLiteParameter("@ID",  userID)
            };
            using var conn = _dbService.CreateConnection();
            await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p);
        }

        async Task DeleteUserInternalAsync(int userID)
        {
            var sql = "DELETE FROM Users WHERE UserID=@ID";
            using var conn = _dbService.CreateConnection();
            await SqliteHelper.ExecuteNonQueryAsync(conn, sql, new[] { new SQLiteParameter("@ID", userID) });
        }

        public async Task<bool> TryDeleteUserAsync(int userID)
        {
            var user = await GetUserByIDAsync(userID);
            if (user == null) return false;
            if (user.IsAdmin)
            {
                const string sql = "SELECT COUNT(*) FROM Users WHERE IsAdmin = 1";
                using var conn = _dbService.CreateConnection();
                var adminCount = Convert.ToInt32(await SqliteHelper.ExecuteScalarAsync(conn, sql));
                if (adminCount <= 1) return false;
            }
            await DeleteUserInternalAsync(userID);
            return true;
        }

        public Task<bool> DeleteUserAsync(int userID) => TryDeleteUserAsync(userID);
    }
}

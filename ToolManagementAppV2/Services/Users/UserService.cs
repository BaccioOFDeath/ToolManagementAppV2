using System;
using System.Data.SQLite;
using System.Data;
using System.Threading.Tasks;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Interfaces;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ToolManagementAppV2.Services.Users
{
    public class UserService : IUserService
    {
        readonly DatabaseService _dbService;
        readonly IUserContext _context;
        readonly ILogger<UserService> _logger;
        readonly IAuthorizationService _auth;

        public UserService(DatabaseService dbService, IUserContext context, IAuthorizationService? authorizationService = null, ILogger<UserService>? logger = null)
        {
            _dbService = dbService;
            _context = context;
            _auth = authorizationService ?? new NoOpAuthorizationService();
            _logger = logger ?? NullLogger<UserService>.Instance;
        }


        User MapUser(IDataRecord rdr)
        {
            bool HasColumn(string columnName)
            {
                for (int i = 0; i < rdr.FieldCount; i++)
                    if (rdr.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                        return true;
                return false;
            }

            return new User
            {
                UserID = rdr["UserID"] != DBNull.Value ? Convert.ToInt32(rdr["UserID"]) : 0,
                UserName = rdr["UserName"].ToString(),
                Password = HasColumn("Password") && rdr["Password"] != DBNull.Value ? rdr["Password"].ToString() : null,
                Salt = HasColumn("Salt") && rdr["Salt"] != DBNull.Value ? rdr["Salt"].ToString() : null,
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
                LockoutUntil = rdr["LockoutUntil"] == DBNull.Value ? null : Convert.ToDateTime(rdr["LockoutUntil"]),
                PasswordExpired = rdr["PasswordExpired"] != DBNull.Value && Convert.ToInt32(rdr["PasswordExpired"]) == 1
            };
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            using var conn = _dbService.CreateConnection();
            const string sql = "SELECT UserID, UserName, UserPhotoPath, IsAdmin, Email, Phone, Mobile, Address, Role, IsActive, CreatedAt, FailedAttempts, LockoutUntil, PasswordExpired FROM Users";
            return await SqliteHelper.ExecuteReaderAsync(conn, sql, null, MapUser);
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

            await SqliteHelper.ExecuteNonQueryAsync(
                conn,
                "UPDATE Users SET FailedAttempts=IFNULL(FailedAttempts,0) WHERE FailedAttempts IS NULL"
            );

            var users = await SqliteHelper.ExecuteReaderAsync(conn,
                "SELECT * FROM Users WHERE UserName=@UserName",
                new[] { new SQLiteParameter("@UserName", userName) }, MapUser);

            var u = users.FirstOrDefault();
            if (u == null) return null;

            if (u.LockoutUntil.HasValue && u.LockoutUntil > DateTime.UtcNow) return null;

            if (u.LockoutUntil.HasValue && u.LockoutUntil <= DateTime.UtcNow)
            {
                var reset = new[]
                {
            new SQLiteParameter("@Attempts", 0),
            new SQLiteParameter("@Lockout", DBNull.Value),
            new SQLiteParameter("@ID", u.UserID)
        };
                await SqliteHelper.ExecuteNonQueryAsync(conn, "UPDATE Users SET FailedAttempts=IFNULL(@Attempts,0), LockoutUntil=@Lockout WHERE UserID=@ID", reset);
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
                    var upgradedResult = await SecurityHelper.HashPasswordAsync(password ?? string.Empty).ConfigureAwait(false);
                    var p = new[]
                    {
                new SQLiteParameter("@Pwd", upgradedResult.hash),
                new SQLiteParameter("@Salt", upgradedResult.salt),
                new SQLiteParameter("@ID", u.UserID)
            };
                    await SqliteHelper.ExecuteNonQueryAsync(conn, "UPDATE Users SET Password=@Pwd, Salt=@Salt WHERE UserID=@ID", p);
                    u.Password = upgradedResult.hash;
                    u.Salt = upgradedResult.salt;
                }
            }
            else
            {
                success = await SecurityHelper.VerifyPasswordAsync(password ?? string.Empty, u.Salt, u.Password).ConfigureAwait(false);
            }

            if (success)
            {
                var reset = new[]
                {
            new SQLiteParameter("@Attempts", 0),
            new SQLiteParameter("@Lockout", DBNull.Value),
            new SQLiteParameter("@ID", u.UserID)
        };
                await SqliteHelper.ExecuteNonQueryAsync(conn, "UPDATE Users SET FailedAttempts=IFNULL(@Attempts,0), LockoutUntil=@Lockout WHERE UserID=@ID", reset);
                u.FailedAttempts = 0;
                u.LockoutUntil = null;
                return u;
            }

            u.FailedAttempts = Math.Max(0, u.FailedAttempts) + 1;
            DateTime? lockout = null;
            if (u.FailedAttempts >= 3) lockout = DateTime.UtcNow.AddMinutes(15);

            var update = new[]
            {
        new SQLiteParameter("@Attempts", u.FailedAttempts),
        new SQLiteParameter("@Lockout", (object?)lockout ?? DBNull.Value),
        new SQLiteParameter("@ID", u.UserID)
    };
            await SqliteHelper.ExecuteNonQueryAsync(conn, "UPDATE Users SET FailedAttempts=IFNULL(@Attempts,0), LockoutUntil=@Lockout WHERE UserID=@ID", update);
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
            var existingUsers = await GetAllUsersAsync();
            if (existingUsers.Count > 0)
                _auth.EnsureAdmin();
            const string sql = @"
                INSERT INTO Users
                  (UserName, Password, Salt, UserPhotoPath, IsAdmin, Email, Phone, Mobile, Address, Role, IsActive, CreatedAt, FailedAttempts, LockoutUntil, PasswordExpired)
                VALUES
                  (@UserName,@Password,@Salt,@Photo,@Admin,@Email,@Phone,@Mobile,@Address,@Role,@IsActive,@CreatedAt,@FailedAttempts,@Lockout,@PasswordExpired);
                SELECT last_insert_rowid();";

            using var conn = _dbService.CreateConnection();
            using var cmd = new SQLiteCommand(sql, conn);

            string hashed = string.Empty;
            string salt = string.Empty;
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                if (!string.IsNullOrWhiteSpace(user.Salt) &&
                    IsBase64String(user.Password) && IsBase64String(user.Salt))
                {
                    hashed = user.Password;
                    salt = user.Salt;
                }
                else
                {
                    var result = await SecurityHelper.HashPasswordAsync(user.Password).ConfigureAwait(false);
                    hashed = result.hash;
                    salt = result.salt;
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
                new SQLiteParameter("@CreatedAt", user.CreatedAt),
                new SQLiteParameter("@FailedAttempts", user.FailedAttempts),
                new SQLiteParameter("@Lockout",    (object?)user.LockoutUntil ?? DBNull.Value),
                new SQLiteParameter("@PasswordExpired", user.PasswordExpired ? 1 : 0)
            });
            try
            {
                user.UserID = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Constraint &&
                                             ex.Message.Contains("Users.UserName", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("A user with the same username already exists.", ex);
            }
            user.Password = hashed;
            user.Salt = salt;
        }

        public async Task UpdateUserAsync(User user)
        {
            _auth.EnsureAdmin();
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
                var result = await SecurityHelper.HashPasswordAsync(user.Password).ConfigureAwait(false);
                hashed = result.hash;
                salt = result.salt;
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

        public async Task<bool> ChangeUserPasswordAsync(int userID, string newPassword)
        {
            if (_context.CurrentUser?.UserID != userID)
                _auth.EnsureAdmin();
            var sql = "UPDATE Users SET Password=@Pwd, Salt=@Salt, PasswordExpired=@Expired WHERE UserID=@ID";
            string hashed = string.Empty;
            string salt = string.Empty;
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                var result = await SecurityHelper.HashPasswordAsync(newPassword).ConfigureAwait(false);
                hashed = result.hash;
                salt = result.salt;
            }

            var expired = newPassword == "admin" || newPassword == "changeme" || newPassword == "newpassword";

            var p = new[]
            {
                new SQLiteParameter("@Pwd", hashed),
                new SQLiteParameter("@Salt", salt),
                new SQLiteParameter("@Expired", expired ? 1 : 0),
                new SQLiteParameter("@ID",  userID)
            };
            using var conn = _dbService.CreateConnection();
            int rows = await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p);
            if (rows == 0)
                _logger.LogWarning("Password update affected 0 rows for UserID {UserID}", userID);
            return rows > 0;
        }

        async Task DeleteUserInternalAsync(int userID)
        {
            var sql = "DELETE FROM Users WHERE UserID=@ID";
            using var conn = _dbService.CreateConnection();
            await SqliteHelper.ExecuteNonQueryAsync(conn, sql, new[] { new SQLiteParameter("@ID", userID) });
        }

        public async Task<bool> TryDeleteUserAsync(int userID)
        {
            _auth.EnsureAdmin();
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

        static bool IsBase64String(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            Span<byte> buffer = new Span<byte>(new byte[input.Length]);
            return Convert.TryFromBase64String(input, buffer, out _);
        }
    }
}

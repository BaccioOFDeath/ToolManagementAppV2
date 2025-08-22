using System;
using System.Data.SQLite;
using System.Data;
using System.Threading.Tasks;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Utilities.Helpers;
using InventoryManagementApp.Interfaces;
using System.Linq;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Services.Users
{
    public class UserService : IUserService
    {
        readonly DatabaseService _dbService;
        readonly IUserContext _context;
        readonly ILogger<UserService> _logger;
        readonly IAuthorizationService _auth;
        readonly ActivityLogService? _activityLog;

        public UserService(DatabaseService dbService, IUserContext context, IAuthorizationService? authorizationService = null, ILogger<UserService>? logger = null, ActivityLogService? activityLogService = null)
        {
            _dbService = dbService;
            _context = context;
            _auth = authorizationService ?? new NoOpAuthorizationService();
            _logger = logger ?? NullLogger<UserService>.Instance;
            _activityLog = activityLogService;
        }


        private static DateTime? ParseToUtcNullable(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            if (value is DateTime dt)
            {
                if (dt.Kind == DateTimeKind.Utc) return dt;
                if (dt.Kind == DateTimeKind.Local) return dt.ToUniversalTime();
                return DateTime.SpecifyKind(dt, DateTimeKind.Local).ToUniversalTime();
            }
            if (value is DateTimeOffset dto) return dto.UtcDateTime;
            var s = value.ToString()?.Trim();
            if (string.IsNullOrEmpty(s)) return null;

            var cultures = new[] { CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo("en-NZ"), CultureInfo.CurrentCulture };
            var exactFormats = new[] { "o", "yyyy-MM-ddTHH:mm:ss.fffffffK" };

            foreach (var c in cultures)
            {
                if (DateTimeOffset.TryParseExact(s, exactFormats, c, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dtoExact))
                    return dtoExact.UtcDateTime;
            }
            foreach (var c in cultures)
            {
                if (DateTimeOffset.TryParse(s, c, DateTimeStyles.AssumeLocal, out var dtoFree))
                    return dtoFree.UtcDateTime;
            }
            return null;
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

            DateTime? createdAt = ParseToUtcNullable(HasColumn("CreatedAt") ? rdr["CreatedAt"] : DBNull.Value);
            if (createdAt.HasValue && createdAt.Value.Kind != DateTimeKind.Local)
                createdAt = createdAt.Value.ToLocalTime();

            return new User
            {
                UserID = HasColumn("UserID") && rdr["UserID"] != DBNull.Value ? Convert.ToInt32(rdr["UserID"]) : 0,
                UserName = HasColumn("UserName") ? rdr["UserName"]?.ToString() : null,
                PasswordHash = HasColumn("PasswordHash") && rdr["PasswordHash"] != DBNull.Value ? rdr["PasswordHash"].ToString() : null,
                PasswordSalt = HasColumn("PasswordSalt") && rdr["PasswordSalt"] != DBNull.Value ? rdr["PasswordSalt"].ToString() : null,
                UserPhotoPath = HasColumn("UserPhotoPath") ? rdr["UserPhotoPath"]?.ToString() : null,
                IsAdmin = HasColumn("IsAdmin") && rdr["IsAdmin"] != DBNull.Value && Convert.ToInt32(rdr["IsAdmin"]) == 1,
                Email = HasColumn("Email") ? rdr["Email"]?.ToString() : null,
                Phone = HasColumn("Phone") ? rdr["Phone"]?.ToString() : null,
                Mobile = HasColumn("Mobile") ? rdr["Mobile"]?.ToString() : null,
                Address = HasColumn("Address") ? rdr["Address"]?.ToString() : null,
                Role = HasColumn("Role") ? rdr["Role"]?.ToString() : null,
                IsActive = HasColumn("IsActive") && rdr["IsActive"] != DBNull.Value && Convert.ToInt32(rdr["IsActive"]) == 1,
                CreatedAt = createdAt,
                PasswordExpired = HasColumn("PasswordExpired") && rdr["PasswordExpired"] != DBNull.Value && Convert.ToInt32(rdr["PasswordExpired"]) == 1
            };
        }


        public async Task<List<User>> GetAllUsersAsync()
        {
            using var conn = _dbService.CreateConnection();
            const string sql = "SELECT UserID, UserName, UserPhotoPath, IsAdmin, Email, Phone, Mobile, Address, Role, IsActive, CreatedAt, PasswordExpired FROM Users";
            return await SqliteHelper.ExecuteReaderAsync(conn, sql, null, MapUser);
        }

        public async Task<User?> GetUserByIDAsync(int userID)
        {
            using var conn = _dbService.CreateConnection();
            var list = await SqliteHelper.ExecuteReaderAsync(conn, "SELECT * FROM Users WHERE UserID=@ID",
                new[] { new SQLiteParameter("@ID", userID) }, MapUser);
            return list.FirstOrDefault();
        }

        public async Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password)
        {
            using var conn = _dbService.CreateConnection();

            userName = (userName ?? string.Empty).Trim();
            password = (password ?? string.Empty).Trim();

            var users = await SqliteHelper.ExecuteReaderAsync(conn,
                "SELECT * FROM Users WHERE UserName=@UserName",
                new[] { new SQLiteParameter("@UserName", userName) }, MapUser);

            var u = users.FirstOrDefault();
            if (u == null) return (AuthenticationResult.IncorrectPassword, null);
            if (!u.IsActive) return (AuthenticationResult.Inactive, null);

            bool success;
            if (string.IsNullOrWhiteSpace(u.PasswordSalt) && SecurityHelper.IsSha256Hash(u.PasswordHash))
            {
                var legacy = SecurityHelper.ComputeSha256HashLegacy(password);
                success = u.PasswordHash == legacy;
                if (success)
                {
                    var upgradedResult = await SecurityHelper.HashPasswordAsync(password).ConfigureAwait(false);
                    var p = new[]
                    {
                new SQLiteParameter("@Pwd", upgradedResult.hash),
                new SQLiteParameter("@Salt", upgradedResult.salt),
                new SQLiteParameter("@ID", u.UserID)
            };
                    await SqliteHelper.ExecuteNonQueryAsync(conn, "UPDATE Users SET PasswordHash=@Pwd, PasswordSalt=@Salt WHERE UserID=@ID", p);
                    u.PasswordHash = upgradedResult.hash;
                    u.PasswordSalt = upgradedResult.salt;
                }
            }
            else
            {
                success = await SecurityHelper.VerifyPasswordAsync(password, u.PasswordSalt, u.PasswordHash).ConfigureAwait(false);
            }

            if (success)
            {
                if (_activityLog != null)
                    await _activityLog.LogActionAsync(u.UserID, u.UserName ?? string.Empty, "User login").ConfigureAwait(false);
                return (AuthenticationResult.Success, u);
            }
            return (AuthenticationResult.IncorrectPassword, null);
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
            if (existingUsers.Count == 0)
            {
                // Seed first user as an administrator regardless of input flag
                user.IsAdmin = true;
            }
            else
            {
                _auth.EnsureAdmin();
            }
            const string sql = @"
                INSERT INTO Users
                  (UserName, PasswordHash, PasswordSalt, UserPhotoPath, IsAdmin, Email, Phone, Mobile, Address, Role, IsActive, CreatedAt, PasswordExpired)
                VALUES
                  (@UserName,@PasswordHash,@PasswordSalt,@Photo,@Admin,@Email,@Phone,@Mobile,@Address,@Role,@IsActive,@CreatedAt,@PasswordExpired);
                SELECT last_insert_rowid();";

            using var conn = _dbService.CreateConnection();
            using var cmd = new SQLiteCommand(sql, conn);

            var password = (user.PasswordHash ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty.", nameof(user.PasswordHash));
            if (!PasswordValidator.IsValid(password, out var error))
                throw new ArgumentException(error, nameof(user.PasswordHash));

            var result = await SecurityHelper.HashPasswordAsync(password).ConfigureAwait(false);
            string hashed = result.hash;
            string salt = result.salt;

            if (user.CreatedAt == null)
                user.CreatedAt = DateTime.UtcNow;
            cmd.Parameters.AddRange(new[]
            {
                new SQLiteParameter("@UserName", user.UserName),
                new SQLiteParameter("@PasswordHash", hashed),
                new SQLiteParameter("@PasswordSalt",     salt),
                new SQLiteParameter("@Photo",    (object)user.UserPhotoPath ?? DBNull.Value),
                new SQLiteParameter("@Admin",    user.IsAdmin ? 1 : 0),
                new SQLiteParameter("@Email",    (object)user.Email ?? DBNull.Value),
                new SQLiteParameter("@Phone",    (object)user.Phone ?? DBNull.Value),
                new SQLiteParameter("@Mobile",   (object)user.Mobile ?? DBNull.Value),
                new SQLiteParameter("@Address",  (object)user.Address ?? DBNull.Value),
                new SQLiteParameter("@Role",     (object)user.Role ?? DBNull.Value),
                new SQLiteParameter("@IsActive", user.IsActive ? 1 : 0),
                new SQLiteParameter("@CreatedAt", user.CreatedAt),
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
            user.PasswordHash = hashed;
            user.PasswordSalt = salt;
        }

        public async Task UpdateUserAsync(User user)
        {
            _auth.EnsureAdmin();
            const string sql = @"
                UPDATE Users SET
                  UserName      = @UserName,
                  PasswordHash  = @PasswordHash,
                  PasswordSalt  = @PasswordSalt,
                  UserPhotoPath = @Photo,
                  IsAdmin       = @Admin,
                  Email         = @Email,
                  Phone         = @Phone,
                  Mobile        = @Mobile,
                  Address       = @Address,
                  Role          = @Role,
                  IsActive      = @IsActive
                WHERE UserID = @UserID";

            string hashed = user.PasswordHash;
            string salt = user.PasswordSalt;

            if (string.IsNullOrWhiteSpace(user.PasswordHash) || string.IsNullOrWhiteSpace(user.PasswordSalt))
            {
                var existing = await GetUserByIDAsync(user.UserID);
                if (existing != null)
                {
                    if (string.IsNullOrWhiteSpace(user.PasswordHash))
                        hashed = existing.PasswordHash;
                    if (string.IsNullOrWhiteSpace(user.PasswordSalt))
                        salt = existing.PasswordSalt;
                }
            }

            if (!string.IsNullOrWhiteSpace(user.PasswordHash) && string.IsNullOrWhiteSpace(user.PasswordSalt))
            {
                var result = await SecurityHelper.HashPasswordAsync(user.PasswordHash).ConfigureAwait(false);
                hashed = result.hash;
                salt = result.salt;
            }

            var p = new[]
            {
                new SQLiteParameter("@UserID",   user.UserID),
                new SQLiteParameter("@UserName", user.UserName),
                new SQLiteParameter("@PasswordHash", hashed),
                new SQLiteParameter("@PasswordSalt",     salt),
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
            user.PasswordHash = hashed;
            user.PasswordSalt = salt;
        }

        public async Task<bool> ChangeUserPasswordAsync(int userID, string newPassword)
        {
            if (_context.CurrentUser?.UserID != userID)
                _auth.EnsureAdmin();

            newPassword = (newPassword ?? string.Empty).Trim();
            if (!PasswordValidator.IsValid(newPassword, out var error))
                throw new ArgumentException(error, nameof(newPassword));

            var sql = "UPDATE Users SET PasswordHash=@Pwd, PasswordSalt=@Salt, PasswordExpired=@Expired WHERE UserID=@ID";
            var result = await SecurityHelper.HashPasswordAsync(newPassword).ConfigureAwait(false);
            string hashed = result.hash;
            string salt = result.salt;

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

    }
}

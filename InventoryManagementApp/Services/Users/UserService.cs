using System;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Threading;
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
    /// <summary>
    /// Service for managing user accounts including authentication, CRUD operations, and password management.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly DatabaseService _dbService;
        private readonly IUserContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly IAuthorizationService _auth;
        private readonly ActivityLogService? _activityLog;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="dbService">Database service for data access.</param>
        /// <param name="context">User context for tracking current user.</param>
        /// <param name="authorizationService">Optional authorization service for access control.</param>
        /// <param name="logger">Optional logger for diagnostic output.</param>
        /// <param name="activityLogService">Optional activity log service for audit trails.</param>
        public UserService(DatabaseService dbService, IUserContext context, IAuthorizationService? authorizationService = null, ILogger<UserService>? logger = null, ActivityLogService? activityLogService = null)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _auth = authorizationService ?? new NoOpAuthorizationService();
            _logger = logger ?? NullLogger<UserService>.Instance;
            _activityLog = activityLogService;
        }


        /// <summary>
        /// Parses a value to UTC DateTime, handling various input formats and timezones.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>UTC DateTime if parsing succeeds; otherwise, null.</returns>
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

            string GetString(string columnName)
            {
                if (!HasColumn(columnName)) return string.Empty;
                var value = rdr[columnName];
                return value is DBNull ? string.Empty : value?.ToString() ?? string.Empty;
            }

            return new User
            {
                UserID = HasColumn("UserID") && rdr["UserID"] != DBNull.Value ? Convert.ToInt32(rdr["UserID"]) : 0,
                UserName = GetString("UserName"),
                PasswordHash = GetString("PasswordHash"),
                PasswordSalt = GetString("PasswordSalt"),
                UserPhotoPath = GetString("UserPhotoPath"),
                IsAdmin = HasColumn("IsAdmin") && rdr["IsAdmin"] != DBNull.Value && Convert.ToInt32(rdr["IsAdmin"]) == 1,
                Email = GetString("Email"),
                Phone = GetString("Phone"),
                Mobile = GetString("Mobile"),
                Address = GetString("Address"),
                Role = GetString("Role"),
                IsActive = HasColumn("IsActive") && rdr["IsActive"] != DBNull.Value && Convert.ToInt32(rdr["IsActive"]) == 1,
                CreatedAt = createdAt,
                PasswordExpired = HasColumn("PasswordExpired") && rdr["PasswordExpired"] != DBNull.Value && Convert.ToInt32(rdr["PasswordExpired"]) == 1
            };
        }


        public async Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            using var conn = _dbService.CreateConnection();
            const string sql = "SELECT UserID, UserName, UserPhotoPath, IsAdmin, Email, Phone, Mobile, Address, Role, IsActive, CreatedAt, PasswordExpired FROM Users";
            return await SqliteHelper.ExecuteReaderAsync(conn, sql, MapUser, cancellationToken: cancellationToken);
        }

        public async Task<int> CountUsersAsync(CancellationToken cancellationToken = default)
        {
            using var conn = _dbService.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM Users";
            var result = await SqliteHelper.ExecuteScalarAsync(conn, sql, cancellationToken: cancellationToken);
            return Convert.ToInt32(result ?? 0);
        }

        public async Task<User?> GetUserByIDAsync(int userID, CancellationToken cancellationToken = default)
        {
            using var conn = _dbService.CreateConnection();
            var list = await SqliteHelper.ExecuteReaderAsync(conn, "SELECT * FROM Users WHERE UserID=@ID",
                MapUser,
                new[] { new SqliteParameter("@ID", userID) },
                cancellationToken: cancellationToken);
            return list.FirstOrDefault();
        }

        public async Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password)
        {
            using var conn = _dbService.CreateConnection();

            userName = (userName ?? string.Empty).Trim();
            password = (password ?? string.Empty).Trim();

            var users = await SqliteHelper.ExecuteReaderAsync(conn,
                "SELECT * FROM Users WHERE UserName=@UserName",
                MapUser,
                new[] { new SqliteParameter("@UserName", userName) });

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
                new SqliteParameter("@Pwd", upgradedResult.hash),
                new SqliteParameter("@Salt", upgradedResult.salt),
                new SqliteParameter("@ID", u.UserID)
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
                return await GetUserByIDAsync(u.UserID, CancellationToken.None);
            return null;
        }

        public async Task AddUserAsync(User user)
        {
            var existingUsers = await GetAllUsersAsync(CancellationToken.None);
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
            using var cmd = new SqliteCommand(sql, conn);

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
                new SqliteParameter("@UserName", user.UserName),
                new SqliteParameter("@PasswordHash", hashed),
                new SqliteParameter("@PasswordSalt",     salt),
                new SqliteParameter("@Photo",    (object)user.UserPhotoPath ?? DBNull.Value),
                new SqliteParameter("@Admin",    user.IsAdmin ? 1 : 0),
                new SqliteParameter("@Email",    (object)user.Email ?? DBNull.Value),
                new SqliteParameter("@Phone",    (object)user.Phone ?? DBNull.Value),
                new SqliteParameter("@Mobile",   (object)user.Mobile ?? DBNull.Value),
                new SqliteParameter("@Address",  (object)user.Address ?? DBNull.Value),
                new SqliteParameter("@Role",     (object)user.Role ?? DBNull.Value),
                new SqliteParameter("@IsActive", user.IsActive ? 1 : 0),
                new SqliteParameter("@CreatedAt", user.CreatedAt),
                new SqliteParameter("@PasswordExpired", user.PasswordExpired ? 1 : 0)
            });
            try
            {
                user.UserID = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_CONSTRAINT &&
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
                var existing = await GetUserByIDAsync(user.UserID, CancellationToken.None);
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
                new SqliteParameter("@UserID",   user.UserID),
                new SqliteParameter("@UserName", user.UserName),
                new SqliteParameter("@PasswordHash", hashed),
                new SqliteParameter("@PasswordSalt",     salt),
                new SqliteParameter("@Photo",    (object)user.UserPhotoPath ?? DBNull.Value),
                new SqliteParameter("@Admin",    user.IsAdmin ? 1 : 0),
                new SqliteParameter("@Email",    (object)user.Email ?? DBNull.Value),
                new SqliteParameter("@Phone",    (object)user.Phone ?? DBNull.Value),
                new SqliteParameter("@Mobile",   (object)user.Mobile ?? DBNull.Value),
                new SqliteParameter("@Address",  (object)user.Address ?? DBNull.Value),
                new SqliteParameter("@Role",     (object)user.Role ?? DBNull.Value),
                new SqliteParameter("@IsActive", user.IsActive ? 1 : 0)
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
                new SqliteParameter("@Pwd", hashed),
                new SqliteParameter("@Salt", salt),
                new SqliteParameter("@Expired", expired ? 1 : 0),
                new SqliteParameter("@ID",  userID)
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
            await SqliteHelper.ExecuteNonQueryAsync(conn, sql, new[] { new SqliteParameter("@ID", userID) });
        }

        public async Task<bool> TryDeleteUserAsync(int userID)
        {
            _auth.EnsureAdmin();
            var user = await GetUserByIDAsync(userID, CancellationToken.None);
            if (user == null) return false;
            if (user.IsAdmin)
            {
                const string sql = "SELECT COUNT(*) FROM Users WHERE IsAdmin = 1";
                using var conn = _dbService.CreateConnection();
                var adminCount = Convert.ToInt32(await SqliteHelper.ExecuteScalarAsync(conn, sql) ?? 0);
                if (adminCount <= 1) return false;
            }
            await DeleteUserInternalAsync(userID);
            return true;
        }

    }
}

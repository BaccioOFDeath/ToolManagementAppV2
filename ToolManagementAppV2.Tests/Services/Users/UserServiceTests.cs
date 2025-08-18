using System;
using System.IO;
using System.Threading.Tasks;
using System.Data.SQLite;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Helpers;
using Xunit;

public class UserServiceTests
{
    class StubUserContext : IUserContext
    {
        public User? CurrentUser { get; set; }
        public event EventHandler<User?>? UserChanged;
        public bool IsAdmin => CurrentUser?.IsAdmin ?? false;
        public string UserName => CurrentUser?.UserName ?? string.Empty;
        public string Role => CurrentUser?.Role ?? string.Empty;
    }

    [Fact]
    public async Task AddUserAsync_AllowsSeedingAdminWithoutAuthorization()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            var ctx = new StubUserContext { CurrentUser = new User { UserName = "seed", IsAdmin = false } };
            var auth = new AuthorizationService(ctx);
            var userService = new UserService(dbService, ctx, auth);
            var admin = new User { UserName = "admin", PasswordHash = "pw", IsAdmin = true };
            await userService.AddUserAsync(admin);
            Assert.NotEqual(0, admin.UserID);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task AddUserAsync_RequiresAdminAfterSeeding()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            var ctx = new StubUserContext { CurrentUser = new User { UserName = "seed", IsAdmin = false } };
            var auth = new AuthorizationService(ctx);
            var userService = new UserService(dbService, ctx, auth);
            var admin = new User { UserName = "admin", PasswordHash = "pw", IsAdmin = true };
            await userService.AddUserAsync(admin);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => userService.AddUserAsync(new User { UserName = "user", PasswordHash = "pw" }));
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }
    [Fact]
    public async Task TryDeleteUserAsync_ReturnsFalse_WhenDeletingOnlyAdmin()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());
            var admin = new User { UserName = "admin", PasswordHash = "pw", IsAdmin = true };
            await userService.AddUserAsync(admin);
            var result = await userService.TryDeleteUserAsync(admin.UserID);
            Assert.False(result);
            Assert.NotNull(userService.GetUserByID(admin.UserID));
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task TryDeleteUserAsync_AllowsDeletingAdmin_WhenMultipleAdminsExist()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());
            var admin1 = new User { UserName = "admin1", PasswordHash = "pw", IsAdmin = true };
            var admin2 = new User { UserName = "admin2", PasswordHash = "pw", IsAdmin = true };
            await userService.AddUserAsync(admin1);
            await userService.AddUserAsync(admin2);
            var result = await userService.TryDeleteUserAsync(admin1.UserID);
            Assert.True(result);
            Assert.Null(userService.GetUserByID(admin1.UserID));
            Assert.NotNull(userService.GetUserByID(admin2.UserID));
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public void AddUser_ThrowsInvalidOperationException_WhenUserNameExists()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());
            userService.AddUser(new User { UserName = "dup", PasswordHash = "pw" });
            var ex = Assert.Throws<InvalidOperationException>(() => userService.AddUser(new User { UserName = "dup", PasswordHash = "pw" }));
            Assert.Contains("username", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task AddUserAsync_ThrowsInvalidOperationException_WhenUserNameExists()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            var userService = new UserService(dbService, new ApplicationUserContext());
            await userService.AddUserAsync(new User { UserName = "dup", PasswordHash = "pw" });
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.AddUserAsync(new User { UserName = "dup", PasswordHash = "pw" }));
            Assert.Contains("username", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task AddUserAsync_SetsFailedAttemptsToZero()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            var userService = new UserService(dbService, new ApplicationUserContext());
            var user = new User { UserName = "user1", PasswordHash = "pw" };
            await userService.AddUserAsync(user);
            var stored = userService.GetUserByID(user.UserID);
            Assert.NotNull(stored);
            Assert.Equal(0, stored!.FailedAttempts);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public void ChangeUserPassword_ReturnsFalse_ForInvalidUserID()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());
            var result = userService.ChangeUserPassword(9999, "newpass");
            Assert.False(result);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ChangeUserPasswordAsync_ReturnsFalse_ForInvalidUserID()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());
            var result = await userService.ChangeUserPasswordAsync(9999, "newpass");
            Assert.False(result);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ChangeUserPasswordAsync_TrimsInputBeforeHashing()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());
            var user = new User { UserName = "trim", PasswordHash = "pw" };
            await userService.AddUserAsync(user);
            var result = await userService.ChangeUserPasswordAsync(user.UserID, "  newpass  ");
            Assert.True(result);
            var auth = await userService.AuthenticateUserAsync("trim", "newpass");
            Assert.Equal(AuthenticationResult.Success, auth.Result);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ChangeUserPasswordAsync_ReturnsFalse_WhenPasswordIsWhitespace()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());
            var user = new User { UserName = "blank", PasswordHash = "pw" };
            await userService.AddUserAsync(user);
            var result = await userService.ChangeUserPasswordAsync(user.UserID, "   ");
            Assert.False(result);
            var auth = await userService.AuthenticateUserAsync("blank", "pw");
            Assert.Equal(AuthenticationResult.Success, auth.Result);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task UnlockUserAsync_ResetsFailedAttemptsAndLockout()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());
            var user = new User { UserName = "locked", PasswordHash = "pw" };
            await userService.AddUserAsync(user);

            await userService.AuthenticateUserAsync("locked", "bad");
            await userService.AuthenticateUserAsync("locked", "bad");
            await userService.AuthenticateUserAsync("locked", "bad");

            var locked = userService.GetUserByID(user.UserID);
            Assert.NotNull(locked);
            Assert.True(locked!.FailedAttempts >= 3);
            Assert.NotNull(locked.LockoutUntil);

            await userService.UnlockUserAsync(user.UserID);

            var unlocked = userService.GetUserByID(user.UserID);
            Assert.NotNull(unlocked);
            Assert.Equal(0, unlocked!.FailedAttempts);
            Assert.Null(unlocked.LockoutUntil);
            Assert.False(unlocked.IsLocked);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ChangeUserPasswordAsync_ResetsFailedAttemptsAndLockout()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());
            var user = new User { UserName = "lock", PasswordHash = "pw" };
            await userService.AddUserAsync(user);

            await userService.AuthenticateUserAsync("lock", "bad");
            await userService.AuthenticateUserAsync("lock", "bad");
            await userService.AuthenticateUserAsync("lock", "bad");

            var locked = userService.GetUserByID(user.UserID);
            Assert.NotNull(locked);
            Assert.True(locked!.FailedAttempts >= 3);
            Assert.NotNull(locked.LockoutUntil);

            var changed = await userService.ChangeUserPasswordAsync(user.UserID, "newpass");
            Assert.True(changed);

            var updated = userService.GetUserByID(user.UserID);
            Assert.NotNull(updated);
            Assert.Equal(0, updated!.FailedAttempts);
            Assert.Null(updated.LockoutUntil);
            Assert.False(updated.IsLocked);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ChangeUserPasswordAsync_AllowsNullFailedAttempts()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());
            var user = new User { UserName = "nulltest", PasswordHash = "pw" };
            await userService.AddUserAsync(user);

            using (var conn = dbService.CreateConnection())
            {
                await SqliteHelper.ExecuteNonQueryAsync(conn,
                    "UPDATE Users SET FailedAttempts=NULL WHERE UserID=@ID",
                    new[] { new SQLiteParameter("@ID", user.UserID) });
            }

            var changed = await userService.ChangeUserPasswordAsync(user.UserID, "newpass");
            Assert.True(changed);

            var updated = userService.GetUserByID(user.UserID);
            Assert.NotNull(updated);
            Assert.Equal(0, updated!.FailedAttempts);
            Assert.False(updated.IsLocked);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public void GetAllUsers_DoesNotIncludeSensitiveFields()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());
            userService.AddUser(new User { UserName = "list", PasswordHash = "pw", PasswordSalt = "s" });
            var users = userService.GetAllUsers();
            var user = users[0];
            Assert.Null(user.PasswordHash);
            Assert.Null(user.PasswordSalt);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task GetAllUsersAsync_DoesNotIncludeSensitiveFields()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            var userService = new UserService(dbService, new ApplicationUserContext());
            await userService.AddUserAsync(new User { UserName = "list", PasswordHash = "pw", PasswordSalt = "s" });
            var users = await userService.GetAllUsersAsync();
            var user = users[0];
            Assert.Null(user.PasswordHash);
            Assert.Null(user.PasswordSalt);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public void AddUser_UsesPreHashedPassword_WhenProvided()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());
            var hash = SecurityHelper.HashPassword("secret", out var salt);
            var user = new User { UserName = "prehashed", PasswordHash = hash, PasswordSalt = salt, PasswordExpired = true };
            userService.AddUser(user);
            var fetched = userService.GetUserByID(user.UserID)!;
            Assert.Equal(hash, fetched.PasswordHash);
            Assert.Equal(salt, fetched.PasswordSalt);
            Assert.True(fetched.PasswordExpired);
            Assert.True(SecurityHelper.VerifyPassword("secret", fetched.PasswordSalt, fetched.PasswordHash));
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }
}

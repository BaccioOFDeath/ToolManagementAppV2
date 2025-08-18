using System;
using System.IO;
using System.Linq;
using System.Data.SQLite;
using System.Threading.Tasks;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Interfaces;
using Xunit;

public class UserAuthenticationTests
{
    [Fact]
    public async Task AuthenticateUser_HashesPassword()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());

            var user = new User { UserName = "test", PasswordHash = "Strong1!", IsAdmin = false };
            await userService.AddUserAsync(user);
            var added = userService.GetUserByID(user.UserID)!;

            Assert.NotEqual("Strong1!", added.PasswordHash);
            Assert.False(SecurityHelper.IsSha256Hash(added.PasswordHash));
            Assert.False(string.IsNullOrWhiteSpace(added.PasswordSalt));
            Assert.True(SecurityHelper.VerifyPassword("Strong1!", added.PasswordSalt, added.PasswordHash));

            var auth = userService.AuthenticateUser("test", "Strong1!");
            Assert.Equal(AuthenticationResult.Success, auth.Result);
            Assert.NotNull(auth.User);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void AuthenticateUser_UpgradesLegacyHash()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());

            var legacy = SecurityHelper.ComputeSha256HashLegacy("secret");
            using (var conn = dbService.CreateConnection())
            using (var cmd = new SQLiteCommand("INSERT INTO Users (UserName, PasswordHash, IsAdmin) VALUES (@u,@p,0);", conn))
            {
                cmd.Parameters.AddWithValue("@u", "legacy");
                cmd.Parameters.AddWithValue("@p", legacy);
                cmd.ExecuteNonQuery();
            }

            var auth = userService.AuthenticateUser("legacy", "secret");
            Assert.Equal(AuthenticationResult.Success, auth.Result);
            Assert.NotNull(auth.User);

            var updated = userService.GetUserByID(auth.User!.UserID)!;
            Assert.False(SecurityHelper.IsSha256Hash(updated.PasswordHash));
            Assert.False(string.IsNullOrWhiteSpace(updated.PasswordSalt));
            Assert.True(SecurityHelper.VerifyPassword("secret", updated.PasswordSalt, updated.PasswordHash));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void AuthenticateUser_EmptySalt_ReturnsNull()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());

            var user = new User { UserName = "emptysalt", PasswordHash = "secret", IsAdmin = false };
            userService.AddUser(user);

            using (var conn = dbService.CreateConnection())
            using (var cmd = new SQLiteCommand("UPDATE Users SET PasswordSalt='' WHERE UserID=@ID", conn))
            {
                cmd.Parameters.AddWithValue("@ID", user.UserID);
                cmd.ExecuteNonQuery();
            }

            var auth = userService.AuthenticateUser("emptysalt", "secret");
            Assert.Equal(AuthenticationResult.IncorrectPassword, auth.Result);
            Assert.Null(auth.User);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void AuthenticateUser_LockoutAfterFailedAttempts()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());

            var user = new User { UserName = "lock", PasswordHash = "secret", IsAdmin = false };
            userService.AddUser(user);

            for (int i = 0; i < 3; i++)
            {
                var auth = userService.AuthenticateUser("lock", "bad");
                if (i < 2)
                    Assert.Equal(AuthenticationResult.IncorrectPassword, auth.Result);
                else
                    Assert.Equal(AuthenticationResult.LockedOut, auth.Result);
            }

            var stored = userService.GetAllUsers().First();
            Assert.Equal(3, stored.FailedAttempts);
            Assert.NotNull(stored.LockoutUntil);
            Assert.Equal(DateTimeKind.Utc, stored.LockoutUntil!.Value.Kind);
            Assert.True(stored.IsLocked);

            var afterLock = userService.AuthenticateUser("lock", "secret");
            Assert.Equal(AuthenticationResult.LockedOut, afterLock.Result);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void AuthenticateUser_ResetAfterSuccess()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());

            var user = new User { UserName = "reset", PasswordHash = "secret", IsAdmin = false };
            userService.AddUser(user);

            for (int i = 0; i < 3; i++)
            userService.AuthenticateUser("reset", "bad");

            using (var conn = dbService.CreateConnection())
            using (var cmd = new SQLiteCommand("UPDATE Users SET LockoutUntil=@t WHERE UserID=@id", conn))
            {
                cmd.Parameters.AddWithValue("@t", DateTime.UtcNow.AddMinutes(-1).ToString("o"));
                cmd.Parameters.AddWithValue("@id", user.UserID);
                cmd.ExecuteNonQuery();
            }

            var auth = userService.AuthenticateUser("reset", "secret");
            Assert.Equal(AuthenticationResult.Success, auth.Result);
            Assert.NotNull(auth.User);

            var stored = userService.GetAllUsers().First(u => u.UserName == "reset");
            Assert.Equal(0, stored.FailedAttempts);
            Assert.Null(stored.LockoutUntil);
            Assert.False(stored.IsLocked);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void ChangeUserPassword_SetsPasswordExpiredFlag()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());

            var user = new User { UserName = "flag", PasswordHash = "secret", IsAdmin = false };
            userService.AddUser(user);

            userService.ChangeUserPassword(user.UserID, "admin");
            var updated = userService.GetAllUsers().First();
            Assert.True(updated.PasswordExpired);

            userService.ChangeUserPassword(user.UserID, "newpass");
            updated = userService.GetAllUsers().First();
            Assert.False(updated.PasswordExpired);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task AuthenticateUserAsync_HashesPassword()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());

            var user = new User { UserName = "atest", PasswordHash = "secret", IsAdmin = false };
            userService.AddUser(user);
            var added = userService.GetUserByID(user.UserID)!;

            Assert.NotEqual("secret", added.PasswordHash);
            Assert.False(SecurityHelper.IsSha256Hash(added.PasswordHash));
            Assert.False(string.IsNullOrWhiteSpace(added.PasswordSalt));
            Assert.True(SecurityHelper.VerifyPassword("secret", added.PasswordSalt, added.PasswordHash));

            var auth = await userService.AuthenticateUserAsync(" atest ", " secret ");
            Assert.Equal(AuthenticationResult.Success, auth.Result);
            Assert.NotNull(auth.User);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task AuthenticateUserAsync_LockoutAfterFailedAttempts()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());

            var user = new User { UserName = "lockasync", PasswordHash = "secret", IsAdmin = false };
            userService.AddUser(user);

            for (int i = 0; i < 3; i++)
            {
                var auth = await userService.AuthenticateUserAsync(" lockasync ", " bad ");
                if (i < 2)
                    Assert.Equal(AuthenticationResult.IncorrectPassword, auth.Result);
                else
                    Assert.Equal(AuthenticationResult.LockedOut, auth.Result);
            }

            var stored = userService.GetAllUsers().First(u => u.UserName == "lockasync");
            Assert.Equal(3, stored.FailedAttempts);
            Assert.NotNull(stored.LockoutUntil);
            Assert.Equal(DateTimeKind.Utc, stored.LockoutUntil!.Value.Kind);
            Assert.True(stored.IsLocked);

            var afterLock = await userService.AuthenticateUserAsync(" lockasync ", " secret ");
            Assert.Equal(AuthenticationResult.LockedOut, afterLock.Result);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task AuthenticateUserAsync_ResetAfterSuccess()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());

            var user = new User { UserName = "areset", PasswordHash = "Strong1!", IsAdmin = false };
            await userService.AddUserAsync(user);

            for (int i = 0; i < 3; i++)
                await userService.AuthenticateUserAsync(" areset ", " bad ");

            using (var conn = dbService.CreateConnection())
            using (var cmd = new SQLiteCommand("UPDATE Users SET LockoutUntil=@t WHERE UserID=@id", conn))
            {
                cmd.Parameters.AddWithValue("@t", DateTime.UtcNow.AddMinutes(-1).ToString("o"));
                cmd.Parameters.AddWithValue("@id", user.UserID);
                cmd.ExecuteNonQuery();
            }

            var auth = await userService.AuthenticateUserAsync(" areset ", " secret ");
            Assert.Equal(AuthenticationResult.Success, auth.Result);
            Assert.NotNull(auth.User);

            var stored = (await userService.GetAllUsersAsync()).First(u => u.UserName == "areset");
            Assert.Equal(0, stored.FailedAttempts);
            Assert.Null(stored.LockoutUntil);
            Assert.False(stored.IsLocked);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}

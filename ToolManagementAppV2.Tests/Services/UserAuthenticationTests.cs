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
    public void AuthenticateUser_HashesPassword()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());

            var user = new User { UserName = "test", Password = "secret", IsAdmin = false };
            userService.AddUser(user);
            var added = userService.GetUserByID(user.UserID)!;

            Assert.NotEqual("secret", added.Password);
            Assert.False(SecurityHelper.IsSha256Hash(added.Password));
            Assert.False(string.IsNullOrWhiteSpace(added.Salt));
            Assert.True(SecurityHelper.VerifyPassword("secret", added.Salt, added.Password));

            var auth = userService.AuthenticateUser("test", "secret");
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
            using (var cmd = new SQLiteCommand("INSERT INTO Users (UserName, Password, IsAdmin) VALUES (@u,@p,0);", conn))
            {
                cmd.Parameters.AddWithValue("@u", "legacy");
                cmd.Parameters.AddWithValue("@p", legacy);
                cmd.ExecuteNonQuery();
            }

            var auth = userService.AuthenticateUser("legacy", "secret");
            Assert.Equal(AuthenticationResult.Success, auth.Result);
            Assert.NotNull(auth.User);

            var updated = userService.GetUserByID(auth.User!.UserID)!;
            Assert.False(SecurityHelper.IsSha256Hash(updated.Password));
            Assert.False(string.IsNullOrWhiteSpace(updated.Salt));
            Assert.True(SecurityHelper.VerifyPassword("secret", updated.Salt, updated.Password));
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

            var user = new User { UserName = "emptysalt", Password = "secret", IsAdmin = false };
            userService.AddUser(user);

            using (var conn = dbService.CreateConnection())
            using (var cmd = new SQLiteCommand("UPDATE Users SET Salt='' WHERE UserID=@ID", conn))
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

            var user = new User { UserName = "lock", Password = "secret", IsAdmin = false };
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

            var user = new User { UserName = "reset", Password = "secret", IsAdmin = false };
            userService.AddUser(user);

            for (int i = 0; i < 3; i++)
            userService.AuthenticateUser("reset", "bad");

            using (var conn = dbService.CreateConnection())
            using (var cmd = new SQLiteCommand("UPDATE Users SET LockoutUntil=@t WHERE UserID=@id", conn))
            {
                cmd.Parameters.AddWithValue("@t", DateTime.UtcNow.AddMinutes(-1));
                cmd.Parameters.AddWithValue("@id", user.UserID);
                cmd.ExecuteNonQuery();
            }

            var auth = userService.AuthenticateUser("reset", "secret");
            Assert.Equal(AuthenticationResult.Success, auth.Result);
            Assert.NotNull(auth.User);

            var stored = userService.GetAllUsers().First(u => u.UserName == "reset");
            Assert.Equal(0, stored.FailedAttempts);
            Assert.Null(stored.LockoutUntil);
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

            var user = new User { UserName = "flag", Password = "secret", IsAdmin = false };
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

            var user = new User { UserName = "atest", Password = "secret", IsAdmin = false };
            userService.AddUser(user);
            var added = userService.GetUserByID(user.UserID)!;

            Assert.NotEqual("secret", added.Password);
            Assert.False(SecurityHelper.IsSha256Hash(added.Password));
            Assert.False(string.IsNullOrWhiteSpace(added.Salt));
            Assert.True(SecurityHelper.VerifyPassword("secret", added.Salt, added.Password));

            var auth = await userService.AuthenticateUserAsync("atest", "secret");
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

            var user = new User { UserName = "lockasync", Password = "secret", IsAdmin = false };
            userService.AddUser(user);

            for (int i = 0; i < 3; i++)
            {
                var auth = await userService.AuthenticateUserAsync("lockasync", "bad");
                if (i < 2)
                    Assert.Equal(AuthenticationResult.IncorrectPassword, auth.Result);
                else
                    Assert.Equal(AuthenticationResult.LockedOut, auth.Result);
            }

            var stored = userService.GetAllUsers().First(u => u.UserName == "lockasync");
            Assert.Equal(3, stored.FailedAttempts);
            Assert.NotNull(stored.LockoutUntil);

            var afterLock = await userService.AuthenticateUserAsync("lockasync", "secret");
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

            var user = new User { UserName = "areset", Password = "secret", IsAdmin = false };
            await userService.AddUserAsync(user);

            for (int i = 0; i < 3; i++)
                await userService.AuthenticateUserAsync("areset", "bad");

            using (var conn = dbService.CreateConnection())
            using (var cmd = new SQLiteCommand("UPDATE Users SET LockoutUntil=@t WHERE UserID=@id", conn))
            {
                cmd.Parameters.AddWithValue("@t", DateTime.UtcNow.AddMinutes(-1));
                cmd.Parameters.AddWithValue("@id", user.UserID);
                cmd.ExecuteNonQuery();
            }

            var auth = await userService.AuthenticateUserAsync("areset", "secret");
            Assert.Equal(AuthenticationResult.Success, auth.Result);
            Assert.NotNull(auth.User);

            var stored = (await userService.GetAllUsersAsync()).First(u => u.UserName == "areset");
            Assert.Equal(0, stored.FailedAttempts);
            Assert.Null(stored.LockoutUntil);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}

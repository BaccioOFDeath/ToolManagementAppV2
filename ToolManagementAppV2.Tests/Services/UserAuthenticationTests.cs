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

            var legacy = SecurityHelper.ComputeSha256HashLegacy("Strong1!");
            using (var conn = dbService.CreateConnection())
            using (var cmd = new SQLiteCommand("INSERT INTO Users (UserName, PasswordHash, IsAdmin) VALUES (@u,@p,0);", conn))
            {
                cmd.Parameters.AddWithValue("@u", "legacy");
                cmd.Parameters.AddWithValue("@p", legacy);
                cmd.ExecuteNonQuery();
            }

            var auth = userService.AuthenticateUser("legacy", "Strong1!");
            Assert.Equal(AuthenticationResult.Success, auth.Result);
            Assert.NotNull(auth.User);

            var updated = userService.GetUserByID(auth.User!.UserID)!;
            Assert.False(SecurityHelper.IsSha256Hash(updated.PasswordHash));
            Assert.False(string.IsNullOrWhiteSpace(updated.PasswordSalt));
            Assert.True(SecurityHelper.VerifyPassword("Strong1!", updated.PasswordSalt, updated.PasswordHash));
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

            var user = new User { UserName = "emptysalt", PasswordHash = "Strong1!", IsAdmin = false };
            userService.AddUser(user);

            using (var conn = dbService.CreateConnection())
            using (var cmd = new SQLiteCommand("UPDATE Users SET PasswordSalt='' WHERE UserID=@ID", conn))
            {
                cmd.Parameters.AddWithValue("@ID", user.UserID);
                cmd.ExecuteNonQuery();
            }

            var auth = userService.AuthenticateUser("emptysalt", "Strong1!");
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
    public void ChangeUserPassword_Throws_ForDefaultPassword()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());

            var user = new User { UserName = "flag", PasswordHash = "Strong1!", IsAdmin = false };
            userService.AddUser(user);

            Assert.Throws<ArgumentException>(() => userService.ChangeUserPassword(user.UserID, "admin"));
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

            var user = new User { UserName = "atest", PasswordHash = "Strong1!", IsAdmin = false };
            userService.AddUser(user);
            var added = userService.GetUserByID(user.UserID)!;

            Assert.NotEqual("Strong1!", added.PasswordHash);
            Assert.False(SecurityHelper.IsSha256Hash(added.PasswordHash));
            Assert.False(string.IsNullOrWhiteSpace(added.PasswordSalt));
            Assert.True(SecurityHelper.VerifyPassword("Strong1!", added.PasswordSalt, added.PasswordHash));

            var auth = await userService.AuthenticateUserAsync(" atest ", " Strong1! ");
            Assert.Equal(AuthenticationResult.Success, auth.Result);
            Assert.NotNull(auth.User);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

}

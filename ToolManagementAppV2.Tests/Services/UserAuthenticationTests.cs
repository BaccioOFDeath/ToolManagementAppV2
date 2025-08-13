using System.IO;
using System.Linq;
using System.Data.SQLite;
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
            var added = userService.GetAllUsers().First();

            Assert.NotEqual("secret", added.Password);
            Assert.False(SecurityHelper.IsSha256Hash(added.Password));
            Assert.False(string.IsNullOrWhiteSpace(added.Salt));
            Assert.True(SecurityHelper.VerifyPassword("secret", added.Salt, added.Password));

            var auth = userService.AuthenticateUser("test", "secret");
            Assert.NotNull(auth);
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
            Assert.NotNull(auth);

            var updated = userService.GetAllUsers().First(u => u.UserName == "legacy");
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
            Assert.Null(auth);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}

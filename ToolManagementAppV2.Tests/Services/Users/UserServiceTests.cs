using System;
using System.IO;
using System.Threading.Tasks;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Interfaces;
using Xunit;

public class UserServiceTests
{
    [Fact]
    public void TryDeleteUser_ReturnsFalse_WhenDeletingOnlyAdmin()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());

            var admin = new User { UserName = "admin", Password = "pw", IsAdmin = true };
            userService.AddUser(admin);

            var result = userService.TryDeleteUser(admin.UserID);

            Assert.False(result);
            Assert.NotNull(userService.GetUserByID(admin.UserID));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void TryDeleteUser_AllowsDeletingAdmin_WhenMultipleAdminsExist()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());

            var admin1 = new User { UserName = "admin1", Password = "pw", IsAdmin = true };
            var admin2 = new User { UserName = "admin2", Password = "pw", IsAdmin = true };
            userService.AddUser(admin1);
            userService.AddUser(admin2);

            var result = userService.TryDeleteUser(admin1.UserID);

            Assert.True(result);
            Assert.Null(userService.GetUserByID(admin1.UserID));
            Assert.NotNull(userService.GetUserByID(admin2.UserID));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
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

            userService.AddUser(new User { UserName = "dup", Password = "pw" });

            var ex = Assert.Throws<InvalidOperationException>(() =>
                userService.AddUser(new User { UserName = "dup", Password = "pw" }));
            Assert.Contains("username", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
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

            await userService.AddUserAsync(new User { UserName = "dup", Password = "pw" });

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                userService.AddUserAsync(new User { UserName = "dup", Password = "pw" }));
            Assert.Contains("username", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
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

            userService.AddUser(new User { UserName = "list", Password = "pw", Salt = "s" });
            var users = userService.GetAllUsers();
            var user = users[0];
            Assert.Null(user.Password);
            Assert.Null(user.Salt);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
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

            await userService.AddUserAsync(new User { UserName = "list", Password = "pw", Salt = "s" });
            var users = await userService.GetAllUsersAsync();
            var user = users[0];
            Assert.Null(user.Password);
            Assert.Null(user.Salt);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}

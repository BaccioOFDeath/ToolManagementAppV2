using System;
using System.IO;
using System.Threading.Tasks;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Tests.Extensions;
using Xunit;

public class UserServiceTests
{
    [Fact]
    public async Task TryDeleteUserAsync_ReturnsFalse_WhenDeletingOnlyAdmin()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());
            var admin = new User { UserName = "admin", Password = "pw", IsAdmin = true };
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
            var admin1 = new User { UserName = "admin1", Password = "pw", IsAdmin = true };
            var admin2 = new User { UserName = "admin2", Password = "pw", IsAdmin = true };
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
            userService.AddUser(new User { UserName = "dup", Password = "pw" });
            var ex = Assert.Throws<InvalidOperationException>(() => userService.AddUser(new User { UserName = "dup", Password = "pw" }));
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
            await userService.AddUserAsync(new User { UserName = "dup", Password = "pw" });
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.AddUserAsync(new User { UserName = "dup", Password = "pw" }));
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
            var user = new User { UserName = "user1", Password = "pw" };
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
            await userService.AddUserAsync(new User { UserName = "list", Password = "pw", Salt = "s" });
            var users = await userService.GetAllUsersAsync();
            var user = users[0];
            Assert.Null(user.Password);
            Assert.Null(user.Salt);
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
            var user = new User { UserName = "prehashed", Password = hash, Salt = salt, PasswordExpired = true };
            userService.AddUser(user);
            var fetched = userService.GetUserByID(user.UserID)!;
            Assert.Equal(hash, fetched.Password);
            Assert.Equal(salt, fetched.Salt);
            Assert.True(fetched.PasswordExpired);
            Assert.True(SecurityHelper.VerifyPassword("secret", fetched.Salt, fetched.Password));
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task UpdateUserAsync_NonAdmin_Throws()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            var context = new NonAdminContext();
            var userService = new UserService(dbService, context);
            await userService.AddUserAsync(new User { UserName = "u", Password = "p" });
            var user = (await userService.GetAllUsersAsync())[0];
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => userService.UpdateUserAsync(user));
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    class NonAdminContext : IUserContext
    {
        public User? CurrentUser { get; set; } = new User { UserName = "u", IsAdmin = false };
        public event EventHandler<User?>? UserChanged { add { } remove { } }
        public bool IsAdmin => false;
        public string UserName => CurrentUser?.UserName ?? "";
        public string Role => "User";
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using System.Data.SQLite;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Utilities.Helpers;
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
    public async Task AddUserAsync_PromotesFirstUserToAdmin()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            var ctx = new StubUserContext { CurrentUser = new User { UserName = "seed", IsAdmin = false } };
            var auth = new AuthorizationService(ctx);
            var userService = new UserService(dbService, ctx, auth);
            var first = new User { UserName = "admin", PasswordHash = "Strong1!", IsAdmin = false };
            await userService.AddUserAsync(first);
            Assert.NotEqual(0, first.UserID);
            Assert.True(first.IsAdmin);
            var stored = userService.GetUserByID(first.UserID);
            Assert.NotNull(stored);
            Assert.True(stored!.IsAdmin);
            Assert.Equal(DateTimeKind.Local, stored.CreatedAt.Kind);
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
            var admin = new User { UserName = "admin", PasswordHash = "Strong1!", IsAdmin = false };
            await userService.AddUserAsync(admin);
            Assert.True(admin.IsAdmin);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => userService.AddUserAsync(new User { UserName = "user", PasswordHash = "Strong1!" }));
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
            var admin = new User { UserName = "admin", PasswordHash = "Strong1!", IsAdmin = true };
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
            var admin1 = new User { UserName = "admin1", PasswordHash = "Strong1!", IsAdmin = true };
            var admin2 = new User { UserName = "admin2", PasswordHash = "Strong1!", IsAdmin = true };
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
            userService.AddUser(new User { UserName = "dup", PasswordHash = "Strong1!" });
            var ex = Assert.Throws<InvalidOperationException>(() => userService.AddUser(new User { UserName = "dup", PasswordHash = "Strong1!" }));
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
            await userService.AddUserAsync(new User { UserName = "dup", PasswordHash = "Strong1!" });
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.AddUserAsync(new User { UserName = "dup", PasswordHash = "Strong1!" }));
            Assert.Contains("username", ex.Message, StringComparison.OrdinalIgnoreCase);
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
            var result = userService.ChangeUserPassword(9999, "Newpass1!");
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
            var result = await userService.ChangeUserPasswordAsync(9999, "Newpass1!");
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
            var user = new User { UserName = "trim", PasswordHash = "Strong1!" };
            await userService.AddUserAsync(user);
            var result = await userService.ChangeUserPasswordAsync(user.UserID, "  Newpass1!  ");
            Assert.True(result);
            var auth = await userService.AuthenticateUserAsync("trim", "Newpass1!");
            Assert.Equal(AuthenticationResult.Success, auth.Result);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ChangeUserPasswordAsync_Throws_WhenPasswordIsWhitespace()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());
            var user = new User { UserName = "blank", PasswordHash = "Strong1!" };
            await userService.AddUserAsync(user);
            await Assert.ThrowsAsync<ArgumentException>(() => userService.ChangeUserPasswordAsync(user.UserID, "   "));
            var auth = await userService.AuthenticateUserAsync("blank", "Strong1!");
            Assert.Equal(AuthenticationResult.Success, auth.Result);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }


    [Fact]
    public async Task AddUserAsync_Throws_WhenPasswordEmpty()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            var userService = new UserService(dbService, new ApplicationUserContext());
            await Assert.ThrowsAsync<ArgumentException>(() => userService.AddUserAsync(new User { UserName = "nopass" }));
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
            userService.AddUser(new User { UserName = "list", PasswordHash = "Strong1!" });
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
            await userService.AddUserAsync(new User { UserName = "list", PasswordHash = "Strong1!" });
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
    public void AddUser_HashesPassword_WhenProvided()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());
            var user = new User { UserName = "prehashed", PasswordHash = "Strong1!", PasswordExpired = true };
            userService.AddUser(user);
            var fetched = userService.GetUserByID(user.UserID)!;
            Assert.True(SecurityHelper.VerifyPassword("Strong1!", fetched.PasswordSalt, fetched.PasswordHash));
            Assert.True(fetched.PasswordExpired);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }
}

using System.IO;
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
}

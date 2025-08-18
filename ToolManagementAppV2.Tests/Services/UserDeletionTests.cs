using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Interfaces;
using Xunit;

public class UserDeletionTests
{
    [Fact]
    public async Task Deleting_Last_Admin_Is_Blocked()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());

            var admin = new User { UserName = "admin", PasswordHash = "Strong1!", IsAdmin = true };
            await userService.AddUserAsync(admin);

            var added = (await userService.GetAllUsersAsync()).First();
            var result = await userService.TryDeleteUserAsync(added.UserID);
            Assert.False(result);
            Assert.Single(await userService.GetAllUsersAsync());
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}

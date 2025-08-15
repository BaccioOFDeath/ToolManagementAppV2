using System;
using System.IO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Interfaces;
using Xunit;

public class UserDeletionTests
{
    [Fact]
    public void Deleting_Last_Admin_Is_Blocked()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            IUserService userService = new UserService(dbService, new ApplicationUserContext());

            var admin = new User { UserName = "admin", Password = "pw", IsAdmin = true };
            userService.AddUser(admin);

            var added = userService.GetAllUsers().First();
            Assert.Throws<InvalidOperationException>(() => userService.DeleteUser(added.UserID));
            Assert.Single(userService.GetAllUsers());
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}

using System.IO;
using System.Linq;
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
            IUserService userService = new UserService(dbService);

            var user = new User { UserName = "test", Password = "secret", IsAdmin = false };
            userService.AddUser(user);
            var added = userService.GetAllUsers().First();

            Assert.NotEqual("secret", added.Password);
            Assert.Equal(SecurityHelper.ComputeSha256Hash("secret"), added.Password);

            var auth = userService.AuthenticateUser("test", "secret");
            Assert.NotNull(auth);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}

using System.IO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Interfaces;
using Xunit;

namespace ToolManagementAppV2.Tests.Services
{
    public class UserMobileTests
    {
        [Fact]
        public void AddUser_PersistsMobile()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IUserService userService = new UserService(dbService);

                var user = new User { UserName = "u", Password = "p", Mobile = "111" };
                userService.AddUser(user);

                var added = userService.GetAllUsers().First();
                Assert.Equal("111", added.Mobile);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void UpdateUser_PersistsMobile()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IUserService userService = new UserService(dbService);

                var user = new User { UserName = "u", Password = "p", Mobile = "1" };
                userService.AddUser(user);
                var added = userService.GetAllUsers().First();

                added.Mobile = "2";
                userService.UpdateUser(added);

                var updated = userService.GetUserByID(added.UserID);
                Assert.Equal("2", updated.Mobile);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}

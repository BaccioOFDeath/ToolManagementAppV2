using System.IO;
using System.Linq;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Interfaces;
using Xunit;

namespace InventoryManagementApp.Tests.Services
{
    public class UserMobileTests
    {
        [Fact]
        public void AddUser_PersistsMobile()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using var dbService = new DatabaseService(dbPath);
                IUserService userService = new UserService(dbService, new ApplicationUserContext());

                var user = new User { UserName = "u", PasswordHash = "Strong1!", Mobile = "111" };
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
                using var dbService = new DatabaseService(dbPath);
                IUserService userService = new UserService(dbService, new ApplicationUserContext());

                var user = new User { UserName = "u", PasswordHash = "Strong1!", Mobile = "1" };
                userService.AddUser(user);
                var added = userService.GetAllUsers().First();

                added.Mobile = "2";
                userService.UpdateUser(added);

                var updated = userService.GetUserByID(added.UserID);
                Assert.NotNull(updated);
                Assert.Equal("2", updated!.Mobile);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}

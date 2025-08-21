using System.IO;
using System.Linq;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Tests;
using Xunit;

namespace InventoryManagementApp.Tests.Services
{
    public class UserPasswordRetentionTests
    {
        [Fact]
        public void UpdateUser_KeepsExistingHashAndSalt_WhenPasswordNotChanged()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db, new ApplicationUserContext());

                var user = new User { UserName = "u", PasswordHash = "Strong1!" };
                userService.AddUser(user);

                var originalHash = user.PasswordHash;
                var originalSalt = user.PasswordSalt;

                var loaded = userService.GetAllUsers().First();
                loaded.Email = "new@example.com";

                userService.UpdateUser(loaded);

                var updated = userService.GetUserByID(loaded.UserID);
                Assert.NotNull(updated);
                Assert.Equal(originalHash, updated!.PasswordHash);
                Assert.Equal(originalSalt, updated.PasswordSalt);
                Assert.Equal("new@example.com", updated.Email);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}


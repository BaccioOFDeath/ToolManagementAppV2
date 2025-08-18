using System;
using System.IO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Interfaces;
using Xunit;

namespace ToolManagementAppV2.Tests.Services
{
    public class UserActiveTests
    {
        [Fact]
        public void AddUser_SetsCreatedAtAndIsActive()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db, new ApplicationUserContext());
                var user = new User { UserName = "u", PasswordHash = "Strong1!" };
                userService.AddUser(user);
                var added = userService.GetAllUsers().First();
                Assert.True(added.IsActive);
                Assert.True((DateTime.UtcNow - added.CreatedAt).TotalMinutes < 5);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void UpdateUser_PersistsIsActive()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db, new ApplicationUserContext());
                var user = new User { UserName = "u", PasswordHash = "Strong1!" };
                userService.AddUser(user);
                var added = userService.GetAllUsers().First();
                added.IsActive = false;
                userService.UpdateUser(added);
                var updated = userService.GetUserByID(added.UserID);
                Assert.NotNull(updated);
                Assert.False(updated!.IsActive);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}

using System;
using System.IO;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class LoginWindowBehaviorTests
    {
        [Fact]
        public void LoginWindow_IsTopmost()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            using var db = new DatabaseService(dbPath);
            var userContext = new ApplicationUserContext();
            var userService = new UserService(db, userContext);
            var settingsService = new SettingsService(db);
            var window = new LoginWindow(userContext, userService, settingsService);
            Assert.True(window.Topmost);
            window.Close();
        }
    }
}

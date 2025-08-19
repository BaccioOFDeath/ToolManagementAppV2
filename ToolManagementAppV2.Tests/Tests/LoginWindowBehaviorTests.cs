using System;
using System.IO;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2;
using ToolManagementAppV2.Views.Windows;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace ToolManagementAppV2.Tests.Tests
{
    public class LoginWindowBehaviorTests
    {
        [Fact]
        public void LoginWindow_IsNotTopmost()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            using var db = new DatabaseService(dbPath);
            var userContext = new ApplicationUserContext();
            var userService = new UserService(db, userContext);
            var settingsService = new SettingsService(db);
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var vm = new LoginViewModel(userService, settingsService, new DialogService(serviceProvider), userContext);
            var window = new LoginWindow(vm);
            Assert.False(window.Topmost);
            window.Close();
        }
    }
}

using System;
using System.IO;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Services;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp;
using InventoryManagementApp.Views.Windows;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagementApp.Tests.Tests
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

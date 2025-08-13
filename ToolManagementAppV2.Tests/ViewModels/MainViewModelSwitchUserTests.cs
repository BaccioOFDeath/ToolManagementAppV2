using System;
using System.IO;
using System.Windows;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.ViewModels;
using Xunit;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class MainViewModelSwitchUserTests
    {
        [Fact]
        public void SwitchUserCommand_UpdatesCurrentUser()
        {
            if (Application.Current == null)
                new Application();

            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var userContext = new ApplicationUserContext();
                var userService = new UserService(db, userContext);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);

                var newUser = new User { UserName = "newuser", IsAdmin = true };
                Func<bool> stubLogin = () =>
                {
                    userContext.CurrentUser = newUser;
                    return true;
                };

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService,
                    new StubFileDialogService(), activityLogService, settingsService, db, null, stubLogin);

                userContext.CurrentUser = new User { UserName = "old", IsAdmin = false };
                vm.RefreshCurrentUser();

                vm.SwitchUserCommand.Execute(null);

                Assert.Equal("newuser", vm.CurrentUserName);
                Assert.True(vm.IsCurrentUserAdmin);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }

    class StubFileDialogService : IFileDialogService
    {
        public string OpenFile(string filter) => null;
        public string SaveFile(string filter) => null;
    }
}

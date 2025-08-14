using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Helpers;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class LoginViewModelTests
    {
        [Fact]
        public void SelectUserCommand_SetsCurrentUser()
        {
            if (Application.Current == null)
                new Application();

            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                using var dbService = new DatabaseService(dbPath);
                var userContext = new ApplicationUserContext();
                var userService = new UserService(dbService, userContext);
                var settingsService = new SettingsService(dbService);
                userService.AddUser(new User { UserName = "user", Password = "newpassword", IsAdmin = false });

                var vm = new LoginViewModel(userService, settingsService, new StubDialogService(), userContext);
                bool success = false;
                vm.LoginSucceeded += (_, __) => success = true;

                vm.SelectUserCommand.Execute(vm.Users.First());

                Assert.True(success);
                Assert.NotNull(userContext.CurrentUser);
                Assert.Equal("user", userContext.UserName);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void SelectUserCommand_PromptsForPasswordChange_WhenExpired()
        {
            if (Application.Current == null)
                new Application();

            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                using var dbService = new DatabaseService(dbPath);
                var userContext = new ApplicationUserContext();
                var userService = new UserService(dbService, userContext);
                var settingsService = new SettingsService(dbService);
                var user = new User { UserName = "user", Password = "newpassword", IsAdmin = false, PasswordExpired = true };
                userService.AddUser(user);

                var vm = new LoginViewModel(userService, settingsService, new StubDialogService(), userContext)
                {
                    PromptForNewPassword = () => "changed"
                };
                bool success = false;
                vm.LoginSucceeded += (_, __) => success = true;

                vm.SelectUserCommand.Execute(vm.Users.First());

                Assert.True(success);
                var updated = userService.GetUserByID(user.UserID)!;
                Assert.False(updated.PasswordExpired);
                Assert.True(SecurityHelper.VerifyPassword("changed", updated.Salt, updated.Password));
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }

    class StubDialogService : IDialogService
    {
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => true;
       public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
        public void ShowToolDetails(ToolModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ToolModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
        public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
        public System.Func<ToolModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
        public void ShowScannerStatus() { }
    }
}

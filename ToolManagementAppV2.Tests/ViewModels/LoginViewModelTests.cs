using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Threading;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class LoginViewModelTests
    {
        [Fact]
        public async Task SelectUserCommand_SetsCurrentUser()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                using var dbService = new DatabaseService(dbPath);
                var userContext = new ApplicationUserContext();
                var userService = new UserService(dbService, userContext);
                var settingsService = new SettingsService(dbService);
                userService.AddUser(new User { UserName = "user", Password = "newpassword", IsAdmin = false });

                var vm = new LoginViewModel(userService, settingsService, new StubDialogService(), userContext);
                await vm.LoadUsersCommand.ExecuteAsync(null);
                bool success = false;
                vm.LoginSucceeded += (_, __) => success = true;

                await vm.SelectUserCommand.ExecuteAsync(vm.Users.First());

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
        public async Task SelectUserCommand_PromptsForPasswordChange_WhenExpired()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

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
                await vm.LoadUsersCommand.ExecuteAsync(null);
                bool success = false;
                vm.LoginSucceeded += (_, __) => success = true;

                await vm.SelectUserCommand.ExecuteAsync(vm.Users.First());

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

        [Fact]
        public async Task SelectUserCommand_PromptsForPassword_AuthenticatesAdmin()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                using var dbService = new DatabaseService(dbPath);
                var userContext = new ApplicationUserContext();
                var userService = new UserService(dbService, userContext);
                var settingsService = new SettingsService(dbService);
                userService.AddUser(new User { UserName = "admin", Password = "secret", IsAdmin = true });

                var vm = new LoginViewModel(userService, settingsService, new StubDialogService(), userContext)
                {
                    PromptForPasswordAsync = (u, ct) =>
                        Task.FromResult<PasswordPromptResult?>(new PasswordPromptResult("secret", false))
                };
                await vm.LoadUsersCommand.ExecuteAsync(null);
                bool success = false;
                vm.LoginSucceeded += (_, __) => success = true;

                await vm.SelectUserCommand.ExecuteAsync(vm.Users.First());

                Assert.True(success);
                Assert.Equal("admin", userContext.UserName);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SelectUserCommand_CanBeCancelled()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                using var dbService = new DatabaseService(dbPath);
                var userContext = new ApplicationUserContext();
                var userService = new UserService(dbService, userContext);
                var settingsService = new SettingsService(dbService);
                userService.AddUser(new User { UserName = "admin", Password = "secret", IsAdmin = true });

                var vm = new LoginViewModel(userService, settingsService, new StubDialogService(), userContext)
                {
                    PromptForPasswordAsync = async (u, ct) =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), ct);
                        return new PasswordPromptResult("secret", false);
                    }
                };
                await vm.LoadUsersCommand.ExecuteAsync(null);

                var execTask = vm.SelectUserCommand.ExecuteAsync(vm.Users.First());
                vm.SelectUserCommand.Cancel();

                await Assert.ThrowsAsync<OperationCanceledException>(async () => await execTask);
                Assert.Null(userContext.CurrentUser);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void ChangePasswordWindow_Dispose_ClosesWindow()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var win = new ChangePasswordWindow();
            var closed = false;
            win.Closed += (_, __) => closed = true;

            win.Dispose();

            Assert.True(closed);
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

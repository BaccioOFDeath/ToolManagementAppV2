using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using Microsoft.Extensions.Logging;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class MainViewModelSwitchUserTests
    {
        [Fact]
        public async Task SwitchUserCommand_UpdatesCurrentUser()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

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
                Func<Task<bool>> stubLogin = () =>
                {
                    userContext.CurrentUser = newUser;
                    return Task.FromResult(true);
                };

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService,
                    new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService(), null, stubLogin);

                userContext.CurrentUser = new User { UserName = "old", IsAdmin = false };
                Assert.Equal("old", vm.CurrentUserName);
                Assert.False(vm.IsCurrentUserAdmin);

                await vm.SwitchUserCommand.ExecuteAsync(null);

                Assert.Equal("newuser", vm.CurrentUserName);
                Assert.True(vm.IsCurrentUserAdmin);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SwitchUserCommand_ShowsWarning_WhenLoginCancelled()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

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

                var dialog = new StubDialogService();
                var logger = new StubLogger<MainViewModel>();

                Func<Task<bool>> stubLogin = () => Task.FromResult(false);

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService,
                    new StubFileDialogService(), activityLogService, settingsService, db, dialog, logger, stubLogin);

                await vm.SwitchUserCommand.ExecuteAsync(null);

                Assert.True(dialog.InfoShown);
                Assert.Equal("Switch user cancelled.", logger.LastWarning);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SwitchUserCommand_ShowsError_WhenLoginThrows()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

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

                var dialog = new StubDialogService();
                var logger = new StubLogger<MainViewModel>();

                Func<Task<bool>> stubLogin = () => throw new InvalidOperationException("boom");

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService,
                    new StubFileDialogService(), activityLogService, settingsService, db, dialog, logger, stubLogin);

                await vm.SwitchUserCommand.ExecuteAsync(null);

                Assert.True(dialog.InfoShown);
                Assert.Equal("Switch user failed.", logger.LastError);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SwitchUserCommand_DoesNotAlterPreviousUserPassword()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

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

                var oldUser = new User { UserName = "old", PasswordHash = "OldPass1!" };
                await userService.AddUserAsync(oldUser);
                var oldHash = oldUser.PasswordHash;
                var oldSalt = oldUser.PasswordSalt;

                var newUser = new User { UserName = "new", PasswordHash = "NewPass1!", PasswordExpired = true };
                await userService.AddUserAsync(newUser);

                userContext.CurrentUser = oldUser;

                Func<Task<bool>> stubLogin = async () =>
                {
                    var loginVm = new LoginViewModel(userService, settingsService, new StubDialogService(), userContext)
                    {
                        PromptForPasswordAsync = (u, ct) => Task.FromResult<PasswordPromptResult?>(new PasswordPromptResult("NewPass1!", false)),
                        PromptForNewPassword = () => "Changed1!"
                    };
                    await loginVm.SelectUserCommand.ExecuteAsync(newUser);
                    return true;
                };

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService,
                    new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService(), null, stubLogin);

                await vm.SwitchUserCommand.ExecuteAsync(null);

                var oldFromDb = await userService.GetUserByIDAsync(oldUser.UserID);
                Assert.Equal(oldHash, oldFromDb.PasswordHash);
                Assert.Equal(oldSalt, oldFromDb.PasswordSalt);

                var authOld = await userService.AuthenticateUserAsync("old", "OldPass1!");
                Assert.Equal(AuthenticationResult.Success, authOld.Result);
                var authOldChanged = await userService.AuthenticateUserAsync("old", "Changed1!");
                Assert.Equal(AuthenticationResult.IncorrectPassword, authOldChanged.Result);

                var authNew = await userService.AuthenticateUserAsync("new", "Changed1!");
                Assert.Equal(AuthenticationResult.Success, authNew.Result);
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
        public string OpenFile(string filter, string? initialDirectory = null) => null;
        public string SaveFile(string filter) => null;
    }

    class StubDialogService : IDialogService
    {
        public bool InfoShown { get; private set; }
        public void ShowInfo(string message, string title) => InfoShown = true;
        public bool ShowConfirmation(string message, string title) => false;
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

    class StubLogger<T> : ILogger<T>
    {
        public string? LastWarning { get; private set; }
        public string? LastError { get; private set; }

        public IDisposable BeginScope<TState>(TState state) => NullDisposable.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel == LogLevel.Warning)
                LastWarning = formatter(state, exception);
            if (logLevel == LogLevel.Error)
                LastError = formatter(state, exception);
        }

        private sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new NullDisposable();
            public void Dispose() { }
        }
    }
}

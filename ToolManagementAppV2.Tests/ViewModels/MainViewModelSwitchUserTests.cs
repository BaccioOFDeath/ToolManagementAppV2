using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
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
                    new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService(), null, stubLogin);

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

        [Fact]
        public void SwitchUserCommand_ShowsWarning_WhenLoginCancelled()
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

                var dialog = new StubDialogService();
                var logger = new StubLogger<MainViewModel>();

                Func<bool> stubLogin = () => false;

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService,
                    new StubFileDialogService(), activityLogService, settingsService, db, dialog, logger, stubLogin);

                vm.SwitchUserCommand.Execute(null);

                Assert.True(dialog.InfoShown);
                Assert.Equal("Switch user cancelled.", logger.LastWarning);
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

    class StubDialogService : IDialogService
    {
        public bool InfoShown { get; private set; }
        public void ShowInfo(string message, string title) => InfoShown = true;
        public bool ShowConfirmation(string message, string title) => false;
        public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
        public void ShowToolDetails(ToolModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
    }

    class StubLogger<T> : ILogger<T>
    {
        public string? LastWarning { get; private set; }

        public IDisposable BeginScope<TState>(TState state) => NullDisposable.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel == LogLevel.Warning)
                LastWarning = formatter(state, exception);
        }

        private sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new NullDisposable();
            public void Dispose() { }
        }
    }
}

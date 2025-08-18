using System;
using System.IO;
using System.Windows;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class MainViewModelTests
    {
        [Fact]
        public void ExitCommand_ShowsError_WhenShutdownThrows()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var app = System.Windows.Application.Current;
            void ThrowOnExit(object? sender, ExitEventArgs e) => throw new InvalidOperationException("fail");
            app.Exit += ThrowOnExit;

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

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService,
                    new StubFileDialogService(), activityLogService, settingsService, db, dialog);

                vm.ExitCommand.Execute(null);

                Assert.True(dialog.InfoShown);
            }
            finally
            {
                app.Exit -= ThrowOnExit;
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void OpenPrintLabelWindow_ShowsError_WhenDialogFails()
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
                var dialog = new StubDialogService { ThrowOnShowPrintLabelDialog = true };

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService,
                    new StubFileDialogService(), activityLogService, settingsService, db, dialog);

                vm.OpenPrintLabelWindowCommand.Execute(null);

                Assert.True(dialog.InfoShown);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task OpenUsersCommand_ShowsError_WhenGetAllUsersFails()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new GetAllUsersFailingUserService();
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);
                var dialog = new StubDialogService();

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService,
                    new StubFileDialogService(), activityLogService, settingsService, db, dialog);

                var ex = await Record.ExceptionAsync(() => vm.OpenUsersCommand.ExecuteAsync(null));

                Assert.Null(ex);
                Assert.True(dialog.InfoShown);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }

    class StubDialogService : IDialogService
    {
        public bool InfoShown { get; private set; }
        public bool ThrowOnShowPrintLabelDialog { get; set; }
        public void ShowInfo(string message, string title) => InfoShown = true;
        public bool ShowConfirmation(string message, string title) => false;
        public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
        public void ShowToolDetails(ToolModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, System.Collections.Generic.IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ToolModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
        public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
        public System.Func<ToolModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog()
        {
            if (ThrowOnShowPrintLabelDialog)
                throw new InvalidOperationException("fail");
        }
        public void ShowScannerStatus() { }
    }

    class StubFileDialogService : IFileDialogService
    {
        public string OpenFile(string filter, string? initialDirectory = null) => null;
        public string SaveFile(string filter) => null;
    }
}

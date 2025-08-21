using System;
using System.IO;
using System.Windows;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
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
                var toolService = new ItemService(db);
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
                var toolService = new ItemService(db);
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
                var toolService = new ItemService(db);
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
        public ItemModel? ShowEditItemDialog(ItemModel item) => null;
        public void ShowItemDetails(ItemModel item) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, System.Collections.Generic.IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel item, System.Collections.Generic.IEnumerable<RentalModel> history) { }
        public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
        public System.Func<ItemModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog()
        {
            if (ThrowOnShowPrintLabelDialog)
                throw new InvalidOperationException("fail");
        }
    }

    class StubFileDialogService : IFileDialogService
    {
        public string OpenFile(string filter, string? initialDirectory = null) => null;
        public string SaveFile(string filter) => null;
    }
}

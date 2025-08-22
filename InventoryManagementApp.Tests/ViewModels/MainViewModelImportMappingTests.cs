using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
    public class MainViewModelImportMappingTests
    {
        [Fact]
        public async Task OpenImportMappingWindowAsync_LogsSelectedHeadersAndMapping()
        {
            var dbPath = Path.GetTempFileName();
            var csvPath = Path.GetTempFileName();
            File.WriteAllText(csvPath, "ItemNumber\n");
            var logs = new List<LogEntry>();
            try
            {
                using var factory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
                var db = new DatabaseService(dbPath);
                var itemService = new ItemService(db);
                var userContext = new ApplicationUserContext();
                var userService = new UserService(db, userContext);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);
                var fileDlg = new StubFileDialogService { FileToReturn = csvPath };
                var dialog = new StubDialogService { MapToReturn = new Dictionary<string,string>{{"ItemNumber","ItemNumber"}} };
                var vm = new MainViewModel(itemService, userService, userContext, customerService, rentalService,
                    fileDlg, activityLogService, settingsService, db, dialog,
                    logger: factory.CreateLogger<MainViewModel>());

                await vm.OpenImportMappingWindowCommand.ExecuteAsync(null);

                Assert.Contains(logs, l => l.Message.Contains("Import mapping selected") && l.Message.Contains("ItemNumber -> ItemNumber"));
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
                if (File.Exists(csvPath)) File.Delete(csvPath);
            }
        }

        class StubDialogService : IDialogService
        {
            public Dictionary<string,string>? MapToReturn { get; set; }
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => false;
            public ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public void ShowItemDetails(ItemModel item) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => MapToReturn;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }

        class StubFileDialogService : IFileDialogService
        {
            public string? FileToReturn { get; set; }
            public string OpenFile(string filter, string? initialDirectory = null) => FileToReturn ?? string.Empty;
            public string SaveFile(string filter) => string.Empty;
        }
    }
}

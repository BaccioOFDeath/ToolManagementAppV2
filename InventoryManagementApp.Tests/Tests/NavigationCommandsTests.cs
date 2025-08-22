using System.IO;
using InventoryManagementApp;
using InventoryManagementApp.Tests;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.Views.Windows;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementApp.Tests.Tests
{
    public class NavigationCommandsTests
    {
        [Fact]
        public async Task OpenSearchItemsCommand_NavigatesToItemSearchPage()
        {
            var (window, dbPath) = TestHelpers.CreateMainWindow();
            try
            {
                var vm = Assert.IsType<MainViewModel>(window.DataContext);

                await vm.OpenSearchItemsCommand.ExecuteAsync(null);

                Assert.IsType<ItemSearchPage>(vm.CurrentPage);
            }
            finally
            {
                window.Close();
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task OpenSearchItemsCommand_LoadsItemsAndSetsDataContext()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using var db = new DatabaseService(dbPath);
                IItemService itemService = new ItemService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new UserService(db, userContext);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);
                itemService.AddItem(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" });

                var vm = new MainViewModel(itemService, userService, userContext, customerService, rentalService, new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService());
                await vm.OpenSearchItemsCommand.ExecuteAsync(null);

                var page = Assert.IsType<ItemSearchPage>(vm.CurrentPage);
                Assert.Same(vm.ItemManagement, page.DataContext);
                Assert.Same(vm.ItemManagement.Items, page.ItemsList.ItemsSource);
                Assert.NotEmpty(vm.ItemManagement.Items);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
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
    public void ShowInfo(string message, string title) { }
    public bool ShowConfirmation(string message, string title) => false;
    public ItemModel? ShowEditItemDialog(ItemModel item) => null;
    public void ShowItemDetails(ItemModel item) { }
    public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
    public CustomerModel? ShowAddCustomerDialog() => null;
    public void ShowRentalsFilter(InventoryManagementApp.ViewModels.ManageRentalsViewModel viewModel) { }
    public void ShowRentalHistory(ItemModel item, System.Collections.Generic.IEnumerable<RentalModel> history) { }
    public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties, System.Collections.Generic.IEnumerable<string>? requiredPropertyNames = null) => null;
    public System.Func<ItemModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
    public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
    public void ShowPrintLabelDialog() { }
}

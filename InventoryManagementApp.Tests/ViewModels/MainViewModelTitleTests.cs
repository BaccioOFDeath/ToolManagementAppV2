using System;
using System.IO;
using System.Windows;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Helpers;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class MainViewModelTitleTests
    {
        [Fact]
        public void WindowTitle_Updates_WhenApplicationNameChanges()
        {
            if (Application.Current == null)
                new Application();

            var dbPath = Path.GetTempFileName();
            var originalSingular = LabelProvider.Instance.ItemLabelSingular;
            var originalPlural = LabelProvider.Instance.ItemLabelPlural;
            LabelProvider.Instance.UpdateLabels("ItemModel", "Items");
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService itemService = new ItemService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new UserService(db, userContext);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);

                var vm = new MainViewModel(itemService, userService, userContext, customerService, rentalService,
                    new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService());

                Assert.Equal("Items Management", vm.WindowTitle);

                vm.Settings.ApplicationName = "My App";
                Assert.Equal("My App", vm.WindowTitle);

                vm.Settings.ApplicationName = string.Empty;
                Assert.Equal("Items Management", vm.WindowTitle);
            }
            finally
            {
                LabelProvider.Instance.UpdateLabels(originalSingular, originalPlural);
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}

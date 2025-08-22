using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using InventoryManagementApp;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Tests
{
    public static class TestHelpers
    {
        public static (MainWindow window, string dbPath) CreateMainWindow()
        {
            var dbPath = Path.GetTempFileName();
            var db = new DatabaseService(dbPath);
            var auth = new AllowAllAuthorizationService();
            var itemService = new ItemService(db, auth);
            var customerService = new CustomerService(db, auth);
            var userContext = new ApplicationUserContext();
            var userService = new UserService(db, userContext, auth);
            var rentalService = new RentalService(db, auth, itemService);
            var activityLogService = new ActivityLogService(db);
            var fileDialogService = new StubFileDialogService();
            var settingsService = new SettingsService(db, auth);
            var dialogService = new StubDialogService();
            var vm = new MainViewModel(itemService, userService, userContext, customerService, rentalService,
                fileDialogService, activityLogService, settingsService, db, dialogService);
            var window = new MainWindow(vm, db);
            return (window, dbPath);
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
                yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T t)
                    yield return t;

                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
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
    }
}

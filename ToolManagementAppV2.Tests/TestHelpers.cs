using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using ToolManagementAppV2;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2.Tests
{
    public static class TestHelpers
    {
        public static (MainWindow window, string dbPath) CreateMainWindow()
        {
            var dbPath = Path.GetTempFileName();
            var db = new DatabaseService(dbPath);
            var auth = new AllowAllAuthorizationService();
            var toolService = new ToolService(db, auth);
            var customerService = new CustomerService(db, auth);
            var userContext = new ApplicationUserContext();
            var userService = new UserService(db, userContext, auth);
            var rentalService = new RentalService(db, auth, toolService);
            var activityLogService = new ActivityLogService(db);
            var fileDialogService = new StubFileDialogService();
            var settingsService = new SettingsService(db, auth);
            var dialogService = new StubDialogService();
            var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService,
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
        }
    }
}

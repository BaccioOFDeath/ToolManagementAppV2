using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Items;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class MainViewModelDisposalTests
    {
        [Fact]
        public void Dispose_RemovesToolManagementPropertyChangedHandler()
        {
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
                var fileDialogService = new StubFileDialogService();
                var dialogService = new StubDialogService();

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService,
                    fileDialogService, activityLogService, settingsService, db, dialogService);

                var field = typeof(ObservableObject).GetField("PropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
                var handlersBefore = ((MulticastDelegate?)field?.GetValue(vm.ItemManagement))?.GetInvocationList() ?? Array.Empty<Delegate>();
                Assert.Contains(handlersBefore, h => ReferenceEquals(h.Target, vm));

                vm.Dispose();

                var handlersAfter = ((MulticastDelegate?)field?.GetValue(vm.ItemManagement))?.GetInvocationList() ?? Array.Empty<Delegate>();
                Assert.DoesNotContain(handlersAfter, h => ReferenceEquals(h.Target, vm));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void RepeatedCreation_DoesNotCauseMemoryGrowth()
        {
            long before = GC.GetTotalMemory(true);

            for (int i = 0; i < 50; i++)
            {
                var dbPath = Path.GetTempFileName();
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var userContext = new ApplicationUserContext();
                var userService = new UserService(db, userContext);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);
                var fileDialogService = new StubFileDialogService();
                var dialogService = new StubDialogService();

                using var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService,
                    fileDialogService, activityLogService, settingsService, db, dialogService);

                db.Dispose();
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long after = GC.GetTotalMemory(true);
            Assert.True(after - before < 5_000_000);
        }

        class StubDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => false;
            public ToolModel? ShowEditItemDialog(ToolModel tool) => null;
            public void ShowItemDetails(ToolModel tool) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ToolModel tool, System.Collections.Generic.IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ToolModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
            public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
            public Func<ToolModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }

        class StubFileDialogService : IFileDialogService
        {
            public string OpenFile(string filter, string? initialDirectory = null) => null;
            public string SaveFile(string filter) => null;
        }
    }
}


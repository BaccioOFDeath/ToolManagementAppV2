using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Views;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Settings;
using Xunit;


namespace ToolManagementAppV2.Tests.ViewModels
{
    public class MainViewModelNavigationTests
    {
        [Fact]
        public void Constructor_ComposesSubViewModels()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new UserService(db, userContext);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService, new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService());

                Assert.NotNull(vm.ToolManagement);
                Assert.NotNull(vm.UserManagement);
                Assert.NotNull(vm.CustomerManagement);
                Assert.NotNull(vm.ManageRentals);
                Assert.NotNull(vm.ImportExport);
                Assert.NotNull(vm.ActivityLogs);
                Assert.NotNull(vm.Reports);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void OpenDashboardCommand_NavigatesToDashboardPage()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new UserService(db, userContext);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService, new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService());
                vm.OpenDashboardCommand.Execute(null);

                var page = Assert.IsType<DashboardPage>(vm.CurrentPage);
                Assert.IsType<DashboardViewModel>(page.DataContext);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void OpenImportExportCommand_SetsDataContext()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new UserService(db, userContext);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService, new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService());
                vm.OpenImportExportCommand.Execute(null);

                var page = Assert.IsType<ImportExportPage>(vm.CurrentPage);
                Assert.IsType<ImportExportViewModel>(page.DataContext);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task OpenActivityLogsCommand_LoadsLogsAsync()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var activityLogService = new ActivityLogService(db);
                await activityLogService.LogActionAsync(1, "user", "action");
                IToolService toolService = new ToolService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new UserService(db, userContext);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var settingsService = new SettingsService(db);

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService, new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService());
                await vm.OpenActivityLogsCommand.ExecuteAsync(null);

                var page = Assert.IsType<ActivityLogsPage>(vm.CurrentPage);
                var logsVm = Assert.IsType<ActivityLogsViewModel>(page.DataContext);
                Assert.NotEmpty(logsVm.Logs);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void OpenReportsCommand_SetsDataContext()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var activityLogService = new ActivityLogService(db);
                IToolService toolService = new ToolService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new UserService(db, userContext);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var settingsService = new SettingsService(db);

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService, new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService());
                vm.OpenReportsCommand.Execute(null);

                var page = Assert.IsType<ReportsPage>(vm.CurrentPage);
                Assert.IsType<ReportsViewModel>(page.DataContext);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void OpenSettingsCommand_NavigatesToSettingsPage()
        {
            var dbPath = Path.GetTempFileName();
            var tempDb = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tool_inventory.db");
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new UserService(db, userContext);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService, new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService());
                vm.OpenSettingsCommand.Execute(null);

                var page = Assert.IsType<SettingsPage>(vm.CurrentPage);
                var settingsVm = Assert.IsType<SettingsViewModel>(page.DataContext);
                var field = typeof(SettingsViewModel).GetField("_settingsService", BindingFlags.NonPublic | BindingFlags.Instance);
                var svc = field!.GetValue(settingsVm);
                Assert.Same(settingsService, svc);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
                if (File.Exists(tempDb))
                    File.Delete(tempDb);
            }
        }

        [Fact]
        public void OpenSettingsCommand_ReusesSettingsViewModelInstance()
        {
            var dbPath = Path.GetTempFileName();
            var tempDb = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tool_inventory.db");
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new UserService(db, userContext);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService, new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService());

                vm.OpenSettingsCommand.Execute(null);
                var firstPage = Assert.IsType<SettingsPage>(vm.CurrentPage);
                var firstVm = Assert.IsType<SettingsViewModel>(firstPage.DataContext);
                firstVm.ApplicationName = "My App";

                vm.OpenDashboardCommand.Execute(null);
                vm.OpenSettingsCommand.Execute(null);
                var secondPage = Assert.IsType<SettingsPage>(vm.CurrentPage);
                var secondVm = Assert.IsType<SettingsViewModel>(secondPage.DataContext);

                Assert.Same(firstVm, secondVm);
                Assert.Equal("My App", secondVm.ApplicationName);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
                if (File.Exists(tempDb))
                    File.Delete(tempDb);
            }
        }

        [Fact]
        public async Task OpenRentalsCommand_NavigatesToManageRentalsPage()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new UserService(db, userContext);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService, new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService());
                await vm.OpenRentalsCommand.ExecuteAsync(null);

                var page = Assert.IsType<ManageRentalsPage>(vm.CurrentPage);
                Assert.IsType<ManageRentalsViewModel>(page.DataContext);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task GlobalSearchCommand_NavigatesAndExecutesSearch()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new UserService(db, userContext);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);

                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });
                toolService.AddTool(new Tool { ToolNumber = "T2", NameDescription = "Saw" });

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService, new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService());
                vm.GlobalSearchText = "Ham";

                await vm.GlobalSearchCommand.ExecuteAsync(CancellationToken.None);

                var page = Assert.IsType<ToolSearchPage>(vm.CurrentPage);
                Assert.Equal("Ham", vm.ToolManagement.SearchText);
                Assert.Empty(vm.GlobalSearchText);
                Assert.Single(vm.ToolManagement.SearchResults);
                Assert.Equal("Hammer", vm.ToolManagement.SearchResults.First().NameDescription);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task OpenImportMappingWindowCommand_NoFile_Returns()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new UserService(db, userContext);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService, new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService());
                await vm.OpenImportMappingWindowCommand.ExecuteAsync(null);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void OpenImageImportMappingWindowCommand_NotNull()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new UserService(db, userContext);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService, new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService());
                Assert.NotNull(vm.OpenImageImportMappingWindowCommand);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task OpenImportMappingWindowCommand_ShowsInfo_OnSuccess()
        {
            var dbPath = Path.GetTempFileName();
            var csvPath = Path.GetTempFileName();
            File.WriteAllText(csvPath, "ToolNumber,NameDescription\nT1,Hammer\n");
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new UserService(db, userContext);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);

                var fileDialog = new StubFileDialogService { FileToOpen = csvPath };
                var dialog = new StubDialogService
                {
                    ImportMap = new Dictionary<string, string>
                    {
                        { "ToolNumber", "ToolNumber" },
                        { "NameDescription", "NameDescription" }
                    }
                };

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService, fileDialog, activityLogService, settingsService, db, dialog);
                await vm.OpenImportMappingWindowCommand.ExecuteAsync(null);

                Assert.True(dialog.InfoShown);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
                if (File.Exists(csvPath))
                    File.Delete(csvPath);
            }
        }
    }
}

class StubFileDialogService : ToolManagementAppV2.Interfaces.IFileDialogService
{
    public string FileToOpen;
    public string OpenFile(string filter, string? initialDirectory = null) => FileToOpen;
    public string SaveFile(string filter) => null;
}

class StubDialogService : IDialogService
{
    public bool InfoShown { get; private set; }
    public Dictionary<string, string>? ImportMap { get; set; }

    public void ShowInfo(string message, string title) => InfoShown = true;
    public bool ShowConfirmation(string message, string title) => false;
    public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
    public void ShowToolDetails(ToolModel tool) { }
    public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;
    public CustomerModel? ShowAddCustomerDialog() => null;

    public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties) => ImportMap;
    public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
    public void ShowRentalHistory(ToolModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
    public System.Func<ToolModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
    public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
    public void ShowPrintLabelDialog() { }
}

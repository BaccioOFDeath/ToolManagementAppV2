using System.IO;
using ToolManagementAppV2;
using ToolManagementAppV2.Tests;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ToolManagementAppV2.Tests.Tests
{
    public class NavigationCommandsTests
    {
        [Fact]
        public async Task OpenSearchToolsCommand_NavigatesToToolSearchPage()
        {
            var (window, dbPath) = TestHelpers.CreateMainWindow();
            try
            {
                var vm = Assert.IsType<MainViewModel>(window.DataContext);

                await vm.OpenSearchToolsCommand.ExecuteAsync(null);

                Assert.IsType<ToolSearchPage>(vm.CurrentPage);
            }
            finally
            {
                window.Close();
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task OpenSearchToolsCommand_LoadsToolsAndSetsDataContext()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new UserService(db, userContext);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);
                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService, new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService());
                await vm.OpenSearchToolsCommand.ExecuteAsync(null);

                var page = Assert.IsType<ToolSearchPage>(vm.CurrentPage);
                Assert.Same(vm.ToolManagement, page.DataContext);
                Assert.Same(vm.ToolManagement.Tools, page.ToolsList.ItemsSource);
                Assert.NotEmpty(vm.ToolManagement.Tools);
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

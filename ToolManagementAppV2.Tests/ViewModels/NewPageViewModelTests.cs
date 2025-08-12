using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class NewPageViewModelTests
    {
        [Fact]
        public void ActivityLogsViewModel_LoadsLogs()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var service = new ActivityLogService(db);
                service.LogAction(1, "user", "action");
                var vm = new ActivityLogsViewModel(service);
                vm.LoadLogs();
                Assert.NotEmpty(vm.Logs);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void ImportExportViewModel_ImportToolsCommand_LogsSuccess()
        {
            var toolService = new StubToolService();
            var customerService = new StubCustomerService();
            var vm = new ImportExportViewModel(toolService, customerService, new StubFileDialogService());
            vm.ImportToolsCommand.Execute(null);
            Assert.True(toolService.ImportCalled);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Successfully imported tools", vm.ImportExportLogs[0]);
        }

        [Fact]
        public void ImportExportViewModel_ExportToolsCommand_LogsSuccess()
        {
            var toolService = new StubToolService();
            var customerService = new StubCustomerService();
            var vm = new ImportExportViewModel(toolService, customerService, new StubFileDialogService());
            vm.ExportToolsCommand.Execute(null);
            Assert.True(toolService.ExportCalled);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Successfully exported tools", vm.ImportExportLogs[0]);
        }

        [Fact]
        public void ImportExportViewModel_ImportCustomersCommand_LogsSuccess()
        {
            var toolService = new StubToolService();
            var customerService = new StubCustomerService();
            var vm = new ImportExportViewModel(toolService, customerService, new StubFileDialogService());
            vm.ImportCustomersCommand.Execute(null);
            Assert.True(customerService.ImportCalled);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Successfully imported customers", vm.ImportExportLogs[0]);
        }

        [Fact]
        public void ImportExportViewModel_ExportCustomersCommand_LogsSuccess()
        {
            var toolService = new StubToolService();
            var customerService = new StubCustomerService();
            var vm = new ImportExportViewModel(toolService, customerService, new StubFileDialogService());
            vm.ExportCustomersCommand.Execute(null);
            Assert.True(customerService.ExportCalled);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Successfully exported customers", vm.ImportExportLogs[0]);
        }

        [Fact]
        public void ImportExportViewModel_ImportToolsCommand_LogsFailure()
        {
            var toolService = new FailToolService();
            var customerService = new StubCustomerService();
            var vm = new ImportExportViewModel(toolService, customerService, new StubFileDialogService());
            vm.ImportToolsCommand.Execute(null);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Failed to import tools", vm.ImportExportLogs[0]);
        }

        [Fact]
        public void ImportExportViewModel_ExportToolsCommand_LogsFailure()
        {
            var toolService = new FailToolService();
            var customerService = new StubCustomerService();
            var vm = new ImportExportViewModel(toolService, customerService, new StubFileDialogService());
            vm.ExportToolsCommand.Execute(null);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Failed to export tools", vm.ImportExportLogs[0]);
        }

        [Fact]
        public void ImportExportViewModel_ImportCustomersCommand_LogsFailure()
        {
            var toolService = new StubToolService();
            var customerService = new FailCustomerService();
            var vm = new ImportExportViewModel(toolService, customerService, new StubFileDialogService());
            vm.ImportCustomersCommand.Execute(null);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Failed to import customers", vm.ImportExportLogs[0]);
        }

        [Fact]
        public void ImportExportViewModel_ExportCustomersCommand_LogsFailure()
        {
            var toolService = new StubToolService();
            var customerService = new FailCustomerService();
            var vm = new ImportExportViewModel(toolService, customerService, new StubFileDialogService());
            vm.ExportCustomersCommand.Execute(null);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Failed to export customers", vm.ImportExportLogs[0]);
        }

        [Fact]
        public void ReportsViewModel_RunReport_PopulatesResults()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var reportService = new ReportService(new StubToolService(), new StubRentalService(), new ActivityLogService(db), new StubCustomerService(), new StubUserService());
                var vm = new ReportsViewModel(reportService);
                vm.SelectedReport = vm.ReportTypes.First();
                vm.RunReportCommand.Execute(null);
                Assert.NotNull(vm.ReportResults);
                Assert.True(vm.ReportResults.Rows.Count > 0);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }

    class StubFileDialogService : IFileDialogService
    {
        public string OpenFile(string filter) => "path.csv";
    }

    class StubToolService : IToolService
    {
        public bool ImportCalled { get; private set; }
        public bool ExportCalled { get; private set; }
        public List<int> ImportToolsFromCsv(string filePath, IDictionary<string, string> map)
        {
            ImportCalled = true;
            return new();
        }
        public void ExportToolsToCsv(string filePath) => ExportCalled = true;
        public List<ToolModel> GetAllTools() => new();
        public void AddTool(ToolModel tool) => throw new System.NotImplementedException();
        public void UpdateTool(ToolModel tool) => throw new System.NotImplementedException();
        public void DeleteTool(string toolID) => throw new System.NotImplementedException();
        public ToolModel GetToolByID(string toolID) => throw new System.NotImplementedException();
        public List<ToolModel> SearchTools(string? searchText) => new();
        public void ToggleToolCheckOutStatus(string toolID, string currentUser) => throw new System.NotImplementedException();
        public List<ToolModel> GetToolsCheckedOutBy(string userName) => new();
        public void UpdateToolImage(string toolID, string imagePath) => throw new System.NotImplementedException();
        public ImageImportResult ImportToolImages(string folderPath, System.Func<ToolModel, IEnumerable<string>> keySelector) => new();
        public void UpdateToolQuantities(string toolID, int qtyChange, bool isRental) => throw new System.NotImplementedException();
    }

    class FailToolService : IToolService
    {
        public List<int> ImportToolsFromCsv(string filePath, IDictionary<string, string> map) => throw new System.Exception("fail");
        public void ExportToolsToCsv(string filePath) => throw new System.Exception("fail");
        public List<ToolModel> GetAllTools() => new();
        public void AddTool(ToolModel tool) => throw new System.NotImplementedException();
        public void UpdateTool(ToolModel tool) => throw new System.NotImplementedException();
        public void DeleteTool(string toolID) => throw new System.NotImplementedException();
        public ToolModel GetToolByID(string toolID) => throw new System.NotImplementedException();
        public List<ToolModel> SearchTools(string? searchText) => new();
        public void ToggleToolCheckOutStatus(string toolID, string currentUser) => throw new System.NotImplementedException();
        public List<ToolModel> GetToolsCheckedOutBy(string userName) => new();
        public void UpdateToolImage(string toolID, string imagePath) => throw new System.NotImplementedException();
        public ImageImportResult ImportToolImages(string folderPath, System.Func<ToolModel, IEnumerable<string>> keySelector) => new();
        public void UpdateToolQuantities(string toolID, int qtyChange, bool isRental) => throw new System.NotImplementedException();
    }

    class StubRentalService : IRentalService
    {
        public void RentTool(string toolID, int customerID, System.DateTime rentalDate, System.DateTime dueDate) => throw new System.NotImplementedException();
        public void ReturnTool(int rentalID, System.DateTime returnDate) => throw new System.NotImplementedException();
        public void ExtendRental(int rentalID, System.DateTime newDueDate) => throw new System.NotImplementedException();
        public List<Rental> GetActiveRentals() => new();
        public List<Rental> GetOverdueRentals() => new();
        public List<Rental> GetAllRentals() => new();
        public List<Rental> GetRentalHistoryForTool(string toolID) => new();
        public List<Rental> GetRentalHistoryForCustomer(int customerID) => new();
    }

    class StubCustomerService : ICustomerService
    {
        public bool ImportCalled { get; private set; }
        public bool ExportCalled { get; private set; }
        public void ImportCustomersFromCsv(string filePath, IDictionary<string, string> map) => ImportCalled = true;
        public void ExportCustomersToCsv(string filePath) => ExportCalled = true;
        public void AddCustomer(Customer customer) => throw new System.NotImplementedException();
        public void UpdateCustomer(Customer customer) => throw new System.NotImplementedException();
        public void DeleteCustomer(int customerID) => throw new System.NotImplementedException();
        public Customer GetCustomerByID(int customerID) => throw new System.NotImplementedException();
        public List<Customer> GetAllCustomers() => new();
        public List<Customer> SearchCustomers(string searchTerm) => new();
    }

    class FailCustomerService : ICustomerService
    {
        public void ImportCustomersFromCsv(string filePath, IDictionary<string, string> map) => throw new System.Exception("fail");
        public void ExportCustomersToCsv(string filePath) => throw new System.Exception("fail");
        public void AddCustomer(Customer customer) => throw new System.NotImplementedException();
        public void UpdateCustomer(Customer customer) => throw new System.NotImplementedException();
        public void DeleteCustomer(int customerID) => throw new System.NotImplementedException();
        public Customer GetCustomerByID(int customerID) => throw new System.NotImplementedException();
        public List<Customer> GetAllCustomers() => new();
        public List<Customer> SearchCustomers(string searchTerm) => new();
    }

    class StubUserService : IUserService
    {
        public List<User> GetAllUsers() => new();
        public User GetUserByID(int userID) => throw new System.NotImplementedException();
        public User AuthenticateUser(string userName, string password) => throw new System.NotImplementedException();
        public User GetCurrentUser() => throw new System.NotImplementedException();
        public void AddUser(User user) => throw new System.NotImplementedException();
        public void UpdateUser(User user) => throw new System.NotImplementedException();
        public bool TryDeleteUser(int userID) => throw new System.NotImplementedException();
        public bool DeleteUser(int userID) => throw new System.NotImplementedException();
        public void ChangeUserPassword(int userID, string newPassword) => throw new System.NotImplementedException();
    }
}

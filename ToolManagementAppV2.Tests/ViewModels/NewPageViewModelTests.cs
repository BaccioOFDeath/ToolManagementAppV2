using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Models.ImportExport;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.ViewModels;
using Xunit;
using System.Threading.Tasks;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class NewPageViewModelTests
    {
        [Fact]
        public async Task ActivityLogsViewModel_LoadsLogsAsync()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var service = new ActivityLogService(db);
                await service.LogActionAsync(1, "user", "action");
                var vm = new ActivityLogsViewModel(service);
                await vm.LoadLogsAsync();
                Assert.NotEmpty(vm.Logs);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task ImportExportViewModel_ImportToolsCommand_LogsSuccess()
        {
            var toolService = new StubItemService();
            var customerService = new StubCustomerService();
            var fileDlg = new StubFileDialogService();
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "ItemNumber\n");
            fileDlg.FileToReturn = tmp;
            var dialog = new StubDialogService();
            var vm = new ImportExportViewModel(toolService, customerService, fileDlg, new StubDatabaseBackupService(), dialog);
            await vm.ImportToolsCommand.ExecuteAsync(null);
            Assert.True(toolService.ImportCalled);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Successfully imported tools", vm.ImportExportLogs[0]);
            File.Delete(tmp);
        }

        [Fact]
        public async Task ImportExportViewModel_ExportToolsCommand_LogsSuccess()
        {
            var toolService = new StubItemService();
            var customerService = new StubCustomerService();
            var vm = new ImportExportViewModel(toolService, customerService, new StubFileDialogService(), new StubDatabaseBackupService(), new StubDialogService());
            await vm.ExportToolsCommand.ExecuteAsync(null);
            Assert.True(toolService.ExportCalled);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Successfully exported tools", vm.ImportExportLogs[0]);
        }

        [Fact]
        public async Task ImportExportViewModel_ImportCustomersCommand_LogsSuccess()
        {
            var toolService = new StubItemService();
            var customerService = new StubCustomerService();
            var fileDlg = new StubFileDialogService();
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "Company\n");
            fileDlg.FileToReturn = tmp;
            var dialog = new StubDialogService();
            var vm = new ImportExportViewModel(toolService, customerService, fileDlg, new StubDatabaseBackupService(), dialog);
            await vm.ImportCustomersCommand.ExecuteAsync(null);
            Assert.True(customerService.ImportCalled);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Successfully imported customers", vm.ImportExportLogs[0]);
            File.Delete(tmp);
        }

        [Fact]
        public async Task ImportExportViewModel_ExportCustomersCommand_LogsSuccess()
        {
            var toolService = new StubItemService();
            var customerService = new StubCustomerService();
            var vm = new ImportExportViewModel(toolService, customerService, new StubFileDialogService(), new StubDatabaseBackupService(), new StubDialogService());
            await vm.ExportCustomersCommand.ExecuteAsync(null);
            Assert.True(customerService.ExportCalled);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Successfully exported customers", vm.ImportExportLogs[0]);
        }

        [Fact]
        public async Task ImportExportViewModel_BackupDatabaseCommand_LogsSuccess()
        {
            var vm = new ImportExportViewModel(new StubItemService(), new StubCustomerService(), new StubFileDialogService(), new StubDatabaseBackupService(), new StubDialogService());
            await vm.BackupDatabaseCommand.ExecuteAsync(null);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Successfully backed up database", vm.ImportExportLogs[0]);
        }

        [Fact]
        public async Task ImportExportViewModel_BackupDatabaseCommand_CancelledOperation_LogsCancellation()
        {
            var vm = new ImportExportViewModel(new StubItemService(), new StubCustomerService(), new StubFileDialogService(), new CancellableDatabaseBackupService(), new StubDialogService());
            var task = vm.BackupDatabaseCommand.ExecuteAsync(null);
            vm.BackupDatabaseCommand.Cancel();
            await task;
            Assert.Single(vm.ImportExportLogs);
            Assert.Equal("Database backup was cancelled.", vm.ImportExportLogs[0]);
        }

        [Fact]
        public async Task ImportExportViewModel_ImportToolsCommand_LogsFailure()
        {
            var toolService = new FailItemService();
            var customerService = new StubCustomerService();
            var fileDlg = new StubFileDialogService();
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "ItemNumber\n");
            fileDlg.FileToReturn = tmp;
            var dialog = new StubDialogService();
            var vm = new ImportExportViewModel(toolService, customerService, fileDlg, new StubDatabaseBackupService(), dialog);
            await vm.ImportToolsCommand.ExecuteAsync(null);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Failed to import tools", vm.ImportExportLogs[0]);
            File.Delete(tmp);
        }

        [Fact]
        public async Task ImportExportViewModel_ExportToolsCommand_LogsFailure()
        {
            var toolService = new FailItemService();
            var customerService = new StubCustomerService();
            var vm = new ImportExportViewModel(toolService, customerService, new StubFileDialogService(), new StubDatabaseBackupService(), new StubDialogService());
            await vm.ExportToolsCommand.ExecuteAsync(null);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Failed to export tools", vm.ImportExportLogs[0]);
        }

        [Fact]
        public async Task ImportExportViewModel_ImportCustomersCommand_LogsFailure()
        {
            var toolService = new StubItemService();
            var customerService = new FailCustomerService();
            var fileDlg = new StubFileDialogService();
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "Company\n");
            fileDlg.FileToReturn = tmp;
            var dialog = new StubDialogService();
            var vm = new ImportExportViewModel(toolService, customerService, fileDlg, new StubDatabaseBackupService(), dialog);
            await vm.ImportCustomersCommand.ExecuteAsync(null);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Failed to import customers", vm.ImportExportLogs[0]);
            File.Delete(tmp);
        }

        [Fact]
        public async Task ImportExportViewModel_ExportCustomersCommand_LogsFailure()
        {
            var toolService = new StubItemService();
            var customerService = new FailCustomerService();
            var vm = new ImportExportViewModel(toolService, customerService, new StubFileDialogService(), new StubDatabaseBackupService(), new StubDialogService());
            await vm.ExportCustomersCommand.ExecuteAsync(null);
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
                var reportService = new ReportService(new StubItemService(), new StubRentalService(), new ActivityLogService(db), new StubCustomerService(), new StubUserService());
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
        public string FileToReturn { get; set; } = "path.csv";
        public string OpenFile(string filter, string? initialDirectory = null) => FileToReturn;
        public string SaveFile(string filter) => FileToReturn;
    }

    class StubDialogService : IDialogService
    {
        public Dictionary<string, string>? MapToReturn { get; set; } = new();
        public bool ShowImportMappingCalled { get; private set; }
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => false;
        public ItemModel? ShowEditToolDialog(ItemModel tool) => null;
        public void ShowToolDetails(ItemModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ItemModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel tool, IEnumerable<RentalModel> history) { }
        public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties)
        {
            ShowImportMappingCalled = true;
            return MapToReturn;
        }
        public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }

    class StubItemService : IItemService
    {
        public bool ImportCalled { get; private set; }
        public bool ExportCalled { get; private set; }
        public Task<List<int>> ImportToolsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken)
        {
            ImportCalled = true;
            return Task.FromResult(new List<int>());
        }
        public Task ExportToolsToCsvAsync(string filePath, CancellationToken cancellationToken = default)
        {
            ExportCalled = true;
            return Task.CompletedTask;
        }
        public Task<List<ItemModel>> GetAllToolsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task AddToolAsync(ItemModel tool, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public Task UpdateToolAsync(ItemModel tool, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public Task DeleteToolAsync(int toolID, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public Task<ItemModel?> GetToolByIDAsync(int toolID, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public Task<List<ItemModel>> SearchToolsAsync(string? searchText, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task<bool> ToggleToolCheckOutStatusAsync(int toolID, string currentUser, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public Task<List<ItemModel>> GetToolsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task UpdateToolImageAsync(int toolID, string imagePath, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public Task<ImageImportResult> ImportToolImagesAsync(string folderPath, System.Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
        public Task UpdateToolQuantitiesAsync(int toolID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult("T1");
    }

    class FailItemService : IItemService
    {
        public Task<List<int>> ImportToolsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => Task.FromException<List<int>>(new System.Exception("fail"));
        public Task ExportToolsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.FromException(new System.Exception("fail"));
        public Task<List<ItemModel>> GetAllToolsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task AddToolAsync(ItemModel tool, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public Task UpdateToolAsync(ItemModel tool, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public Task DeleteToolAsync(int toolID, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public Task<ItemModel?> GetToolByIDAsync(int toolID, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public Task<List<ItemModel>> SearchToolsAsync(string? searchText, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task<bool> ToggleToolCheckOutStatusAsync(int toolID, string currentUser, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public Task<List<ItemModel>> GetToolsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task UpdateToolImageAsync(int toolID, string imagePath, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public Task<ImageImportResult> ImportToolImagesAsync(string folderPath, System.Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
        public Task UpdateToolQuantitiesAsync(int toolID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult("T1");
    }

    class StubRentalService : IRentalService
    {
        public void RentTool(int toolID, int customerID, System.DateTime rentalDate, System.DateTime dueDate) => throw new System.NotImplementedException();
        public void ReturnTool(int rentalID, System.DateTime returnDate) => throw new System.NotImplementedException();
        public void ExtendRental(int rentalID, System.DateTime newDueDate) => throw new System.NotImplementedException();
        public List<Rental> GetActiveRentals() => new();
        public List<Rental> GetOverdueRentals() => new();
        public List<Rental> GetAllRentals() => new();
        public List<Rental> GetRentalHistoryForTool(int toolID) => new();
        public List<Rental> GetRentalHistoryForCustomer(int customerID) => new();
    }

    class StubCustomerService : ICustomerService
    {
        public bool ImportCalled { get; private set; }
        public bool ExportCalled { get; private set; }
        public CustomerImportResult ImportCustomersFromCsv(string filePath, IDictionary<string, string> map)
        {
            ImportCalled = true;
            return new CustomerImportResult();
        }
        public void ExportCustomersToCsv(string filePath) => ExportCalled = true;
        public Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default)
        {
            ExportCalled = true;
            return Task.CompletedTask;
        }
        public void AddCustomer(Customer customer) => throw new System.NotImplementedException();
        public Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public void UpdateCustomer(Customer customer) => throw new System.NotImplementedException();
        public Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public void DeleteCustomer(int customerID) => throw new System.NotImplementedException();
        public Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public Customer GetCustomerByID(int customerID) => throw new System.NotImplementedException();
        public Task<Customer> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public List<Customer> GetAllCustomers() => new();
        public Task<List<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Customer>());
        public List<Customer> SearchCustomers(string searchTerm) => new();
        public Task<List<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default) => Task.FromResult(new List<Customer>());
        public Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default)
        {
            ImportCalled = true;
            return Task.FromResult(new CustomerImportResult());
        }
    }

    class FailCustomerService : ICustomerService
    {
        public CustomerImportResult ImportCustomersFromCsv(string filePath, IDictionary<string, string> map) => throw new System.Exception("fail");
        public Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default) => throw new System.Exception("fail");
        public void ExportCustomersToCsv(string filePath) => throw new System.Exception("fail");
        public Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default) => throw new System.Exception("fail");
        public void AddCustomer(Customer customer) => throw new System.NotImplementedException();
        public Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public void UpdateCustomer(Customer customer) => throw new System.NotImplementedException();
        public Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public void DeleteCustomer(int customerID) => throw new System.NotImplementedException();
        public Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public Customer GetCustomerByID(int customerID) => throw new System.NotImplementedException();
        public Task<Customer> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
        public List<Customer> GetAllCustomers() => new();
        public Task<List<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Customer>());
        public List<Customer> SearchCustomers(string searchTerm) => new();
        public Task<List<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default) => Task.FromResult(new List<Customer>());
    }

    class StubDatabaseBackupService : IDatabaseBackupService
    {
        public bool Called { get; private set; }
        public Task BackupDatabaseAsync(string backupFilePath, CancellationToken cancellationToken)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }

    class CancellableDatabaseBackupService : IDatabaseBackupService
    {
        public Task BackupDatabaseAsync(string backupFilePath, CancellationToken cancellationToken)
            => Task.Delay(Timeout.Infinite, cancellationToken);
    }

    class StubUserService : IUserService
    {
        public List<User> GetAllUsers() => new();
        public Task<List<User>> GetAllUsersAsync() => Task.FromResult(new List<User>());
        public User? GetUserByID(int userID) => null;
        public Task<User?> GetUserByIDAsync(int userID) => Task.FromResult<User?>(null);
        public User? AuthenticateUser(string userName, string password) => null;
        public Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password) => Task.FromResult<(AuthenticationResult, User?)>((AuthenticationResult.IncorrectPassword, null));
        public User? GetCurrentUser() => null;
        public Task<User?> GetCurrentUserAsync() => Task.FromResult<User?>(null);
        public void AddUser(User user) { }
        public Task AddUserAsync(User user) => Task.CompletedTask;
        public void UpdateUser(User user) { }
        public Task UpdateUserAsync(User user) => Task.CompletedTask;
        public Task<bool> TryDeleteUserAsync(int userID) => Task.FromResult(false);
        public bool ChangeUserPassword(int userID, string newPassword) => false;
        public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword) => Task.FromResult(false);
    }
}

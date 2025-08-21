using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Models.ImportExport;
using InventoryManagementApp.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class ImportExportViewModelTests
    {
        [Fact]
        public async System.Threading.Tasks.Task ImportItemsCommand_UsesMappingFromDialog()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "ItemNumber\n");
            var fileDlg = new StubFileDialogService { FileToReturn = tmp };
            var itemService = new CapturingItemService();
            var dialog = new StubDialogService { MapToReturn = new Dictionary<string,string>{{"ItemNumber","ItemNumber"}} };
            var vm = new ImportExportViewModel(itemService, new StubCustomerService(), fileDlg, new StubDatabaseBackupService(), dialog);

            await vm.ImportItemsCommand.ExecuteAsync(null);

            Assert.True(dialog.ShowImportMappingCalled);
            Assert.True(itemService.ImportCalled);
            Assert.Equal(AppContext.BaseDirectory, fileDlg.InitialDirectoryUsed);
            Assert.Equal("ItemNumber", itemService.MapUsed!["ItemNumber"]);
            File.Delete(tmp);
        }

        [Fact]
        public async System.Threading.Tasks.Task ImportCustomersCommand_UsesMappingFromDialog()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "Company\n");
            var fileDlg = new StubFileDialogService { FileToReturn = tmp };
            var customerSvc = new CapturingCustomerService();
            var dialog = new StubDialogService { MapToReturn = new Dictionary<string,string>{{"Company","Company"}} };
            var vm = new ImportExportViewModel(new StubItemService(), customerSvc, fileDlg, new StubDatabaseBackupService(), dialog);

            await vm.ImportCustomersCommand.ExecuteAsync(null);

            Assert.True(dialog.ShowImportMappingCalled);
            Assert.True(customerSvc.ImportCalled);
            Assert.Equal(AppContext.BaseDirectory, fileDlg.InitialDirectoryUsed);
            Assert.Equal("Company", customerSvc.MapUsed!["Company"]);
            File.Delete(tmp);
        }

        [Fact]
        public async System.Threading.Tasks.Task ImportCustomersCommand_CancelledMapping_DoesNotCallService()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "Company\n");
            var fileDlg = new StubFileDialogService { FileToReturn = tmp };
            var customerSvc = new CapturingCustomerService();
            var dialog = new StubDialogService { MapToReturn = null };
            var vm = new ImportExportViewModel(new StubItemService(), customerSvc, fileDlg, new StubDatabaseBackupService(), dialog);

            await vm.ImportCustomersCommand.ExecuteAsync(null);

            Assert.True(dialog.ShowImportMappingCalled);
            Assert.False(customerSvc.ImportCalled);
            File.Delete(tmp);
        }

        [Fact]
        public async System.Threading.Tasks.Task ImportItemsCommand_CancelledMapping_DoesNotCallService()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "ItemNumber\n");
            var fileDlg = new StubFileDialogService { FileToReturn = tmp };
            var itemService = new CapturingItemService();
            var dialog = new StubDialogService { MapToReturn = null };
            var vm = new ImportExportViewModel(itemService, new StubCustomerService(), fileDlg, new StubDatabaseBackupService(), dialog);

            await vm.ImportItemsCommand.ExecuteAsync(null);

            Assert.True(dialog.ShowImportMappingCalled);
            Assert.False(itemService.ImportCalled);
            File.Delete(tmp);
        }

        [Fact]
        public async System.Threading.Tasks.Task ImportItemsCommand_CanBeCancelled()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "ItemNumber\n");
            var fileDlg = new StubFileDialogService { FileToReturn = tmp };
            var itemService = new CancelableItemService();
            var dialog = new StubDialogService { MapToReturn = new Dictionary<string,string>{{"ItemNumber","ItemNumber"}} };
            var vm = new ImportExportViewModel(itemService, new StubCustomerService(), fileDlg, new StubDatabaseBackupService(), dialog);

            var execute = vm.ImportItemsCommand.ExecuteAsync(null);
            vm.ImportItemsCommand.Cancel();
            await execute;

            Assert.Single(vm.ImportExportLogs);
            Assert.Contains("cancel", vm.ImportExportLogs[0], StringComparison.OrdinalIgnoreCase);
            File.Delete(tmp);
        }

        [Fact]
        public async System.Threading.Tasks.Task ExportCustomersCommand_LogsSuccess()
        {
            var customerSvc = new CapturingCustomerService();
            var fileDlg = new StubFileDialogService { FileToReturn = "path.csv" };
            var vm = new ImportExportViewModel(new StubItemService(), customerSvc, fileDlg, new StubDatabaseBackupService(), new StubDialogService());

            await vm.ExportCustomersCommand.ExecuteAsync(null);

            Assert.True(customerSvc.ExportCalled);
            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Successfully exported customers", vm.ImportExportLogs[0]);
        }

        [Fact]
        public async System.Threading.Tasks.Task ExportCustomersCommand_LogsFailure()
        {
            var customerSvc = new FailCustomerService();
            var fileDlg = new StubFileDialogService { FileToReturn = "path.csv" };
            var vm = new ImportExportViewModel(new StubItemService(), customerSvc, fileDlg, new StubDatabaseBackupService(), new StubDialogService());

            await vm.ExportCustomersCommand.ExecuteAsync(null);

            Assert.Single(vm.ImportExportLogs);
            Assert.StartsWith("Failed to export customers", vm.ImportExportLogs[0]);
        }
    }

    class StubFileDialogService : IFileDialogService
    {
        public string FileToReturn { get; set; } = string.Empty;
        public string? InitialDirectoryUsed { get; private set; }
        public string OpenFile(string filter, string? initialDirectory = null)
        {
            InitialDirectoryUsed = initialDirectory;
            return FileToReturn;
        }
        public string SaveFile(string filter) => FileToReturn;
    }

    class StubDialogService : IDialogService
    {
        public Dictionary<string, string>? MapToReturn { get; set; }
        public bool ShowImportMappingCalled { get; private set; }
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => false;
        public ItemModel? ShowEditItemDialog(ItemModel item) => null;
        public void ShowItemDetails(ItemModel item) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
        public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties)
        {
            ShowImportMappingCalled = true;
            return MapToReturn;
        }
        public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }

    class CapturingItemService : IItemService
    {
        public bool ImportCalled { get; private set; }
        public IDictionary<string,string>? MapUsed { get; private set; }
        public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken)
        {
            ImportCalled = true;
            MapUsed = map;
            return Task.FromResult(new List<int>());
        }
        public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<List<ItemModel>> GetAllItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<ItemModel>> SearchItemsAsync(string? searchText, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task<bool> ToggleItemCheckOutStatusAsync(int itemID, string currentUser, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
        public Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, System.Data.SQLite.SQLiteConnection? conn = null, System.Data.SQLite.SQLiteTransaction? tx = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult("T1");
    }

    class CapturingCustomerService : ICustomerService
    {
        public bool ImportCalled { get; private set; }
        public bool ExportCalled { get; private set; }
        public IDictionary<string,string>? MapUsed { get; private set; }
        public CustomerImportResult ImportCustomersFromCsv(string filePath, IDictionary<string, string> map)
        {
            ImportCalled = true;
            MapUsed = map;
            return new CustomerImportResult();
        }
        public void ExportCustomersToCsv(string filePath) => ExportCalled = true;
        public System.Threading.Tasks.Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default)
        {
            ExportCalled = true;
            return System.Threading.Tasks.Task.CompletedTask;
        }
        public void AddCustomer(Customer customer) => throw new NotImplementedException();
        public System.Threading.Tasks.Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public void UpdateCustomer(Customer customer) => throw new NotImplementedException();
        public System.Threading.Tasks.Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public void DeleteCustomer(int customerID) => throw new NotImplementedException();
        public System.Threading.Tasks.Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Customer GetCustomerByID(int customerID) => throw new NotImplementedException();
        public System.Threading.Tasks.Task<Customer> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public List<Customer> GetAllCustomers() => new();
        public System.Threading.Tasks.Task<List<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.FromResult(new List<Customer>());
        public List<Customer> SearchCustomers(string searchTerm) => new();
        public System.Threading.Tasks.Task<List<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.FromResult(new List<Customer>());
        public System.Threading.Tasks.Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default)
        {
            ImportCalled = true;
            MapUsed = map;
            return System.Threading.Tasks.Task.FromResult(new CustomerImportResult());
        }
    }

    class StubItemService : IItemService
    {
        public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken)
            => Task.FromResult(new List<int>());
        public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<List<ItemModel>> GetAllItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<ItemModel>> SearchItemsAsync(string? searchText, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task<bool> ToggleItemCheckOutStatusAsync(int itemID, string currentUser, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
        public Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, System.Data.SQLite.SQLiteConnection? conn = null, System.Data.SQLite.SQLiteTransaction? tx = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult("T1");
    }

    class CancelableItemService : IItemService
    {
        public async Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return new List<int>();
        }
        public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<List<ItemModel>> GetAllItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<ItemModel>> SearchItemsAsync(string? searchText, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task<bool> ToggleItemCheckOutStatusAsync(int itemID, string currentUser, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
        public Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, System.Data.SQLite.SQLiteConnection? conn = null, System.Data.SQLite.SQLiteTransaction? tx = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult("T1");
    }

    class StubCustomerService : ICustomerService
    {
        public CustomerImportResult ImportCustomersFromCsv(string filePath, IDictionary<string, string> map) => new CustomerImportResult();
        public void ExportCustomersToCsv(string filePath) { }
        public System.Threading.Tasks.Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.CompletedTask;
        public void AddCustomer(Customer customer) => throw new NotImplementedException();
        public System.Threading.Tasks.Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public void UpdateCustomer(Customer customer) => throw new NotImplementedException();
        public System.Threading.Tasks.Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public void DeleteCustomer(int customerID) => throw new NotImplementedException();
        public System.Threading.Tasks.Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Customer GetCustomerByID(int customerID) => throw new NotImplementedException();
        public System.Threading.Tasks.Task<Customer> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public List<Customer> GetAllCustomers() => new();
        public System.Threading.Tasks.Task<List<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.FromResult(new List<Customer>());
        public List<Customer> SearchCustomers(string searchTerm) => new();
        public System.Threading.Tasks.Task<List<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.FromResult(new List<Customer>());
        public System.Threading.Tasks.Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.FromResult(new CustomerImportResult());
    }

    class FailCustomerService : ICustomerService
    {
        public CustomerImportResult ImportCustomersFromCsv(string filePath, IDictionary<string, string> map) => new CustomerImportResult();
        public void ExportCustomersToCsv(string filePath) => throw new System.Exception("fail");
        public System.Threading.Tasks.Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.FromException(new System.Exception("fail"));
        public void AddCustomer(Customer customer) => throw new NotImplementedException();
        public System.Threading.Tasks.Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public void UpdateCustomer(Customer customer) => throw new NotImplementedException();
        public System.Threading.Tasks.Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public void DeleteCustomer(int customerID) => throw new NotImplementedException();
        public System.Threading.Tasks.Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Customer GetCustomerByID(int customerID) => throw new NotImplementedException();
        public System.Threading.Tasks.Task<Customer> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public List<Customer> GetAllCustomers() => new();
        public System.Threading.Tasks.Task<List<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.FromResult(new List<Customer>());
        public List<Customer> SearchCustomers(string searchTerm) => new();
        public System.Threading.Tasks.Task<List<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.FromResult(new List<Customer>());
        public System.Threading.Tasks.Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.FromResult(new CustomerImportResult());
    }

    class StubDatabaseBackupService : IDatabaseBackupService
    {
        public System.Threading.Tasks.Task BackupDatabaseAsync(string backupFilePath, CancellationToken cancellationToken)
            => System.Threading.Tasks.Task.CompletedTask;
    }
}

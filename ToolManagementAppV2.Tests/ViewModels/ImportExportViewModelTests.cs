using System;
using System.Collections.Generic;
using System.IO;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Models.ImportExport;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class ImportExportViewModelTests
    {
        [Fact]
        public void ImportToolsCommand_UsesMappingFromDialog()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "ToolNumber\n");
            var fileDlg = new StubFileDialogService { FileToReturn = tmp };
            var toolSvc = new CapturingToolService();
            var dialog = new StubDialogService { MapToReturn = new Dictionary<string,string>{{"ToolNumber","ToolNumber"}} };
            var vm = new ImportExportViewModel(toolSvc, new StubCustomerService(), fileDlg, new StubDatabaseBackupService(), dialog);

            vm.ImportToolsCommand.Execute(null);

            Assert.True(dialog.ShowImportMappingCalled);
            Assert.True(toolSvc.ImportCalled);
            Assert.Equal("ToolNumber", toolSvc.MapUsed!["ToolNumber"]);
            File.Delete(tmp);
        }

        [Fact]
        public void ImportCustomersCommand_UsesMappingFromDialog()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "Company\n");
            var fileDlg = new StubFileDialogService { FileToReturn = tmp };
            var customerSvc = new CapturingCustomerService();
            var dialog = new StubDialogService { MapToReturn = new Dictionary<string,string>{{"Company","Company"}} };
            var vm = new ImportExportViewModel(new StubToolService(), customerSvc, fileDlg, new StubDatabaseBackupService(), dialog);

            vm.ImportCustomersCommand.Execute(null);

            Assert.True(dialog.ShowImportMappingCalled);
            Assert.True(customerSvc.ImportCalled);
            Assert.Equal("Company", customerSvc.MapUsed!["Company"]);
            File.Delete(tmp);
        }

        [Fact]
        public void ImportToolsCommand_CancelledMapping_DoesNotCallService()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "ToolNumber\n");
            var fileDlg = new StubFileDialogService { FileToReturn = tmp };
            var toolSvc = new CapturingToolService();
            var dialog = new StubDialogService { MapToReturn = null };
            var vm = new ImportExportViewModel(toolSvc, new StubCustomerService(), fileDlg, new StubDatabaseBackupService(), dialog);

            vm.ImportToolsCommand.Execute(null);

            Assert.True(dialog.ShowImportMappingCalled);
            Assert.False(toolSvc.ImportCalled);
            File.Delete(tmp);
        }
    }

    class StubFileDialogService : IFileDialogService
    {
        public string FileToReturn { get; set; } = string.Empty;
        public string OpenFile(string filter) => FileToReturn;
        public string SaveFile(string filter) => FileToReturn;
    }

    class StubDialogService : IDialogService
    {
        public Dictionary<string, string>? MapToReturn { get; set; }
        public bool ShowImportMappingCalled { get; private set; }
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => false;
        public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
        public void ShowToolDetails(ToolModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ToolModel tool, IEnumerable<RentalModel> history) { }
        public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties)
        {
            ShowImportMappingCalled = true;
            return MapToReturn;
        }
        public Func<ToolModel, IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
        public void ShowScannerStatus() { }
    }

    class CapturingToolService : IToolService
    {
        public bool ImportCalled { get; private set; }
        public IDictionary<string,string>? MapUsed { get; private set; }
        public List<int> ImportToolsFromCsv(string filePath, IDictionary<string, string> map)
        {
            ImportCalled = true;
            MapUsed = map;
            return new();
        }
        public void ExportToolsToCsv(string filePath) { }
        public List<ToolModel> GetAllTools() => new();
        public void AddTool(ToolModel tool) => throw new NotImplementedException();
        public void UpdateTool(ToolModel tool) => throw new NotImplementedException();
        public void DeleteTool(int toolID) => throw new NotImplementedException();
        public ToolModel GetToolByID(int toolID) => throw new NotImplementedException();
        public List<ToolModel> SearchTools(string? searchText) => new();
        public void ToggleToolCheckOutStatus(int toolID, string currentUser) => throw new NotImplementedException();
        public List<ToolModel> GetToolsCheckedOutBy(string userName) => new();
        public void UpdateToolImage(int toolID, string imagePath) => throw new NotImplementedException();
        public ImageImportResult ImportToolImages(string folderPath, Func<ToolModel, IEnumerable<string>> keySelector) => new();
        public void UpdateToolQuantities(int toolID, int qtyChange, bool isRental, System.Data.SQLite.SQLiteConnection? conn = null, System.Data.SQLite.SQLiteTransaction? tx = null) => throw new NotImplementedException();
    }

    class CapturingCustomerService : ICustomerService
    {
        public bool ImportCalled { get; private set; }
        public IDictionary<string,string>? MapUsed { get; private set; }
        public CustomerImportResult ImportCustomersFromCsv(string filePath, IDictionary<string, string> map)
        {
            ImportCalled = true;
            MapUsed = map;
            return new CustomerImportResult();
        }
        public void ExportCustomersToCsv(string filePath) { }
        public System.Threading.Tasks.Task ExportCustomersToCsvAsync(string filePath) => System.Threading.Tasks.Task.CompletedTask;
        public void AddCustomer(Customer customer) => throw new NotImplementedException();
        public System.Threading.Tasks.Task AddCustomerAsync(Customer customer) => throw new NotImplementedException();
        public void UpdateCustomer(Customer customer) => throw new NotImplementedException();
        public System.Threading.Tasks.Task UpdateCustomerAsync(Customer customer) => throw new NotImplementedException();
        public void DeleteCustomer(int customerID) => throw new NotImplementedException();
        public System.Threading.Tasks.Task DeleteCustomerAsync(int customerID) => throw new NotImplementedException();
        public Customer GetCustomerByID(int customerID) => throw new NotImplementedException();
        public System.Threading.Tasks.Task<Customer> GetCustomerByIDAsync(int customerID) => throw new NotImplementedException();
        public List<Customer> GetAllCustomers() => new();
        public System.Threading.Tasks.Task<List<Customer>> GetAllCustomersAsync() => System.Threading.Tasks.Task.FromResult(new List<Customer>());
        public List<Customer> SearchCustomers(string searchTerm) => new();
        public System.Threading.Tasks.Task<List<Customer>> SearchCustomersAsync(string searchTerm) => System.Threading.Tasks.Task.FromResult(new List<Customer>());
        public System.Threading.Tasks.Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map)
        {
            ImportCalled = true;
            MapUsed = map;
            return System.Threading.Tasks.Task.FromResult(new CustomerImportResult());
        }
    }

    class StubToolService : IToolService
    {
        public List<int> ImportToolsFromCsv(string filePath, IDictionary<string, string> map) => new();
        public void ExportToolsToCsv(string filePath) { }
        public List<ToolModel> GetAllTools() => new();
        public void AddTool(ToolModel tool) => throw new NotImplementedException();
        public void UpdateTool(ToolModel tool) => throw new NotImplementedException();
        public void DeleteTool(int toolID) => throw new NotImplementedException();
        public ToolModel GetToolByID(int toolID) => throw new NotImplementedException();
        public List<ToolModel> SearchTools(string? searchText) => new();
        public void ToggleToolCheckOutStatus(int toolID, string currentUser) => throw new NotImplementedException();
        public List<ToolModel> GetToolsCheckedOutBy(string userName) => new();
        public void UpdateToolImage(int toolID, string imagePath) => throw new NotImplementedException();
        public ImageImportResult ImportToolImages(string folderPath, Func<ToolModel, IEnumerable<string>> keySelector) => new();
        public void UpdateToolQuantities(int toolID, int qtyChange, bool isRental, System.Data.SQLite.SQLiteConnection? conn = null, System.Data.SQLite.SQLiteTransaction? tx = null) => throw new NotImplementedException();
    }

    class StubCustomerService : ICustomerService
    {
        public CustomerImportResult ImportCustomersFromCsv(string filePath, IDictionary<string, string> map) => new CustomerImportResult();
        public void ExportCustomersToCsv(string filePath) { }
        public System.Threading.Tasks.Task ExportCustomersToCsvAsync(string filePath) => System.Threading.Tasks.Task.CompletedTask;
        public void AddCustomer(Customer customer) => throw new NotImplementedException();
        public System.Threading.Tasks.Task AddCustomerAsync(Customer customer) => throw new NotImplementedException();
        public void UpdateCustomer(Customer customer) => throw new NotImplementedException();
        public System.Threading.Tasks.Task UpdateCustomerAsync(Customer customer) => throw new NotImplementedException();
        public void DeleteCustomer(int customerID) => throw new NotImplementedException();
        public System.Threading.Tasks.Task DeleteCustomerAsync(int customerID) => throw new NotImplementedException();
        public Customer GetCustomerByID(int customerID) => throw new NotImplementedException();
        public System.Threading.Tasks.Task<Customer> GetCustomerByIDAsync(int customerID) => throw new NotImplementedException();
        public List<Customer> GetAllCustomers() => new();
        public System.Threading.Tasks.Task<List<Customer>> GetAllCustomersAsync() => System.Threading.Tasks.Task.FromResult(new List<Customer>());
        public List<Customer> SearchCustomers(string searchTerm) => new();
        public System.Threading.Tasks.Task<List<Customer>> SearchCustomersAsync(string searchTerm) => System.Threading.Tasks.Task.FromResult(new List<Customer>());
        public System.Threading.Tasks.Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map) => System.Threading.Tasks.Task.FromResult(new CustomerImportResult());
    }

    class StubDatabaseBackupService : IDatabaseBackupService
    {
        public System.Threading.Tasks.Task BackupDatabaseAsync(string backupFilePath) => System.Threading.Tasks.Task.CompletedTask;
    }
}

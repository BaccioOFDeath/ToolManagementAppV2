using System;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Models.ImportExport;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.Views
{
    public class ImportExportPageTests
    {
        [Fact]
        public void ButtonsExecuteCommandsAndUpdateLogs()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "ToolNumber\n");
            Exception? threadException = null;

            try
            {
                var thread = new Thread(() =>
                {
                    try
                    {
                        var toolSvc = new StubToolService();
                        var customerSvc = new StubCustomerService();
                        var fileDlg = new StubFileDialogService { FileToReturn = tmp };
                        var dialog = new StubDialogService();
                        var vm = new ImportExportViewModel(toolSvc, customerSvc, fileDlg, new StubDatabaseBackupService(), dialog);
                        var page = new ImportExportPage { DataContext = vm };
                        var grid = (Grid)page.Content;
                        var panel = (WrapPanel)((Border)grid.Children[0]).Child;
                        var importBtn = (Button)panel.Children[0];

                        Assert.Equal(vm.ImportToolsCommand, importBtn.Command);
                        var asyncCmd = (CommunityToolkit.Mvvm.Input.IAsyncRelayCommand)importBtn.Command;
                        var task = asyncCmd.ExecuteAsync(null);
                        task.GetAwaiter().GetResult();
                        Assert.True(toolSvc.ImportCalled);
                        Assert.Single(vm.ImportExportLogs);
                    }
                    catch (Exception ex)
                    {
                        threadException = ex;
                    }
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

                if (threadException != null)
                {
                    throw threadException;
                }
            }
            finally
            {
                if (File.Exists(tmp))
                    File.Delete(tmp);
            }
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
        public System.Collections.Generic.Dictionary<string, string>? MapToReturn { get; set; } = new();
        public bool ShowImportMappingCalled { get; private set; }
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => false;
        public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
        public void ShowToolDetails(ToolModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, System.Collections.Generic.IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ToolModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
        public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties)
        {
            ShowImportMappingCalled = true;
            return MapToReturn;
        }
        public Func<ToolModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
        public void ShowScannerStatus() { }
    }

    class StubToolService : IToolService
    {
        public bool ImportCalled { get; private set; }
        public System.Collections.Generic.List<int> ImportToolsFromCsv(string filePath, System.Collections.Generic.IDictionary<string, string> map)
        {
            ImportCalled = true;
            return new();
        }
        public System.Threading.Tasks.Task<System.Collections.Generic.List<int>> ImportToolsFromCsvAsync(string filePath, System.Collections.Generic.IDictionary<string, string> map, CancellationToken cancellationToken)
        {
            ImportCalled = true;
            return System.Threading.Tasks.Task.FromResult(new System.Collections.Generic.List<int>());
        }
        public void ExportToolsToCsv(string filePath) { }
        public System.Threading.Tasks.Task ExportToolsToCsvAsync(string filePath) => System.Threading.Tasks.Task.CompletedTask;
        public System.Collections.Generic.List<ToolModel> GetAllTools() => new();
        public void AddTool(ToolModel tool) => throw new NotImplementedException();
        public void UpdateTool(ToolModel tool) => throw new NotImplementedException();
        public void DeleteTool(int toolID) => throw new NotImplementedException();
        public ToolModel GetToolByID(int toolID) => throw new NotImplementedException();
        public System.Collections.Generic.List<ToolModel> SearchTools(string? searchText) => new();
        public void ToggleToolCheckOutStatus(int toolID, string currentUser) => throw new NotImplementedException();
        public System.Collections.Generic.List<ToolModel> GetToolsCheckedOutBy(string userName) => new();
        public void UpdateToolImage(int toolID, string imagePath) => throw new NotImplementedException();
        public ImageImportResult ImportToolImages(string folderPath, Func<ToolModel, System.Collections.Generic.IEnumerable<string>> keySelector) => new();
        public void UpdateToolQuantities(int toolID, int qtyChange, bool isRental, System.Data.SQLite.SQLiteConnection? conn = null, System.Data.SQLite.SQLiteTransaction? tx = null) => throw new NotImplementedException();
    }

    class StubCustomerService : ICustomerService
    {
        public CustomerImportResult ImportCustomersFromCsv(string filePath, System.Collections.Generic.IDictionary<string, string> map) => new CustomerImportResult();
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
        public System.Collections.Generic.List<Customer> GetAllCustomers() => new();
        public System.Threading.Tasks.Task<System.Collections.Generic.List<Customer>> GetAllCustomersAsync() => System.Threading.Tasks.Task.FromResult(new System.Collections.Generic.List<Customer>());
        public System.Collections.Generic.List<Customer> SearchCustomers(string searchTerm) => new();
        public System.Threading.Tasks.Task<System.Collections.Generic.List<Customer>> SearchCustomersAsync(string searchTerm) => System.Threading.Tasks.Task.FromResult(new System.Collections.Generic.List<Customer>());
        public System.Threading.Tasks.Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, System.Collections.Generic.IDictionary<string, string> map) => System.Threading.Tasks.Task.FromResult(new CustomerImportResult());
    }

    class StubDatabaseBackupService : IDatabaseBackupService
    {
        public System.Threading.Tasks.Task BackupDatabaseAsync(string backupFilePath, CancellationToken cancellationToken)
            => System.Threading.Tasks.Task.CompletedTask;
    }
}

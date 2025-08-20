using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Models.ImportExport;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views.Pages;
using ToolManagementAppV2.Views.Windows;
using Xunit;

namespace ToolManagementAppV2.Tests.Views
{
    public class ImportExportPageTests
    {
        [Fact]
        public void ButtonsExecuteCommandsAndUpdateLogs()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "ItemNumber\n");
            Exception? threadException = null;

            try
            {
                var thread = new Thread(() =>
                {
                    try
                    {
                        var toolSvc = new StubItemService();
                        var customerSvc = new StubCustomerService();
                        var fileDlg = new StubFileDialogService { FileToReturn = tmp };
                        var dialog = new StubDialogService();
                        var vm = new ImportExportViewModel(toolSvc, customerSvc, fileDlg, new StubDatabaseBackupService(), dialog);
                        var page = new ImportExportPage { DataContext = vm };
                        var grid = (Grid)page.Content;
                        var panel = (WrapPanel)((Border)grid.Children[0]).Child;
                        var importBtn = (Button)panel.Children[0];

                        Assert.Equal(vm.ImportItemsCommand, importBtn.Command);
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
        public string OpenFile(string filter, string? initialDirectory = null) => FileToReturn;
        public string SaveFile(string filter) => FileToReturn;
    }

    class StubDialogService : IDialogService
    {
        public System.Collections.Generic.Dictionary<string, string>? MapToReturn { get; set; } = new();
        public bool ShowImportMappingCalled { get; private set; }
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => false;
        public ItemModel? ShowEditItemDialog(ItemModel tool) => null;
        public void ShowItemDetails(ItemModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel tool, System.Collections.Generic.IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
        public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties)
        {
            ShowImportMappingCalled = true;
            return MapToReturn;
        }
        public Func<ItemModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }

    class StubItemService : IItemService
    {
        public bool ImportCalled { get; private set; }
        public Task<System.Collections.Generic.List<int>> ImportItemsFromCsvAsync(string filePath, System.Collections.Generic.IDictionary<string, string> map, CancellationToken cancellationToken)
        {
            ImportCalled = true;
            return Task.FromResult(new System.Collections.Generic.List<int>());
        }
        public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<System.Collections.Generic.List<ItemModel>> GetAllItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new System.Collections.Generic.List<ItemModel>());
        public Task AddItemAsync(ItemModel tool, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateItemAsync(ItemModel tool, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteItemAsync(int toolID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ItemModel?> GetItemByIDAsync(int toolID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<System.Collections.Generic.List<ItemModel>> SearchItemsAsync(string? searchText, CancellationToken cancellationToken = default) => Task.FromResult(new System.Collections.Generic.List<ItemModel>());
        public Task<bool> ToggleItemCheckOutStatusAsync(int toolID, string currentUser, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<System.Collections.Generic.List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new System.Collections.Generic.List<ItemModel>());
        public Task UpdateItemImageAsync(int toolID, string imagePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, System.Collections.Generic.IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
        public Task UpdateItemQuantitiesAsync(int toolID, int qtyChange, bool isRental, System.Data.SQLite.SQLiteConnection? conn = null, System.Data.SQLite.SQLiteTransaction? tx = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult("T1");
    }

    class StubCustomerService : ICustomerService
    {
        public CustomerImportResult ImportCustomersFromCsv(string filePath, System.Collections.Generic.IDictionary<string, string> map) => new CustomerImportResult();
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
        public System.Collections.Generic.List<Customer> GetAllCustomers() => new();
        public System.Threading.Tasks.Task<System.Collections.Generic.List<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.FromResult(new System.Collections.Generic.List<Customer>());
        public System.Collections.Generic.List<Customer> SearchCustomers(string searchTerm) => new();
        public System.Threading.Tasks.Task<System.Collections.Generic.List<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.FromResult(new System.Collections.Generic.List<Customer>());
        public System.Threading.Tasks.Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, System.Collections.Generic.IDictionary<string, string> map, CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.FromResult(new CustomerImportResult());
    }

    class StubDatabaseBackupService : IDatabaseBackupService
    {
        public System.Threading.Tasks.Task BackupDatabaseAsync(string backupFilePath, CancellationToken cancellationToken)
            => System.Threading.Tasks.Task.CompletedTask;
    }
}

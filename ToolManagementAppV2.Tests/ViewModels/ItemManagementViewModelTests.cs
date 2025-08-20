using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Data.SQLite;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.ImportExport;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.ViewModels;
using Xunit;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class ItemManagementViewModelTests
    {
        [Fact]
        public async Task SearchCommand_FiltersToolsBySearchTerm()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ItemManagementViewModel(toolService, customerService, rentalService, dialog);
                toolService.AddTool(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" });
                toolService.AddTool(new ItemModel { ItemNumber = "T2", NameDescription = "Saw" });
                vm.SearchTerm = "Ham";
                await vm.SearchCommand.ExecuteAsync(CancellationToken.None);
                Assert.Single(vm.SearchResults);
                Assert.Equal("Hammer", vm.SearchResults.First().NameDescription);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SearchCommand_SupportsMultipleTerms()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ItemManagementViewModel(toolService, customerService, rentalService, dialog);
                toolService.AddTool(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", Brand = "BrandA" });
                toolService.AddTool(new ItemModel { ItemNumber = "T2", NameDescription = "Hammer", Brand = "BrandB" });
                vm.SearchTerm = "Hammer BrandA";
                await vm.SearchCommand.ExecuteAsync(CancellationToken.None);
                Assert.Single(vm.SearchResults);
                Assert.Equal("BrandA", vm.SearchResults.First().Brand);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SearchCommand_ReturnsAllTools_WhenNoSearchTerm()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ItemManagementViewModel(toolService, customerService, rentalService, dialog);
                toolService.AddTool(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" });
                toolService.AddTool(new ItemModel { ItemNumber = "T2", NameDescription = "Cordless Drill", IsPowerTool = true });
                vm.SearchTerm = string.Empty;
                await vm.SearchCommand.ExecuteAsync(CancellationToken.None);
                Assert.Equal(2, vm.SearchResults.Count);
                Assert.Contains(vm.SearchResults, t => t.NameDescription == "Hammer");
                Assert.Contains(vm.SearchResults, t => t.NameDescription == "Cordless Drill");
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task Categories_Update_WhenToolsCollectionChanges()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ItemManagementViewModel(toolService, customerService, rentalService, dialog);
                toolService.AddTool(new ItemModel { ItemNumber = "T1", Brand = "BrandA" });
                await vm.LoadToolsAsync();

                Assert.Contains("BrandA", vm.Categories);

                vm.Tools.Add(new ItemModel { ItemNumber = "T2", Brand = "BrandB" });

                Assert.Contains("BrandB", vm.Categories);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task LoadToolsAsync_DoesNotDuplicateCollectionChangedHandlers()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                var vm = new ItemManagementViewModel(toolService, new StubCustomerService(), new StubRentalService(), new StubDialogService());

                toolService.AddTool(new ItemModel { ItemNumber = "T1" });
                await vm.LoadToolsAsync();
                await vm.LoadToolsAsync();

                var field = typeof(ObservableCollection<ItemModel>).GetField("CollectionChanged", BindingFlags.Instance | BindingFlags.NonPublic);
                var handlers = field?.GetValue(vm.Tools) as MulticastDelegate;
                var count = handlers?.GetInvocationList().Length ?? 0;
                Assert.Equal(1, count);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task LoadToolsAsync_CanBeCalledMultipleTimes()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                var vm = new ItemManagementViewModel(toolService, new StubCustomerService(), new StubRentalService(), new StubDialogService());

                toolService.AddTool(new ItemModel { ItemNumber = "T1", Brand = "BrandA" });
                await vm.LoadToolsAsync();

                toolService.AddTool(new ItemModel { ItemNumber = "T2", Brand = "BrandB" });
                await vm.LoadToolsAsync();

                Assert.Equal(2, vm.Tools.Count);
                Assert.Contains("BrandA", vm.Categories);
                Assert.Contains("BrandB", vm.Categories);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task AddTool_ShowsDialog_OnError()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ItemManagementViewModel(toolService, customerService, rentalService, dialog);
                vm.NewTool.ItemNumber = string.Empty;
                await vm.NewToolCommand.ExecuteAsync(null);
                Assert.True(dialog.InfoShown);
                Assert.Empty(toolService.GetAllTools());
                Assert.Equal(string.Empty, vm.NewTool.ItemNumber);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        class StubDialogService : IDialogService
        {
            public bool InfoShown;
            public bool ConfirmationResult;
            public Func<ItemModel, ItemModel?>? EditToolHandler;
            public Action<ItemModel>? ViewDetailsHandler;

            public void ShowInfo(string message, string title) => InfoShown = true;
            public bool ShowConfirmation(string message, string title) => ConfirmationResult;
            public ItemModel? ShowEditToolDialog(ItemModel tool) => EditToolHandler?.Invoke(tool);
            public void ShowToolDetails(ItemModel tool) => ViewDetailsHandler?.Invoke(tool);
            public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ItemModel tool, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
            public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
            public System.Func<ItemModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(10001)]
        public async Task AddToolCommand_ShowsDialog_OnInvalidQuantity(int quantity)
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ItemManagementViewModel(toolService, customerService, rentalService, dialog);
                vm.NewTool.ItemNumber = "TN1";
                vm.NewTool.NameDescription = "Hammer";
                vm.NewTool.QuantityOnHand = quantity;
                await vm.NewToolCommand.ExecuteAsync(null);
                Assert.True(dialog.InfoShown);
                Assert.Empty(toolService.GetAllTools());
                Assert.Equal(quantity, vm.NewTool.QuantityOnHand);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task NewToolCommand_PersistsNewToolValues()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ItemManagementViewModel(toolService, customerService, rentalService, dialog);
                vm.NewTool.ItemNumber = "TN1";
                vm.NewTool.NameDescription = "Hammer";
                vm.NewTool.PartNumber = "PN1";
                vm.NewTool.Brand = "BrandA";
                vm.NewTool.Location = "Shelf";
                vm.NewTool.QuantityOnHand = 5;
                vm.NewTool.Supplier = "ABC";
                vm.NewTool.Notes = "Note";
                vm.NewTool.IsPowerTool = true;
                await vm.NewToolCommand.ExecuteAsync(null);
                var tools = toolService.GetAllTools();
                Assert.Single(tools);
                var tool = tools.First();
                Assert.Equal("TN1", tool.ItemNumber);
                Assert.Equal("Hammer", tool.NameDescription);
                Assert.Equal("PN1", tool.PartNumber);
                Assert.Equal("BrandA", tool.Brand);
                Assert.Equal("Shelf", tool.Location);
                Assert.Equal(5, tool.QuantityOnHand);
                Assert.Equal("ABC", tool.Supplier);
                Assert.Equal("Note", tool.Notes);
                Assert.True(tool.IsPowerTool);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task EditToolCommand_UpdatesExistingTool_WhenDialogReturnsTool()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ItemManagementViewModel(toolService, customerService, rentalService, dialog);
                var tool = new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", ImagePath = "img1.png" };
                toolService.AddTool(tool);
                await vm.LoadToolsAsync();
                vm.SelectedItem = vm.Tools.First();
                dialog.EditToolHandler = t =>
                {
                    t.NameDescription = "Updated Hammer";
                    return t;
                };
                await vm.EditToolCommand.ExecuteAsync(null);
                var updated = toolService.GetAllTools().First();
                Assert.Equal("Updated Hammer", updated.NameDescription);
                Assert.Equal("Updated Hammer", vm.Tools.First().NameDescription);
                Assert.Equal("img1.png", updated.ImagePath);
                Assert.Equal("img1.png", vm.Tools.First().ImagePath);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task EditToolCommand_DoesNothing_WhenDialogReturnsNull()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ItemManagementViewModel(toolService, customerService, rentalService, dialog);
                var tool = new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" };
                toolService.AddTool(tool);
                await vm.LoadToolsAsync();
                vm.SelectedItem = vm.Tools.First();
                dialog.EditToolHandler = _ => null;
                await vm.EditToolCommand.ExecuteAsync(null);
                var unchanged = toolService.GetAllTools().First();
                Assert.Equal("Hammer", unchanged.NameDescription);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task DeleteToolCommand_RemovesTool()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService { ConfirmationResult = true };
                var vm = new ItemManagementViewModel(toolService, customerService, rentalService, dialog);
                var tool = new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" };
                toolService.AddTool(tool);
                await vm.LoadToolsAsync();
                vm.SelectedItem = vm.Tools.First();
                await vm.DeleteToolCommand.ExecuteAsync(null);
                Assert.Empty(toolService.GetAllTools());
                Assert.Empty(vm.Tools);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task DeleteToolCommand_Cancelled_DoesNotRemoveTool()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService { ConfirmationResult = false };
                var vm = new ItemManagementViewModel(toolService, customerService, rentalService, dialog);
                var tool = new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" };
                toolService.AddTool(tool);
                await vm.LoadToolsAsync();
                vm.SelectedItem = vm.Tools.First();
                await vm.DeleteToolCommand.ExecuteAsync(null);
                Assert.Single(toolService.GetAllTools());
                Assert.Single(vm.Tools);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task DeleteToolCommand_OnError_ShowsDialogAndLogs()
        {
            var toolService = new FailingItemService();
            var dialog = new StubDialogService { ConfirmationResult = true };
            var logger = new CapturingLogger<ItemManagementViewModel>();
            var vm = new ItemManagementViewModel(toolService, new StubCustomerService(), new StubRentalService(), dialog, logger);
            var tool = new ItemModel { ItemID = 1, ItemNumber = "T1", NameDescription = "Hammer" };
            vm.Tools.Add(tool);
            vm.SelectedItem = tool;

            await vm.DeleteToolCommand.ExecuteAsync(null);

            Assert.True(dialog.InfoShown);
            Assert.Equal("Failed to delete tool 1", logger.LastError);
            Assert.Single(vm.Tools);
            Assert.Equal(tool, vm.SelectedItem);
        }

        [Fact]
        public async Task OpenRentalsCommand_CanExecuteDependsOnSelectedItem()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ItemManagementViewModel(toolService, customerService, rentalService, dialog);

                Assert.False(vm.OpenRentalsCommand.CanExecute(null));

                toolService.AddTool(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" });
                await vm.LoadToolsAsync();
                vm.SelectedItem = vm.Tools.First();

                Assert.True(vm.OpenRentalsCommand.CanExecute(null));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task ViewDetailsCommand_InvokesDialog()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ItemManagementViewModel(toolService, customerService, rentalService, dialog);
                var tool = new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" };
                toolService.AddTool(tool);
                await vm.LoadToolsAsync();
                vm.SelectedItem = vm.Tools.First();
                bool called = false;
                ItemModel? passed = null;
                dialog.ViewDetailsHandler = t => { called = true; passed = t; };
                vm.ViewDetailsCommand.Execute(null);
                Assert.True(called);
                Assert.Equal(vm.SelectedItem, passed);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task ViewDetailsCommand_CanExecuteDependsOnSelectedItem()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ItemManagementViewModel(toolService, customerService, rentalService, dialog);

                Assert.False(vm.ViewDetailsCommand.CanExecute(null));

                toolService.AddTool(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" });
                await vm.LoadToolsAsync();
                vm.SelectedItem = vm.Tools.First();

                Assert.True(vm.ViewDetailsCommand.CanExecute(null));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task FilterToolsAsync_UsesSearchService_WhenTermProvided()
        {
            var tools = new List<ItemModel>
            {
                new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", Brand = "BrandA" },
                new ItemModel { ItemNumber = "T2", NameDescription = "Saw", Brand = "BrandB" }
            };
            var toolService = new CountingItemService(tools);
            var vm = new ItemManagementViewModel(toolService, new StubCustomerService(), new StubRentalService(), new StubDialogService());
            vm.SearchTerm = "Ham";
            await vm.SearchCommand.ExecuteAsync(CancellationToken.None);
            Assert.Equal(1, toolService.SearchToolsAsyncCalls);
            Assert.Equal(0, toolService.GetAllToolsAsyncCalls);
        }

        [Fact]
        public async Task FilterToolsAsync_ReusesCache_WhenNoSearchTerm()
        {
            var tools = new List<ItemModel>
            {
                new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", Brand = "BrandA" }
            };
            var toolService = new CountingItemService(tools);
            var vm = new ItemManagementViewModel(toolService, new StubCustomerService(), new StubRentalService(), new StubDialogService());

            await vm.SearchCommand.ExecuteAsync(CancellationToken.None);
            Assert.Equal(1, toolService.GetAllToolsAsyncCalls);
            Assert.Equal(0, toolService.SearchToolsAsyncCalls);

            await vm.SearchCommand.ExecuteAsync(CancellationToken.None);
            Assert.Equal(1, toolService.GetAllToolsAsyncCalls);
        }

        [Fact]
        public void SearchText_DebouncesRapidChanges()
        {
            var tools = new List<ItemModel>
            {
                new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" }
            };
            var toolService = new CountingItemService(tools);
            var timer = new TestDispatcherTimer();
            var vm = new ItemManagementViewModel(toolService, new StubCustomerService(), new StubRentalService(), new StubDialogService(), null, timer);

            vm.SearchText = "H";
            vm.SearchText = "Ha";
            vm.SearchText = "Ham";

            Assert.Equal(0, toolService.SearchToolsAsyncCalls);

            timer.RaiseTick();

            Assert.Equal(1, toolService.SearchToolsAsyncCalls);
        }

        [Fact]
        public void Dispose_StopsSearchDebounceTimer()
        {
            var tools = new List<ItemModel>
            {
                new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" }
            };
            var toolService = new CountingItemService(tools);
            var timer = new TestDispatcherTimer();
            var vm = new ItemManagementViewModel(toolService, new StubCustomerService(), new StubRentalService(), new StubDialogService(), null, timer);

            vm.SearchText = "Ha";
            Assert.True(timer.IsEnabled);

            vm.Dispose();
            var ex = Record.Exception(() => vm.Dispose());
            Assert.Null(ex);
            Assert.False(timer.IsEnabled);
        }

        [Fact]
        public async Task SearchCommand_CanBeCancelled()
        {
            var tools = new List<ItemModel>
            {
                new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", Brand = "BrandA" }
            };
            var toolService = new CountingItemService(tools);
            var vm = new ItemManagementViewModel(toolService, new StubCustomerService(), new StubRentalService(), new StubDialogService());
            vm.SearchTerm = "Ham";
            var cts = new CancellationTokenSource();
            cts.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => vm.SearchCommand.ExecuteAsync(cts.Token));
        }

        class CountingItemService : IItemService
        {
            public int GetAllToolsAsyncCalls { get; private set; }
            public int SearchToolsAsyncCalls { get; private set; }
            readonly List<ItemModel> _tools;
            public CountingItemService(IEnumerable<ItemModel> tools) => _tools = tools.ToList();

            public Task<List<ItemModel>> GetAllToolsAsync(CancellationToken cancellationToken = default)
            {
                GetAllToolsAsyncCalls++;
                return Task.FromResult(_tools.ToList());
            }

            public Task<List<ItemModel>> SearchToolsAsync(string? searchText, CancellationToken cancellationToken = default)
            {
                SearchToolsAsyncCalls++;
                if (cancellationToken.IsCancellationRequested)
                    return Task.FromCanceled<List<ItemModel>>(cancellationToken);
                if (string.IsNullOrWhiteSpace(searchText))
                    return Task.FromResult(_tools.ToList());
                var term = searchText.Trim();
                var results = _tools.Where(t =>
                    (t.ItemNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (t.NameDescription?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (t.Brand?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (t.PartNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
                return Task.FromResult(results);
            }

            public Task AddToolAsync(ItemModel tool, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task UpdateToolAsync(ItemModel tool, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task DeleteToolAsync(int toolID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<ItemModel?> GetToolByIDAsync(int toolID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<bool> ToggleToolCheckOutStatusAsync(int toolID, string currentUser, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<ItemModel>> GetToolsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task UpdateToolImageAsync(int toolID, string imagePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<int>> ImportToolsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task ExportToolsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<ImageImportResult> ImportToolImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task UpdateToolQuantitiesAsync(int toolID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult("T1");
        }

        class FailingItemService : IItemService
        {
            public Task AddToolAsync(ItemModel tool, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task UpdateToolAsync(ItemModel tool, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task DeleteToolAsync(int toolID, CancellationToken cancellationToken = default) => throw new Exception("failure");
            public Task<ItemModel?> GetToolByIDAsync(int toolID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<ItemModel>> GetAllToolsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task<List<ItemModel>> SearchToolsAsync(string? searchText, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task<bool> ToggleToolCheckOutStatusAsync(int toolID, string currentUser, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<ItemModel>> GetToolsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task UpdateToolImageAsync(int toolID, string imagePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<int>> ImportToolsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => Task.FromResult(new List<int>());
            public Task ExportToolsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ImageImportResult> ImportToolImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
            public Task UpdateToolQuantitiesAsync(int toolID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult("T1");
        }

        class CapturingLogger<T> : ILogger<T>
        {
            public string? LastError { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => NullDisposable.Instance;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Error)
                    LastError = formatter(state, exception);
            }

            private sealed class NullDisposable : IDisposable
            {
                public static readonly NullDisposable Instance = new();
                public void Dispose() { }
            }
        }

        class TestDispatcherTimer : IDispatcherTimer
        {
            public event EventHandler Tick;
            public TimeSpan Interval { get; set; }
            public bool IsEnabled { get; private set; }
            public void Start() => IsEnabled = true;
            public void Stop() => IsEnabled = false;
            public void RaiseTick()
            {
                if (IsEnabled)
                    Tick?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

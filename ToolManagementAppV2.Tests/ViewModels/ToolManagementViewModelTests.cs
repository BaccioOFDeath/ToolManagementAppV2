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
    public class ToolManagementViewModelTests
    {
        [Fact]
        public async Task SearchCommand_FiltersToolsBySearchTerm()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);
                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });
                toolService.AddTool(new Tool { ToolNumber = "T2", NameDescription = "Saw" });
                vm.SearchTerm = "Ham";
                await vm.SearchCommand.ExecuteAsync(null);
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
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);
                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer", Brand = "BrandA" });
                toolService.AddTool(new Tool { ToolNumber = "T2", NameDescription = "Hammer", Brand = "BrandB" });
                vm.SearchTerm = "Hammer BrandA";
                await vm.SearchCommand.ExecuteAsync(null);
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
        public async Task SearchCommand_SortsResultsIntoCategories()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);
                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });
                toolService.AddTool(new Tool { ToolNumber = "T2", NameDescription = "Cordless Drill", IsPowerTool = true });
                vm.SearchTerm = string.Empty;
                await vm.SearchCommand.ExecuteAsync(null);
                Assert.Single(vm.HandTools);
                Assert.Single(vm.PowerTools);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SearchCommand_FiltersToolsBySelectedCategory()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);
                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer", Brand = "BrandA" });
                toolService.AddTool(new Tool { ToolNumber = "T2", NameDescription = "Saw", Brand = "BrandB" });
                vm.SelectedCategory = "BrandA";
                await vm.SearchCommand.ExecuteAsync(null);
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
        public async Task Categories_Update_WhenToolsCollectionChanges()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);
                toolService.AddTool(new Tool { ToolNumber = "T1", Brand = "BrandA" });
                await vm.LoadToolsAsync();

                Assert.Contains("BrandA", vm.Categories);

                vm.Tools.Add(new Tool { ToolNumber = "T2", Brand = "BrandB" });

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
                IToolService toolService = new ToolService(db);
                var vm = new ToolManagementViewModel(toolService, new StubCustomerService(), new StubRentalService(), new StubDialogService());

                toolService.AddTool(new Tool { ToolNumber = "T1" });
                await vm.LoadToolsAsync();
                await vm.LoadToolsAsync();

                var field = typeof(ObservableCollection<ToolModel>).GetField("CollectionChanged", BindingFlags.Instance | BindingFlags.NonPublic);
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
                IToolService toolService = new ToolService(db);
                var vm = new ToolManagementViewModel(toolService, new StubCustomerService(), new StubRentalService(), new StubDialogService());

                toolService.AddTool(new Tool { ToolNumber = "T1", Brand = "BrandA" });
                await vm.LoadToolsAsync();

                toolService.AddTool(new Tool { ToolNumber = "T2", Brand = "BrandB" });
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
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);
                vm.NewTool.ToolNumber = string.Empty;
                await vm.NewToolCommand.ExecuteAsync(null);
                Assert.True(dialog.InfoShown);
                Assert.Empty(toolService.GetAllTools());
                Assert.Equal(string.Empty, vm.NewTool.ToolNumber);
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
            public Func<ToolModel, ToolModel?>? EditToolHandler;
            public Action<ToolModel>? ViewDetailsHandler;

            public void ShowInfo(string message, string title) => InfoShown = true;
            public bool ShowConfirmation(string message, string title) => ConfirmationResult;
            public ToolModel? ShowEditToolDialog(ToolModel tool) => EditToolHandler?.Invoke(tool);
            public void ShowToolDetails(ToolModel tool) => ViewDetailsHandler?.Invoke(tool);
            public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ToolModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
            public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
            public System.Func<ToolModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
            public void ShowScannerStatus() { }
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
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);
                vm.NewTool.ToolNumber = "TN1";
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
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);
                vm.NewTool.ToolNumber = "TN1";
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
                Assert.Equal("TN1", tool.ToolNumber);
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
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);
                var tool = new Tool { ToolNumber = "T1", NameDescription = "Hammer", ToolImagePath = "img1.png" };
                toolService.AddTool(tool);
                await vm.LoadToolsAsync();
                vm.SelectedTool = vm.Tools.First();
                dialog.EditToolHandler = t =>
                {
                    t.NameDescription = "Updated Hammer";
                    return t;
                };
                await vm.EditToolCommand.ExecuteAsync(null);
                var updated = toolService.GetAllTools().First();
                Assert.Equal("Updated Hammer", updated.NameDescription);
                Assert.Equal("Updated Hammer", vm.Tools.First().NameDescription);
                Assert.Equal("img1.png", updated.ToolImagePath);
                Assert.Equal("img1.png", vm.Tools.First().ToolImagePath);
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
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);
                var tool = new Tool { ToolNumber = "T1", NameDescription = "Hammer" };
                toolService.AddTool(tool);
                await vm.LoadToolsAsync();
                vm.SelectedTool = vm.Tools.First();
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
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService { ConfirmationResult = true };
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);
                var tool = new Tool { ToolNumber = "T1", NameDescription = "Hammer" };
                toolService.AddTool(tool);
                await vm.LoadToolsAsync();
                vm.SelectedTool = vm.Tools.First();
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
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService { ConfirmationResult = false };
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);
                var tool = new Tool { ToolNumber = "T1", NameDescription = "Hammer" };
                toolService.AddTool(tool);
                await vm.LoadToolsAsync();
                vm.SelectedTool = vm.Tools.First();
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
            var toolService = new FailingToolService();
            var dialog = new StubDialogService { ConfirmationResult = true };
            var logger = new CapturingLogger<ToolManagementViewModel>();
            var vm = new ToolManagementViewModel(toolService, new StubCustomerService(), new StubRentalService(), dialog, logger);
            var tool = new Tool { ToolID = 1, ToolNumber = "T1", NameDescription = "Hammer" };
            vm.Tools.Add(tool);
            vm.SelectedTool = tool;

            await vm.DeleteToolCommand.ExecuteAsync(null);

            Assert.True(dialog.InfoShown);
            Assert.Equal("Failed to delete tool 1", logger.LastError);
            Assert.Single(vm.Tools);
            Assert.Equal(tool, vm.SelectedTool);
        }

        [Fact]
        public async Task OpenRentalsCommand_CanExecuteDependsOnSelectedTool()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);

                Assert.False(vm.OpenRentalsCommand.CanExecute(null));

                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });
                await vm.LoadToolsAsync();
                vm.SelectedTool = vm.Tools.First();

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
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);
                var tool = new Tool { ToolNumber = "T1", NameDescription = "Hammer" };
                toolService.AddTool(tool);
                await vm.LoadToolsAsync();
                vm.SelectedTool = vm.Tools.First();
                bool called = false;
                Tool? passed = null;
                dialog.ViewDetailsHandler = t => { called = true; passed = t; };
                vm.ViewDetailsCommand.Execute(null);
                Assert.True(called);
                Assert.Equal(vm.SelectedTool, passed);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task ViewDetailsCommand_CanExecuteDependsOnSelectedTool()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var dialog = new StubDialogService();
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);

                Assert.False(vm.ViewDetailsCommand.CanExecute(null));

                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });
                await vm.LoadToolsAsync();
                vm.SelectedTool = vm.Tools.First();

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
            var tools = new List<ToolModel>
            {
                new Tool { ToolNumber = "T1", NameDescription = "Hammer", Brand = "BrandA" },
                new Tool { ToolNumber = "T2", NameDescription = "Saw", Brand = "BrandB" }
            };
            var toolService = new CountingToolService(tools);
            var vm = new ToolManagementViewModel(toolService, new StubCustomerService(), new StubRentalService(), new StubDialogService());
            vm.SearchTerm = "Ham";
            await vm.SearchCommand.ExecuteAsync(null);
            Assert.Equal(1, toolService.SearchToolsAsyncCalls);
            Assert.Equal(0, toolService.GetAllToolsAsyncCalls);
        }

        [Fact]
        public async Task FilterToolsAsync_ReusesCache_WhenNoSearchTerm()
        {
            var tools = new List<ToolModel>
            {
                new Tool { ToolNumber = "T1", NameDescription = "Hammer", Brand = "BrandA" }
            };
            var toolService = new CountingToolService(tools);
            var vm = new ToolManagementViewModel(toolService, new StubCustomerService(), new StubRentalService(), new StubDialogService());

            await vm.SearchCommand.ExecuteAsync(null);
            Assert.Equal(1, toolService.GetAllToolsAsyncCalls);
            Assert.Equal(0, toolService.SearchToolsAsyncCalls);

            await vm.SearchCommand.ExecuteAsync(null);
            Assert.Equal(1, toolService.GetAllToolsAsyncCalls);
        }

        [Fact]
        public void SearchText_DebouncesRapidChanges()
        {
            var tools = new List<ToolModel>
            {
                new Tool { ToolNumber = "T1", NameDescription = "Hammer" }
            };
            var toolService = new CountingToolService(tools);
            var timer = new TestDispatcherTimer();
            var vm = new ToolManagementViewModel(toolService, new StubCustomerService(), new StubRentalService(), new StubDialogService(), null, timer);

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
            var tools = new List<ToolModel>
            {
                new Tool { ToolNumber = "T1", NameDescription = "Hammer" }
            };
            var toolService = new CountingToolService(tools);
            var timer = new TestDispatcherTimer();
            var vm = new ToolManagementViewModel(toolService, new StubCustomerService(), new StubRentalService(), new StubDialogService(), null, timer);

            vm.SearchText = "Ha";
            Assert.True(timer.IsEnabled);

            vm.Dispose();

            Assert.False(timer.IsEnabled);
        }

        class CountingToolService : IToolService
        {
            public int GetAllToolsAsyncCalls { get; private set; }
            public int SearchToolsAsyncCalls { get; private set; }
            readonly List<ToolModel> _tools;
            public CountingToolService(IEnumerable<ToolModel> tools) => _tools = tools.ToList();

            public List<ToolModel> GetAllTools()
            {
                GetAllToolsAsyncCalls++;
                return _tools.ToList();
            }

            public Task<List<ToolModel>> GetAllToolsAsync()
            {
                GetAllToolsAsyncCalls++;
                return Task.FromResult(_tools.ToList());
            }

            public List<ToolModel> SearchTools(string? searchText)
            {
                SearchToolsAsyncCalls++;
                return _tools.ToList();
            }

            public Task<List<ToolModel>> SearchToolsAsync(string? searchText)
            {
                SearchToolsAsyncCalls++;
                if (string.IsNullOrWhiteSpace(searchText))
                    return Task.FromResult(_tools.ToList());
                var term = searchText.Trim();
                var results = _tools.Where(t =>
                    (t.ToolNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (t.NameDescription?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (t.Brand?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (t.PartNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
                return Task.FromResult(results);
            }

            public void AddTool(ToolModel tool) => throw new NotImplementedException();
            public Task AddToolAsync(ToolModel tool) => throw new NotImplementedException();
            public void UpdateTool(ToolModel tool) => throw new NotImplementedException();
            public Task UpdateToolAsync(ToolModel tool) => throw new NotImplementedException();
            public void DeleteTool(int toolID) => throw new NotImplementedException();
            public Task DeleteToolAsync(int toolID) => throw new NotImplementedException();
            public ToolModel GetToolByID(int toolID) => throw new NotImplementedException();
            public Task<ToolModel> GetToolByIDAsync(int toolID) => throw new NotImplementedException();
            public void ToggleToolCheckOutStatus(int toolID, string currentUser) => throw new NotImplementedException();
            public Task ToggleToolCheckOutStatusAsync(int toolID, string currentUser) => throw new NotImplementedException();
            public List<ToolModel> GetToolsCheckedOutBy(string userName) => throw new NotImplementedException();
            public Task<List<ToolModel>> GetToolsCheckedOutByAsync(string userName) => throw new NotImplementedException();
            public void UpdateToolImage(int toolID, string imagePath) => throw new NotImplementedException();
            public Task UpdateToolImageAsync(int toolID, string imagePath) => throw new NotImplementedException();
            public List<int> ImportToolsFromCsv(string filePath, IDictionary<string, string> map) => throw new NotImplementedException();
            public Task<List<int>> ImportToolsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => throw new NotImplementedException();
            public void ExportToolsToCsv(string filePath) => throw new NotImplementedException();
            public Task ExportToolsToCsvAsync(string filePath) => throw new NotImplementedException();
            public ImageImportResult ImportToolImages(string folderPath, Func<ToolModel, IEnumerable<string>> keySelector) => throw new NotImplementedException();
            public Task<ImageImportResult> ImportToolImagesAsync(string folderPath, Func<ToolModel, IEnumerable<string>> keySelector) => throw new NotImplementedException();
            public void UpdateToolQuantities(int toolID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null) => throw new NotImplementedException();
            public Task UpdateToolQuantitiesAsync(int toolID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null) => throw new NotImplementedException();
        }

        class FailingToolService : IToolService
        {
            public void AddTool(ToolModel tool) => throw new NotImplementedException();
            public Task AddToolAsync(ToolModel tool) => throw new NotImplementedException();
            public void UpdateTool(ToolModel tool) => throw new NotImplementedException();
            public Task UpdateToolAsync(ToolModel tool) => throw new NotImplementedException();
            public void DeleteTool(int toolID) => throw new NotImplementedException();
            public Task DeleteToolAsync(int toolID) => throw new Exception("failure");
            public ToolModel GetToolByID(int toolID) => throw new NotImplementedException();
            public Task<ToolModel> GetToolByIDAsync(int toolID) => throw new NotImplementedException();
            public List<ToolModel> GetAllTools() => new();
            public Task<List<ToolModel>> GetAllToolsAsync() => Task.FromResult(new List<ToolModel>());
            public List<ToolModel> SearchTools(string? searchText) => new();
            public Task<List<ToolModel>> SearchToolsAsync(string? searchText) => Task.FromResult(new List<ToolModel>());
            public void ToggleToolCheckOutStatus(int toolID, string currentUser) => throw new NotImplementedException();
            public Task ToggleToolCheckOutStatusAsync(int toolID, string currentUser) => throw new NotImplementedException();
            public List<ToolModel> GetToolsCheckedOutBy(string userName) => new();
            public Task<List<ToolModel>> GetToolsCheckedOutByAsync(string userName) => Task.FromResult(new List<ToolModel>());
            public void UpdateToolImage(int toolID, string imagePath) => throw new NotImplementedException();
            public Task UpdateToolImageAsync(int toolID, string imagePath) => throw new NotImplementedException();
            public List<int> ImportToolsFromCsv(string filePath, IDictionary<string, string> map) => new();
            public Task<List<int>> ImportToolsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => Task.FromResult(new List<int>());
            public void ExportToolsToCsv(string filePath) { }
            public Task ExportToolsToCsvAsync(string filePath) => Task.CompletedTask;
            public ImageImportResult ImportToolImages(string folderPath, Func<ToolModel, IEnumerable<string>> keySelector) => new();
            public Task<ImageImportResult> ImportToolImagesAsync(string folderPath, Func<ToolModel, IEnumerable<string>> keySelector) => Task.FromResult(new ImageImportResult());
            public void UpdateToolQuantities(int toolID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null) => throw new NotImplementedException();
            public Task UpdateToolQuantitiesAsync(int toolID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null) => throw new NotImplementedException();
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

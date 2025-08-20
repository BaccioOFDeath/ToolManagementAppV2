using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Interfaces;
using System.Collections.Generic;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class ManageRentalsViewModelTests
    {
        [Fact]
        public async Task ApplyFilter_FiltersByStatus()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool1 = new ItemModel { ToolNumber = "T1" };
                var tool2 = new ItemModel { ToolNumber = "T2" };
                toolService.AddTool(tool1);
                toolService.AddTool(tool2);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool(tool1.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                rentalService.RentTool(tool2.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                var all = rentalService.GetAllRentals();
                rentalService.ReturnTool(all[1].RentalID, DateTime.Today);

                var vm = new ManageRentalsViewModel(rentalService, new StubDialogService());
                await vm.LoadRentalsAsync();

                Assert.Equal(2, vm.Rentals.Count);

                vm.SelectedStatus = "Returned";
                vm.ApplyFilterCommand.Execute(null);

                Assert.Single(vm.Rentals);
                Assert.Equal("Returned", vm.Rentals[0].Status);

                vm.ClearFilterCommand.Execute(null);
                Assert.Equal(2, vm.Rentals.Count);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task ApplyFilter_FiltersBySearchText()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool1 = new ItemModel { ToolNumber = "Alpha" };
                var tool2 = new ItemModel { ToolNumber = "Beta" };
                toolService.AddTool(tool1);
                toolService.AddTool(tool2);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool(tool1.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                rentalService.RentTool(tool2.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var vm = new ManageRentalsViewModel(rentalService, new StubDialogService());
                await vm.LoadRentalsAsync();

                vm.SearchText = "Alpha";
                vm.ApplyFilterCommand.Execute(null);

                Assert.Single(vm.Rentals);
                Assert.Contains("Alpha", vm.Rentals[0].ToolNumber);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task ApplyFilter_ShowsMessage_WhenFromAfterTo()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool = new ItemModel { ToolNumber = "T1" };
                toolService.AddTool(tool);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool(tool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                rentalService.RentTool(tool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var dialog = new StubDialogService();
                var vm = new ManageRentalsViewModel(rentalService, dialog);
                await vm.LoadRentalsAsync();

                vm.FilterFrom = DateTime.Today.AddDays(1);
                vm.FilterTo = DateTime.Today;

                vm.ApplyFilterCommand.Execute(null);

                Assert.Equal(2, vm.Rentals.Count);
                Assert.Equal("\"From\" date cannot be later than \"To\" date.", dialog.LastInfoMessage);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void CloseCommand_DoesNotThrowWithoutWindow()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var rentalService = new RentalService(db);
                var vm = new ManageRentalsViewModel(rentalService, new StubDialogService());
                vm.CloseCommand.Execute(null);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task DeleteRentalCommand_RemovesRentalAndClearsSelection()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool = new ItemModel { ToolNumber = "T1" };
                toolService.AddTool(tool);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool(tool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var vm = new ManageRentalsViewModel(rentalService, new StubDialogService());
                await vm.LoadRentalsAsync();
                vm.SelectedRental = vm.Rentals.First();

                await vm.DeleteRentalCommand.ExecuteAsync(null);

                Assert.Empty(vm.Rentals);
                Assert.Empty(rentalService.GetAllRentals());
                Assert.Null(vm.SelectedRental);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task CheckInCommand_UpdatesRentalStatus()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool = new ItemModel { ToolNumber = "T1" };
                toolService.AddTool(tool);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool(tool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var vm = new ManageRentalsViewModel(rentalService, new StubDialogService());
                await vm.LoadRentalsAsync();
                vm.SelectedRental = vm.Rentals.First();

                await vm.CheckInCommand.ExecuteAsync(null);

                Assert.Equal("Returned", vm.Rentals.First().Status);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task ExtendCommand_ExtendsDueDate()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool = new ItemModel { ToolNumber = "T1" };
                toolService.AddTool(tool);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool(tool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var vm = new ManageRentalsViewModel(rentalService, new StubDialogService());
                await vm.LoadRentalsAsync();
                vm.SelectedRental = vm.Rentals.First();
                var oldDue = vm.SelectedRental.DueDate;

                await vm.ExtendCommand.ExecuteAsync(null);

                Assert.Equal(oldDue.AddDays(7), vm.Rentals.First().DueDate);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task CheckInCommand_ShowsDialogOnFailure()
        {
            var rentals = new List<Rental>
            {
                new Rental
                {
                    RentalID = 1,
                    ToolID = 1,
                    ToolNumber = "T1",
                    CustomerID = 1,
                    CustomerName = "C1",
                    RentalDate = DateTime.Today,
                    DueDate = DateTime.Today.AddDays(1),
                    Status = "Rented"
                }
            };
            var rentalService = new ExceptionRentalService(rentals);
            var dialog = new StubDialogService();
            var vm = new ManageRentalsViewModel(rentalService, dialog);
            await vm.LoadRentalsAsync();
            vm.SelectedRental = vm.Rentals.First();

            await vm.CheckInCommand.ExecuteAsync(null);

            Assert.Contains("boom", dialog.LastInfoMessage);
        }

        [Fact]
        public async Task ExtendCommand_ShowsDialogOnFailure()
        {
            var rentals = new List<Rental>
            {
                new Rental
                {
                    RentalID = 1,
                    ToolID = 1,
                    ToolNumber = "T1",
                    CustomerID = 1,
                    CustomerName = "C1",
                    RentalDate = DateTime.Today,
                    DueDate = DateTime.Today.AddDays(1),
                    Status = "Rented"
                }
            };
            var rentalService = new ExceptionRentalService(rentals);
            var dialog = new StubDialogService();
            var vm = new ManageRentalsViewModel(rentalService, dialog);
            await vm.LoadRentalsAsync();
            vm.SelectedRental = vm.Rentals.First();

            await vm.ExtendCommand.ExecuteAsync(null);

            Assert.Contains("boom", dialog.LastInfoMessage);
        }

        [Fact]
        public async Task DeleteRentalCommand_ShowsDialogOnFailure()
        {
            var rentals = new List<Rental>
            {
                new Rental
                {
                    RentalID = 1,
                    ToolID = 1,
                    ToolNumber = "T1",
                    CustomerID = 1,
                    CustomerName = "C1",
                    RentalDate = DateTime.Today,
                    DueDate = DateTime.Today.AddDays(1),
                    Status = "Rented"
                }
            };
            var rentalService = new ExceptionRentalService(rentals);
            var dialog = new StubDialogService();
            var vm = new ManageRentalsViewModel(rentalService, dialog);
            await vm.LoadRentalsAsync();
            vm.SelectedRental = vm.Rentals.First();

            await vm.DeleteRentalCommand.ExecuteAsync(null);

            Assert.Contains("boom", dialog.LastInfoMessage);
        }

        [Fact]
        public async Task OpenHistory_ShowsDialogOnFailure()
        {
            var rentals = new List<Rental>
            {
                new Rental
                {
                    RentalID = 1,
                    ToolID = 1,
                    ToolNumber = "T1",
                    CustomerID = 1,
                    CustomerName = "C1",
                    RentalDate = DateTime.Today,
                    DueDate = DateTime.Today.AddDays(1),
                    Status = "Rented"
                }
            };
            var rentalService = new ExceptionRentalService(rentals);
            var dialog = new StubDialogService();
            var vm = new ManageRentalsViewModel(rentalService, dialog);
            await vm.LoadRentalsAsync();
            vm.SelectedRental = vm.Rentals.First();

            await vm.OpenHistoryCommand.ExecuteAsync(null);

            Assert.Contains("boom", dialog.LastInfoMessage);
        }

        [Fact]
        public async Task PrintRental_ShowsDialogOnFailure()
        {
            var rentals = new List<Rental>
            {
                new Rental
                {
                    RentalID = 1,
                    ToolID = 1,
                    ToolNumber = "T1",
                    CustomerID = 1,
                    CustomerName = "C1",
                    RentalDate = DateTime.Today,
                    DueDate = DateTime.Today.AddDays(1),
                    Status = "Rented"
                }
            };
            var rentalService = new ExceptionRentalService(rentals);
            var dialog = new ExceptionDialogService();
            var vm = new ManageRentalsViewModel(rentalService, dialog);
            await vm.LoadRentalsAsync();
            vm.SelectedRental = vm.Rentals.First();

            vm.PrintRentalCommand.Execute(null);

            Assert.Contains("boom", dialog.LastInfoMessage);
        }
    }

    class StubDialogService : IDialogService
    {
        public string? LastInfoMessage { get; private set; }
        public string? LastInfoTitle { get; private set; }
        public void ShowInfo(string message, string title)
        {
            LastInfoMessage = message;
            LastInfoTitle = title;
        }
        public Task ShowInfoAsync(string message, string title)
        {
            ShowInfo(message, title);
            return Task.CompletedTask;
        }
        public bool ShowConfirmation(string message, string title) => false;
        public ItemModel? ShowEditToolDialog(ItemModel tool) => null;
        public void ShowToolDetails(ItemModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ItemModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
        public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
        public System.Func<ItemModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }

    class ExceptionDialogService : IDialogService
    {
        public string? LastInfoMessage { get; private set; }
        public void ShowInfo(string message, string title) => LastInfoMessage = message;
        public bool ShowConfirmation(string message, string title) => false;
        public ItemModel? ShowEditToolDialog(ItemModel tool) => null;
        public void ShowToolDetails(ItemModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ItemModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel tool, IEnumerable<RentalModel> history) { }
        public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties) => null;
        public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) => throw new InvalidOperationException("boom");
        public void ShowPrintLabelDialog() { }
    }

    class ExceptionRentalService : IRentalService
    {
        readonly List<Rental> _rentals;
        public ExceptionRentalService(List<Rental> rentals) => _rentals = rentals;
        public Task<List<Rental>> GetAllRentalsAsync() => Task.FromResult(_rentals);
        public Task ReturnToolAsync(int rentalID, DateTime returnDate) => throw new InvalidOperationException("boom");
        public Task ExtendRentalAsync(int rentalID, DateTime newDueDate) => throw new InvalidOperationException("boom");
        public Task DeleteRentalAsync(int rentalID) => throw new InvalidOperationException("boom");
        public Task<List<Rental>> GetRentalHistoryForToolAsync(int toolID) => throw new InvalidOperationException("boom");
        public void RentTool(int toolID, int customerID, DateTime rentalDate, DateTime dueDate) => throw new NotImplementedException();
        public Task RentToolAsync(int toolID, int customerID, DateTime rentalDate, DateTime dueDate) => throw new NotImplementedException();
        public void ReturnTool(int rentalID, DateTime returnDate) => throw new NotImplementedException();
        public void ExtendRental(int rentalID, DateTime newDueDate) => throw new NotImplementedException();
        public void DeleteRental(int rentalID) => throw new NotImplementedException();
        public List<Rental> GetActiveRentals() => throw new NotImplementedException();
        public Task<List<Rental>> GetActiveRentalsAsync() => throw new NotImplementedException();
        public List<Rental> GetOverdueRentals() => throw new NotImplementedException();
        public Task<List<Rental>> GetOverdueRentalsAsync() => throw new NotImplementedException();
        public List<Rental> GetAllRentals() => throw new NotImplementedException();
        public List<Rental> GetRentalHistoryForTool(int toolID) => throw new NotImplementedException();
        public List<Rental> GetRentalHistoryForCustomer(int customerID) => throw new NotImplementedException();
        public Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID) => throw new NotImplementedException();
    }
}

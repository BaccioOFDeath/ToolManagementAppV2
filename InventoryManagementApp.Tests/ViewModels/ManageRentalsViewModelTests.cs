using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Interfaces;
using System.Collections.Generic;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
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

                var tool1 = new ItemModel { ItemNumber = "T1" };
                var tool2 = new ItemModel { ItemNumber = "T2" };
                toolService.AddItem(tool1);
                toolService.AddItem(tool2);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentItem(tool1.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                rentalService.RentItem(tool2.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                var all = rentalService.GetAllRentals();
                rentalService.ReturnItem(all[1].RentalID, DateTime.Today);

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

                var tool1 = new ItemModel { ItemNumber = "Alpha" };
                var tool2 = new ItemModel { ItemNumber = "Beta" };
                toolService.AddItem(tool1);
                toolService.AddItem(tool2);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentItem(tool1.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                rentalService.RentItem(tool2.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var vm = new ManageRentalsViewModel(rentalService, new StubDialogService());
                await vm.LoadRentalsAsync();

                vm.SearchText = "Alpha";
                vm.ApplyFilterCommand.Execute(null);

                Assert.Single(vm.Rentals);
                Assert.Contains("Alpha", vm.Rentals[0].ItemNumber);
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

                var tool = new ItemModel { ItemNumber = "T1" };
                toolService.AddItem(tool);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentItem(tool.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                rentalService.RentItem(tool.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

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

                var tool = new ItemModel { ItemNumber = "T1" };
                toolService.AddItem(tool);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentItem(tool.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

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

                var tool = new ItemModel { ItemNumber = "T1" };
                toolService.AddItem(tool);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentItem(tool.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

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

                var tool = new ItemModel { ItemNumber = "T1" };
                toolService.AddItem(tool);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentItem(tool.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

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
                    ItemID = 1,
                    ItemNumber = "T1",
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
                    ItemID = 1,
                    ItemNumber = "T1",
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
                    ItemID = 1,
                    ItemNumber = "T1",
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
                    ItemID = 1,
                    ItemNumber = "T1",
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
                    ItemID = 1,
                    ItemNumber = "T1",
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
        public ItemModel? ShowEditItemDialog(ItemModel item) => null;
        public void ShowItemDetails(ItemModel item) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(InventoryManagementApp.ViewModels.ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel item, System.Collections.Generic.IEnumerable<RentalModel> history) { }
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
        public ItemModel? ShowEditItemDialog(ItemModel item) => null;
        public void ShowItemDetails(ItemModel item) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(InventoryManagementApp.ViewModels.ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
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
        public Task ReturnItemAsync(int rentalID, DateTime returnDate) => throw new InvalidOperationException("boom");
        public Task ExtendRentalAsync(int rentalID, DateTime newDueDate) => throw new InvalidOperationException("boom");
        public Task DeleteRentalAsync(int rentalID) => throw new InvalidOperationException("boom");
        public Task<List<Rental>> GetRentalHistoryForItemAsync(int itemID) => throw new InvalidOperationException("boom");
        public void RentItem(int itemID, int customerID, DateTime rentalDate, DateTime dueDate) => throw new NotImplementedException();
        public Task RentItemAsync(int itemID, int customerID, DateTime rentalDate, DateTime dueDate) => throw new NotImplementedException();
        public void ReturnItem(int rentalID, DateTime returnDate) => throw new NotImplementedException();
        public void ExtendRental(int rentalID, DateTime newDueDate) => throw new NotImplementedException();
        public void DeleteRental(int rentalID) => throw new NotImplementedException();
        public List<Rental> GetActiveRentals() => throw new NotImplementedException();
        public Task<List<Rental>> GetActiveRentalsAsync() => throw new NotImplementedException();
        public List<Rental> GetOverdueRentals() => throw new NotImplementedException();
        public Task<List<Rental>> GetOverdueRentalsAsync() => throw new NotImplementedException();
        public List<Rental> GetAllRentals() => throw new NotImplementedException();
        public List<Rental> GetRentalHistoryForItem(int itemID) => throw new NotImplementedException();
        public List<Rental> GetRentalHistoryForCustomer(int customerID) => throw new NotImplementedException();
        public Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID) => throw new NotImplementedException();
    }
}

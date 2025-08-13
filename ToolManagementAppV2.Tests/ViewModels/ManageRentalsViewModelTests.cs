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
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool1 = new Tool { ToolNumber = "T1" };
                var tool2 = new Tool { ToolNumber = "T2" };
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
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool1 = new Tool { ToolNumber = "Alpha" };
                var tool2 = new Tool { ToolNumber = "Beta" };
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
        public async Task DeleteRentalCommand_RemovesRental()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool = new Tool { ToolNumber = "T1" };
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
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool = new Tool { ToolNumber = "T1" };
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
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool = new Tool { ToolNumber = "T1" };
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
    }

    class StubDialogService : IDialogService
    {
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => false;
        public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
        public void ShowToolDetails(ToolModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
    }
}

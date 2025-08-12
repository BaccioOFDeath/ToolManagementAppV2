using System;
using System.IO;
using System.Linq;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Models.Domain;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class ManageRentalsViewModelTests
    {
        [Fact]
        public void ApplyFilter_FiltersByStatus()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool1 = new Tool { ToolID = "T1", ToolNumber = "T1" };
                var tool2 = new Tool { ToolID = "T2", ToolNumber = "T2" };
                toolService.AddTool(tool1);
                toolService.AddTool(tool2);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool("T1", cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                rentalService.RentTool("T2", cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                var all = rentalService.GetAllRentals();
                rentalService.ReturnTool(all[1].RentalID, DateTime.Today);

                var vm = new ManageRentalsViewModel(rentalService);
                vm.LoadRentals();

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
        public void ApplyFilter_FiltersBySearchText()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool1 = new Tool { ToolID = "T1", ToolNumber = "Alpha" };
                var tool2 = new Tool { ToolID = "T2", ToolNumber = "Beta" };
                toolService.AddTool(tool1);
                toolService.AddTool(tool2);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool("T1", cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                rentalService.RentTool("T2", cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var vm = new ManageRentalsViewModel(rentalService);
                vm.LoadRentals();

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
                var vm = new ManageRentalsViewModel(rentalService);
                vm.CloseCommand.Execute(null);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}

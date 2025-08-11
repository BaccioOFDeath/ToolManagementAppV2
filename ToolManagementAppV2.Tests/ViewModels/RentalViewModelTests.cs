using System;
using System.IO;
using System.Linq;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.ViewModels;
using Xunit;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class RentalViewModelTests
    {
        [Fact]
        public void LoadRentals_PopulatesActiveAndOverdue()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                // seed tool and customer
                var tool1 = new Tool { ToolID = "T1", ToolNumber = "T1" };
                var tool2 = new Tool { ToolID = "T2", ToolNumber = "T2" };
                toolService.AddTool(tool1);
                toolService.AddTool(tool2);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                // create one active rental and one overdue rental
                rentalService.RentTool("T1", cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                rentalService.RentTool("T2", cust.CustomerID, DateTime.Today.AddDays(-10), DateTime.Today.AddDays(-5));

                var vm = new RentalViewModel(rentalService);
                vm.LoadRentals();

                Assert.Single(vm.ActiveRentals);
                Assert.Single(vm.OverdueRentals);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}

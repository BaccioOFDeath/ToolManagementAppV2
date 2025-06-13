using System;
using System.IO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Tools;
using Xunit;

namespace ToolManagementAppV2.Tests.Services
{
    public class RentalServiceTests
    {
        [Fact]
        public void GetRentalHistoryForTool_ReturnsHistory()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool = new Tool { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 5 };
                toolService.AddTool(tool);
                var addedTool = toolService.GetAllTools().First();

                var cust = new Customer { Company = "Acme" };
                customerService.AddCustomer(cust);
                var addedCust = customerService.GetAllCustomers().First();

                rentalService.RentTool(addedTool.ToolID, addedCust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                rentalService.RentTool(addedTool.ToolID, addedCust.CustomerID, DateTime.Today.AddDays(2), DateTime.Today.AddDays(3));

                var history = rentalService.GetRentalHistoryForTool(addedTool.ToolID);
                Assert.Equal(2, history.Count);
                Assert.True(history[0].RentalDate > history[1].RentalDate);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void RentTool_NoAvailability_DoesNotThrow()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool = new Tool { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 0 };
                toolService.AddTool(tool);
                var addedTool = toolService.GetAllTools().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                var ex = Record.Exception(() => rentalService.RentTool(addedTool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1)));
                Assert.Null(ex);
                Assert.Empty(rentalService.GetAllRentals());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void RentToolWithTransaction_NoAvailability_DoesNotThrow()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool = new Tool { ToolNumber = "T2", NameDescription = "Wrench", QuantityOnHand = 0 };
                toolService.AddTool(tool);
                var addedTool = toolService.GetAllTools().First();

                customerService.AddCustomer(new Customer { Company = "Beta" });
                var cust = customerService.GetAllCustomers().First();

                var ex = Record.Exception(() => rentalService.RentToolWithTransaction(addedTool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1)));
                Assert.Null(ex);
                Assert.Empty(rentalService.GetAllRentals());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ReturnTool_InvalidRentalID_DoesNotThrow()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var rentalService = new RentalService(db);

                var ex = Record.Exception(() => rentalService.ReturnTool(1, DateTime.Today));
                Assert.Null(ex);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ReturnToolWithTransaction_InvalidRentalID_DoesNotThrow()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var rentalService = new RentalService(db);

                var ex = Record.Exception(() => rentalService.ReturnToolWithTransaction(1, DateTime.Today));
                Assert.Null(ex);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}

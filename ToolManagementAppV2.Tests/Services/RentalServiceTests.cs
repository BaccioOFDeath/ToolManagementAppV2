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
    }
}

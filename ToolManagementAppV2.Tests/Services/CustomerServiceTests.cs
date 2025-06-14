using System.IO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Interfaces;
using Xunit;

namespace ToolManagementAppV2.Tests.Services
{
    public class CustomerServiceTests
    {
        [Fact]
        public void SearchCustomers_WithNull_ReturnsAllCustomers()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ICustomerService service = new CustomerService(dbService);

                service.AddCustomer(new Customer { Company = "Acme", Contact = "J" });

                var results = service.SearchCustomers(null);
                Assert.Single(results);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void GetCustomerByID_ReturnsCustomer()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ICustomerService service = new CustomerService(dbService);

                service.AddCustomer(new Customer { Company = "Acme", Contact = "John" });
                var cust = service.GetAllCustomers().First();

                var fetched = service.GetCustomerByID(cust.CustomerID);
                Assert.NotNull(fetched);
                Assert.Equal(cust.CustomerID, fetched.CustomerID);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}

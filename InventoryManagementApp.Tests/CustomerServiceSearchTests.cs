using System;
using System.IO;
using System.Threading.Tasks;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Customers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CustomerServiceSearchTests
    {
        [Fact]
        public async Task SearchCustomersAsync_FindsCustomersByContactName()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

            try
            {
                using var database = new DatabaseService(dbPath, NullLogger<DatabaseService>.Instance);
                var service = new CustomerService(database, logger: NullLogger<CustomerService>.Instance);

                await service.AddCustomerAsync(new CustomerModel
                {
                    Company = "Northwind Rentals",
                    Email = "dispatch@example.com",
                    Contact = "Mara Contactson",
                    Phone = "555-0100",
                    Mobile = string.Empty,
                    Address = "1 Test Lane"
                });

                var results = await service.SearchCustomersAsync("Contactson");

                var customer = Assert.Single(results);
                Assert.Equal("Mara Contactson", customer.Contact);
                Assert.Equal("Northwind Rentals", customer.Company);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}

using System.Collections.Generic;
using System.Data.SQLite;
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

        [Fact]
        public void AddCustomer_AssignsCustomerID()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ICustomerService service = new CustomerService(dbService);

                var customer = new Customer { Company = "Acme", Contact = "John" };
                service.AddCustomer(customer);

                Assert.NotEqual(0, customer.CustomerID);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ImportCustomersFromCsv_PartialFailure_RollsBack()
        {
            var dbPath = Path.GetTempFileName();
            var csvPath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(csvPath, "Company,Contact,Phone\nAcme,John,1\nAcme,Jane,2");
                var dbService = new DatabaseService(dbPath);
                using (var conn = dbService.CreateConnection())
                {
                    SqliteHelper.ExecuteNonQuery(conn, "CREATE UNIQUE INDEX idx_customers_company ON Customers(Company)", null);
                }
                var service = new CustomerService(dbService);
                var map = new Dictionary<string, string>
                {
                    {"Company", "Company"},
                    {"Contact", "Contact"},
                    {"Phone", "Phone"}
                };

                Assert.Throws<SQLiteException>(() => service.ImportCustomersFromCsv(csvPath, map));
                Assert.Empty(service.GetAllCustomers());
            }
            finally
            {
                if (File.Exists(csvPath)) File.Delete(csvPath);
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}

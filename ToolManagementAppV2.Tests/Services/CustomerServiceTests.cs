using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Interfaces;
using Xunit;
using System.Threading.Tasks;
using ToolManagementAppV2.Models.ImportExport;
using Microsoft.Extensions.Logging;
using ToolManagementAppV2.Tests;
using System.Threading;

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

        [Fact]
        public void ImportCustomersFromCsv_InvalidRows_ReturnsSummary()
        {
            var dbPath = Path.GetTempFileName();
            var csvPath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(csvPath,
                    "Company,Contact,Phone,Mobile\n" +
                    "Acme,John,1,\n" +
                    ",,,\n" +
                    "Acme,John,1,\n" +
                    "NoPhone,Jane,,\n");
                var dbService = new DatabaseService(dbPath);
                var service = new CustomerService(dbService);
                var map = new Dictionary<string, string>
                {
                    {"Company", "Company"},
                    {"Contact", "Contact"},
                    {"Phone", "Phone"},
                    {"Mobile", "Mobile"}
                };

                var result = service.ImportCustomersFromCsv(csvPath, map);
                Assert.Equal(1, result.ImportedCount);
                Assert.Equal(3, result.SkippedRows.Count);
                Assert.Contains(result.SkippedRows, r => r.Contains("Row 3"));
                Assert.Contains(result.SkippedRows, r => r.Contains("Row 4") && r.Contains("Duplicate"));
                Assert.Contains(result.SkippedRows, r => r.Contains("Row 5") && r.Contains("Phone and Mobile"));
            }
            finally
            {
                if (File.Exists(csvPath)) File.Delete(csvPath);
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void GetAllCustomers_DbFailure_LogsError()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                using (var conn = dbService.CreateConnection())
                {
                    SqliteHelper.ExecuteNonQuery(conn, "DROP TABLE Customers", null);
                }
                var logger = new TestLogger<CustomerService>();
                var service = new CustomerService(dbService, logger);
                Assert.Throws<SQLiteException>(() => service.GetAllCustomers());
                Assert.Contains(logger.Entries, e => e.Level == LogLevel.Error && e.Message.Contains("Failed to get all customers"));
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task GetAllCustomersAsync_ReturnsCustomers()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ICustomerService service = new CustomerService(dbService);
                service.AddCustomer(new Customer { Company = "Acme", Contact = "J" });
                var customers = await service.GetAllCustomersAsync();
                Assert.Single(customers);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task AddCustomer_SyncAndAsync_BothPersist()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var service = new CustomerService(dbService);
                service.AddCustomer(new Customer { Company = "Acme", Contact = "J" });
                await service.AddCustomerAsync(new Customer { Company = "Beta", Contact = "B" });
                var allSync = service.GetAllCustomers();
                var allAsync = await service.GetAllCustomersAsync();
                Assert.Equal(allSync.Count, allAsync.Count);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task GetAllCustomersAsync_RespectsCancellation()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var service = new CustomerService(dbService);
                var cts = new CancellationTokenSource();
                cts.Cancel();
                await Assert.ThrowsAsync<OperationCanceledException>(() => service.GetAllCustomersAsync(cts.Token));
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task ImportCustomersFromCsvAsync_RespectsCancellation()
        {
            var dbPath = Path.GetTempFileName();
            var csvPath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(csvPath, "Company,Contact,Phone\nAcme,John,1\nBeta,Jane,2");
                var dbService = new DatabaseService(dbPath);
                var service = new CustomerService(dbService);
                var map = new Dictionary<string, string>
                {
                    {"Company", "Company"},
                    {"Contact", "Contact"},
                    {"Phone", "Phone"}
                };
                var cts = new CancellationTokenSource();
                cts.Cancel();
                await Assert.ThrowsAsync<OperationCanceledException>(() => service.ImportCustomersFromCsvAsync(csvPath, map, cts.Token));
            }
            finally
            {
                if (File.Exists(csvPath)) File.Delete(csvPath);
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}

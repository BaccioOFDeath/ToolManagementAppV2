using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Models.ImportExport;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Utilities.IO;
using ToolManagementAppV2.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;

namespace ToolManagementAppV2.Services.Customers
{
    public class CustomerService : ICustomerService
    {
        readonly DatabaseService _dbService;
        readonly ILogger<CustomerService> _logger;

        public CustomerService(DatabaseService dbService, ILogger<CustomerService>? logger = null)
        {
            _dbService = dbService;
            _logger = logger ?? NullLogger<CustomerService>.Instance;
        }

        public CustomerImportResult ImportCustomersFromCsv(string filePath, IDictionary<string, string> map)
        {
            var customers = CsvHelperUtil.LoadCustomersFromCsv(filePath, map);
            var result = new CustomerImportResult();
            using var conn = _dbService.CreateConnection();
            using var tran = conn.BeginTransaction();
            try
            {
                for (int i = 0; i < customers.Count; i++)
                {
                    var c = customers[i];
                    var row = i + 2; // account for header row
                    var reason = GetSkipReason(c);
                    if (reason != null)
                    {
                        var msg = $"Row {row}: {reason}";
                        result.SkippedRows.Add(msg);
                        _logger.LogInformation("{Message}", msg);
                        continue;
                    }

                    if (CustomerExists(c.Contact, c.Phone, c.Mobile))
                    {
                        var msg = $"Row {row}: Duplicate customer";
                        result.SkippedRows.Add(msg);
                        _logger.LogInformation("{Message}", msg);
                        continue;
                    }

                    InsertCustomer(conn, tran, c);
                    result.ImportedCount++;
                }
                tran.Commit();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import customers from CSV");
                tran.Rollback();
                throw;
            }
        }

        static string? GetSkipReason(CustomerModel c)
        {
            var reasons = new List<string>();
            if (string.IsNullOrWhiteSpace(c.Company)) reasons.Add("Company missing");
            if (string.IsNullOrWhiteSpace(c.Contact)) reasons.Add("Contact missing");
            if (string.IsNullOrWhiteSpace(c.Phone) && string.IsNullOrWhiteSpace(c.Mobile)) reasons.Add("Phone and Mobile missing");
            return reasons.Count > 0 ? string.Join(", ", reasons) : null;
        }



        public void ExportCustomersToCsv(string filePath)
        {
            var all = GetAllCustomers();
            CsvHelperUtil.ExportCustomersToCsv(filePath, all);
        }

        public List<CustomerModel> GetAllCustomers()
        {
            const string sql = "SELECT * FROM Customers";
            using var conn = _dbService.CreateConnection();
            try
            {
                return SqliteHelper.ExecuteReader(conn, sql, null, MapCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all customers");
                throw;
            }
        }

        public List<CustomerModel> SearchCustomers(string searchTerm)
        {
            const string sql = @"
                SELECT * FROM Customers
                WHERE Company LIKE @t OR Email LIKE @t OR Phone LIKE @t OR Mobile LIKE @t OR Address LIKE @t";
            var p = new[] { new SQLiteParameter("@t", $"%{searchTerm}%") };
            using var conn = _dbService.CreateConnection();
            try
            {
                return SqliteHelper.ExecuteReader(conn, sql, p, MapCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search customers with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        public CustomerModel GetCustomerByID(int customerID)
        {
            const string sql = "SELECT * FROM Customers WHERE CustomerID = @id";
            var p = new[] { new SQLiteParameter("@id", customerID) };
            using var conn = _dbService.CreateConnection();
            try
            {
                return SqliteHelper.ExecuteReader(conn, sql, p, MapCustomer).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get customer {CustomerID}", customerID);
                throw;
            }
        }

        /// <summary>
        /// Adds a single customer to the database. Bulk import operations call the
        /// underlying InsertCustomer method inside their own transaction scope, so
        /// transaction management is handled by the caller in those scenarios.
        /// </summary>
        public void AddCustomer(CustomerModel customer)
        {
            using var conn = _dbService.CreateConnection();
            try
            {
                InsertCustomer(conn, null, customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add customer {Contact}", customer.Contact);
                throw;
            }
        }

        void InsertCustomer(SQLiteConnection conn, SQLiteTransaction? tran, CustomerModel customer)
        {
            const string sql = @"
        INSERT INTO Customers (Company, Email, Contact, Phone, Mobile, Address)
        VALUES (@Company, @Email, @Contact, @Phone, @Mobile, @Address);
        SELECT last_insert_rowid();";

            var p = new[]
            {
                new SQLiteParameter("@Company", customer.Company ?? string.Empty),
                new SQLiteParameter("@Email",   customer.Email ?? string.Empty),
                new SQLiteParameter("@Contact", customer.Contact ?? string.Empty),
                new SQLiteParameter("@Phone",   customer.Phone ?? string.Empty),
                new SQLiteParameter("@Mobile",  customer.Mobile ?? string.Empty),
                new SQLiteParameter("@Address", customer.Address ?? string.Empty)
            };

            try
            {
                using var cmd = new SQLiteCommand(sql, conn, tran);
                cmd.Parameters.AddRange(p);
                customer.CustomerID = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert customer {Contact}", customer.Contact);
                throw;
            }
        }

        async Task InsertCustomerAsync(SQLiteConnection conn, SQLiteTransaction? tran, CustomerModel customer)
        {
            const string sql = @"
        INSERT INTO Customers (Company, Email, Contact, Phone, Mobile, Address)
        VALUES (@Company, @Email, @Contact, @Phone, @Mobile, @Address);
        SELECT last_insert_rowid();";

            var p = new[]
            {
                new SQLiteParameter("@Company", customer.Company ?? string.Empty),
                new SQLiteParameter("@Email",   customer.Email ?? string.Empty),
                new SQLiteParameter("@Contact", customer.Contact ?? string.Empty),
                new SQLiteParameter("@Phone",   customer.Phone ?? string.Empty),
                new SQLiteParameter("@Mobile",  customer.Mobile ?? string.Empty),
                new SQLiteParameter("@Address", customer.Address ?? string.Empty)
            };

            try
            {
                using var cmd = new SQLiteCommand(sql, conn, tran);
                cmd.Parameters.AddRange(p);
                customer.CustomerID = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert customer {Contact}", customer.Contact);
                throw;
            }
        }


        public void UpdateCustomer(CustomerModel customer)
        {
            const string sql = @"
                UPDATE Customers
                SET Company = @Company, Email = @Email, Contact = @Contact,
                    Phone = @Phone, Mobile = @Mobile, Address = @Address
                WHERE CustomerID = @CustomerID";
            var p = new[]
            {
                new SQLiteParameter("@Company", customer.Company),
                new SQLiteParameter("@Email", customer.Email),
                new SQLiteParameter("@Contact", customer.Contact),
                new SQLiteParameter("@Phone", customer.Phone),
                new SQLiteParameter("@Mobile", customer.Mobile),
                new SQLiteParameter("@Address", customer.Address),
                new SQLiteParameter("@CustomerID", customer.CustomerID)
            };
            using var conn = _dbService.CreateConnection();
            try
            {
                SqliteHelper.ExecuteNonQuery(conn, sql, p);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update customer {CustomerID}", customer.CustomerID);
                throw;
            }
        }

        public void DeleteCustomer(int customerID)
        {
            const string sql = "DELETE FROM Customers WHERE CustomerID = @CustomerID";
            var p = new[] { new SQLiteParameter("@CustomerID", customerID) };
            using var conn = _dbService.CreateConnection();
            try
            {
                SqliteHelper.ExecuteNonQuery(conn, sql, p);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete customer {CustomerID}", customerID);
                throw;
            }
        }

        private bool CustomerExists(string contact, string phone, string mobile)
        {
            const string sql = @"
        SELECT COUNT(*) FROM Customers
         WHERE Contact = @Contact AND (Phone = @Phone OR Mobile = @Mobile)";
            using var conn = _dbService.CreateConnection();
            try
            {
                var count = Convert.ToInt32(SqliteHelper.ExecuteScalar(conn, sql, new[]
                {
                    new SQLiteParameter("@Contact", contact),
                    new SQLiteParameter("@Phone", phone ?? ""),
                    new SQLiteParameter("@Mobile", mobile ?? "")
                }));
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if customer exists");
                throw;
            }
        }


        private CustomerModel MapCustomer(IDataRecord r) => new()
        {
            CustomerID = Convert.ToInt32(r["CustomerID"]),
            Company = r["Company"].ToString(),
            Email = r["Email"].ToString(),
            Contact = r["Contact"].ToString(),
            Phone = r["Phone"].ToString(),
            Mobile = r["Mobile"].ToString(),
            Address = r["Address"].ToString()
        };

        public async Task AddCustomerAsync(CustomerModel customer)
        {
            using var conn = _dbService.CreateConnection();
            try
            {
                await InsertCustomerAsync(conn, null, customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add customer {Contact}", customer.Contact);
                throw;
            }
        }

        public async Task UpdateCustomerAsync(CustomerModel customer)
        {
            const string sql = @"
                UPDATE Customers
                SET Company = @Company, Email = @Email, Contact = @Contact,
                    Phone = @Phone, Mobile = @Mobile, Address = @Address
                WHERE CustomerID = @CustomerID";
            var p = new[]
            {
                new SQLiteParameter("@Company", customer.Company),
                new SQLiteParameter("@Email", customer.Email),
                new SQLiteParameter("@Contact", customer.Contact),
                new SQLiteParameter("@Phone", customer.Phone),
                new SQLiteParameter("@Mobile", customer.Mobile),
                new SQLiteParameter("@Address", customer.Address),
                new SQLiteParameter("@CustomerID", customer.CustomerID),
            };
            using var conn = _dbService.CreateConnection();
            try
            {
                await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update customer {CustomerID}", customer.CustomerID);
                throw;
            }
        }

        public async Task DeleteCustomerAsync(int customerID)
        {
            const string sql = "DELETE FROM Customers WHERE CustomerID = @CustomerID";
            var p = new[] { new SQLiteParameter("@CustomerID", customerID) };
            using var conn = _dbService.CreateConnection();
            try
            {
                await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete customer {CustomerID}", customerID);
                throw;
            }
        }

        public async Task<CustomerModel> GetCustomerByIDAsync(int customerID)
        {
            const string sql = "SELECT * FROM Customers WHERE CustomerID = @id";
            var p = new[] { new SQLiteParameter("@id", customerID) };
            using var conn = _dbService.CreateConnection();
            try
            {
                var list = await SqliteHelper.ExecuteReaderAsync(conn, sql, p, MapCustomer);
                return list.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get customer {CustomerID}", customerID);
                throw;
            }
        }

        public async Task<List<CustomerModel>> GetAllCustomersAsync()
        {
            const string sql = "SELECT * FROM Customers";
            using var conn = _dbService.CreateConnection();
            try
            {
                return await SqliteHelper.ExecuteReaderAsync(conn, sql, null, MapCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all customers");
                throw;
            }
        }

        public async Task<List<CustomerModel>> SearchCustomersAsync(string searchTerm)
        {
            const string sql = @"
                SELECT * FROM Customers
                WHERE Company LIKE @t OR Email LIKE @t OR Phone LIKE @t OR Mobile LIKE @t OR Address LIKE @t";
            var p = new[] { new SQLiteParameter("@t", $"%{searchTerm}%") };
            using var conn = _dbService.CreateConnection();
            try
            {
                return await SqliteHelper.ExecuteReaderAsync(conn, sql, p, MapCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search customers with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        public Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map)
            => Task.FromResult(ImportCustomersFromCsv(filePath, map));

        public Task ExportCustomersToCsvAsync(string filePath)
        {
            ExportCustomersToCsv(filePath);
            return Task.CompletedTask;
        }
    }
}

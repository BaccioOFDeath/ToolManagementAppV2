using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Models.ImportExport;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Utilities.IO;
using InventoryManagementApp.Services.Users;

namespace InventoryManagementApp.Services.Customers
{
    public class CustomerService : ICustomerService
    {
        readonly DatabaseService _dbService;
        readonly ILogger<CustomerService> _logger;
        readonly IAuthorizationService _auth;

        public CustomerService(DatabaseService dbService, IAuthorizationService? authorizationService = null, ILogger<CustomerService>? logger = null)
        {
            _dbService = dbService;
            _auth = authorizationService ?? new NoOpAuthorizationService();
            _logger = logger ?? NullLogger<CustomerService>.Instance;
        }

        public Task AddCustomerAsync(CustomerModel customer, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            return AddCustomerInternalAsync(customer, cancellationToken);
        }

        public Task UpdateCustomerAsync(CustomerModel customer, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            return UpdateCustomerInternalAsync(customer, cancellationToken);
        }

        public Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            return DeleteCustomerInternalAsync(customerID, cancellationToken);
        }

        public Task<CustomerModel> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default) =>
            GetCustomerByIDInternalAsync(customerID, cancellationToken);

        public Task<List<CustomerModel>> GetAllCustomersAsync(CancellationToken cancellationToken = default) =>
            GetAllCustomersInternalAsync(cancellationToken);

        public Task<int> CountCustomersAsync(CancellationToken cancellationToken = default) =>
            CountCustomersInternalAsync(cancellationToken);

        public Task<List<CustomerModel>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default) =>
            SearchCustomersInternalAsync(searchTerm, cancellationToken);

        public Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            return ImportCustomersFromCsvInternalAsync(filePath, map, cancellationToken);
        }

        public Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default) =>
            ExportCustomersToCsvInternalAsync(filePath, cancellationToken);

        async Task AddCustomerInternalAsync(CustomerModel customer, CancellationToken cancellationToken)
        {
            using var conn = _dbService.CreateConnection();
            try
            {
                await InsertCustomerAsync(conn, null, customer, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add customer {Contact}", customer.Contact);
                throw;
            }
        }

        async Task UpdateCustomerInternalAsync(CustomerModel customer, CancellationToken cancellationToken)
        {
            const string sql = @"
                UPDATE Customers
                SET Company = @Company, Email = @Email, Contact = @Contact,
                    Phone = @Phone, Mobile = @Mobile, Address = @Address
                WHERE CustomerID = @CustomerID";
            var p = new[]
            {
                new SqliteParameter("@Company", customer.Company),
                new SqliteParameter("@Email", customer.Email),
                new SqliteParameter("@Contact", customer.Contact),
                new SqliteParameter("@Phone", customer.Phone),
                new SqliteParameter("@Mobile", customer.Mobile),
                new SqliteParameter("@Address", customer.Address),
                new SqliteParameter("@CustomerID", customer.CustomerID),
            };
            using var conn = _dbService.CreateConnection();
            try
            {
                await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update customer {CustomerID}", customer.CustomerID);
                throw;
            }
        }

        async Task DeleteCustomerInternalAsync(int customerID, CancellationToken cancellationToken)
        {
            const string sql = "DELETE FROM Customers WHERE CustomerID = @CustomerID";
            var p = new[] { new SqliteParameter("@CustomerID", customerID) };
            using var conn = _dbService.CreateConnection();
            try
            {
                await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete customer {CustomerID}", customerID);
                throw;
            }
        }

        async Task<CustomerModel> GetCustomerByIDInternalAsync(int customerID, CancellationToken cancellationToken)
        {
            const string sql = "SELECT * FROM Customers WHERE CustomerID = @id";
            var p = new[] { new SqliteParameter("@id", customerID) };
            using var conn = _dbService.CreateConnection();
            try
            {
                var list = await SqliteHelper.ExecuteReaderAsync(conn, sql, MapCustomer, p, cancellationToken);
                return list.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get customer {CustomerID}", customerID);
                throw;
            }
        }

        async Task<List<CustomerModel>> GetAllCustomersInternalAsync(CancellationToken cancellationToken)
        {
            const string sql = "SELECT * FROM Customers";
            using var conn = _dbService.CreateConnection();
            try
            {
                return await SqliteHelper.ExecuteReaderAsync(conn, sql, MapCustomer, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all customers");
                throw;
            }
        }

        async Task<int> CountCustomersInternalAsync(CancellationToken cancellationToken)
        {
            const string sql = "SELECT COUNT(*) FROM Customers";
            using var conn = _dbService.CreateConnection();
            try
            {
                var result = await SqliteHelper.ExecuteScalarAsync(conn, sql, null, cancellationToken);
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to count customers");
                throw;
            }
        }

        async Task<List<CustomerModel>> SearchCustomersInternalAsync(string searchTerm, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT * FROM Customers
                WHERE Company LIKE @t OR Email LIKE @t OR Phone LIKE @t OR Mobile LIKE @t OR Address LIKE @t";
            var p = new[] { new SqliteParameter("@t", $"%{searchTerm}%") };
            using var conn = _dbService.CreateConnection();
            try
            {
                return await SqliteHelper.ExecuteReaderAsync(conn, sql, MapCustomer, p, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search customers with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        async Task<CustomerImportResult> ImportCustomersFromCsvInternalAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken)
        {
            var customers = await CsvHelperUtil.LoadCustomersFromCsvAsync(filePath, map, cancellationToken);
            var result = new CustomerImportResult();
            using var conn = _dbService.CreateConnection();
            using var tran = conn.BeginTransaction();
            try
            {
                for (int i = 0; i < customers.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var c = customers[i];
                    var row = i + 2;
                    var reason = GetSkipReason(c);
                    if (reason != null)
                    {
                        var msg = $"Row {row}: {reason}";
                        result.SkippedRows.Add(msg);
                        _logger.LogInformation("{Message}", msg);
                        continue;
                    }

                    if (await CustomerExistsAsync(c.Contact, c.Phone, c.Mobile, cancellationToken))
                    {
                        var msg = $"Row {row}: Duplicate customer";
                        result.SkippedRows.Add(msg);
                        _logger.LogInformation("{Message}", msg);
                        continue;
                    }

                    await InsertCustomerAsync(conn, tran, c, cancellationToken);
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

        async Task ExportCustomersToCsvInternalAsync(string filePath, CancellationToken cancellationToken)
        {
            var all = await GetAllCustomersInternalAsync(cancellationToken);
            await Task.Run(() => CsvHelperUtil.ExportCustomersToCsv(filePath, all), cancellationToken);
        }

        async Task InsertCustomerAsync(SqliteConnection conn, SqliteTransaction? tran, CustomerModel customer, CancellationToken cancellationToken)
        {
            const string sql = @"
        INSERT INTO Customers (Company, Email, Contact, Phone, Mobile, Address)
        VALUES (@Company, @Email, @Contact, @Phone, @Mobile, @Address);
        SELECT last_insert_rowid();";

            var p = new[]
            {
                new SqliteParameter("@Company", customer.Company ?? string.Empty),
                new SqliteParameter("@Email",   customer.Email ?? string.Empty),
                new SqliteParameter("@Contact", customer.Contact ?? string.Empty),
                new SqliteParameter("@Phone",   customer.Phone ?? string.Empty),
                new SqliteParameter("@Mobile",  customer.Mobile ?? string.Empty),
                new SqliteParameter("@Address", customer.Address ?? string.Empty)
            };

            try
            {
                using var cmd = new SqliteCommand(sql, conn, tran);
                cmd.Parameters.AddRange(p);
                customer.CustomerID = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert customer {Contact}", customer.Contact);
                throw;
            }
        }

        async Task<bool> CustomerExistsAsync(string contact, string phone, string mobile, CancellationToken cancellationToken)
        {
            const string sql = @"
        SELECT COUNT(*) FROM Customers
         WHERE Contact = @Contact AND (Phone = @Phone OR Mobile = @Mobile)";
            using var conn = _dbService.CreateConnection();
            try
            {
                var count = Convert.ToInt32(await SqliteHelper.ExecuteScalarAsync(conn, sql, new[]
                {
                    new SqliteParameter("@Contact", contact),
                    new SqliteParameter("@Phone", phone ?? string.Empty),
                    new SqliteParameter("@Mobile", mobile ?? string.Empty)
                }, cancellationToken));
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if customer exists");
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

        CustomerModel MapCustomer(IDataRecord r) => new()
        {
            CustomerID = Convert.ToInt32(r["CustomerID"]),
            Company = r["Company"].ToString(),
            Email = r["Email"].ToString(),
            Contact = r["Contact"].ToString(),
            Phone = r["Phone"].ToString(),
            Mobile = r["Mobile"].ToString(),
            Address = r["Address"].ToString()
        };
    }
}


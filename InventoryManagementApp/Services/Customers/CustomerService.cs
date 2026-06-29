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
    /// <summary>
    /// Service for managing customer data including CRUD operations, import/export, and search functionality.
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly DatabaseService _dbService;
        private readonly ILogger<CustomerService> _logger;
        private readonly IAuthorizationService _auth;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerService"/> class.
        /// </summary>
        /// <param name="dbService">Database service for data access.</param>
        /// <param name="authorizationService">Optional authorization service for access control.</param>
        /// <param name="logger">Optional logger for diagnostic output.</param>
        public CustomerService(DatabaseService dbService, IAuthorizationService? authorizationService = null, ILogger<CustomerService>? logger = null)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _auth = authorizationService ?? new NoOpAuthorizationService();
            _logger = logger ?? NullLogger<CustomerService>.Instance;
        }

        /// <summary>
        /// Adds a new customer to the database. Requires admin privileges.
        /// </summary>
        /// <param name="customer">The customer to add.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <exception cref="ArgumentNullException">Thrown if customer is null.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if user lacks admin privileges.</exception>
        public Task AddCustomerAsync(CustomerModel customer, CancellationToken cancellationToken = default)
        {
            if (customer is null)
                throw new ArgumentNullException(nameof(customer));
            
            _auth.EnsureAdmin();
            return AddCustomerInternalAsync(customer, cancellationToken);
        }

        /// <summary>
        /// Updates an existing customer in the database. Requires admin privileges.
        /// </summary>
        /// <param name="customer">The customer with updated information.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <exception cref="ArgumentNullException">Thrown if customer is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if customer ID is less than 1.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if user lacks admin privileges.</exception>
        public Task UpdateCustomerAsync(CustomerModel customer, CancellationToken cancellationToken = default)
        {
            if (customer is null)
                throw new ArgumentNullException(nameof(customer));
            if (customer.CustomerID < 1)
                throw new ArgumentOutOfRangeException(nameof(customer), "Customer ID must be greater than 0.");
            
            _auth.EnsureAdmin();
            return UpdateCustomerInternalAsync(customer, cancellationToken);
        }

        /// <summary>
        /// Deletes a customer from the database. Requires admin privileges.
        /// </summary>
        /// <param name="customerID">The ID of the customer to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if customerID is less than 1.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if user lacks admin privileges.</exception>
        public Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default)
        {
            if (customerID < 1)
                throw new ArgumentOutOfRangeException(nameof(customerID), "Customer ID must be greater than 0.");
            
            _auth.EnsureAdmin();
            return DeleteCustomerInternalAsync(customerID, cancellationToken);
        }

        /// <summary>
        /// Retrieves a customer by their unique identifier.
        /// </summary>
        /// <param name="customerID">The ID of the customer to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>The customer if found; otherwise, null.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if customerID is less than 1.</exception>
        public Task<CustomerModel?> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default)
        {
            if (customerID < 1)
                throw new ArgumentOutOfRangeException(nameof(customerID), "Customer ID must be greater than 0.");
            
            return GetCustomerByIDInternalAsync(customerID, cancellationToken);
        }

        /// <summary>
        /// Retrieves all customers from the database.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of all customers.</returns>
        public Task<List<CustomerModel>> GetAllCustomersAsync(CancellationToken cancellationToken = default) =>
            GetAllCustomersInternalAsync(cancellationToken);

        /// <summary>
        /// Gets the total count of customers in the database.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>The number of customers.</returns>
        public Task<int> CountCustomersAsync(CancellationToken cancellationToken = default) =>
            CountCustomersInternalAsync(cancellationToken);

        /// <summary>
        /// Searches for customers matching the specified search term.
        /// </summary>
        /// <param name="searchTerm">The term to search for in customer data.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of matching customers.</returns>
        public Task<List<CustomerModel>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default) =>
            SearchCustomersInternalAsync(searchTerm ?? string.Empty, cancellationToken);

        /// <summary>
        /// Imports customers from a CSV file. Requires admin privileges.
        /// </summary>
        /// <param name="filePath">Path to the CSV file.</param>
        /// <param name="map">Column mapping dictionary.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Import result with success/failure statistics.</returns>
        /// <exception cref="ArgumentNullException">Thrown if filePath or map is null.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if user lacks admin privileges.</exception>
        public Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (map is null)
                throw new ArgumentNullException(nameof(map));
            
            _auth.EnsureAdmin();
            return ImportCustomersFromCsvInternalAsync(filePath, map, cancellationToken);
        }

        /// <summary>
        /// Exports all customers to a CSV file.
        /// </summary>
        /// <param name="filePath">Path where the CSV file will be saved.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <exception cref="ArgumentNullException">Thrown if filePath is null or empty.</exception>
        public Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            
            return ExportCustomersToCsvInternalAsync(filePath, cancellationToken);
        }

        async Task AddCustomerInternalAsync(CustomerModel customer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

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
            cancellationToken.ThrowIfCancellationRequested();

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
                await EnsureCustomerRowExistsAsync(conn, customer.CustomerID, cancellationToken);
                var updatedRows = await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken);
                EnsureCustomerWriteSucceeded(updatedRows, customer.CustomerID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update customer {CustomerID}", customer.CustomerID);
                throw;
            }
        }

        async Task DeleteCustomerInternalAsync(int customerID, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "DELETE FROM Customers WHERE CustomerID = @CustomerID";
            var p = new[] { new SqliteParameter("@CustomerID", customerID) };
            using var conn = _dbService.CreateConnection();
            try
            {
                await EnsureCustomerRowExistsAsync(conn, customerID, cancellationToken);
                var deletedRows = await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken);
                EnsureCustomerWriteSucceeded(deletedRows, customerID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete customer {CustomerID}", customerID);
                throw;
            }
        }

        async Task<CustomerModel?> GetCustomerByIDInternalAsync(int customerID, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "SELECT * FROM Customers WHERE CustomerID = @id";
            var p = new[] { new SqliteParameter("@id", customerID) };
            using var conn = _dbService.CreateConnection();
            try
            {
                var list = await SqliteHelper.ExecuteReaderAsync(conn, sql, MapCustomer, p, cancellationToken);
                if (list.Count == 0)
                {
                    throw new KeyNotFoundException($"Customer {customerID} not found.");
                }
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
            cancellationToken.ThrowIfCancellationRequested();

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
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "SELECT COUNT(*) FROM Customers";
            using var conn = _dbService.CreateConnection();
            try
            {
                var result = await SqliteHelper.ExecuteScalarAsync(conn, sql, null, cancellationToken);
                return Convert.ToInt32(result ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to count customers");
                throw;
            }
        }

        async Task<List<CustomerModel>> SearchCustomersInternalAsync(string searchTerm, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = @"
                SELECT * FROM Customers
                WHERE Company LIKE @t OR Contact LIKE @t OR Email LIKE @t OR Phone LIKE @t OR Mobile LIKE @t OR Address LIKE @t";
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
            cancellationToken.ThrowIfCancellationRequested();

            var customers = await CsvHelperUtil.LoadCustomersFromCsvAsync(filePath, map, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

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
            cancellationToken.ThrowIfCancellationRequested();

            var all = await GetAllCustomersInternalAsync(cancellationToken);
            await Task.Run(() => CsvHelperUtil.ExportCustomersToCsv(filePath, all), cancellationToken);
        }

        async Task InsertCustomerAsync(SqliteConnection conn, SqliteTransaction? tran, CustomerModel customer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = @"
        INSERT INTO Customers (Company, Email, Contact, Phone, Mobile, Address)
        VALUES (@Company, @Email, @Contact, @Phone, @Mobile, @Address);";

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
                var insertedRows = await cmd.ExecuteNonQueryAsync(cancellationToken);
                EnsureCustomerCreateSucceeded(insertedRows);

                using var idCmd = new SqliteCommand("SELECT last_insert_rowid();", conn, tran);
                customer.CustomerID = Convert.ToInt32(await idCmd.ExecuteScalarAsync(cancellationToken));
                if (customer.CustomerID < 1)
                    throw new InvalidOperationException("Unable to create customer.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert customer {Contact}", customer.Contact);
                throw;
            }
        }

        async Task<bool> CustomerExistsAsync(string contact, string phone, string mobile, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

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
                }, cancellationToken) ?? 0);
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if customer exists");
                throw;
            }
        }

        static async Task EnsureCustomerRowExistsAsync(SqliteConnection conn, int customerID, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "SELECT COUNT(*) FROM Customers WHERE CustomerID = @CustomerID";
            var count = Convert.ToInt32(await SqliteHelper.ExecuteScalarAsync(conn, sql, new[]
            {
                new SqliteParameter("@CustomerID", customerID)
            }, cancellationToken) ?? 0);

            if (count == 0)
                throw new KeyNotFoundException($"Customer {customerID} not found.");
        }

        static void EnsureCustomerCreateSucceeded(int affectedRows)
        {
            if (affectedRows == 0)
                throw new InvalidOperationException("Unable to create customer.");
        }

        static void EnsureCustomerWriteSucceeded(int affectedRows, int customerID)
        {
            if (affectedRows == 0)
                throw new KeyNotFoundException($"Customer {customerID} not found.");
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
            Company = r["Company"]?.ToString() ?? string.Empty,
            Email = r["Email"]?.ToString() ?? string.Empty,
            Contact = r["Contact"]?.ToString() ?? string.Empty,
            Phone = r["Phone"]?.ToString() ?? string.Empty,
            Mobile = r["Mobile"]?.ToString() ?? string.Empty,
            Address = r["Address"]?.ToString() ?? string.Empty
        };

        public async Task<int> ImportCustomersAsync(string filePath, IDataImporter<Customer> importer, CancellationToken cancellationToken = default)
        {
            _auth.EnsureAdmin();
            cancellationToken.ThrowIfCancellationRequested();
            
            var (customers, skippedRows) = await importer.ImportAsync(filePath, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            
            int importedCount = 0;
            using var conn = _dbService.CreateConnection();
            
            foreach (var customer in customers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var customerModel = new CustomerModel
                {
                    Company = customer.Company ?? string.Empty,
                    Email = customer.Email ?? string.Empty,
                    Contact = customer.Contact ?? string.Empty,
                    Phone = customer.Phone ?? string.Empty,
                    Mobile = customer.Mobile ?? string.Empty,
                    Address = customer.Address ?? string.Empty
                };

                var skipReason = GetSkipReason(customerModel);
                if (skipReason != null)
                    continue;

                bool exists = await CustomerExistsAsync(customerModel.Contact, customerModel.Phone, customerModel.Mobile, cancellationToken);
                if (exists)
                    continue;

                await InsertCustomerAsync(conn, null, customerModel, cancellationToken);
                importedCount++;
            }

            return importedCount;
        }

        public async Task ExportCustomersAsync(string filePath, IDataExporter<Customer> exporter, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var all = await GetAllCustomersAsync(cancellationToken).ConfigureAwait(false);
            await exporter.ExportAsync(filePath, all, cancellationToken).ConfigureAwait(false);
        }
    }
}

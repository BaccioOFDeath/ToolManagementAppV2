using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Utilities.IO;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.Services.Customers
{
    public class CustomerService : ICustomerService
    {
        readonly DatabaseService _dbService;

        public CustomerService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public void ImportCustomersFromCsv(string filePath, IDictionary<string, string> map)
        {
            var customers = CsvHelperUtil.LoadCustomersFromCsv(filePath, map);
            using var conn = _dbService.CreateConnection();
            using var tran = conn.BeginTransaction();
            try
            {
                foreach (var c in customers)
                {
                    if (string.IsNullOrWhiteSpace(c.Company)) continue;
                    if (string.IsNullOrWhiteSpace(c.Contact)) continue;
                    if (string.IsNullOrWhiteSpace(c.Phone) && string.IsNullOrWhiteSpace(c.Mobile)) continue;

                    if (!CustomerExists(c.Contact, c.Phone, c.Mobile))
                        InsertCustomer(conn, tran, c);
                }
                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
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
            return SqliteHelper.ExecuteReader(conn, sql, null, MapCustomer);
        }

        public List<CustomerModel> SearchCustomers(string searchTerm)
        {
            const string sql = @"
                SELECT * FROM Customers
                WHERE Company LIKE @t OR Email LIKE @t OR Phone LIKE @t OR Mobile LIKE @t OR Address LIKE @t";
            var p = new[] { new SQLiteParameter("@t", $"%{searchTerm}%") };
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReader(conn, sql, p, MapCustomer);
        }

        public CustomerModel GetCustomerByID(int customerID)
        {
            const string sql = "SELECT * FROM Customers WHERE CustomerID = @id";
            var p = new[] { new SQLiteParameter("@id", customerID) };
            using var conn = _dbService.CreateConnection();
            return SqliteHelper.ExecuteReader(conn, sql, p, MapCustomer).FirstOrDefault();
        }

        /// <summary>
        /// Adds a single customer to the database. Bulk import operations call the
        /// underlying InsertCustomer method inside their own transaction scope, so
        /// transaction management is handled by the caller in those scenarios.
        /// </summary>
        public void AddCustomer(CustomerModel customer)
        {
            using var conn = _dbService.CreateConnection();
            InsertCustomer(conn, null, customer);
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

            using var cmd = new SQLiteCommand(sql, conn, tran);
            cmd.Parameters.AddRange(p);
            customer.CustomerID = Convert.ToInt32(cmd.ExecuteScalar());
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
            SqliteHelper.ExecuteNonQuery(conn, sql, p);
        }

        public void DeleteCustomer(int customerID)
        {
            const string sql = "DELETE FROM Customers WHERE CustomerID = @CustomerID";
            var p = new[] { new SQLiteParameter("@CustomerID", customerID) };
            using var conn = _dbService.CreateConnection();
            SqliteHelper.ExecuteNonQuery(conn, sql, p);
        }

        private bool CustomerExists(string contact, string phone, string mobile)
        {
            const string sql = @"
        SELECT COUNT(*) FROM Customers
         WHERE Contact = @Contact AND (Phone = @Phone OR Mobile = @Mobile)";
            using var conn = _dbService.CreateConnection();
            var count = Convert.ToInt32(SqliteHelper.ExecuteScalar(conn, sql, new[]
            {
            new SQLiteParameter("@Contact", contact),
            new SQLiteParameter("@Phone", phone ?? ""),
            new SQLiteParameter("@Mobile", mobile ?? "")
            }));
            return count > 0;
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

        public Task AddCustomerAsync(CustomerModel customer) => Task.Run(() => AddCustomer(customer));
        public Task UpdateCustomerAsync(CustomerModel customer) => Task.Run(() => UpdateCustomer(customer));
        public Task DeleteCustomerAsync(int customerID) => Task.Run(() => DeleteCustomer(customerID));
        public Task<CustomerModel> GetCustomerByIDAsync(int customerID) => Task.Run(() => GetCustomerByID(customerID));
        public Task<List<CustomerModel>> GetAllCustomersAsync() => Task.Run(GetAllCustomers);
        public Task<List<CustomerModel>> SearchCustomersAsync(string searchTerm) => Task.Run(() => SearchCustomers(searchTerm));
        public Task ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map) => Task.Run(() => ImportCustomersFromCsv(filePath, map));
        public Task ExportCustomersToCsvAsync(string filePath) => Task.Run(() => ExportCustomersToCsv(filePath));
    }
}

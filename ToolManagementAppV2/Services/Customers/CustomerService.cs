using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Utilities.IO;

namespace ToolManagementAppV2.Services.Customers
{
    public class CustomerService
    {
        readonly string _connString;

        public CustomerService(DatabaseService dbService)
        {
            _connString = dbService.ConnectionString;
        }

        public void ImportCustomersFromCsv(string filePath, IDictionary<string, string> map)
        {
            var customers = CsvHelperUtil.LoadCustomersFromCsv(filePath, map);
            foreach (var c in customers)
            {
                if (!CustomerExists(c.Name, c.Email))
                    AddCustomer(c);
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
            return SqliteHelper.ExecuteReader(_connString, sql, null, MapCustomer);
        }

        public List<CustomerModel> SearchCustomers(string searchTerm)
        {
            const string sql = @"
                SELECT * FROM Customers
                WHERE ToolNumber LIKE @t OR Email LIKE @t OR Phone LIKE @t OR Address LIKE @t";
            var p = new[] { new SQLiteParameter("@t", $"%{searchTerm}%") };
            return SqliteHelper.ExecuteReader(_connString, sql, p, MapCustomer);
        }

        public CustomerModel GetCustomerByID(int customerID)
        {
            const string sql = "SELECT * FROM Customers WHERE CustomerID = @id";
            var p = new[] { new SQLiteParameter("@id", customerID) };
            return SqliteHelper.ExecuteReader(_connString, sql, p, MapCustomer).FirstOrDefault();
        }

        public void AddCustomer(CustomerModel customer)
        {
            const string sql = @"
                INSERT INTO Customers (ToolNumber, Email, Contact, Phone, Address)
                VALUES (@ToolNumber, @Email, @Contact, @Phone, @Address)";
            var p = new[]
            {
                new SQLiteParameter("@ToolNumber", customer.Name),
                new SQLiteParameter("@Email", customer.Email),
                new SQLiteParameter("@Contact", customer.Contact),
                new SQLiteParameter("@Phone", customer.Phone),
                new SQLiteParameter("@Address", customer.Address)
            };
            SqliteHelper.ExecuteNonQuery(_connString, sql, p);
        }

        public void UpdateCustomer(CustomerModel customer)
        {
            const string sql = @"
                UPDATE Customers
                SET ToolNumber = @ToolNumber, Email = @Email, Contact = @Contact,
                    Phone = @Phone, Address = @Address
                WHERE CustomerID = @CustomerID";
            var p = new[]
            {
                new SQLiteParameter("@ToolNumber", customer.Name),
                new SQLiteParameter("@Email", customer.Email),
                new SQLiteParameter("@Contact", customer.Contact),
                new SQLiteParameter("@Phone", customer.Phone),
                new SQLiteParameter("@Address", customer.Address),
                new SQLiteParameter("@CustomerID", customer.CustomerID)
            };
            SqliteHelper.ExecuteNonQuery(_connString, sql, p);
        }

        public void DeleteCustomer(int customerID)
        {
            const string sql = "DELETE FROM Customers WHERE CustomerID = @CustomerID";
            var p = new[] { new SQLiteParameter("@CustomerID", customerID) };
            SqliteHelper.ExecuteNonQuery(_connString, sql, p);
        }

        private bool CustomerExists(string name, string email)
        {
            const string sql = "SELECT COUNT(*) FROM Customers WHERE ToolNumber = @N AND Email = @E";
            var count = Convert.ToInt32(SqliteHelper.ExecuteScalar(_connString, sql, new[]
            {
                new SQLiteParameter("@N", name),
                new SQLiteParameter("@E", email)
            }));
            return count > 0;
        }

        private CustomerModel MapCustomer(IDataRecord r) => new()
        {
            CustomerID = Convert.ToInt32(r["CustomerID"]),
            Name = r["ToolNumber"].ToString(),
            Email = r["Email"].ToString(),
            Contact = r["Contact"].ToString(),
            Phone = r["Phone"].ToString(),
            Address = r["Address"].ToString()
        };
    }
}

using System.Data;
using System.Data.SQLite;
using System.IO;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Helpers;

namespace ToolManagementAppV2.Services
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
            var lines = File.ReadAllLines(filePath);
            if (lines.Length < 2) return;

            var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
            var idx = map.ToDictionary(
                kv => kv.Key,
                kv => Array.IndexOf(headers, kv.Value),
                StringComparer.OrdinalIgnoreCase
            );

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cols = line.Split(',');
                string get(string prop) =>
                    idx.TryGetValue(prop, out var i) && i >= 0 && i < cols.Length
                        ? cols[i].Trim()
                        : string.Empty;

                var c = new Customer
                {
                    Name = get("ToolNumber"),
                    Email = get("Email"),
                    Contact = get("Contact"),
                    Phone = get("Phone"),
                    Address = get("Address")
                };
                AddCustomer(c);
            }
        }

        public void ExportCustomersToCsv(string filePath)
        {
            var all = GetAllCustomers();
            using var writer = new StreamWriter(filePath);
            writer.WriteLine("ToolNumber,Email,Contact,Phone,Address");
            foreach (var c in all)
                writer.WriteLine($"{c.Name},{c.Email},{c.Contact},{c.Phone},{c.Address}");
        }

        public List<Customer> GetAllCustomers()
        {
            const string sql = "SELECT * FROM Customers";
            return SqliteHelper.ExecuteReader(_connString, sql, null, MapCustomer);
        }

        public List<Customer> SearchCustomers(string searchTerm)
        {
            const string sql = @"
                SELECT * FROM Customers
                  WHERE ToolNumber   LIKE @t
                     OR Email  LIKE @t
                     OR Phone  LIKE @t
                     OR Address LIKE @t";
            var p = new[] { new SQLiteParameter("@t", $"%{searchTerm}%") };
            return SqliteHelper.ExecuteReader(_connString, sql, p, MapCustomer);
        }

        public Customer GetCustomerByID(int customerID)
        {
            const string sql = "SELECT * FROM Customers WHERE CustomerID = @id";
            var p = new[] { new SQLiteParameter("@id", customerID) };
            return SqliteHelper.ExecuteReader(_connString, sql, p, MapCustomer).FirstOrDefault();
        }

        public void AddCustomer(Customer customer)
        {
            const string sql = @"
                INSERT INTO Customers (ToolNumber, Email, Contact, Phone, Address)
                VALUES (@ToolNumber, @Email, @Contact, @Phone, @Address)";
            var p = new[]
            {
                new SQLiteParameter("@ToolNumber",    customer.Name),
                new SQLiteParameter("@Email",   customer.Email),
                new SQLiteParameter("@Contact", customer.Contact),
                new SQLiteParameter("@Phone",   customer.Phone),
                new SQLiteParameter("@Address", customer.Address)
            };
            SqliteHelper.ExecuteNonQuery(_connString, sql, p);
        }

        public void UpdateCustomer(Customer customer)
        {
            const string sql = @"
                UPDATE Customers
                   SET ToolNumber    = @ToolNumber,
                       Email   = @Email,
                       Contact = @Contact,
                       Phone   = @Phone,
                       Address = @Address
                 WHERE CustomerID = @CustomerID";
            var p = new[]
            {
                new SQLiteParameter("@ToolNumber",       customer.Name),
                new SQLiteParameter("@Email",      customer.Email),
                new SQLiteParameter("@Contact",    customer.Contact),
                new SQLiteParameter("@Phone",      customer.Phone),
                new SQLiteParameter("@Address",    customer.Address),
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

        Customer MapCustomer(IDataRecord r) => new()
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

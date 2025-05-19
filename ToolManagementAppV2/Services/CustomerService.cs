using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using ToolManagementAppV2.Models;

namespace ToolManagementAppV2.Services
{
    public class CustomerService
    {
        private readonly DatabaseService _dbService;

        public CustomerService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public void ImportCustomersFromCsv(string filePath, IDictionary<string, string> map)
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length < 2) return;
            var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
            var idx = map.ToDictionary(
                kv => kv.Key,
                kv => Array.IndexOf(headers, kv.Value.Trim()),
                StringComparer.OrdinalIgnoreCase
            );

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cols = line.Split(',');
                string get(string prop)
                    => idx[prop] >= 0 && idx[prop] < cols.Length
                        ? cols[idx[prop]].Trim()
                        : "";

                var c = new Customer
                {
                    Name = get("Name"),
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
            writer.WriteLine("Name,Email,Contact,Phone,Address");
            foreach (var c in all)
            {
                writer.WriteLine(string.Join(",",
                    c.Name,
                    c.Email,
                    c.Contact,
                    c.Phone,
                    c.Address
                ));
            }
        }


        public List<Customer> GetAllCustomers()
        {
            var customers = new List<Customer>();
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = "SELECT * FROM Customers";
            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                customers.Add(new Customer
                {
                    CustomerID = Convert.ToInt32(reader["CustomerID"]),
                    Name = reader["Name"].ToString(),
                    Email = reader["Email"].ToString(),
                    Contact = reader["Contact"].ToString(),
                    Phone = reader["Phone"].ToString(),
                    Address = reader["Address"].ToString()
                });
            }

            return customers;
        }


        public void AddCustomer(Customer customer)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = @"
        INSERT INTO Customers (Name, Email, Contact, Phone, Address)
        VALUES (@Name, @Email, @Contact, @Phone, @Address)";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Name", customer.Name);
            command.Parameters.AddWithValue("@Email", customer.Email);
            command.Parameters.AddWithValue("@Contact", customer.Contact);
            command.Parameters.AddWithValue("@Phone", customer.Phone);
            command.Parameters.AddWithValue("@Address", customer.Address);
            command.ExecuteNonQuery();
        }


        public void UpdateCustomer(Customer customer)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = @"
        UPDATE Customers
        SET Name = @Name, Email = @Email, Contact = @Contact, Phone = @Phone, Address = @Address
        WHERE CustomerID = @CustomerID";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Name", customer.Name);
            command.Parameters.AddWithValue("@Email", customer.Email);
            command.Parameters.AddWithValue("@Contact", customer.Contact);
            command.Parameters.AddWithValue("@Phone", customer.Phone);
            command.Parameters.AddWithValue("@Address", customer.Address);
            command.Parameters.AddWithValue("@CustomerID", customer.CustomerID);
            command.ExecuteNonQuery();
        }


        public void DeleteCustomer(int customerID)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();

            var query = "DELETE FROM Customers WHERE CustomerID = @CustomerID";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerID", customerID);
            command.ExecuteNonQuery();
        }

        public List<Customer> SearchCustomers(string searchTerm)
        {
            var customers = new List<Customer>();
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "SELECT * FROM Customers WHERE Name LIKE @SearchTerm OR Email LIKE @SearchTerm OR Phone LIKE @SearchTerm OR Address LIKE @SearchTerm";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                customers.Add(new Customer
                {
                    CustomerID = Convert.ToInt32(reader["CustomerID"]),
                    Name = reader["Name"].ToString(),
                    Email = reader["Email"].ToString(),
                    Contact = reader["Contact"].ToString(),
                    Phone = reader["Phone"].ToString(),
                    Address = reader["Address"].ToString()
                });
            }
            return customers;
        }

        public Customer GetCustomerByID(int customerID)
        {
            using var connection = new SQLiteConnection(_dbService.ConnectionString);
            connection.Open();
            var query = "SELECT * FROM Customers WHERE CustomerID = @CustomerID";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerID", customerID);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Customer
                {
                    CustomerID = Convert.ToInt32(reader["CustomerID"]),
                    Name = reader["Name"].ToString(),
                    Email = reader["Email"].ToString(),
                    Contact = reader["Contact"].ToString(),
                    Phone = reader["Phone"].ToString(),
                    Address = reader["Address"].ToString()
                };
            }
            return null;
        }


    }
}

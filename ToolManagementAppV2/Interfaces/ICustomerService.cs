using System.Collections.Generic;
using System.Threading.Tasks;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Interfaces
{
    public interface ICustomerService
    {
        void AddCustomer(Customer customer);
        Task AddCustomerAsync(Customer customer);
        void UpdateCustomer(Customer customer);
        Task UpdateCustomerAsync(Customer customer);
        void DeleteCustomer(int customerID);
        Task DeleteCustomerAsync(int customerID);
        Customer GetCustomerByID(int customerID);
        Task<Customer> GetCustomerByIDAsync(int customerID);
        List<Customer> GetAllCustomers();
        Task<List<Customer>> GetAllCustomersAsync();
        List<Customer> SearchCustomers(string searchTerm);
        Task<List<Customer>> SearchCustomersAsync(string searchTerm);
        void ImportCustomersFromCsv(string filePath, IDictionary<string, string> map);
        Task ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map);
        void ExportCustomersToCsv(string filePath);
        Task ExportCustomersToCsvAsync(string filePath);
    }
}


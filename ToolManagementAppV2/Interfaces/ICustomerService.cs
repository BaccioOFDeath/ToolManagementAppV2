using System.Collections.Generic;
using System.Threading.Tasks;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Models.ImportExport;

namespace ToolManagementAppV2.Interfaces
{
    public interface ICustomerService
    {
        Task AddCustomerAsync(Customer customer);
        Task UpdateCustomerAsync(Customer customer);
        Task DeleteCustomerAsync(int customerID);
        Task<Customer> GetCustomerByIDAsync(int customerID);
        Task<List<Customer>> GetAllCustomersAsync();
        Task<List<Customer>> SearchCustomersAsync(string searchTerm);
        Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map);
        Task ExportCustomersToCsvAsync(string filePath);
    }
}


using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Models.ImportExport;

namespace InventoryManagementApp.Interfaces
{
    public interface ICustomerService
    {
        Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
        Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
        Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default);
        Task<Customer> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default);
        Task<List<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default);
        Task<int> GetCustomerCountAsync(CancellationToken cancellationToken = default);
        Task<List<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default);
        Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default);
        Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default);
    }
}


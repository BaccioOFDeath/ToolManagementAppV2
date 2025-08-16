using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Models.ImportExport;

namespace ToolManagementAppV2.Interfaces
{
    public interface ICustomerService
    {
        Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
        Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
        Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default);
        Task<Customer> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default);
        Task<List<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default);
        Task<List<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default);
        Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default);
        Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default);
    }
}


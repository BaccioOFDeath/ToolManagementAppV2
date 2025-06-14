using System.Collections.Generic;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Interfaces
{
    public interface ICustomerService
    {
        void AddCustomer(Customer customer);
        void UpdateCustomer(Customer customer);
        void DeleteCustomer(int customerID);
        Customer GetCustomerByID(int customerID);
        List<Customer> GetAllCustomers();
        List<Customer> SearchCustomers(string searchTerm);
        void ImportCustomersFromCsv(string filePath, IDictionary<string,string> map);
        void ExportCustomersToCsv(string filePath);
    }
}

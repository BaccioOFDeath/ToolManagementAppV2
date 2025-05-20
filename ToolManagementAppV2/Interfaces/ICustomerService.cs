using System.Collections.Generic;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Interfaces
{
    internal interface ICustomerService
    {
        void AddCustomer(Customer customer);
        void UpdateCustomer(Customer customer);
        void DeleteCustomer(int customerID);
        Customer GetCustomerByID(int customerID);
        List<Customer> GetAllCustomers();
        List<Customer> SearchCustomers(string searchTerm);
    }
}

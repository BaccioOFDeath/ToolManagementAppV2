using System;
using System.Collections.Generic;
using System.Threading;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Models.ImportExport;
using ToolManagementAppV2.Utilities.Helpers;

namespace ToolManagementAppV2.Tests;

public static class AsyncServiceExtensions
{
    // ItemService wrappers
    public static void AddTool(this IItemService svc, ItemModel tool, CancellationToken token = default)
        => svc.AddToolAsync(tool, token).GetAwaiter().GetResult();
    public static void UpdateTool(this IItemService svc, ItemModel tool, CancellationToken token = default)
        => svc.UpdateToolAsync(tool, token).GetAwaiter().GetResult();
    public static void DeleteTool(this IItemService svc, int id, CancellationToken token = default)
        => svc.DeleteToolAsync(id, token).GetAwaiter().GetResult();
    public static ItemModel? GetToolByID(this IItemService svc, int id, CancellationToken token = default)
        => svc.GetToolByIDAsync(id, token).GetAwaiter().GetResult();
    public static List<ItemModel> GetAllTools(this IItemService svc, CancellationToken token = default)
        => svc.GetAllToolsAsync(token).GetAwaiter().GetResult();
    public static List<ItemModel> SearchTools(this IItemService svc, string? text, CancellationToken token = default)
        => svc.SearchToolsAsync(text, token).GetAwaiter().GetResult();
    public static string GenerateNextItemNumber(this IItemService svc, CancellationToken token = default)
        => svc.GenerateNextItemNumberAsync(token).GetAwaiter().GetResult();

    // CustomerService wrappers
    public static void AddCustomer(this ICustomerService svc, Customer customer, CancellationToken token = default)
        => svc.AddCustomerAsync(customer, token).GetAwaiter().GetResult();
    public static void UpdateCustomer(this ICustomerService svc, Customer customer, CancellationToken token = default)
        => svc.UpdateCustomerAsync(customer, token).GetAwaiter().GetResult();
    public static void DeleteCustomer(this ICustomerService svc, int id, CancellationToken token = default)
        => svc.DeleteCustomerAsync(id, token).GetAwaiter().GetResult();
    public static Customer GetCustomerByID(this ICustomerService svc, int id, CancellationToken token = default)
        => svc.GetCustomerByIDAsync(id, token).GetAwaiter().GetResult();
    public static List<Customer> GetAllCustomers(this ICustomerService svc, CancellationToken token = default)
        => svc.GetAllCustomersAsync(token).GetAwaiter().GetResult();
    public static List<Customer> SearchCustomers(this ICustomerService svc, string term, CancellationToken token = default)
        => svc.SearchCustomersAsync(term, token).GetAwaiter().GetResult();
    public static CustomerImportResult ImportCustomersFromCsv(this ICustomerService svc, string path, IDictionary<string,string> map, CancellationToken token = default)
        => svc.ImportCustomersFromCsvAsync(path, map, token).GetAwaiter().GetResult();
    public static void ExportCustomersToCsv(this ICustomerService svc, string path, CancellationToken token = default)
        => svc.ExportCustomersToCsvAsync(path, token).GetAwaiter().GetResult();

    // RentalService wrappers
    public static void RentTool(this IRentalService svc, int toolID, int customerID, DateTime rentalDate, DateTime dueDate)
        => svc.RentToolAsync(toolID, customerID, rentalDate, dueDate).GetAwaiter().GetResult();
    public static void ReturnTool(this IRentalService svc, int rentalID, DateTime returnDate)
        => svc.ReturnToolAsync(rentalID, returnDate).GetAwaiter().GetResult();
    public static void ExtendRental(this IRentalService svc, int rentalID, DateTime newDueDate)
        => svc.ExtendRentalAsync(rentalID, newDueDate).GetAwaiter().GetResult();
    public static void DeleteRental(this IRentalService svc, int rentalID)
        => svc.DeleteRentalAsync(rentalID).GetAwaiter().GetResult();
    public static List<Rental> GetActiveRentals(this IRentalService svc)
        => svc.GetActiveRentalsAsync().GetAwaiter().GetResult();
    public static List<Rental> GetOverdueRentals(this IRentalService svc)
        => svc.GetOverdueRentalsAsync().GetAwaiter().GetResult();
    public static List<Rental> GetAllRentals(this IRentalService svc)
        => svc.GetAllRentalsAsync().GetAwaiter().GetResult();
    public static List<Rental> GetRentalHistoryForTool(this IRentalService svc, int toolID)
        => svc.GetRentalHistoryForToolAsync(toolID).GetAwaiter().GetResult();
    public static List<Rental> GetRentalHistoryForCustomer(this IRentalService svc, int customerID)
        => svc.GetRentalHistoryForCustomerAsync(customerID).GetAwaiter().GetResult();

    // UserService wrappers
    public static List<User> GetAllUsers(this IUserService svc)
        => svc.GetAllUsersAsync().GetAwaiter().GetResult();
    public static User? GetUserByID(this IUserService svc, int id)
        => svc.GetUserByIDAsync(id).GetAwaiter().GetResult();
    public static (AuthenticationResult Result, User? User) AuthenticateUser(this IUserService svc, string userName, string password)
        => svc.AuthenticateUserAsync(userName, password).GetAwaiter().GetResult();
    public static User? GetCurrentUser(this IUserService svc)
        => svc.GetCurrentUserAsync().GetAwaiter().GetResult();
    public static void AddUser(this IUserService svc, User user)
        => svc.AddUserAsync(user).GetAwaiter().GetResult();
    public static void UpdateUser(this IUserService svc, User user)
        => svc.UpdateUserAsync(user).GetAwaiter().GetResult();
    public static bool ChangeUserPassword(this IUserService svc, int id, string newPassword)
        => svc.ChangeUserPasswordAsync(id, newPassword).GetAwaiter().GetResult();
}

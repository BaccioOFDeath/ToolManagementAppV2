using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.Interfaces
{
    public interface IRentalService
    {
        /// <summary>
        /// Rents an item to a customer within a database transaction.
        /// This replaces the former <c>RentItemWithTransaction</c> method by
        /// internally handling the transaction scope.
        /// </summary>
        /// <param name="itemID">Identifier of the item to be rented.</param>
        /// <param name="customerID">Identifier of the customer renting the item.</param>
        /// <param name="rentalDate">The date the rental begins.</param>
        /// <param name="dueDate">The date the item is due to be returned.</param>
        Task RentItemAsync(int itemID, int customerID, DateTime rentalDate, DateTime dueDate);

        /// <summary>
        /// Returns a rented item within a database transaction.
        /// This supersedes the old <c>ReturnItemWithTransaction</c> by managing
        /// the transaction internally.
        /// </summary>
        /// <param name="rentalID">Identifier of the rental record.</param>
        /// <param name="returnDate">The date the item is returned.</param>
        Task ReturnItemAsync(int rentalID, DateTime returnDate);
        Task ExtendRentalAsync(int rentalID, DateTime newDueDate);
        Task DeleteRentalAsync(int rentalID);
        Task<List<Rental>> GetActiveRentalsAsync();
        Task<List<Rental>> GetOverdueRentalsAsync();
        Task<List<Rental>> GetAllRentalsAsync();
        Task<List<Rental>> GetRentalHistoryForItemAsync(int itemID);
        Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID);
        Task<int> GetActiveRentalCountAsync(CancellationToken cancellationToken = default);
    }
}

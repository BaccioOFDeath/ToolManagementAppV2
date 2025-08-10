using System;
using System.Collections.Generic;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Interfaces
{
    public interface IRentalService
    {
        /// <summary>
        /// Rents a tool to a customer within a database transaction.
        /// This replaces the former <c>RentToolWithTransaction</c> method by
        /// internally handling the transaction scope.
        /// </summary>
        /// <param name="toolID">Identifier of the tool to be rented.</param>
        /// <param name="customerID">Identifier of the customer renting the tool.</param>
        /// <param name="rentalDate">The date the rental begins.</param>
        /// <param name="dueDate">The date the tool is due to be returned.</param>
        void RentTool(string toolID, int customerID, DateTime rentalDate, DateTime dueDate);

        /// <summary>
        /// Returns a rented tool within a database transaction.
        /// This supersedes the old <c>ReturnToolWithTransaction</c> by managing
        /// the transaction internally.
        /// </summary>
        /// <param name="rentalID">Identifier of the rental record.</param>
        /// <param name="returnDate">The date the tool is returned.</param>
        void ReturnTool(int rentalID, DateTime returnDate);
        void ExtendRental(int rentalID, DateTime newDueDate);
        List<Rental> GetActiveRentals();
        List<Rental> GetOverdueRentals();
        List<Rental> GetAllRentals();
        List<Rental> GetRentalHistoryForTool(string toolID);
        List<Rental> GetRentalHistoryForCustomer(int customerID);
    }
}

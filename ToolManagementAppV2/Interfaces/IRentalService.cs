using System;
using System.Collections.Generic;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Interfaces
{
    public interface IRentalService
    {
        void RentTool(string toolID, int customerID, DateTime rentalDate, DateTime dueDate);
        void RentToolWithTransaction(string toolID, int customerID, DateTime rentalDate, DateTime dueDate);
        void ReturnTool(int rentalID, DateTime returnDate);
        void ReturnToolWithTransaction(int rentalID, DateTime returnDate);
        void ExtendRental(int rentalID, DateTime newDueDate);
        List<Rental> GetActiveRentals();
        List<Rental> GetOverdueRentals();
        List<Rental> GetAllRentals();
        List<Rental> GetRentalHistoryForTool(string toolID);
    }
}

using System;
using System.Threading.Tasks;

namespace InventoryManagementApp.Interfaces
{
    /// <summary>
    /// Defines the contract for the login view model.
    /// </summary>
    public interface ILoginViewModel
    {
        /// <summary>
        /// Initializes the view model.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Raised when login succeeds.
        /// </summary>
        event EventHandler? LoginSucceeded;
    }
}

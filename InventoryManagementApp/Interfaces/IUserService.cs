using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<int> CountUsersAsync();
        Task<User?> GetUserByIDAsync(int userID);
        Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password);
        Task<User?> GetCurrentUserAsync();
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<bool> TryDeleteUserAsync(int userID);
        Task<bool> ChangeUserPasswordAsync(int userID, string newPassword);
    }
}


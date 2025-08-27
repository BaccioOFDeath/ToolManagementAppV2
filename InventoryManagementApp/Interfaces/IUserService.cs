using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<int> CountUsersAsync(CancellationToken cancellationToken = default);
        Task<User?> GetUserByIDAsync(int userID, CancellationToken cancellationToken = default);
        Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password);
        Task<User?> GetCurrentUserAsync();
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<bool> TryDeleteUserAsync(int userID);
        Task<bool> ChangeUserPasswordAsync(int userID, string newPassword);
    }
}


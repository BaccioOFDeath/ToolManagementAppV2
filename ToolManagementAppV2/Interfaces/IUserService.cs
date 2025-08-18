using System.Collections.Generic;
using System.Threading.Tasks;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIDAsync(int userID);
        Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password);
        Task<User?> GetCurrentUserAsync();
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<bool> TryDeleteUserAsync(int userID);
        Task<bool> ChangeUserPasswordAsync(int userID, string newPassword);
        Task UnlockUserAsync(int userId);
    }
}


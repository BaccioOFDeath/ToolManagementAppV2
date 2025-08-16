using System.Collections.Generic;
using System.Threading.Tasks;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        User? GetUserByID(int userID);
        Task<User?> GetUserByIDAsync(int userID);
        Task<User?> AuthenticateUserAsync(string userName, string password);
        User? GetCurrentUser();
        Task<User?> GetCurrentUserAsync();
        void AddUser(User user);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<bool> TryDeleteUserAsync(int userID);
        bool ChangeUserPassword(int userID, string newPassword);
        Task<bool> ChangeUserPasswordAsync(int userID, string newPassword);
    }
}


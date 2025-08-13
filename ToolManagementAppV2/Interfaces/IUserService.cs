using System.Collections.Generic;
using System.Threading.Tasks;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Interfaces
{
    public interface IUserService
    {
        List<User> GetAllUsers();
        Task<List<User>> GetAllUsersAsync();
        User? GetUserByID(int userID);
        Task<User?> GetUserByIDAsync(int userID);
        User? AuthenticateUser(string userName, string password);
        Task<User?> AuthenticateUserAsync(string userName, string password);
        User? GetCurrentUser();
        Task<User?> GetCurrentUserAsync();
        void AddUser(User user);
        Task AddUserAsync(User user);
        void UpdateUser(User user);
        Task UpdateUserAsync(User user);
        bool TryDeleteUser(int userID);
        Task<bool> TryDeleteUserAsync(int userID);
        bool DeleteUser(int userID);
        Task<bool> DeleteUserAsync(int userID);
        void ChangeUserPassword(int userID, string newPassword);
        Task ChangeUserPasswordAsync(int userID, string newPassword);
    }
}


using System.Collections.Generic;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Interfaces
{
    internal interface IUserService
    {
        List<User> GetAllUsers();
        User GetUserByID(int userID);
        User AuthenticateUser(string userName, string password);
        User GetCurrentUser();
        void AddUser(User user);
        void UpdateUser(User user);
        void DeleteUser(int userID);
        bool TryDeleteUser(int userID);
    }
}

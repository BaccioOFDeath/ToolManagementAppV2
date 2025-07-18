﻿using System.Collections.Generic;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Interfaces
{
    public interface IUserService
    {
        List<User> GetAllUsers();
        User GetUserByID(int userID);
        User AuthenticateUser(string userName, string password);
        User GetCurrentUser();
        void AddUser(User user);
        void UpdateUser(User user);
        bool TryDeleteUser(int userID);
        bool DeleteUser(int userID);
        void ChangeUserPassword(int userID, string newPassword);
    }
}

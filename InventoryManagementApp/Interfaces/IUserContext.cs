using System;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.Interfaces
{
    public interface IUserContext
    {
        User? CurrentUser { get; set; }

        event EventHandler<User?>? UserChanged;

        bool IsAdmin { get; }

        string UserName { get; }

        string Role { get; }
    }
}

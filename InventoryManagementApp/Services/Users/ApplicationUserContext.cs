using System.Windows;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.Services.Users
{
    public class ApplicationUserContext : IUserContext
    {
        const string Key = "CurrentUser";

        public event EventHandler<User?>? UserChanged;

        public User? CurrentUser
        {
            get => System.Windows.Application.Current?.Properties[Key] as User;
            set
            {
                if (System.Windows.Application.Current == null) return;
                if (value == null)
                    System.Windows.Application.Current.Properties.Remove(Key);
                else
                    System.Windows.Application.Current.Properties[Key] = value;

                UserChanged?.Invoke(this, value);
            }
        }

        public bool IsAdmin => CurrentUser?.IsAdmin ?? false;

        public string UserName => CurrentUser?.UserName ?? "Guest";

        public string Role => IsAdmin ? "Admin" : "User";
    }
}

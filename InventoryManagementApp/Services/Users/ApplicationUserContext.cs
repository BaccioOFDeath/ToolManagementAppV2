using System.ComponentModel;
using System.Windows;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.Services.Users
{
    public class ApplicationUserContext : IUserContext
    {
        private const string Key = "CurrentUser";

        public event EventHandler<User?>? UserChanged;

        private User? _subscribedUser;

        public User? CurrentUser
        {
            get => System.Windows.Application.Current?.Properties[Key] as User;
            set
            {
                if (System.Windows.Application.Current == null) return;

                if (_subscribedUser != null)
                    _subscribedUser.PropertyChanged -= OnCurrentUserPropertyChanged;

                if (value == null)
                    System.Windows.Application.Current.Properties.Remove(Key);
                else
                {
                    System.Windows.Application.Current.Properties[Key] = value;
                    value.PropertyChanged += OnCurrentUserPropertyChanged;
                }

                _subscribedUser = value;
                UserChanged?.Invoke(this, value);
            }
        }

        private void OnCurrentUserPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is User user)
                UserChanged?.Invoke(this, user);
        }

        public bool IsAdmin => CurrentUser?.IsAdmin ?? false;

        public string UserName => CurrentUser?.UserName ?? string.Empty;

        public string Role
        {
            get
            {
                if (IsAdmin)
                    return "Admin";

                if (CurrentUser == null)
                    return string.Empty;

                return string.IsNullOrWhiteSpace(CurrentUser.Role)
                    ? "User"
                    : CurrentUser.Role;
            }
        }
    }
}

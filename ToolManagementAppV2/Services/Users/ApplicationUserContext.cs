using System.Windows;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Services.Users
{
    public class ApplicationUserContext : IUserContext
    {
        const string Key = "CurrentUser";

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
            }
        }
    }
}

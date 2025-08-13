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
            get => Application.Current?.Properties[Key] as User;
            set
            {
                if (Application.Current == null) return;
                if (value == null)
                    Application.Current.Properties.Remove(Key);
                else
                    Application.Current.Properties[Key] = value;
            }
        }
    }
}

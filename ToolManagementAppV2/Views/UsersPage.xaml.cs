using System;
using System.Windows.Controls;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2.Views
{
    /// <summary>
    /// Page for viewing and managing users.
    /// The <see cref="DataContext"/> is expected to be a <see cref="UserManagementViewModel"/>.
    /// </summary>
    public partial class UsersPage : Page
    {
        public UsersPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Convenience accessor for the strongly typed view model.
        /// Throws if the DataContext is not correctly set.
        /// </summary>
        public UserManagementViewModel ViewModel =>
            DataContext as UserManagementViewModel
            ?? throw new InvalidOperationException("UsersPage requires a UserManagementViewModel DataContext.");
    }
}

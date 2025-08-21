using System.Windows.Controls;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    /// <summary>
    /// Page for viewing and managing users.
    /// The <see cref="DataContext"/> is expected to be a <see cref="UserManagementViewModel"/>.
    /// </summary>
    public partial class UsersPage : Page
    {
        public UsersPage(UserManagementViewModel? viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        /// <summary>
        /// Convenience accessor for the strongly typed view model.
        /// Returns null if the DataContext is not correctly set.
        /// </summary>
        public UserManagementViewModel? ViewModel =>
            DataContext as UserManagementViewModel;
    }
}

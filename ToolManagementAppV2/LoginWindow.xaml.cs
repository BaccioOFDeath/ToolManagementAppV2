using System;
using System.Linq;
using System.Windows;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Services;

namespace ToolManagementAppV2
{
    public partial class LoginWindow : Window
    {
        private readonly UserService _userService;
        public LoginWindow()
        {
            InitializeComponent();
            var dbPath = "tool_inventory.db";
            var databaseService = new DatabaseService(dbPath);
            _userService = new UserService(databaseService);
            LoadUsers();
        }

        private void LoadUsers()
        {
            var users = _userService.GetAllUsers();
            if (users.Count == 0)
            {
                MessageBox.Show("No users exist. A default admin account will be created (username: admin, password: admin).",
                                "Setup", MessageBoxButton.OK, MessageBoxImage.Information);
                var defaultUser = new User
                {
                    UserName = "admin",
                    Password = "admin",
                    IsAdmin = true
                };
                _userService.AddUser(defaultUser);
                users = _userService.GetAllUsers();
            }
            UserComboBox.ItemsSource = users;
            UserComboBox.DisplayMemberPath = "UserName";
            if (users.Any())
                UserComboBox.SelectedIndex = 0;
        }


        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserComboBox.SelectedItem is User selectedUser)
            {
                string enteredPassword = PasswordBox.Password;
                if (string.IsNullOrEmpty(selectedUser.Password) || selectedUser.Password == enteredPassword)
                {
                    // Set DialogResult to true and close the dialog
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Incorrect password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a user.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}

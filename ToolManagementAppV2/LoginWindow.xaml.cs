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
            UsersListBox.ItemsSource = users;
        }

        // LoginWindow.xaml.cs – Updated UserButton_Click to treat an empty admin password as default "admin"
        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is User selectedUser)
            {
                // For admin users, always require a password.
                // If an admin's password is empty, treat it as "admin" and update the record.
                if (selectedUser.IsAdmin && string.IsNullOrEmpty(selectedUser.Password))
                {
                    selectedUser.Password = "admin";
                    _userService.ChangeUserPassword(selectedUser.UserID, "admin");
                }

                // Determine if a password prompt is required:
                // Admin users must always enter a password.
                // Non-admin users require a password only if one is set.
                bool requirePassword = selectedUser.IsAdmin || (!selectedUser.IsAdmin && !string.IsNullOrEmpty(selectedUser.Password));
                if (!requirePassword)
                {
                    App.Current.Properties["CurrentUser"] = selectedUser;
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    PasswordPromptWindow prompt = new PasswordPromptWindow
                    {
                        SelectedUser = selectedUser,
                        // Set the ValidatePassword delegate to authenticate using the user service.
                        ValidatePassword = (pwd) => _userService.AuthenticateUser(selectedUser.UserName, pwd) != null
                    };

                    bool? result = prompt.ShowDialog();
                    if (result == true)
                    {
                        if (prompt.IsPasswordResetRequested)
                        {
                            // After a reset, the admin's password is now "admin".
                            var user = _userService.AuthenticateUser(selectedUser.UserName, "admin");
                            if (user != null)
                            {
                                App.Current.Properties["CurrentUser"] = user;
                                this.DialogResult = true;
                                this.Close();
                            }
                        }
                        else
                        {
                            var user = _userService.AuthenticateUser(selectedUser.UserName, prompt.EnteredPassword);
                            if (user != null)
                            {
                                App.Current.Properties["CurrentUser"] = user;
                                this.DialogResult = true;
                                this.Close();
                            }
                        }
                    }
                }
            }
        }

    }
}


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
        // Updated UserButton_Click in LoginWindow.xaml.cs
        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is User selectedUser)
            {
                // For admin users, if password is empty or whitespace, set default to "admin"
                if (selectedUser.IsAdmin && string.IsNullOrWhiteSpace(selectedUser.Password))
                {
                    selectedUser.Password = "admin";
                    _userService.ChangeUserPassword(selectedUser.UserID, "admin");
                }

                // For non-admin users, if password is empty OR equals the default "newpassword", skip prompt
                if (!selectedUser.IsAdmin && (string.IsNullOrWhiteSpace(selectedUser.Password) ||
                     selectedUser.Password.Equals("newpassword", StringComparison.OrdinalIgnoreCase)))
                {
                    App.Current.Properties["CurrentUser"] = selectedUser;
                    this.DialogResult = true;
                    this.Close();
                    return;
                }

                // Otherwise, prompt for password
                PasswordPromptWindow prompt = new PasswordPromptWindow
                {
                    SelectedUser = selectedUser,
                    ValidatePassword = (pwd) => _userService.AuthenticateUser(selectedUser.UserName, pwd) != null
                };

                bool? result = prompt.ShowDialog();
                if (result == true)
                {
                    if (prompt.IsPasswordResetRequested)
                    {
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


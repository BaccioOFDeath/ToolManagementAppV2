using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Services;

namespace ToolManagementAppV2
{
    public partial class LoginWindow : Window
    {
        private readonly UserService _userService;
        private readonly SettingsService _settingsService;

        public LoginWindow()
        {
            InitializeComponent();

            // --- initialize services pointing at your same database ---
            var dbPath = "tool_inventory.db";
            var databaseService = new DatabaseService(dbPath);
            _settingsService = new SettingsService(databaseService);
            _userService = new UserService(databaseService);

            // --- load & apply application name from settings ---
            var appName = _settingsService.GetSetting("ApplicationName");
            if (string.IsNullOrWhiteSpace(appName))
                appName = "Tool Inventory Management";
            // Window title
            this.Title = $"{appName} – Login";
            // Header TextBlock (named in XAML)
            HeaderTitle.Text = appName;

            // --- load & display logo from settings (or fallback) ---
            var logoPath = _settingsService.GetSetting("CompanyLogoPath");
            var logoUri = !string.IsNullOrEmpty(logoPath) && File.Exists(logoPath)
                ? new Uri(logoPath, UriKind.Absolute)
                : new Uri("pack://application:,,,/Resources/DefaultLogo.png", UriKind.Absolute);
            LoginLogo.Source = new BitmapImage(logoUri);

            // --- populate the user list ---
            LoadUsers();
        }

        private void LoadUsers()
        {
            var users = _userService.GetAllUsers();
            if (users.Count == 0)
            {
                MessageBox.Show(
                    "No users exist. A default admin account will be created (username: admin, password: admin).",
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

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is User selectedUser)
            {
                // For admin users, treat empty password as "admin"
                if (selectedUser.IsAdmin && string.IsNullOrWhiteSpace(selectedUser.Password))
                {
                    selectedUser.Password = "admin";
                    _userService.ChangeUserPassword(selectedUser.UserID, "admin");
                }

                // For non-admins, skip prompt if no real password set
                if (!selectedUser.IsAdmin &&
                    (string.IsNullOrWhiteSpace(selectedUser.Password) ||
                     selectedUser.Password.Equals("newpassword", StringComparison.OrdinalIgnoreCase)))
                {
                    App.Current.Properties["CurrentUser"] = selectedUser;
                    DialogResult = true;
                    Close();
                    return;
                }

                // Otherwise ask for password
                var prompt = new PasswordPromptWindow
                {
                    SelectedUser = selectedUser,
                    ValidatePassword = pwd => _userService.AuthenticateUser(selectedUser.UserName, pwd) != null
                };

                if (prompt.ShowDialog() == true)
                {
                    User user = null;
                    if (prompt.IsPasswordResetRequested)
                    {
                        // reset via admin fallback
                        user = _userService.AuthenticateUser(selectedUser.UserName, "admin");
                    }
                    else
                    {
                        user = _userService.AuthenticateUser(selectedUser.UserName, prompt.EnteredPassword);
                    }

                    if (user != null)
                    {
                        App.Current.Properties["CurrentUser"] = user;
                        DialogResult = true;
                        Close();
                    }
                }
            }
        }
    }
}

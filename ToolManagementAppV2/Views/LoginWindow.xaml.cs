using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Services;

namespace ToolManagementAppV2
{
    public partial class LoginWindow : Window
    {
        readonly UserService _userService;

        public LoginWindow()
        {
            InitializeComponent();

            var dbService = new DatabaseService("tool_inventory.db");
            var settings = new SettingsService(dbService);

            // Load logo
            var logoPath = settings.GetSetting("CompanyLogoPath");
            var logoUri = !string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath)
                           ? new Uri(logoPath)
                           : new Uri("pack://application:,,,/Resources/DefaultLogo.png");
            LoginLogo.Source = new BitmapImage(logoUri);

            // Set window title
            var appName = settings.GetSetting("ApplicationName");
            Title = !string.IsNullOrWhiteSpace(appName)
                    ? $"{appName} – Login"
                    : Title;

            _userService = new UserService(dbService);
            LoadUsers();
        }

        void LoadUsers()
        {
            var users = _userService.GetAllUsers();
            if (users.Count == 0)
            {
                MessageBox.Show(
                    "No users exist. A default admin account will be created (username: admin, password: admin).",
                    "Setup", MessageBoxButton.OK, MessageBoxImage.Information);

                var admin = new User { UserName = "admin", Password = "admin", IsAdmin = true };
                _userService.AddUser(admin);
                users = _userService.GetAllUsers();
            }

            UsersListBox.ItemsSource = users;
        }

        void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement fe && fe.Tag is User user)) return;

            // Admin empty-password fallback
            if (user.IsAdmin && string.IsNullOrWhiteSpace(user.Password))
            {
                _userService.ChangeUserPassword(user.UserID, "admin");
                user.Password = "admin";
            }

            // Non-admin default-password skip
            if (!user.IsAdmin &&
                (string.IsNullOrWhiteSpace(user.Password) ||
                 user.Password.Equals("newpassword", StringComparison.OrdinalIgnoreCase)))
            {
                App.Current.Properties["CurrentUser"] = user;
                DialogResult = true;
                return;
            }

            var prompt = new PasswordPromptWindow
            {
                SelectedUser = user,
                ValidatePassword = pwd => _userService.AuthenticateUser(user.UserName, pwd) != null
            };

            if (prompt.ShowDialog() != true) return;

            var credential = prompt.IsPasswordResetRequested
                ? _userService.AuthenticateUser(user.UserName, "admin")
                : _userService.AuthenticateUser(user.UserName, prompt.EnteredPassword);

            if (credential != null)
            {
                App.Current.Properties["CurrentUser"] = credential;
                DialogResult = true;
            }
        }
    }
}

using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Utilities.Helpers;

namespace ToolManagementAppV2
{
    public partial class LoginWindow : Window
    {
        readonly UserService _userService;

        public LoginWindow()
        {
            InitializeComponent();

            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tool_inventory.db");
            var dbService = new DatabaseService(dbPath);
            var settings = new SettingsService(dbService);

            // Load logo
            var logoPath = settings.GetSetting("CompanyLogoPath");
            Uri logoUri;
            if (!string.IsNullOrWhiteSpace(logoPath))
            {
                var full = Utilities.Helpers.PathHelper.GetAbsolutePath(logoPath);
                logoUri = !string.IsNullOrEmpty(full) && File.Exists(full)
                    ? new Uri(full)
                    : new Uri("pack://application:,,,/Resources/DefaultLogo.png");
            }
            else
            {
                logoUri = new Uri("pack://application:,,,/Resources/DefaultLogo.png");
            }
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = logoUri;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            LoginLogo.Source = bitmap;


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

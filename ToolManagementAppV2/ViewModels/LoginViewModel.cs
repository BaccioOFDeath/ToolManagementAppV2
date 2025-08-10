using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Utilities.Extensions;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Views;

namespace ToolManagementAppV2.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        readonly IUserService _userService;
        readonly ISettingsService _settingsService;

        public ObservableCollection<User> Users { get; } = new();

        User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public BitmapImage CompanyLogo { get; }
        public string WindowTitle { get; }

        public ICommand SelectUserCommand { get; }

        public event EventHandler? LoginSucceeded;

        public LoginViewModel(string? dbPath = null)
        {
            dbPath ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tool_inventory.db");
            var dbService = new DatabaseService(dbPath);
            _settingsService = new SettingsService(dbService);
            _userService = new UserService(dbService);

            CompanyLogo = LoadLogo();
            WindowTitle = GetWindowTitle();

            SelectUserCommand = new RelayCommand<User>(OnUserSelected);

            LoadUsers();
        }

        BitmapImage LoadLogo()
        {
            var logoPath = _settingsService.GetSetting("CompanyLogoPath");
            Uri logoUri;
            if (!string.IsNullOrWhiteSpace(logoPath))
            {
                var full = PathHelper.GetAbsolutePath(logoPath);
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
            return bitmap;
        }

        string GetWindowTitle()
        {
            var appName = _settingsService.GetSetting("ApplicationName");
            return !string.IsNullOrWhiteSpace(appName)
                ? $"{appName} – Login"
                : "Tool Inventory Management – Login";
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

            Users.ReplaceRange(users);
        }

        void OnUserSelected(User user)
        {
            if (user == null) return;

            if (user.IsAdmin && string.IsNullOrWhiteSpace(user.Password))
            {
                _userService.ChangeUserPassword(user.UserID, "admin");
                user.Password = SecurityHelper.ComputeSha256Hash("admin");
            }

            var defaultHash = SecurityHelper.ComputeSha256Hash("newpassword");
            if (!user.IsAdmin &&
                (string.IsNullOrWhiteSpace(user.Password) ||
                 user.Password.Equals(defaultHash, StringComparison.OrdinalIgnoreCase)))
            {
                Application.Current.Properties["CurrentUser"] = user;
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
                return;
            }

            var passwordValidated = false;
            while (!passwordValidated)
            {
                var prompt = new PasswordPromptWindow
                {
                    SelectedUser = user,
                    ValidatePassword = pwd => _userService.AuthenticateUser(user.UserName, pwd) != null
                };

                if (prompt.ShowDialog() != true) return;

                if (prompt.IsPasswordResetRequested)
                {
                    _userService.ChangeUserPassword(user.UserID, "admin");
                    user.Password = SecurityHelper.ComputeSha256Hash("admin");
                    LoadUsers();
                    MessageBox.Show("Password has been reset to default. Please enter the new password to login.",
                        "Password Reset", MessageBoxButton.OK, MessageBoxImage.Information);
                    continue;
                }

                var credential = _userService.AuthenticateUser(user.UserName, prompt.EnteredPassword);
                if (credential != null)
                {
                    Application.Current.Properties["CurrentUser"] = credential;
                    LoginSucceeded?.Invoke(this, EventArgs.Empty);
                    passwordValidated = true;
                }
            }
        }
    }
}

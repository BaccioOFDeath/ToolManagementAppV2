using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Utilities.Extensions;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Views;

namespace ToolManagementAppV2.ViewModels
{
    /// <summary>
    /// View model that drives the login window. It loads users from the database,
    /// creating a default "admin" account if none exist, and coordinates the
    /// authentication workflow via <see cref="SelectUserCommand"/>. The process covers
    /// default password generation and resets so future logins remain possible. When a
    /// user successfully authenticates the <see cref="LoginSucceeded"/> event is raised
    /// and the authenticated user is stored in <see cref="Application.Current"/>.
    /// </summary>
    public class LoginViewModel : ObservableObject
    {
        readonly IUserService _userService;
        readonly ISettingsService _settingsService;
        readonly IDialogService _dialogService;
        readonly IUserContext _userContext;

        public ObservableCollection<User> Users { get; } = new();

        User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public BitmapImage CompanyLogo { get; }
        public string WindowTitle { get; }

        /// <summary>
        /// Command invoked when the user selects an account from the list. It calls
        /// <see cref="OnUserSelected"/> to perform authentication, including prompting
        /// for passwords and handling resets.
        /// </summary>
        public ICommand SelectUserCommand { get; }

        /// <summary>
        /// Raised after <see cref="OnUserSelected"/> successfully authenticates a user
        /// and stores the result in <see cref="IUserContext"/>.
        /// </summary>
        public event EventHandler? LoginSucceeded;

        public LoginViewModel(IDialogService dialogService, IUserContext userContext, string? dbPath = null)
        {
            dbPath ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tool_inventory.db");
            var dbService = new DatabaseService(dbPath);
            _settingsService = new SettingsService(dbService);
            _userService = new UserService(dbService, userContext);
            _dialogService = dialogService;
            _userContext = userContext;

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
                _dialogService.ShowInfo(
                    "No users exist. A default admin account will be created (username: admin, password: admin).",
                    "Setup");

                var admin = new User { UserName = "admin", Password = "admin", IsAdmin = true };
                _userService.AddUser(admin);
                users = _userService.GetAllUsers();
            }

            Users.ReplaceRange(users);
        }

        /// <summary>
        /// Executes the authentication workflow for the selected <paramref name="user"/>.
        /// Ensures administrators always have a default password, allows first-time
        /// non-admin users to log in with a generated password, prompts for existing
        /// passwords and supports resetting credentials back to the default "admin".
        /// Successful authentication stores the user in the shared context
        /// and raises <see cref="LoginSucceeded"/>.
        /// </summary>
        /// <param name="user">The account to authenticate.</param>
        void OnUserSelected(User user)
        {
            if (user == null) return;

            if (user.IsAdmin && string.IsNullOrWhiteSpace(user.Password))
            {
                _userService.ChangeUserPassword(user.UserID, "admin");
                var refreshed = _userService.GetUserByID(user.UserID);
                user.Password = refreshed.Password;
                user.Salt = refreshed.Salt;
            }

            if (!user.IsAdmin &&
                (string.IsNullOrWhiteSpace(user.Password) ||
                 SecurityHelper.VerifyPassword("newpassword", user.Salt, user.Password)))
            {
                _userContext.CurrentUser = user;
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
                return;
            }

            var passwordValidated = false;
            while (!passwordValidated)
            {
                var prompt = new PasswordPromptWindow(_dialogService)
                {
                    SelectedUser = user,
                    ValidatePassword = pwd => _userService.AuthenticateUser(user.UserName, pwd) != null
                };

                if (prompt.ShowDialog() != true) return;

                if (prompt.IsPasswordResetRequested)
                {
                    _userService.ChangeUserPassword(user.UserID, "admin");
                    var refreshed = _userService.GetUserByID(user.UserID);
                    user.Password = refreshed.Password;
                    user.Salt = refreshed.Salt;
                    LoadUsers();
                    _dialogService.ShowInfo("Password has been reset to default. Please enter the new password to login.",
                        "Password Reset");
                    continue;
                }

                var credential = _userService.AuthenticateUser(user.UserName, prompt.EnteredPassword);
                if (credential != null)
                {
                    _userContext.CurrentUser = credential;
                    LoginSucceeded?.Invoke(this, EventArgs.Empty);
                    passwordValidated = true;
                }
            }
        }
    }
}

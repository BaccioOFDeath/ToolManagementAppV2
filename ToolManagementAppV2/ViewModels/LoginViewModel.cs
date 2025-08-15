using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
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
        public IAsyncRelayCommand<User> SelectUserCommand { get; }

        public IAsyncRelayCommand LoadUsersCommand { get; }

        /// <summary>
        /// Raised after <see cref="OnUserSelected"/> successfully authenticates a user
        /// and stores the result in <see cref="IUserContext"/>.
        /// </summary>
        public event EventHandler? LoginSucceeded;

        public Func<string?> PromptForNewPassword { get; set; } = () =>
        {
            using var dlg = new Views.ChangePasswordWindow();
            return dlg.ShowDialog() == true ? dlg.NewPassword : null;
        };

        public Func<User, CancellationToken, Task<PasswordPromptResult?>>? PromptForPasswordAsync { get; set; }
            

        public LoginViewModel(IUserService userService, ISettingsService settingsService, IDialogService dialogService, IUserContext userContext)
        {
            _settingsService = settingsService;
            _userService = userService;
            _dialogService = dialogService;
            _userContext = userContext;

            CompanyLogo = LoadLogoAsync().GetAwaiter().GetResult();
            WindowTitle = GetWindowTitleAsync().GetAwaiter().GetResult();

            SelectUserCommand = new AsyncRelayCommand<User>(OnUserSelected);

            PromptForPasswordAsync = (u, ct) =>
            {
                var prompt = new PasswordPromptWindow(_dialogService)
                {
                    SelectedUser = u
                };
                var result = prompt.ShowDialog();
                if (result == true)
                    return Task.FromResult<PasswordPromptResult?>(new PasswordPromptResult(prompt.EnteredPassword, prompt.IsPasswordResetRequested));
                return Task.FromResult<PasswordPromptResult?>(null);
            };

            LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync);
            LoadUsersCommand.Execute(null);
        }

        async Task<BitmapImage> LoadLogoAsync()
        {
            var logoPath = await _settingsService.GetSettingAsync("CompanyLogoPath");
            Uri logoUri;
            if (!string.IsNullOrWhiteSpace(logoPath))
            {
                try
                {
                    var full = PathHelper.GetAbsolutePath(logoPath, true);
                    logoUri = !string.IsNullOrEmpty(full) && File.Exists(full)
                        ? new Uri(full)
                        : new Uri("pack://application:,,,/Resources/DefaultLogo.png");
                }
                catch (InvalidOperationException)
                {
                    logoUri = new Uri("pack://application:,,,/Resources/DefaultLogo.png");
                }
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

        async Task<string> GetWindowTitleAsync()
        {
            var appName = await _settingsService.GetSettingAsync("ApplicationName");
            return !string.IsNullOrWhiteSpace(appName)
                ? $"{appName} – Login"
                : "Tool Inventory Management – Login";
        }

        async Task LoadUsersAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            if (users.Count == 0)
            {
                await _dialogService.ShowInfoAsync(
                    "No users exist. A default admin account will be created (username: admin, password: admin).",
                    "Setup");

                var hashed = SecurityHelper.HashPassword("admin", out var salt);
                var admin = new User
                {
                    UserName = "admin",
                    Password = hashed,
                    Salt = salt,
                    IsAdmin = true,
                    PasswordExpired = true
                };
                _userService.AddUser(admin);
                users = await _userService.GetAllUsersAsync();
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
        /// <param name="cancellationToken">Token used to cancel the authentication loop.</param>
        async Task OnUserSelected(User user, CancellationToken cancellationToken)
        {
            if (user == null) return;

            if (user.IsAdmin && string.IsNullOrWhiteSpace(user.Password))
            {
                await _userService.ChangeUserPasswordAsync(user.UserID, "admin");
                var refreshed = await _userService.GetUserByIDAsync(user.UserID);
                if (refreshed != null)
                {
                    user.Password = refreshed.Password;
                    user.Salt = refreshed.Salt;
                    user.PasswordExpired = refreshed.PasswordExpired;
                }
            }

            if (!user.IsAdmin &&
                (string.IsNullOrWhiteSpace(user.Password) ||
                 SecurityHelper.VerifyPassword("newpassword", user.Salt, user.Password)))
            {
                if (user.PasswordExpired && !PromptChangePassword(user))
                    return;
                _userContext.CurrentUser = user;
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
                return;
            }

            User? credential = null;
            while (credential == null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var promptResult = await (PromptForPasswordAsync?.Invoke(user, cancellationToken)
                    ?? Task.FromResult<PasswordPromptResult?>(null));
                if (promptResult == null)
                    return;

                if (promptResult.IsPasswordResetRequested)
                {
                    await _userService.ChangeUserPasswordAsync(user.UserID, "admin");
                    var refreshed = await _userService.GetUserByIDAsync(user.UserID);
                    if (refreshed != null)
                    {
                        user.Password = refreshed.Password;
                        user.Salt = refreshed.Salt;
                        user.PasswordExpired = refreshed.PasswordExpired;
                    }
                    await LoadUsersCommand.ExecuteAsync(null);
                    await _dialogService.ShowInfoAsync("Password has been reset to default. Please enter the new password to login.",
                        "Password Reset");
                    continue;
                }

                credential = await _userService.AuthenticateUserAsync(user.UserName, promptResult.Password);
                if (credential == null)
                {
                    await _dialogService.ShowInfoAsync("Incorrect password. Please try again.", "Login Failed");
                }
            }

            if (credential.PasswordExpired && !PromptChangePassword(credential))
                return;
            _userContext.CurrentUser = credential;
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }

        bool PromptChangePassword(User user)
        {
            var newPwd = PromptForNewPassword?.Invoke();
            if (string.IsNullOrWhiteSpace(newPwd))
                return false;
            _userService.ChangeUserPassword(user.UserID, newPwd);
            var refreshed = _userService.GetUserByID(user.UserID);
            if (refreshed != null)
            {
                user.Password = refreshed.Password;
                user.Salt = refreshed.Salt;
                user.PasswordExpired = refreshed.PasswordExpired;
            }
            return true;
        }
    }

    public record PasswordPromptResult(string Password, bool IsPasswordResetRequested);
}

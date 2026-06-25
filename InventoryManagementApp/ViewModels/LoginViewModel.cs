using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Reflection;
using MediaBrush = System.Windows.Media.Brush;
using MediaBrushes = System.Windows.Media.Brushes;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Utilities.Extensions;
using InventoryManagementApp.Utilities.Helpers;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.Views.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Application = System.Windows.Application;

namespace InventoryManagementApp.ViewModels
{
    /// <summary>
    /// View model that drives the login window. It loads users from the database,
    /// creating a default "admin" account if none exist, and coordinates the
    /// authentication workflow via <see cref="SelectUserCommand"/>. The process covers
    /// default password generation and resets so future logins remain possible. When a
    /// user successfully authenticates the <see cref="LoginSucceeded"/> event is raised
    /// and the authenticated user is stored in <see cref="System.Windows.Application.Current"/>.
    /// </summary>
    public class LoginViewModel : ObservableObject, ILoginViewModel
    {
        readonly IUserService _userService;
        readonly ISettingsService _settingsService;
        readonly IDialogService _dialogService;
        readonly IUserContext _userContext;
        readonly ILogger<LoginViewModel> _logger;
        readonly IServiceProvider? _serviceProvider;

        public ObservableCollection<User> Users { get; } = new();

        User? _selectedUser;
        public User? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        BitmapImage? _companyLogo;
        public BitmapImage? CompanyLogo
        {
            get => _companyLogo;
            private set => SetProperty(ref _companyLogo, value);
        }

        string _windowTitle = string.Empty;
        public string WindowTitle
        {
            get => _windowTitle;
            private set => SetProperty(ref _windowTitle, value);
        }

        bool _isUpdateAvailable;
        public bool IsUpdateAvailable
        {
            get => _isUpdateAvailable;
            private set => SetProperty(ref _isUpdateAvailable, value);
        }

        string _updateAvailableMessage = string.Empty;
        public string UpdateAvailableMessage
        {
            get => _updateAvailableMessage;
            private set => SetProperty(ref _updateAvailableMessage, value);
        }

        string _versionDisplayText = string.Empty;
        public string VersionDisplayText
        {
            get => _versionDisplayText;
            private set => SetProperty(ref _versionDisplayText, value);
        }

        MediaBrush _versionStatusBrush = MediaBrushes.ForestGreen;
        public MediaBrush VersionStatusBrush
        {
            get => _versionStatusBrush;
            private set => SetProperty(ref _versionStatusBrush, value);
        }

        /// <summary>
        /// Command invoked when the user selects an account from the list. It calls
        /// <see cref="OnUserSelected"/> to perform authentication, including prompting
        /// for passwords and handling resets.
        /// </summary>
        public IAsyncRelayCommand<User?> SelectUserCommand { get; }

        public IAsyncRelayCommand LoadUsersCommand { get; }

        /// <summary>
        /// Raised after <see cref="OnUserSelected"/> successfully authenticates a user
        /// and stores the result in <see cref="IUserContext"/>.
        /// </summary>
        public event EventHandler? LoginSucceeded;

        public Func<string?> PromptForNewPassword { get; set; } = () =>
        {
            using var dlg = new Views.Windows.ChangePasswordWindow();
            return dlg.ShowDialog() == true ? dlg.NewPassword : null;
        };

        public Func<User, CancellationToken, Task<PasswordPromptResult?>>? PromptForPasswordAsync { get; set; }
            

        public LoginViewModel(IUserService userService, ISettingsService settingsService, IDialogService dialogService, IUserContext userContext, ILogger<LoginViewModel>? logger = null, IServiceProvider? serviceProvider = null)
        {
            _settingsService = settingsService;
            _userService = userService;
            _dialogService = dialogService;
            _userContext = userContext;
            _logger = logger ?? NullLogger<LoginViewModel>.Instance;
            _serviceProvider = serviceProvider;

            SelectUserCommand = new AsyncRelayCommand<User?>(OnUserSelected);

            PromptForPasswordAsync = (u, ct) =>
            {
                PasswordPromptWindow prompt;
                if (_serviceProvider != null)
                    prompt = _serviceProvider.GetRequiredService<PasswordPromptWindow>();
                else
                    prompt = new PasswordPromptWindow(_dialogService);
                prompt.SelectedUser = u;
                var result = prompt.ShowDialog();
                if (result == true)
                    return Task.FromResult<PasswordPromptResult?>(new PasswordPromptResult(prompt.EnteredPassword, prompt.IsPasswordResetRequested));
                return Task.FromResult<PasswordPromptResult?>(null);
            };

            LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync);
        }

        public Task InitializeAsync()
            => InitializeAsync(default);

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            CompanyLogo = await LoadLogoAsync(cancellationToken);
            WindowTitle = await GetWindowTitleAsync(cancellationToken);
            RefreshUpdateAvailability();
            await LoadUsersCommand.ExecuteAsync(null);
        }

        void RefreshUpdateAvailability()
        {
            try
            {
                var runningRelease = GetRunningSharedReleaseName(AppContext.BaseDirectory, out var deploymentRoot);
                VersionDisplayText = BuildVersionDisplayText(runningRelease);
                VersionStatusBrush = MediaBrushes.ForestGreen;
                if (string.IsNullOrWhiteSpace(deploymentRoot))
                {
                    IsUpdateAvailable = false;
                    VersionStatusBrush = MediaBrushes.DimGray;
                    UpdateAvailableMessage = string.Empty;
                    return;
                }

                var markerPath = Path.Combine(deploymentRoot, "current-release.txt");
                if (!File.Exists(markerPath))
                {
                    IsUpdateAvailable = false;
                    UpdateAvailableMessage = string.Empty;
                    return;
                }

                var currentRelease = File.ReadLines(markerPath)
                    .FirstOrDefault(line => !string.IsNullOrWhiteSpace(line))
                    ?.Trim();

                IsUpdateAvailable = !string.IsNullOrWhiteSpace(currentRelease) &&
                    !string.Equals(currentRelease, runningRelease, StringComparison.OrdinalIgnoreCase);
                VersionDisplayText = BuildVersionDisplayText(runningRelease, currentRelease);
                VersionStatusBrush = IsUpdateAvailable ? MediaBrushes.Firebrick : MediaBrushes.ForestGreen;
                UpdateAvailableMessage = IsUpdateAvailable
                    ? "An app update is available. Please close and reopen Inventory Management to start the latest version."
                    : string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check shared release update availability.");
                VersionDisplayText = BuildVersionDisplayText();
                VersionStatusBrush = MediaBrushes.DimGray;
                IsUpdateAvailable = false;
                UpdateAvailableMessage = string.Empty;
            }
        }

        static string BuildVersionDisplayText(string? runningRelease = null, string? currentRelease = null)
        {
            var appVersion = typeof(LoginViewModel).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (string.IsNullOrWhiteSpace(appVersion))
                appVersion = typeof(LoginViewModel).Assembly.GetName().Version?.ToString() ?? "unknown";
            appVersion = appVersion.Split('+')[0];

            if (!string.IsNullOrWhiteSpace(currentRelease) &&
                !string.Equals(currentRelease, runningRelease, StringComparison.OrdinalIgnoreCase))
            {
                return $"App {appVersion} - Outdated release {runningRelease ?? "local"} - latest {currentRelease}";
            }

            if (!string.IsNullOrWhiteSpace(runningRelease))
                return $"App {appVersion} - Current release {runningRelease}";

            return $"App {appVersion} - Local build";
        }

        static string? GetRunningSharedReleaseName(string baseDirectory, out string? deploymentRoot)
        {
            deploymentRoot = null;
            var releaseDirectory = new DirectoryInfo(baseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (releaseDirectory.Parent?.Name.Equals("_releases", StringComparison.OrdinalIgnoreCase) == true)
            {
                deploymentRoot = releaseDirectory.Parent.Parent?.FullName;
                return releaseDirectory.Name;
            }

            var rootMarkerPath = Path.Combine(releaseDirectory.FullName, "current-release.txt");
            if (File.Exists(rootMarkerPath))
            {
                deploymentRoot = releaseDirectory.FullName;
                return null;
            }

            return null;
        }

        async Task<BitmapImage> LoadLogoAsync(CancellationToken cancellationToken)
        {
            var logoPath = await _settingsService.GetSettingAsync("CompanyLogoPath", cancellationToken);
            Uri logoUri;
            if (!string.IsNullOrWhiteSpace(logoPath))
            {
                try
                {
                    var full = PathHelper.GetAbsolutePath(logoPath, true);
                    if (!string.IsNullOrEmpty(full) && File.Exists(full))
                    {
                        logoUri = new Uri(full);
                    }
                    else
                    {
                        await _dialogService.ShowInfoAsync("Company logo path is invalid; using default logo.", "Invalid Path");
                        logoUri = new Uri("pack://application:,,,/Resources/DefaultLogo.png");
                    }
                }
                catch (InvalidOperationException)
                {
                    await _dialogService.ShowInfoAsync("Company logo path is invalid; using default logo.", "Invalid Path");
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

        async Task<string> GetWindowTitleAsync(CancellationToken cancellationToken)
        {
            var appName = await _settingsService.GetSettingAsync("ApplicationName", cancellationToken);
            return !string.IsNullOrWhiteSpace(appName)
                ? $"{appName} - Login"
                : $"{LabelProvider.Instance.ItemLabelSingular} Inventory Management - Login";
        }

        async Task LoadUsersAsync()
        {
            var users = await _userService.GetAllUsersAsync(CancellationToken.None);
            if (users.Count == 0)
            {
                var admin = new User
                {
                    UserName = "admin",
                    PasswordHash = PasswordDefaults.DefaultAdminPassword,
                    IsAdmin = true,
                    PasswordExpired = true
                };
                await _userService.AddUserAsync(admin);
                _logger.LogInformation("Created admin user {UserName}", admin.UserName);
                users = await _userService.GetAllUsersAsync(CancellationToken.None);
            }

            AssignInitialsBrushes(users);
            Users.ReplaceRange(users.Where(u => u.IsActive));
        }

        static string GetInitials(string? name)
        {
            var n = name?.Trim();
            if (string.IsNullOrEmpty(n)) return string.Empty;
            var parts = n.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return string.Empty;
            if (parts.Length == 1) return parts[0][0].ToString().ToUpperInvariant();
            return string.Concat(char.ToUpperInvariant(parts[0][0]), char.ToUpperInvariant(parts[^1][0]));
        }

        static void AssignInitialsBrushes(IList<User> users)
        {
            var palette = (Application.Current?.TryFindResource("UserInitialsBrushes") as IEnumerable<MediaBrush>)?.ToList()
                          ?? new List<MediaBrush>();
            var defaultBrush = Application.Current?.TryFindResource("ForegroundBrush") as MediaBrush
                               ?? MediaBrushes.Transparent;
            var groups = users.GroupBy(u => GetInitials(u.UserName));
            foreach (var group in groups)
            {
                if (group.Count() <= 1)
                {
                    foreach (var user in group)
                        user.InitialsBrush = defaultBrush;
                    continue;
                }

                var idx = 0;
                foreach (var user in group)
                {
                    user.InitialsBrush = palette.Count > 0 ? palette[idx % palette.Count] : MediaBrushes.Transparent;
                    idx++;
                }
            }
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
        async Task OnUserSelected(User? user, CancellationToken cancellationToken)
        {
            if (user == null) return;

            var dbUser = await _userService.GetUserByIDAsync(user.UserID, cancellationToken);
            if (dbUser == null) return;

            dbUser.InitialsBrush = user.InitialsBrush;
            if (dbUser.InitialsBrush == null)
                AssignInitialsBrushes(new[] { dbUser });

            user = dbUser;

            if (user.IsAdmin && string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                try
                {
                    await _userService.ChangeUserPasswordAsync(user.UserID, PasswordDefaults.TemporaryPassword);
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogWarning(ex, "Failed to set default password for admin user {UserID}", user.UserID);
                }

                var refreshed = await _userService.GetUserByIDAsync(user.UserID, cancellationToken);
                if (refreshed != null)
                {
                    user.PasswordHash = refreshed.PasswordHash;
                    user.PasswordSalt = refreshed.PasswordSalt;
                    user.PasswordExpired = refreshed.PasswordExpired;
                    user.FailedLoginAttempts = refreshed.FailedLoginAttempts;
                    user.LockoutEndUtc = refreshed.LockoutEndUtc;
                }
            }

            if (!user.IsAdmin && string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                await _dialogService.ShowInfoAsync("This account has no password. Please reset the password to continue.", "Password Required");
                if (await PromptChangePasswordAsync(user))
                    await _dialogService.ShowInfoAsync("Password has been set. Please log in with your new password.", "Password Updated");
                _userContext.CurrentUser = null;
                return;
            }

            if (user.IsLockedOut)
            {
                await LoadUsersCommand.ExecuteAsync(null);
                await _dialogService.ShowInfoAsync(GetLockoutMessage(user), "Account Locked");
                return;
            }

            if (!user.IsAdmin &&
                await SecurityHelper.VerifyPasswordAsync(PasswordDefaults.TemporaryPassword, user.PasswordSalt, user.PasswordHash).ConfigureAwait(false))
            {
                _userContext.CurrentUser = user;
                if (user.PasswordExpired && !await PromptChangePasswordAsync(user))
                {
                    _userContext.CurrentUser = null;
                    return;
                }
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
                return;
            }

            bool authenticated = false;
            string? lastPassword = null;
            while (!authenticated)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var promptResult = await (PromptForPasswordAsync?.Invoke(user, cancellationToken)
                    ?? Task.FromResult<PasswordPromptResult?>(null));
                if (promptResult == null)
                    return;
                lastPassword = promptResult.Password;

                if (promptResult.IsPasswordResetRequested)
                {
                    await _userService.ChangeUserPasswordAsync(user.UserID, PasswordDefaults.DefaultAdminPassword);
                    var refreshed = await _userService.GetUserByIDAsync(user.UserID, cancellationToken);
                    if (refreshed != null)
                    {
                        user.PasswordHash = refreshed.PasswordHash;
                        user.PasswordSalt = refreshed.PasswordSalt;
                        user.PasswordExpired = refreshed.PasswordExpired;
                        user.FailedLoginAttempts = refreshed.FailedLoginAttempts;
                        user.LockoutEndUtc = refreshed.LockoutEndUtc;
                    }
                    await LoadUsersCommand.ExecuteAsync(null);
                    await _dialogService.ShowInfoAsync(
                        $"Password has been reset to the temporary password \"{PasswordDefaults.TemporaryPassword}\". Please log in and change it.",
                        "Password Reset");
                    continue;
                }

                var authResult = await _userService.AuthenticateUserAsync(user.UserName, promptResult.Password);
                switch (authResult.Result)
                {
                    case AuthenticationResult.IncorrectPassword:
                        await _dialogService.ShowInfoAsync("Incorrect password. Please try again.", "Login Failed");
                        continue;
                    case AuthenticationResult.Inactive:
                        await _dialogService.ShowInfoAsync("User is inactive. Please contact an administrator.", "Login Failed");
                        return;
                    case AuthenticationResult.LockedOut:
                        if (authResult.User != null)
                        {
                            user.FailedLoginAttempts = authResult.User.FailedLoginAttempts;
                            user.LockoutEndUtc = authResult.User.LockoutEndUtc;
                        }
                        await LoadUsersCommand.ExecuteAsync(null);
                        await _dialogService.ShowInfoAsync(GetLockoutMessage(authResult.User ?? user), "Account Locked");
                        return;
                    case AuthenticationResult.Success:
                        if (authResult.User != null)
                        {
                            var defaultBrush = Application.Current?.TryFindResource("ForegroundBrush") as MediaBrush
                                               ?? MediaBrushes.Transparent;
                            authResult.User.InitialsBrush = user.InitialsBrush ?? defaultBrush;
                            if (authResult.User.InitialsBrush == null)
                                AssignInitialsBrushes(new[] { authResult.User });
                            user = authResult.User;
                        }
                        authenticated = true;
                        break;
                }
            }

            _userContext.CurrentUser = user;
            var requiresPasswordChange = user.PasswordExpired ||
                                         (user.IsAdmin && PasswordDefaults.IsDefaultPassword(lastPassword));
            if (requiresPasswordChange && !await PromptChangePasswordAsync(user))
            {
                _userContext.CurrentUser = null;
                return;
            }
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }

        static string GetLockoutMessage(User user)
        {
            if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > DateTime.UtcNow)
            {
                return $"This account is temporarily locked until {user.LockoutEndUtc.Value.ToLocalTime():g} after repeated failed password attempts. Please wait or ask an administrator to reset the password.";
            }

            return "This account is temporarily locked after repeated failed password attempts. Please wait or ask an administrator to reset the password.";
        }

        async Task<bool> PromptChangePasswordAsync(User user)
        {
            var newPwd = PromptForNewPassword?.Invoke();
            if (string.IsNullOrWhiteSpace(newPwd))
                return false;
            try
            {
                // Ensure the password change targets the selected user by
                // resetting the context regardless of any previous login.
                _userContext.CurrentUser = user;

                var updated = await _userService.ChangeUserPasswordAsync(user.UserID, newPwd);
                if (!updated)
                {
                    await _dialogService.ShowInfoAsync("Failed to update password.", "Error");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to change password for user {UserID}", user.UserID);
                await _dialogService.ShowInfoAsync("Failed to update password.", "Error");
                return false;
            }
            var refreshed = await _userService.GetUserByIDAsync(user.UserID, CancellationToken.None);
            if (refreshed != null)
            {
                user.PasswordHash = refreshed.PasswordHash;
                user.PasswordSalt = refreshed.PasswordSalt;
                user.PasswordExpired = refreshed.PasswordExpired;
                user.FailedLoginAttempts = refreshed.FailedLoginAttempts;
                user.LockoutEndUtc = refreshed.LockoutEndUtc;
            }
            await LoadUsersCommand.ExecuteAsync(null);
            return true;
        }
    }

    public record PasswordPromptResult(string Password, bool IsPasswordResetRequested);
}

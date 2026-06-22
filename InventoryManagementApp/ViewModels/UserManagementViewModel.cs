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
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Utilities.Extensions;
using InventoryManagementApp.Utilities.Helpers;
using InventoryManagementApp.Views.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Application = System.Windows.Application;
using MediaBrush = System.Windows.Media.Brush;
using MediaBrushes = System.Windows.Media.Brushes;

namespace InventoryManagementApp.ViewModels
{
    public class UserManagementViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly IFileDialogService _fileDialogService;
        private readonly IDialogService _dialogService;
        private readonly IUserContext? _userContext;
        private readonly ILogger<UserManagementViewModel> _logger;
        private readonly IServiceProvider? _serviceProvider;

        private List<UserModel> _allUsers = new();

        public ObservableCollection<UserModel> Users { get; } = new();

        private string _userSearchText = string.Empty;
        public string UserSearchText
        {
            get => _userSearchText;
            set => SetProperty(ref _userSearchText, value);
        }

        private UserModel? _selectedUser;
        public UserModel? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                {
                    ((AsyncRelayCommand)UpdateUserCommand).NotifyCanExecuteChanged();
                    ((AsyncRelayCommand)EditUserCommand).NotifyCanExecuteChanged();
                }
            }
        }

        public IAsyncRelayCommand LoadUsersCommand { get; }
        public IAsyncRelayCommand UploadUserPhotoCommand { get; }
        public IAsyncRelayCommand UpdateUserCommand { get; }
        public IAsyncRelayCommand AddUserCommand { get; }

        public IRelayCommand SearchUsersCommand { get; }
        public IRelayCommand ClearUserSearchCommand { get; }

        public IAsyncRelayCommand EditUserCommand { get; }
        public IAsyncRelayCommand<UserModel> EditUserFromRowCommand { get; }
        public IAsyncRelayCommand<UserModel> ResetPasswordFromRowCommand { get; }
        public IAsyncRelayCommand<UserModel> DeleteUserFromRowCommand { get; }

        public UserManagementViewModel(IUserService userService,
                                       IFileDialogService fileDialogService,
                                       IDialogService dialogService,
                                       IUserContext? userContext = null,
                                       ILogger<UserManagementViewModel>? logger = null,
                                       IServiceProvider? serviceProvider = null)
        {
            ArgumentNullException.ThrowIfNull(userService);
            ArgumentNullException.ThrowIfNull(fileDialogService);
            ArgumentNullException.ThrowIfNull(dialogService);
            _userService = userService;
            _fileDialogService = fileDialogService;
            _dialogService = dialogService;
            _userContext = userContext;
            _logger = logger ?? NullLogger<UserManagementViewModel>.Instance;
            _serviceProvider = serviceProvider;

            LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync);
            UploadUserPhotoCommand = new AsyncRelayCommand(UploadUserPhotoAsync);
            UpdateUserCommand = new AsyncRelayCommand(UpdateUserAsync, () => SelectedUser != null);
            AddUserCommand = new AsyncRelayCommand(AddUserAsync);

            SearchUsersCommand = new RelayCommand(SearchUsers);
            ClearUserSearchCommand = new RelayCommand(ClearUserSearch);

            EditUserCommand = new AsyncRelayCommand(() => EditUserAsync(SelectedUser), () => SelectedUser != null);
            EditUserFromRowCommand = new AsyncRelayCommand<UserModel>(EditUserAsync);
            ResetPasswordFromRowCommand = new AsyncRelayCommand<UserModel>(ResetPasswordFor);
            DeleteUserFromRowCommand = new AsyncRelayCommand<UserModel>(DeleteUserAsync);
        }

        public async Task LoadUsersAsync()
        {
            try
            {
                _allUsers = await _userService.GetAllUsersAsync(CancellationToken.None);
                AssignInitialsBrushes(_allUsers);
                Users.ReplaceRange(_allUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load users");
                ClearUsersAfterLoadFailure();
                await _dialogService.ShowInfoAsync($"Failed to load users: {ex.Message}. User rows were cleared until refresh succeeds.", "Error");
            }
        }

        private void ClearUsersAfterLoadFailure()
        {
            _allUsers.Clear();
            Users.Clear();
            SelectedUser = null;
            ((AsyncRelayCommand)UpdateUserCommand).NotifyCanExecuteChanged();
            ((AsyncRelayCommand)EditUserCommand).NotifyCanExecuteChanged();
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

        static void AssignInitialsBrushes(IList<UserModel> users)
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

        public async Task UploadUserPhotoAsync()
        {
            if (SelectedUser == null) return;

            var path = _fileDialogService.OpenFile("Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*");
            if (string.IsNullOrWhiteSpace(path))
            {
                await _dialogService.ShowInfoAsync("Selected file path is invalid.", "Invalid Path");
                return;
            }

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve path {Path}", path);
                await _dialogService.ShowInfoAsync("Selected file path is invalid.", "Invalid Path");
                return;
            }

            if (!File.Exists(fullPath))
            {
                await _dialogService.ShowInfoAsync("Selected file does not exist.", "Invalid Path");
                return;
            }

            var baseDir = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            if (!fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var destDir = Path.Combine(baseDir, "Assets", "UserPhotos");
                    Directory.CreateDirectory(destDir);
                    var destPath = Path.Combine(destDir, Path.GetFileName(fullPath));
                    File.Copy(fullPath, destPath, true);
                    fullPath = destPath;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to copy user photo from {Source}", fullPath);
                    await _dialogService.ShowInfoAsync("Failed to copy user photo.", "Error");
                    return;
                }
            }

            SelectedUser.UserPhotoPath = Path.GetRelativePath(baseDir, fullPath);

            try
            {
                await _userService.UpdateUserAsync(SelectedUser);
                var idxAll = _allUsers.IndexOf(SelectedUser);
                if (idxAll >= 0) _allUsers[idxAll] = SelectedUser;
                var idx = Users.IndexOf(SelectedUser);
                if (idx >= 0) Users[idx] = SelectedUser;
                if (_userContext?.CurrentUser?.UserID == SelectedUser.UserID)
                    _userContext.CurrentUser = SelectedUser;
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to update users.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user photo");
                await _dialogService.ShowInfoAsync($"Failed to update user photo: {ex.Message}", "Error");
            }
        }

        public async Task UpdateUserAsync()
        {
            if (SelectedUser == null) return;
            try
            {
                await _userService.UpdateUserAsync(SelectedUser);
                var idxAll = _allUsers.IndexOf(SelectedUser);
                if (idxAll >= 0) _allUsers[idxAll] = SelectedUser;
                var idx = Users.IndexOf(SelectedUser);
                if (idx >= 0) Users[idx] = SelectedUser;
                if (_userContext?.CurrentUser?.UserID == SelectedUser.UserID)
                    _userContext.CurrentUser = SelectedUser;
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to update users.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user");
                await _dialogService.ShowInfoAsync($"Failed to update user: {ex.Message}", "Error");
            }
        }

        public async Task AddUserAsync()
        {
            HashSet<string> existingNames;
            try
            {
                existingNames = new HashSet<string>(
                    (await _userService.GetAllUsersAsync(CancellationToken.None)).Select(u => u.UserName),
                    StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve existing users");
                return;
            }

            var idx = 1;
            string name;
            do { name = $"workshop{idx++}"; } while (existingNames.Contains(name));

            var newUser = new UserModel
            {
                UserName = name,
                Role = "Workshop Staff",
                IsAdmin = false,
                Permissions = User.BuildPermissions(User.DefaultUserPermissions)
            };

            if (!TryPromptForPassword(newUser, out var entered)) return;

            if (string.IsNullOrWhiteSpace(entered))
            {
                newUser.PasswordHash = PasswordDefaults.TemporaryPassword;
                newUser.PasswordSalt = string.Empty;
                newUser.PasswordExpired = true;
            }
            else
            {
                newUser.PasswordHash = entered;
                newUser.PasswordSalt = string.Empty;
            }

            try
            {
                await _userService.AddUserAsync(newUser);
                _allUsers.Add(newUser);
                Users.Add(newUser);
                SelectedUser = newUser;
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to add users.", "Unauthorized");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to add user because the password is invalid");
                await _dialogService.ShowInfoAsync(ex.Message, "Invalid Password");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add user");
                await _dialogService.ShowInfoAsync($"Failed to add user: {ex.Message}", "Error");
            }
        }

        protected virtual bool TryPromptForPassword(UserModel newUser, out string? password)
        {
            password = null;

            if (System.Windows.Application.Current != null)
            {
                try
                {
                    PasswordPromptWindow prompt;
                    if (_serviceProvider != null)
                        prompt = _serviceProvider.GetRequiredService<PasswordPromptWindow>();
                    else
                        prompt = new PasswordPromptWindow(_dialogService);
                    prompt.SelectedUser = newUser;
                    if (prompt.ShowDialog() == true)
                    {
                        password = prompt.EnteredPassword;
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to prompt for password");
                }
            }

            return true;
        }

        void SearchUsers()
        {
            IEnumerable<UserModel> filtered = _allUsers;
            if (!string.IsNullOrWhiteSpace(UserSearchText))
            {
                var term = UserSearchText.Trim();
                filtered = filtered.Where(u =>
                    (u.UserName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.Role?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.Email?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.Phone?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.Mobile?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.Address?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.AccessSummary?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
            }
            Users.ReplaceRange(filtered);
        }

        void ClearUserSearch()
        {
            UserSearchText = string.Empty;
            Users.ReplaceRange(_allUsers);
        }

        public async Task EditUserAsync(UserModel? user)
        {
            if (user == null) return;

            UserModel source = user;
            try
            {
                var loaded = await _userService.GetUserByIDAsync(user.UserID, CancellationToken.None);
                if (loaded != null)
                {
                    loaded.InitialsBrush = user.InitialsBrush;
                    source = loaded;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve full user record");
            }

            var clone = new UserModel
            {
                UserID = source.UserID,
                UserName = source.UserName,
                PasswordHash = source.PasswordHash,
                PasswordSalt = source.PasswordSalt,
                UserPhotoPath = source.UserPhotoPath,
                IsAdmin = source.IsAdmin,
                Email = source.Email,
                Phone = source.Phone,
                Mobile = source.Mobile,
                Address = source.Address,
                Role = source.Role,
                IsActive = source.IsActive,
                CreatedAt = source.CreatedAt,
                PasswordExpired = source.PasswordExpired,
                FailedLoginAttempts = source.FailedLoginAttempts,
                LockoutEndUtc = source.LockoutEndUtc,
                Permissions = source.Permissions,
                InitialsBrush = source.InitialsBrush
            };

            UsersEditWindow? win = null;
            win = new UsersEditWindow(clone,
                onSave: async () =>
                {
                    try
                    {
                        await _userService.UpdateUserAsync(clone);
                        var idx = Users.IndexOf(user);
                        if (idx >= 0) Users[idx] = clone;
                        var idxAll = _allUsers.IndexOf(user);
                        if (idxAll >= 0) _allUsers[idxAll] = clone;
                        AssignInitialsBrushes(_allUsers);
                        if (ReferenceEquals(SelectedUser, user)) SelectedUser = clone;
                        if (_userContext?.CurrentUser?.UserID == clone.UserID)
                            _userContext.CurrentUser = clone;
                        if (win != null)
                            win.DialogResult = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _dialogService.ShowInfo("You are not authorized to update users.", "Unauthorized");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update user");
                        _dialogService.ShowInfo($"Failed to update user: {ex.Message}", "Error");
                    }
                },
                onCancel: () =>
                {
                    if (win != null)
                        win.Close();
                },
                fileDialogService: _fileDialogService);

            try { win.Owner = System.Windows.Application.Current?.MainWindow; } catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for UsersEditWindow"); }
            try { win.ShowDialog(); } catch (Exception ex) { _logger.LogError(ex, "Failed to show UsersEditWindow"); }
        }

        async Task ResetPasswordFor(UserModel? user)
        {
            if (user == null) return;
            var newPassword = PasswordDefaults.TemporaryPassword;
            try
            {
                var changed = await _userService.ChangeUserPasswordAsync(user.UserID, newPassword);
                if (!changed)
                {
                    await _dialogService.ShowInfoAsync("Failed to reset password.", "Error");
                    return;
                }

                var refreshed = await _userService.GetUserByIDAsync(user.UserID, CancellationToken.None);
                if (refreshed != null)
                {
                    refreshed.PasswordExpired = true;
                    await _userService.UpdateUserAsync(refreshed);
                    user.PasswordHash = refreshed.PasswordHash;
                    user.PasswordSalt = refreshed.PasswordSalt;
                    user.PasswordExpired = true;
                    user.FailedLoginAttempts = refreshed.FailedLoginAttempts;
                    user.LockoutEndUtc = refreshed.LockoutEndUtc;
                    user.Permissions = refreshed.Permissions;
                }
                await _dialogService.ShowInfoAsync(
                    $"Password has been reset to \"{newPassword}\". The user must change it at next login.",
                    "Password Reset");
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to reset passwords.", "Unauthorized");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to reset password because the default password is invalid");
                await _dialogService.ShowInfoAsync(ex.Message, "Invalid Password");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset password for user {UserID}", user.UserID);
                await _dialogService.ShowInfoAsync($"Failed to reset password: {ex.Message}", "Error");
            }
        }

        async Task DeleteUserAsync(UserModel? user)
        {
            if (user == null) return;
            try
            {
                var deleted = await _userService.TryDeleteUserAsync(user.UserID);
                if (deleted)
                {
                    _allUsers.Remove(user);
                    Users.Remove(user);
                    if (ReferenceEquals(SelectedUser, user)) SelectedUser = null;
                }
                else
                {
                    _logger.LogWarning("Failed to delete user {UserID}", user.UserID);
                    await _dialogService.ShowInfoAsync("Failed to delete user.", "Error");
                }
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to delete users.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserID}", user.UserID);
                await _dialogService.ShowInfoAsync($"Failed to delete user: {ex.Message}", "Error");
            }
        }
    }
}

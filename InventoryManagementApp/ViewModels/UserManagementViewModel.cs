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
        public const int MaxVisibleUserRows = 500;

        private readonly IUserService _userService;
        private readonly IFileDialogService _fileDialogService;
        private readonly IDialogService _dialogService;
        private readonly IUserContext? _userContext;
        private readonly ILogger<UserManagementViewModel> _logger;
        private readonly IServiceProvider? _serviceProvider;

        private List<UserModel> _allUsers = new();
        private int _matchedUserCount;
        private bool _hasLoadedUsers;

        public ObservableCollection<UserModel> Users { get; } = new();

        private bool _isLoadingUsers;
        public bool IsLoadingUsers
        {
            get => _isLoadingUsers;
            private set
            {
                if (SetProperty(ref _isLoadingUsers, value))
                {
                    NotifyUserDirectoryStateChanged();
                    NotifyUserCommandStatesChanged();
                }
            }
        }

        private string _userSearchText = string.Empty;
        public string UserSearchText
        {
            get => _userSearchText;
            set
            {
                if (SetProperty(ref _userSearchText, value))
                    NotifyUserDirectoryStateChanged();
            }
        }

        private UserModel? _selectedUser;
        public UserModel? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                {
                    NotifySelectedUserStateChanged();
                    NotifyUserCommandStatesChanged();
                }
            }
        }

        public int TotalUserCount => _allUsers.Count;
        public int MatchedUserCount => _matchedUserCount;
        public int VisibleUserCount => Users.Count;
        public int OmittedUserCount => Math.Max(0, MatchedUserCount - VisibleUserCount);
        public bool HasUserFilter => !string.IsNullOrWhiteSpace(UserSearchText);
        public bool IsUserWindowLimited => OmittedUserCount > 0;
        public bool CanUseUserActions => !IsLoadingUsers;
        public bool CanUseSelectedUserActions => !IsLoadingUsers && SelectedUser != null;
        public bool CanPrintUsers => !IsLoadingUsers && Users.Count > 0;

        public string UserDirectoryStatusText
        {
            get
            {
                if (IsLoadingUsers)
                    return Users.Count > 0
                        ? $"Refreshing account directory - keeping {Users.Count} current rows visible"
                        : "Loading account directory";

                if (MatchedUserCount == 0)
                    return HasUserFilter
                        ? $"No users match \"{UserSearchText.Trim()}\""
                        : "No user accounts are available";

                if (IsUserWindowLimited)
                    return $"Admin desk ready - showing first {VisibleUserCount} of {MatchedUserCount} matching accounts ({OmittedUserCount} hidden from the live grid)";

                return $"Admin desk ready - {VisibleUserCount} visible of {TotalUserCount} accounts";
            }
        }

        public string UserFilterStatusText
        {
            get
            {
                if (IsLoadingUsers)
                    return "Search pauses until account rows finish loading";

                if (HasUserFilter)
                    return IsUserWindowLimited
                        ? $"Filter: {UserSearchText.Trim()} - {MatchedUserCount} matches, first {VisibleUserCount} shown"
                        : $"Filter: {UserSearchText.Trim()} - {MatchedUserCount} matches";

                return IsUserWindowLimited
                    ? $"All accounts - first {VisibleUserCount} of {MatchedUserCount} shown"
                    : "All accounts";
            }
        }

        public string UserWindowStatusText =>
            IsUserWindowLimited
                ? $"Showing first {VisibleUserCount} accounts to keep the grid responsive; refine search to reach {OmittedUserCount} more matches."
                : "All matching accounts are visible in the live grid.";

        public string SelectedAccessStatusText =>
            IsLoadingUsers
                ? "Account rows are refreshing"
                : SelectedUser?.AccessSummary ?? "No account selected";

        public string SelectedSecurityStatusText =>
            IsLoadingUsers
                ? "Security state loading"
                : SelectedUser?.LockoutStatus ?? "Select a user";

        public string UserEmptyStateTitle =>
            IsLoadingUsers
                ? "Loading users"
                : HasUserFilter ? "No users match this filter" : "No users are available";

        public string UserEmptyStateMessage =>
            IsLoadingUsers
                ? "Account rows are being prepared. Existing rows stay visible when available."
                : HasUserFilter
                    ? "Clear the filter or search another user, role, contact detail, access area, status, or user ID."
                    : "Add a new account before assigning app access, resetting passwords, or printing directory evidence.";

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

            LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync, () => !IsLoadingUsers);
            UploadUserPhotoCommand = new AsyncRelayCommand(UploadUserPhotoAsync, () => CanUseSelectedUserActions);
            UpdateUserCommand = new AsyncRelayCommand(UpdateUserAsync, () => CanUseSelectedUserActions);
            AddUserCommand = new AsyncRelayCommand(AddUserAsync, () => CanUseUserActions);

            SearchUsersCommand = new RelayCommand(SearchUsers, () => CanUseUserActions);
            ClearUserSearchCommand = new RelayCommand(ClearUserSearch, () => CanUseUserActions);

            EditUserCommand = new AsyncRelayCommand(() => EditUserAsync(SelectedUser), () => CanUseSelectedUserActions);
            EditUserFromRowCommand = new AsyncRelayCommand<UserModel>(EditUserAsync, user => CanUseUserActions && user != null);
            ResetPasswordFromRowCommand = new AsyncRelayCommand<UserModel>(ResetPasswordFor, user => CanUseUserActions && user != null);
            DeleteUserFromRowCommand = new AsyncRelayCommand<UserModel>(DeleteUserAsync, user => CanUseUserActions && user != null);
        }

        public async Task LoadUsersAsync()
        {
            if (IsLoadingUsers)
                return;

            IsLoadingUsers = true;

            try
            {
                ApplyUserRows(await _userService.GetAllUsersAsync(CancellationToken.None));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load users");
                ClearUsersAfterLoadFailure();
                await _dialogService.ShowInfoAsync($"Failed to load users: {ex.Message}. User rows were cleared until refresh succeeds.", "Error");
            }
            finally
            {
                IsLoadingUsers = false;
            }
        }

        private void ApplyUserRows(IEnumerable<UserModel> users)
        {
            _allUsers = users
                .OrderByDescending(user => user.IsActive)
                .ThenBy(user => user.UserName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(user => user.UserID)
                .ToList();
            _hasLoadedUsers = true;

            AssignInitialsBrushes(_allUsers);
            ApplyFilteredUserRows();
        }

        private void ApplyFilteredUserRows()
        {
            var matchedUsers = FilterUsers(_allUsers).ToList();
            var visibleUsers = matchedUsers.Take(MaxVisibleUserRows).ToList();
            _matchedUserCount = matchedUsers.Count;

            if (!AreSameVisibleRows(visibleUsers))
                Users.ReplaceRange(visibleUsers);

            if (SelectedUser != null && !Users.Any(user => user.UserID == SelectedUser.UserID))
                SelectedUser = null;

            NotifyUserDirectoryStateChanged();
            NotifyUserCommandStatesChanged();
        }

        private bool AreSameVisibleRows(IReadOnlyList<UserModel> visibleUsers)
        {
            if (Users.Count != visibleUsers.Count)
                return false;

            for (var i = 0; i < visibleUsers.Count; i++)
            {
                if (!ReferenceEquals(Users[i], visibleUsers[i]))
                    return false;
            }

            return true;
        }

        private void ClearUsersAfterLoadFailure()
        {
            _allUsers.Clear();
            _matchedUserCount = 0;
            _hasLoadedUsers = false;
            Users.Clear();
            SelectedUser = null;
            NotifyUserDirectoryStateChanged();
            NotifyUserCommandStatesChanged();
        }

        private async Task<bool> RefreshUsersAfterMutationFailureAsync(int? preferredUserId, bool clearSelectionWhenMissing)
        {
            try
            {
                ApplyUserRows(await _userService.GetAllUsersAsync(CancellationToken.None));

                var refreshedSelection = preferredUserId.HasValue
                    ? Users.FirstOrDefault(user => user.UserID == preferredUserId.Value)
                    : null;

                if (refreshedSelection != null || clearSelectionWhenMissing)
                {
                    SelectedUser = refreshedSelection;
                }
                else if (SelectedUser != null)
                {
                    SelectedUser = Users.FirstOrDefault(user => user.UserID == SelectedUser.UserID);
                }

                return true;
            }
            catch (Exception refreshEx)
            {
                _logger.LogError(refreshEx, "Failed to refresh users after mutation failure");
                ClearUsersAfterLoadFailure();
                return false;
            }
        }

        private static string BuildMutationFailureMessage(string baseMessage, Exception ex, bool refreshed)
        {
            var recoveryMessage = refreshed
                ? "User rows were refreshed from saved data."
                : "User rows were cleared because the recovery refresh failed.";

            return $"{baseMessage}: {ex.Message}. {recoveryMessage}";
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
            if (!CanUseSelectedUserActions || SelectedUser == null) return;

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

            try
            {
                SelectedUser.UserPhotoPath = AppAssetHelper.CopyImageToAssetFolder(fullPath, AppAssetHelper.UserPhotosFolder, SelectedUser.UserName);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException || ex is InvalidOperationException || ex is UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Failed to copy user photo from {Source}", fullPath);
                await _dialogService.ShowInfoAsync("Failed to copy user photo.", "Error");
                return;
            }

            try
            {
                await _userService.UpdateUserAsync(SelectedUser);
                var idxAll = _allUsers.IndexOf(SelectedUser);
                if (idxAll >= 0) _allUsers[idxAll] = SelectedUser;
                ApplyFilteredUserRows();
                if (_userContext?.CurrentUser?.UserID == SelectedUser.UserID)
                    _userContext.CurrentUser = SelectedUser;
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to update users.", "Unauthorized");
            }
            catch (Exception ex)
            {
                var refreshed = await RefreshUsersAfterMutationFailureAsync(SelectedUser.UserID, clearSelectionWhenMissing: true);
                _logger.LogError(ex, "Failed to update user photo");
                await _dialogService.ShowInfoAsync(BuildMutationFailureMessage("Failed to update user photo", ex, refreshed), "Error");
            }
        }

        public async Task UpdateUserAsync()
        {
            if (!CanUseSelectedUserActions || SelectedUser == null) return;
            try
            {
                await _userService.UpdateUserAsync(SelectedUser);
                var idxAll = _allUsers.IndexOf(SelectedUser);
                if (idxAll >= 0) _allUsers[idxAll] = SelectedUser;
                ApplyFilteredUserRows();
                if (_userContext?.CurrentUser?.UserID == SelectedUser.UserID)
                    _userContext.CurrentUser = SelectedUser;
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to update users.", "Unauthorized");
            }
            catch (Exception ex)
            {
                var refreshed = await RefreshUsersAfterMutationFailureAsync(SelectedUser.UserID, clearSelectionWhenMissing: true);
                _logger.LogError(ex, "Failed to update user");
                await _dialogService.ShowInfoAsync(BuildMutationFailureMessage("Failed to update user", ex, refreshed), "Error");
            }
        }

        public async Task AddUserAsync()
        {
            if (!CanUseUserActions) return;

            HashSet<string> existingNames;
            try
            {
                var existingUsers = _hasLoadedUsers
                    ? _allUsers
                    : await _userService.GetAllUsersAsync(CancellationToken.None);

                existingNames = new HashSet<string>(
                    existingUsers.Select(u => u.UserName),
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
                UserSearchText = string.Empty;
                ApplyUserRows(_allUsers);
                SelectedUser = Users.FirstOrDefault(user => ReferenceEquals(user, newUser))
                    ?? Users.FirstOrDefault(user => user.UserID == newUser.UserID)
                    ?? newUser;
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
                var refreshed = await RefreshUsersAfterMutationFailureAsync(newUser.UserID, clearSelectionWhenMissing: false);
                _logger.LogError(ex, "Failed to add user");
                await _dialogService.ShowInfoAsync(BuildMutationFailureMessage("Failed to add user", ex, refreshed), "Error");
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
            if (!CanUseUserActions) return;
            ApplyFilteredUserRows();
        }

        private IEnumerable<UserModel> FilterUsers(IEnumerable<UserModel> users)
        {
            if (string.IsNullOrWhiteSpace(UserSearchText))
            {
                return users;
            }

            var term = UserSearchText.Trim();
            return users.Where(u =>
                u.UserID.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (u.UserName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.Role?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.Email?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.Phone?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.Mobile?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.Address?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.LockoutStatus?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.AccessSummary?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        void ClearUserSearch()
        {
            if (!CanUseUserActions) return;
            UserSearchText = string.Empty;
            ApplyFilteredUserRows();
        }

        public async Task EditUserAsync(UserModel? user)
        {
            if (!CanUseUserActions || user == null) return;

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
                        var idxAll = _allUsers.IndexOf(user);
                        if (idxAll >= 0) _allUsers[idxAll] = clone;
                        AssignInitialsBrushes(_allUsers);
                        ApplyFilteredUserRows();
                        if (Users.Any(visibleUser => visibleUser.UserID == clone.UserID))
                            SelectedUser = clone;
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
                        var refreshed = await RefreshUsersAfterMutationFailureAsync(clone.UserID, clearSelectionWhenMissing: true);
                        _logger.LogError(ex, "Failed to update user");
                        _dialogService.ShowInfo(BuildMutationFailureMessage("Failed to update user", ex, refreshed), "Error");
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
            if (!CanUseUserActions || user == null) return;
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
                    NotifySelectedUserStateChanged();
                    NotifyUserDirectoryStateChanged();
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
                var refreshed = await RefreshUsersAfterMutationFailureAsync(user.UserID, clearSelectionWhenMissing: true);
                _logger.LogError(ex, "Failed to reset password for user {UserID}", user.UserID);
                await _dialogService.ShowInfoAsync(BuildMutationFailureMessage("Failed to reset password", ex, refreshed), "Error");
            }
        }

        async Task DeleteUserAsync(UserModel? user)
        {
            if (!CanUseUserActions || user == null) return;
            try
            {
                var deleted = await _userService.TryDeleteUserAsync(user.UserID);
                if (deleted)
                {
                    _allUsers.RemoveAll(existing => existing.UserID == user.UserID);
                    if (ReferenceEquals(SelectedUser, user)) SelectedUser = null;
                    ApplyFilteredUserRows();
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
                var refreshed = await RefreshUsersAfterMutationFailureAsync(user.UserID, clearSelectionWhenMissing: true);
                _logger.LogError(ex, "Failed to delete user {UserID}", user.UserID);
                await _dialogService.ShowInfoAsync(BuildMutationFailureMessage("Failed to delete user", ex, refreshed), "Error");
            }
        }

        private void NotifyUserDirectoryStateChanged()
        {
            OnPropertyChanged(nameof(TotalUserCount));
            OnPropertyChanged(nameof(MatchedUserCount));
            OnPropertyChanged(nameof(VisibleUserCount));
            OnPropertyChanged(nameof(OmittedUserCount));
            OnPropertyChanged(nameof(HasUserFilter));
            OnPropertyChanged(nameof(IsUserWindowLimited));
            OnPropertyChanged(nameof(CanUseUserActions));
            OnPropertyChanged(nameof(CanUseSelectedUserActions));
            OnPropertyChanged(nameof(CanPrintUsers));
            OnPropertyChanged(nameof(UserDirectoryStatusText));
            OnPropertyChanged(nameof(UserFilterStatusText));
            OnPropertyChanged(nameof(UserWindowStatusText));
            OnPropertyChanged(nameof(UserEmptyStateTitle));
            OnPropertyChanged(nameof(UserEmptyStateMessage));
            OnPropertyChanged(nameof(SelectedAccessStatusText));
            OnPropertyChanged(nameof(SelectedSecurityStatusText));
        }

        private void NotifySelectedUserStateChanged()
        {
            OnPropertyChanged(nameof(CanUseSelectedUserActions));
            OnPropertyChanged(nameof(SelectedAccessStatusText));
            OnPropertyChanged(nameof(SelectedSecurityStatusText));
        }

        private void NotifyUserCommandStatesChanged()
        {
            LoadUsersCommand.NotifyCanExecuteChanged();
            UploadUserPhotoCommand.NotifyCanExecuteChanged();
            UpdateUserCommand.NotifyCanExecuteChanged();
            AddUserCommand.NotifyCanExecuteChanged();
            SearchUsersCommand.NotifyCanExecuteChanged();
            ClearUserSearchCommand.NotifyCanExecuteChanged();
            EditUserCommand.NotifyCanExecuteChanged();
            EditUserFromRowCommand.NotifyCanExecuteChanged();
            ResetPasswordFromRowCommand.NotifyCanExecuteChanged();
            DeleteUserFromRowCommand.NotifyCanExecuteChanged();
        }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Utilities.Extensions;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ToolManagementAppV2.ViewModels
{
    public class UserManagementViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly IFileDialogService _fileDialogService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<UserManagementViewModel> _logger;

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
                    ((RelayCommand)EditUserCommand).NotifyCanExecuteChanged();
                }
            }
        }

        public IAsyncRelayCommand LoadUsersCommand { get; }
        public IAsyncRelayCommand UploadUserPhotoCommand { get; }
        public IAsyncRelayCommand UpdateUserCommand { get; }
        public IAsyncRelayCommand AddUserCommand { get; }

        public IRelayCommand SearchUsersCommand { get; }
        public IRelayCommand ClearUserSearchCommand { get; }

        public IRelayCommand EditUserCommand { get; }
        public IRelayCommand EditUserFromRowCommand { get; }
        public IAsyncRelayCommand<UserModel> ResetPasswordFromRowCommand { get; }
        public IAsyncRelayCommand<UserModel> DeleteUserFromRowCommand { get; }

        public UserManagementViewModel(IUserService userService,
                                       IFileDialogService fileDialogService,
                                       IDialogService dialogService,
                                       ILogger<UserManagementViewModel>? logger = null)
        {
            _userService = userService;
            _fileDialogService = fileDialogService;
            _dialogService = dialogService;
            _logger = logger ?? NullLogger<UserManagementViewModel>.Instance;

            LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync);
            UploadUserPhotoCommand = new AsyncRelayCommand(UploadUserPhotoAsync);
            UpdateUserCommand = new AsyncRelayCommand(UpdateUserAsync, () => SelectedUser != null);
            AddUserCommand = new AsyncRelayCommand(AddUserAsync);

            SearchUsersCommand = new RelayCommand(SearchUsers);
            ClearUserSearchCommand = new RelayCommand(ClearUserSearch);

            EditUserCommand = new RelayCommand(() => EditUser(SelectedUser!), () => SelectedUser != null);
            EditUserFromRowCommand = new RelayCommand<UserModel>(EditUser);
            ResetPasswordFromRowCommand = new AsyncRelayCommand<UserModel>(ResetPasswordFor);
            DeleteUserFromRowCommand = new AsyncRelayCommand<UserModel>(DeleteUserAsync);
        }

        public async Task LoadUsersAsync()
        {
            try
            {
                _allUsers = await _userService.GetAllUsersAsync();
                Users.ReplaceRange(_allUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load users");
            }
        }

        public async Task UploadUserPhotoAsync()
        {
            if (SelectedUser == null) return;
            var path = _fileDialogService.OpenFile("Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*");
            var full = PathHelper.GetAbsolutePath(path);
            if (string.IsNullOrEmpty(full))
            {
                await _dialogService.ShowInfoAsync("Selected file path is invalid.", "Invalid Path");
                return;
            }

            SelectedUser.UserPhotoPath = full;
            try
            {
                await _userService.UpdateUserAsync(SelectedUser);
                var idxAll = _allUsers.IndexOf(SelectedUser);
                if (idxAll >= 0) _allUsers[idxAll] = SelectedUser;
                var idx = Users.IndexOf(SelectedUser);
                if (idx >= 0) Users[idx] = SelectedUser;
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
                    (await _userService.GetAllUsersAsync()).Select(u => u.UserName),
                    StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve existing users");
                return;
            }

            var idx = 1;
            string name;
            do { name = $"user{idx++}"; } while (existingNames.Contains(name));

            var newUser = new UserModel { UserName = name };

            if (!TryPromptForPassword(newUser, out var entered)) return;

            if (string.IsNullOrWhiteSpace(entered))
            {
                const string defaultPwd = "changeme";
                newUser.Password = SecurityHelper.HashPassword(defaultPwd, out var salt);
                newUser.Salt = salt;
                newUser.PasswordExpired = true;
            }
            else
            {
                newUser.Password = entered;
            }

            try
            {
                await _userService.AddUserAsync(newUser);
                _allUsers.Add(newUser);
                Users.Add(newUser);
                SelectedUser = newUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add user");
            }
        }

        protected virtual bool TryPromptForPassword(UserModel newUser, out string? password)
        {
            password = null;

            if (System.Windows.Application.Current != null)
            {
                try
                {
                    var prompt = new PasswordPromptWindow(_dialogService) { SelectedUser = newUser };
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
                    (u.Email?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.Phone?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.Mobile?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
            }
            Users.ReplaceRange(filtered);
        }

        void ClearUserSearch()
        {
            UserSearchText = string.Empty;
            Users.ReplaceRange(_allUsers);
        }

        void EditUser(UserModel? user)
        {
            if (user == null) return;

            var clone = new UserModel
            {
                UserID = user.UserID,
                UserName = user.UserName,
                Password = user.Password,
                Salt = user.Salt,
                UserPhotoPath = user.UserPhotoPath,
                IsAdmin = user.IsAdmin,
                Email = user.Email,
                Phone = user.Phone,
                Mobile = user.Mobile,
                Address = user.Address,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
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
                        if (ReferenceEquals(SelectedUser, user)) SelectedUser = clone;
                        win.DialogResult = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update user");
                        _dialogService.ShowInfo($"Failed to update user: {ex.Message}", "Error");
                    }
                },
                onCancel: () => win.Close(),
                onRemoveAvatar: () => clone.UserPhotoPath = null);

            try { win.Owner = System.Windows.Application.Current?.MainWindow; } catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for UsersEditWindow"); }
            try { win.ShowDialog(); } catch (Exception ex) { _logger.LogError(ex, "Failed to show UsersEditWindow"); }
        }

        async Task ResetPasswordFor(UserModel user)
        {
            if (user == null) return;
            var newPassword = SecurityHelper.GeneratePassword();
            await _userService.ChangeUserPasswordAsync(user.UserID, newPassword);
            var refreshed = await _userService.GetUserByIDAsync(user.UserID);
            if (refreshed != null)
            {
                refreshed.PasswordExpired = true;
                await _userService.UpdateUserAsync(refreshed);
                user.Password = refreshed.Password;
                user.Salt = refreshed.Salt;
                user.PasswordExpired = true;
            }
            _dialogService.ShowInfo("Password has been reset. The user must change it at next login.", "Password Reset");
        }

        async Task DeleteUserAsync(UserModel user)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserID}", user.UserID);
            }
        }
    }
}

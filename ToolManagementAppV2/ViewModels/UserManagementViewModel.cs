using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ToolManagementAppV2.Interfaces;
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

        private UserModel _selectedUser;
        public UserModel SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                {
                    ((RelayCommand)UpdateUserCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)EditUserCommand).NotifyCanExecuteChanged();
                }
            }
        }

        public IRelayCommand LoadUsersCommand { get; }
        public IRelayCommand UploadUserPhotoCommand { get; }
        public IRelayCommand UpdateUserCommand { get; }
        public IRelayCommand AddUserCommand { get; }

        public IRelayCommand SearchUsersCommand { get; }
        public IRelayCommand ClearUserSearchCommand { get; }

        public IRelayCommand EditUserCommand { get; }
        public IRelayCommand EditUserFromRowCommand { get; }
        public IRelayCommand ResetPasswordFromRowCommand { get; }
        public IRelayCommand DeleteUserFromRowCommand { get; }

        public UserManagementViewModel(IUserService userService,
                                       IFileDialogService fileDialogService,
                                       IDialogService dialogService,
                                       ILogger<UserManagementViewModel>? logger = null)
        {
            _userService = userService;
            _fileDialogService = fileDialogService;
            _dialogService = dialogService;
            _logger = logger ?? NullLogger<UserManagementViewModel>.Instance;
            LoadUsersCommand = new RelayCommand(LoadUsers);
            UploadUserPhotoCommand = new RelayCommand(UploadUserPhoto);
            UpdateUserCommand = new RelayCommand(UpdateUser, () => SelectedUser != null);
            AddUserCommand = new RelayCommand(AddUser);

            SearchUsersCommand = new RelayCommand(SearchUsers);
            ClearUserSearchCommand = new RelayCommand(ClearUserSearch);

            EditUserCommand = new RelayCommand(() => EditUser(SelectedUser), () => SelectedUser != null);
            EditUserFromRowCommand = new RelayCommand<UserModel>(EditUser);
            ResetPasswordFromRowCommand = new RelayCommand<UserModel>(ResetPasswordFor);
            DeleteUserFromRowCommand = new RelayCommand<UserModel>(DeleteUser);
        }

        public void LoadUsers()
        {
            _allUsers = _userService.GetAllUsers();
            Users.ReplaceRange(_allUsers);
        }

        public void UploadUserPhoto()
        {
            if (SelectedUser == null) return;
            var path = _fileDialogService.OpenFile("Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                SelectedUser.UserPhotoPath = path;
                _userService.UpdateUser(SelectedUser);
                var idxAll = _allUsers.IndexOf(SelectedUser);
                if (idxAll >= 0) _allUsers[idxAll] = SelectedUser;
                var idx = Users.IndexOf(SelectedUser);
                if (idx >= 0) Users[idx] = SelectedUser;
            }
        }

        public void UpdateUser()
        {
            if (SelectedUser == null) return;
            _userService.UpdateUser(SelectedUser);
            var idxAll = _allUsers.IndexOf(SelectedUser);
            if (idxAll >= 0) _allUsers[idxAll] = SelectedUser;
            var idx = Users.IndexOf(SelectedUser);
            if (idx >= 0) Users[idx] = SelectedUser;
        }

        public void AddUser()
        {
            var newUser = new UserModel { UserName = $"user{Users.Count + 1}" };

            if (!TryPromptForPassword(newUser, out var entered))
                return;

            // If the user leaves the prompt blank, assign a hashed "changeme" password
            // so the account is initialized with a known placeholder that must be changed.
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

            _userService.AddUser(newUser);
            _allUsers.Add(newUser);
            Users.Add(newUser);
            SelectedUser = newUser;
        }

        protected virtual bool TryPromptForPassword(UserModel newUser, out string password)
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

        void EditUser(UserModel user)
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
                onSave: () =>
                {
                    _userService.UpdateUser(clone);
                    var idx = Users.IndexOf(user);
                    if (idx >= 0) Users[idx] = clone;
                    var idxAll = _allUsers.IndexOf(user);
                    if (idxAll >= 0) _allUsers[idxAll] = clone;
                    if (ReferenceEquals(SelectedUser, user)) SelectedUser = clone;
                    win.DialogResult = true;
                },
                onCancel: () => win.Close(),
                onRemoveAvatar: () => clone.UserPhotoPath = null);

            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for UsersEditWindow"); }
            try { win.ShowDialog(); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show UsersEditWindow"); }
        }

        void ResetPasswordFor(UserModel user)
        {
            if (user == null) return;
            var newPassword = SecurityHelper.GeneratePassword();
            _userService.ChangeUserPassword(user.UserID, newPassword);
            _dialogService.ShowInfo($"Password reset to: {newPassword}", "Password Reset");
            var refreshed = _userService.GetUserByID(user.UserID);
            if (refreshed != null)
            {
                user.Password = refreshed.Password;
                user.Salt = refreshed.Salt;
            }
        }

        void DeleteUser(UserModel user)
        {
            if (user == null) return;
            if (_userService.TryDeleteUser(user.UserID))
            {
                _allUsers.Remove(user);
                Users.Remove(user);
                if (ReferenceEquals(SelectedUser, user)) SelectedUser = null;
            }
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Extensions;
using ToolManagementAppV2.Views;

namespace ToolManagementAppV2.ViewModels
{
    public class UserManagementViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly IFileDialogService _fileDialogService;

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
                    ((RelayCommand)DeleteUserCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)ResetPasswordCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)EditUserCommand).NotifyCanExecuteChanged();
                }
            }
        }

        public IRelayCommand LoadUsersCommand { get; }
        public IRelayCommand UploadUserPhotoCommand { get; }
        public IRelayCommand UpdateUserCommand { get; }
        public IRelayCommand DeleteUserCommand { get; }
        public IRelayCommand AddUserCommand { get; }
        public IRelayCommand ResetPasswordCommand { get; }

        public IRelayCommand SearchUsersCommand { get; }
        public IRelayCommand ClearUserSearchCommand { get; }

        public IRelayCommand EditUserCommand { get; }
        public IRelayCommand EditUserFromRowCommand { get; }
        public IRelayCommand ResetPasswordFromRowCommand { get; }
        public IRelayCommand DeleteUserFromRowCommand { get; }

        public UserManagementViewModel(IUserService userService, IFileDialogService fileDialogService)
        {
            _userService = userService;
            _fileDialogService = fileDialogService;
            LoadUsersCommand = new RelayCommand(LoadUsers);
            UploadUserPhotoCommand = new RelayCommand(UploadUserPhoto);
            UpdateUserCommand = new RelayCommand(UpdateUser, () => SelectedUser != null);
            DeleteUserCommand = new RelayCommand(DeleteSelectedUser, () => SelectedUser != null);
            AddUserCommand = new RelayCommand(AddUser);
            ResetPasswordCommand = new RelayCommand(ResetPassword, () => SelectedUser != null);

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
            }
        }

        public void UpdateUser()
        {
            if (SelectedUser == null) return;
            _userService.UpdateUser(SelectedUser);
        }

        void DeleteSelectedUser()
        {
            if (SelectedUser == null) return;
            DeleteUser(SelectedUser);
        }

        public void AddUser()
        {
            var newUser = new UserModel { UserName = $"user{Users.Count + 1}" };

            if (System.Windows.Application.Current != null)
            {
                try
                {
                    var prompt = new Views.PasswordPromptWindow { SelectedUser = newUser };
                    if (prompt.ShowDialog() == true)
                        newUser.Password = prompt.EnteredPassword;
                }
                catch
                {
                    // Ignore UI errors in non-interactive environments
                }
            }

            _userService.AddUser(newUser);
            _allUsers.Add(newUser);
            Users.Add(newUser);
            SelectedUser = newUser;
        }

        void ResetPassword()
        {
            if (SelectedUser == null) return;
            ResetPasswordFor(SelectedUser);
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
                onBrowseAvatar: () =>
                {
                    var path = _fileDialogService.OpenFile("Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*");
                    if (!string.IsNullOrEmpty(path)) clone.UserPhotoPath = path;
                },
                onRemoveAvatar: () => clone.UserPhotoPath = null);

            try { win.Owner = System.Windows.Application.Current?.MainWindow; } catch { }
            try { win.ShowDialog(); } catch { }
        }

        void ResetPasswordFor(UserModel user)
        {
            if (user == null) return;
            _userService.ChangeUserPassword(user.UserID, "admin");
            var refreshed = _userService.GetUserByID(user.UserID);
            user.Password = refreshed.Password;
            user.Salt = refreshed.Salt;
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

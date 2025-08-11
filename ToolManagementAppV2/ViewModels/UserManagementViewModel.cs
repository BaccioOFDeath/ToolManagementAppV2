using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.ViewModels
{
    public class UserManagementViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly IFileDialogService _fileDialogService;

        public ObservableCollection<UserModel> Users { get; } = new();

        private UserModel _selectedUser;
        public UserModel SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public IRelayCommand LoadUsersCommand { get; }
        public IRelayCommand UploadUserPhotoCommand { get; }
        public IRelayCommand UpdateUserCommand { get; }
        public IRelayCommand DeleteUserCommand { get; }

        public UserManagementViewModel(IUserService userService, IFileDialogService fileDialogService)
        {
            _userService = userService;
            _fileDialogService = fileDialogService;
            LoadUsersCommand = new RelayCommand(LoadUsers);
            UploadUserPhotoCommand = new RelayCommand(UploadUserPhoto);
            UpdateUserCommand = new RelayCommand(UpdateUser);
            DeleteUserCommand = new RelayCommand(DeleteUser);
        }

        public void LoadUsers()
        {
            var all = _userService.GetAllUsers();
            Users.ReplaceRange(all);
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

        public void DeleteUser()
        {
            if (SelectedUser == null) return;
            if (_userService.TryDeleteUser(SelectedUser.UserID))
            {
                Users.Remove(SelectedUser);
                SelectedUser = null;
            }
        }
    }
}

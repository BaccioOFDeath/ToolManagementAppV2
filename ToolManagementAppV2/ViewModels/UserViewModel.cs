// ViewModels/UsersViewModel.cs
using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.ViewModels
{
    public class UsersViewModel : ObservableObject
    {
        readonly IUserService _userService;
        readonly IFileDialogService _fileDialog;

        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();

        User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public IRelayCommand BrowseUserPhotoCommand { get; }
        public IRelayCommand SaveUserCommand { get; }
        public IRelayCommand DeleteUserCommand { get; }
        public IRelayCommand NewUserCommand { get; }

        public UsersViewModel(IUserService userService, IFileDialogService fileDialog)
        {
            _userService = userService;
            _fileDialog = fileDialog;

            BrowseUserPhotoCommand = new RelayCommand(BrowseUserPhoto);
            SaveUserCommand = new RelayCommand(SaveUser);
            DeleteUserCommand = new RelayCommand(DeleteUser);
            NewUserCommand = new RelayCommand(NewUser);

            LoadUsers();
        }

        void LoadUsers()
        {
            Users.Clear();
            foreach (var u in _userService.GetAllUsers())
                Users.Add(u);
            if (Users.Count > 0) SelectedUser = Users[0];
        }

        void BrowseUserPhoto()
        {
            var path = _fileDialog.OpenFile("Image Files|*.png;*.jpg;*.jpeg;*.bmp");
            if (!string.IsNullOrWhiteSpace(path) && SelectedUser != null)
                SelectedUser.UserPhotoPath = path;
        }

        void SaveUser()
        {
            try
            {
                if (SelectedUser == null) return;
                if (SelectedUser.UserID == 0)
                    _userService.AddUser(SelectedUser);
                else
                    _userService.UpdateUser(SelectedUser);
                LoadUsers();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        void DeleteUser()
        {
            try
            {
                if (SelectedUser == null) return;
                if (!_userService.TryDeleteUser(SelectedUser.UserID)) return;
                LoadUsers();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        void NewUser()
        {
            SelectedUser = new User { UserName = string.Empty, IsAdmin = false };
        }
    }
}

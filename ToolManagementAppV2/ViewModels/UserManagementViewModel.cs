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

        public ObservableCollection<UserModel> Users { get; } = new();

        private UserModel _selectedUser;
        public UserModel SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public IRelayCommand LoadUsersCommand { get; }

        public UserManagementViewModel(IUserService userService)
        {
            _userService = userService;
            LoadUsersCommand = new RelayCommand(LoadUsers);
        }

        public void LoadUsers()
        {
            var all = _userService.GetAllUsers();
            Users.ReplaceRange(all);
        }
    }
}

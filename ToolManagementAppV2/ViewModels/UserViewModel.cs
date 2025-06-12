using CommunityToolkit.Mvvm.ComponentModel;

namespace ToolManagementAppV2.ViewModels
{
    internal class UserViewModel : ObservableObject
    {
        private UserModel _user;
        public UserModel User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        public UserViewModel(UserModel user)
        {
            _user = user;
        }

        public string DisplayName => $"{_user.UserName} ({(_user.IsAdmin ? "Admin" : "User")})";
    }
}

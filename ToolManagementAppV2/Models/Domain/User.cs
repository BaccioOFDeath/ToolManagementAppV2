using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media.Imaging;

namespace ToolManagementAppV2.Models.Domain
{
    public class User : ObservableObject
    {
        private int _userID;
        public int UserID { get => _userID; set => SetProperty(ref _userID, value); }

        private string _userName;
        public string UserName { get => _userName; set => SetProperty(ref _userName, value); }

        private string _password;
        public string Password { get => _password; set => SetProperty(ref _password, value); }

        private string _userPhotoPath;
        public string UserPhotoPath { get => _userPhotoPath; set => SetProperty(ref _userPhotoPath, value); }

        private bool _isAdmin;
        public bool IsAdmin { get => _isAdmin; set => SetProperty(ref _isAdmin, value); }

        private BitmapImage _photoBitmap;
        public BitmapImage PhotoBitmap { get => _photoBitmap; set => SetProperty(ref _photoBitmap, value); }

        private string _email;
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private string _phone;
        public string Phone { get => _phone; set => SetProperty(ref _phone, value); }

        private string _mobile;
        public string Mobile { get => _mobile; set => SetProperty(ref _mobile, value); }

        private string _address;
        public string Address { get => _address; set => SetProperty(ref _address, value); }

        private string _role;
        public string Role { get => _role; set => SetProperty(ref _role, value); }
    }
}

using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ToolManagementAppV2.Models.Domain
{
    public class User : ObservableObject
    {
        private int _userID;
        public int UserID { get => _userID; set => SetProperty(ref _userID, value); }

        private string _userName;
        public string UserName { get => _userName; set => SetProperty(ref _userName, value); }

        private string _passwordHash;
        public string PasswordHash { get => _passwordHash; set => SetProperty(ref _passwordHash, value); }

        private string _passwordSalt;
        public string PasswordSalt { get => _passwordSalt; set => SetProperty(ref _passwordSalt, value); }

        private string _userPhotoPath;
        public string UserPhotoPath { get => _userPhotoPath; set => SetProperty(ref _userPhotoPath, value); }

        private bool _isAdmin;
        public bool IsAdmin { get => _isAdmin; set => SetProperty(ref _isAdmin, value); }

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

        private bool _isActive = true;
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

        private DateTime _createdAt;
        public DateTime CreatedAt { get => _createdAt; set => SetProperty(ref _createdAt, value); }

        private int _failedAttempts;
        public int FailedAttempts { get => _failedAttempts; set => SetProperty(ref _failedAttempts, value); }

        private DateTime? _lockoutUntil;
        public DateTime? LockoutUntil
        {
            get => _lockoutUntil;
            set => SetProperty(ref _lockoutUntil, value?.ToUniversalTime(), onChanged: () => OnPropertyChanged(nameof(IsLocked)));
        }

        public bool IsLocked => LockoutUntil?.ToUniversalTime() > DateTime.UtcNow;

        private bool _passwordExpired;
        public bool PasswordExpired { get => _passwordExpired; set => SetProperty(ref _passwordExpired, value); }
    }
}

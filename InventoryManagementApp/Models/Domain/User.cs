using System;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

#nullable enable

namespace InventoryManagementApp.Models.Domain
{
    public class User : ObservableObject
    {
        private int _userID;
        public int UserID { get => _userID; set => SetProperty(ref _userID, value); }

        private string _userName = string.Empty;
        public string UserName { get => _userName; set => SetProperty(ref _userName, value); }

        private string _passwordHash = string.Empty;
        public string PasswordHash { get => _passwordHash; set => SetProperty(ref _passwordHash, value); }

        private string _passwordSalt = string.Empty;
        public string PasswordSalt { get => _passwordSalt; set => SetProperty(ref _passwordSalt, value); }

        private string _userPhotoPath = string.Empty;
        public string UserPhotoPath { get => _userPhotoPath; set => SetProperty(ref _userPhotoPath, value); }

        private bool _isAdmin;
        public bool IsAdmin { get => _isAdmin; set => SetProperty(ref _isAdmin, value); }

        private string _email = string.Empty;
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private string _phone = string.Empty;
        public string Phone { get => _phone; set => SetProperty(ref _phone, value); }

        private string _mobile = string.Empty;
        public string Mobile { get => _mobile; set => SetProperty(ref _mobile, value); }

        private string _address = string.Empty;
        public string Address { get => _address; set => SetProperty(ref _address, value); }

        private string _role = string.Empty;
        public string Role { get => _role; set => SetProperty(ref _role, value); }

        private bool _isActive = true;
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

        private DateTime? _createdAt;
        public DateTime? CreatedAt { get => _createdAt; set => SetProperty(ref _createdAt, value); }

        private bool _passwordExpired;
        public bool PasswordExpired { get => _passwordExpired; set => SetProperty(ref _passwordExpired, value); }

        private Brush _initialsBrush = Brushes.Transparent;
        public Brush InitialsBrush { get => _initialsBrush; set => SetProperty(ref _initialsBrush, value); }
    }
}

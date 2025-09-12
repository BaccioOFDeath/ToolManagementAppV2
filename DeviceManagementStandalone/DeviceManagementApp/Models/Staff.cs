using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceManagementApp.Models
{
    public class Staff : ObservableObject
    {
        int _staffId;
        string _name = string.Empty;
        string? _role;
        string? _email;
        string? _phone;

        public int StaffId
        {
            get => _staffId;
            set => SetProperty(ref _staffId, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string? Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }

        public string? Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string? Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }
    }
}

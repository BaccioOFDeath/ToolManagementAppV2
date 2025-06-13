using CommunityToolkit.Mvvm.ComponentModel;

namespace ToolManagementAppV2.Models.Domain
{
    public class Customer : ObservableObject
    {
        private int _customerID;
        public int CustomerID
        {
            get => _customerID;
            set => SetProperty(ref _customerID, value);
        }

        private string _company = string.Empty;
        public string Company
        {
            get => _company;
            set => SetProperty(ref _company, value);
        }

        private string _contact = string.Empty;
        public string Contact
        {
            get => _contact;
            set => SetProperty(ref _contact, value);
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _phone = string.Empty;
        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        private string _mobile = string.Empty;
        public string Mobile
        {
            get => _mobile;
            set => SetProperty(ref _mobile, value);
        }

        private string _address = string.Empty;
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }
    }
}

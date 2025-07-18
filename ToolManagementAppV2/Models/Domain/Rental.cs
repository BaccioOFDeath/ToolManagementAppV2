using CommunityToolkit.Mvvm.ComponentModel;

namespace ToolManagementAppV2.Models.Domain
{
    public class Rental : ObservableObject
    {
        private int _rentalID;
        public int RentalID
        {
            get => _rentalID;
            set => SetProperty(ref _rentalID, value);
        }

        private string _toolID = string.Empty;
        public string ToolID
        {
            get => _toolID;
            set => SetProperty(ref _toolID, value);
        }

        private int _customerID;
        public int CustomerID
        {
            get => _customerID;
            set => SetProperty(ref _customerID, value);
        }

        private DateTime _rentalDate;
        public DateTime RentalDate
        {
            get => _rentalDate;
            set => SetProperty(ref _rentalDate, value);
        }

        private DateTime _dueDate;
        public DateTime DueDate
        {
            get => _dueDate;
            set => SetProperty(ref _dueDate, value);
        }

        private DateTime? _returnDate;
        public DateTime? ReturnDate
        {
            get => _returnDate;
            set => SetProperty(ref _returnDate, value);
        }

        private string? _status;
        public string? Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private string _toolNumber = string.Empty;
        public string ToolNumber
        {
            get => _toolNumber;
            set => SetProperty(ref _toolNumber, value);
        }

        private string _customerName = string.Empty;
        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
        }

        private string _customerContact = string.Empty;
        public string CustomerContact
        {
            get => _customerContact;
            set => SetProperty(ref _customerContact, value);
        }

        private string _customerEmail = string.Empty;
        public string CustomerEmail
        {
            get => _customerEmail;
            set => SetProperty(ref _customerEmail, value);
        }

        private string _customerPhone = string.Empty;
        public string CustomerPhone
        {
            get => _customerPhone;
            set => SetProperty(ref _customerPhone, value);
        }

        private string _customerMobile = string.Empty;
        public string CustomerMobile
        {
            get => _customerMobile;
            set => SetProperty(ref _customerMobile, value);
        }

        private string _customerAddress = string.Empty;
        public string CustomerAddress
        {
            get => _customerAddress;
            set => SetProperty(ref _customerAddress, value);
        }

        private string _toolImagePath = string.Empty;
        public string ToolImagePath
        {
            get => _toolImagePath;
            set => SetProperty(ref _toolImagePath, value);
        }

        private string _toolLocation = string.Empty;
        public string ToolLocation
        {
            get => _toolLocation;
            set => SetProperty(ref _toolLocation, value);
        }
    }
}

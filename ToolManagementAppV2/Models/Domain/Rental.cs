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
    }
}

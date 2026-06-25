using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace InventoryManagementApp.Models.Domain
{
    public class Reservation : ObservableObject
    {
        private int _reservationID;
        public int ReservationID
        {
            get => _reservationID;
            set => SetProperty(ref _reservationID, value);
        }

        private int _itemID;
        public int ItemID
        {
            get => _itemID;
            set => SetProperty(ref _itemID, value);
        }

        private int _customerID;
        public int CustomerID
        {
            get => _customerID;
            set => SetProperty(ref _customerID, value);
        }

        private string _itemNumber = string.Empty;
        public string ItemNumber
        {
            get => _itemNumber;
            set => SetProperty(ref _itemNumber, value);
        }

        private string _itemName = string.Empty;
        public string ItemName
        {
            get => _itemName;
            set => SetProperty(ref _itemName, value);
        }

        private string _imagePath = string.Empty;
        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        private string _customerName = string.Empty;
        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
        }

        private DateTime _reservationDate;
        public DateTime ReservationDate
        {
            get => _reservationDate;
            set => SetProperty(ref _reservationDate, value);
        }

        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                    OnPropertyChanged(nameof(StatusDisplay));
            }
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        private string _status = "Pending";
        public string Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {
                    OnPropertyChanged(nameof(IsActive));
                    OnPropertyChanged(nameof(IsFulfilled));
                    OnPropertyChanged(nameof(StatusDisplay));
                }
            }
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        private int _createdByUserID;
        public int CreatedByUserID
        {
            get => _createdByUserID;
            set => SetProperty(ref _createdByUserID, value);
        }

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        private int? _rentalID;
        public int? RentalID
        {
            get => _rentalID;
            set
            {
                if (SetProperty(ref _rentalID, value))
                {
                    OnPropertyChanged(nameof(IsFulfilled));
                    OnPropertyChanged(nameof(StatusDisplay));
                }
            }
        }

        public bool IsActive => Status == "Pending" || Status == "Confirmed";

        public bool IsFulfilled => RentalID.HasValue || Status == "Fulfilled";

        public string StatusDisplay
        {
            get
            {
                if (IsFulfilled) return "Fulfilled";
                if (StartDate < DateTime.Now && Status == "Confirmed") return "In Progress";
                return Status;
            }
        }
    }
}

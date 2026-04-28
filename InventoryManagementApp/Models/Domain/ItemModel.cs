using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace InventoryManagementApp.Models.Domain
{
    public class ItemModel : ObservableObject
    {
        private int _itemID;
        public int ItemID
        {
            get => _itemID;
            set => SetProperty(ref _itemID, value);
        }

        private string _itemNumber = string.Empty;
        public string ItemNumber
        {
            get => _itemNumber;
            set => SetProperty(ref _itemNumber, value);
        }

        private string _partNumber = string.Empty;
        public string PartNumber
        {
            get => _partNumber;
            set => SetProperty(ref _partNumber, value);
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _brand = string.Empty;
        public string Brand
        {
            get => _brand;
            set => SetProperty(ref _brand, value);
        }

        private string _location = string.Empty;
        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        private int _quantityOnHand;
        public int QuantityOnHand
        {
            get => _quantityOnHand;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(QuantityOnHand), "Quantity cannot be negative.");
                if (SetProperty(ref _quantityOnHand, value))
                {
                    OnPropertyChanged(nameof(OnHand));
                    OnPropertyChanged(nameof(HasNoOnHand));
                }
            }
        }

        private int _rentedQuantity;
        public int RentedQuantity
        {
            get => _rentedQuantity;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(RentedQuantity), "Quantity cannot be negative.");
                if (SetProperty(ref _rentedQuantity, value))
                {
                    OnPropertyChanged(nameof(HasRentedStock));
                }
            }
        }

        private bool _isRentalItem;
        public bool IsRentalItem
        {
            get => _isRentalItem;
            set => SetProperty(ref _isRentalItem, value);
        }

        private string _supplier = string.Empty;
        public string Supplier
        {
            get => _supplier;
            set => SetProperty(ref _supplier, value);
        }

        private DateTime? _purchasedDate;
        public DateTime? PurchasedDate
        {
            get => _purchasedDate;
            set
            {
                if (SetProperty(ref _purchasedDate, value))
                {
                    OnPropertyChanged(nameof(Purchased));
                }
            }
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        private string _keywords = string.Empty;
        public string Keywords
        {
            get => _keywords;
            set => SetProperty(ref _keywords, value);
        }

        private bool _isPowered;
        public bool IsPowered
        {
            get => _isPowered;
            set => SetProperty(ref _isPowered, value);
        }

        private bool _isCheckedOut;
        public bool IsCheckedOut
        {
            get => _isCheckedOut;
            set => SetProperty(ref _isCheckedOut, value);
        }

        private string _checkedOutBy = string.Empty;
        public string CheckedOutBy
        {
            get => _checkedOutBy;
            set => SetProperty(ref _checkedOutBy, value);
        }

        private DateTime? _checkedOutTime;
        public DateTime? CheckedOutTime
        {
            get => _checkedOutTime;
            set => SetProperty(ref _checkedOutTime, value);
        }

        private string _checkedInBy = string.Empty;
        public string CheckedInBy
        {
            get => _checkedInBy;
            set => SetProperty(ref _checkedInBy, value);
        }

        private DateTime? _checkedInTime;
        public DateTime? CheckedInTime
        {
            get => _checkedInTime;
            set => SetProperty(ref _checkedInTime, value);
        }

        private string _imagePath = string.Empty;
        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        private DateTime _updatedAt;
        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set => SetProperty(ref _updatedAt, value);
        }

        private bool _isIncomplete;
        public bool IsIncomplete
        {
            get => _isIncomplete;
            set => SetProperty(ref _isIncomplete, value);
        }

        private string _missingComponentsNotes = string.Empty;
        public string MissingComponentsNotes
        {
            get => _missingComponentsNotes;
            set => SetProperty(ref _missingComponentsNotes, value);
        }

        private string _issuesNotes = string.Empty;
        public string IssuesNotes
        {
            get => _issuesNotes;
            set => SetProperty(ref _issuesNotes, value);
        }

        private int _checkoutCount;
        public int CheckoutCount
        {
            get => _checkoutCount;
            set => SetProperty(ref _checkoutCount, value);
        }

        public int OnHand => QuantityOnHand;

        public bool HasNoOnHand => QuantityOnHand <= 0;

        public bool HasRentedStock => RentedQuantity > 0;

        public string Purchased => PurchasedDate?.ToString("yyyy-MM-dd") ?? string.Empty;
    }
}

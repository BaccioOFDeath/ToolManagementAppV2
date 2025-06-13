using CommunityToolkit.Mvvm.ComponentModel;

namespace ToolManagementAppV2.Models.Domain
{
    public class Tool : ObservableObject
    {
        private string _toolID = string.Empty;
        public string ToolID
        {
            get => _toolID;
            set => SetProperty(ref _toolID, value);
        }

        private string _toolNumber = string.Empty;
        public string ToolNumber
        {
            get => _toolNumber;
            set => SetProperty(ref _toolNumber, value);
        }

        private string _partNumber = string.Empty;
        public string PartNumber
        {
            get => _partNumber;
            set => SetProperty(ref _partNumber, value);
        }

        private string _nameDescription = string.Empty;
        public string NameDescription
        {
            get => _nameDescription;
            set => SetProperty(ref _nameDescription, value);
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

        private int _quantityOnHand;
        public int QuantityOnHand
        {
            get => _quantityOnHand;
            set
            {
                if (SetProperty(ref _quantityOnHand, value))
                {
                    OnPropertyChanged(nameof(OnHand));
                }
            }
        }

        private int _rentedQuantity;
        public int RentedQuantity
        {
            get => _rentedQuantity;
            set => SetProperty(ref _rentedQuantity, value);
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

        private string _toolImagePath = string.Empty;
        public string ToolImagePath
        {
            get => _toolImagePath;
            set => SetProperty(ref _toolImagePath, value);
        }

        public int OnHand => QuantityOnHand;

        public string Purchased => PurchasedDate?.ToString("yyyy-MM-dd") ?? string.Empty;
    }
}

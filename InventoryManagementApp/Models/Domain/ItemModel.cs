using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace InventoryManagementApp.Models.Domain
{
    public class ItemModel : ObservableObject
    {
        private ImageSource? _thumbnail;
        public ImageSource? Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }

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
            set
            {
                if (SetProperty(ref _itemNumber, value))
                    OnPropertyChanged(nameof(SearchIdentityLine));
            }
        }

        private string _partNumber = string.Empty;
        public string PartNumber
        {
            get => _partNumber;
            set
            {
                if (SetProperty(ref _partNumber, value))
                    OnPropertyChanged(nameof(SearchIdentityLine));
            }
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
            set
            {
                if (SetProperty(ref _brand, value))
                    OnPropertyChanged(nameof(SearchIdentityLine));
            }
        }

        private string _location = string.Empty;
        public string Location
        {
            get => _location;
            set
            {
                if (SetProperty(ref _location, value))
                {
                    OnPropertyChanged(nameof(AvailabilityDetail));
                    OnPropertyChanged(nameof(SearchLocationLine));
                }
            }
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
                    NotifyAvailabilityChanged();
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
                    NotifyAvailabilityChanged();
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
            set
            {
                if (SetProperty(ref _keywords, value))
                    OnPropertyChanged(nameof(SearchLocationLine));
            }
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
            set
            {
                if (SetProperty(ref _isCheckedOut, value))
                    NotifyAvailabilityChanged();
            }
        }

        private string _checkedOutBy = string.Empty;
        public string CheckedOutBy
        {
            get => _checkedOutBy;
            set
            {
                if (SetProperty(ref _checkedOutBy, value))
                    NotifyAvailabilityChanged();
            }
        }

        private DateTime? _checkedOutTime;
        public DateTime? CheckedOutTime
        {
            get => _checkedOutTime;
            set
            {
                if (SetProperty(ref _checkedOutTime, value))
                    NotifyAvailabilityChanged();
            }
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
            set
            {
                if (SetProperty(ref _updatedAt, value))
                    OnPropertyChanged(nameof(ActivitySummary));
            }
        }

        private bool _isIncomplete;
        public bool IsIncomplete
        {
            get => _isIncomplete;
            set
            {
                if (SetProperty(ref _isIncomplete, value))
                    NotifyAvailabilityChanged();
            }
        }

        private string _missingComponentsNotes = string.Empty;
        public string MissingComponentsNotes
        {
            get => _missingComponentsNotes;
            set
            {
                if (SetProperty(ref _missingComponentsNotes, value))
                    OnPropertyChanged(nameof(AvailabilityDetail));
            }
        }

        private string _issuesNotes = string.Empty;
        public string IssuesNotes
        {
            get => _issuesNotes;
            set
            {
                if (SetProperty(ref _issuesNotes, value))
                    OnPropertyChanged(nameof(AvailabilityDetail));
            }
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

        public bool IsUnavailable => IsIncomplete || IsCheckedOut || HasRentedStock || HasNoOnHand;

        public string AvailabilityStatus
        {
            get
            {
                if (IsIncomplete)
                    return "Incomplete";
                if (IsCheckedOut)
                    return "Checked Out";
                if (HasRentedStock)
                    return "Rented";
                if (HasNoOnHand)
                    return "Unavailable";
                return "Available";
            }
        }

        public string AvailabilityDetail
        {
            get
            {
                if (IsIncomplete)
                    return FirstNonEmpty(MissingComponentsNotes, IssuesNotes, "Item is marked incomplete. Review details before issuing.");
                if (IsCheckedOut)
                    return $"Out to {HolderDisplay}{OutSinceSuffix}";
                if (HasRentedStock)
                    return $"{RentedQuantity} rented, {QuantityOnHand} on hand. Open rental or request details if unavailable.";
                if (HasNoOnHand)
                    return "No on-hand stock. Place a request or check current holder/history.";
                return string.IsNullOrWhiteSpace(Location)
                    ? $"{QuantityOnHand} on hand. Location not recorded."
                    : $"{QuantityOnHand} on hand at {Location}.";
            }
        }

        public string HolderDisplay => string.IsNullOrWhiteSpace(CheckedOutBy) ? "holder not recorded" : CheckedOutBy;

        public string OutSinceDisplay => CheckedOutTime.HasValue ? CheckedOutTime.Value.ToString("yyyy-MM-dd HH:mm") : "Not recorded";

        public string StockSummary => $"On hand: {QuantityOnHand} | Rented: {RentedQuantity}";

        public string ActivitySummary
        {
            get
            {
                if (IsCheckedOut && CheckedOutTime.HasValue)
                    return $"Out since {CheckedOutTime.Value:yyyy-MM-dd HH:mm}";
                if (UpdatedAt != default)
                    return $"Updated {UpdatedAt:yyyy-MM-dd}";
                return "No recent activity recorded";
            }
        }

        public string SearchIdentityLine => JoinParts(
            FormatPart("Item #", ItemNumber),
            FormatPart("Part #", PartNumber),
            FormatPart("Brand", Brand));

        public string SearchLocationLine => JoinParts(
            FormatPart("Location", Location),
            FormatPart("Keywords", Keywords));

        public string Purchased => PurchasedDate?.ToString("yyyy-MM-dd") ?? string.Empty;

        private string OutSinceSuffix => CheckedOutTime.HasValue ? $" since {CheckedOutTime.Value:yyyy-MM-dd HH:mm}" : string.Empty;

        private void NotifyAvailabilityChanged()
        {
            OnPropertyChanged(nameof(IsUnavailable));
            OnPropertyChanged(nameof(AvailabilityStatus));
            OnPropertyChanged(nameof(AvailabilityDetail));
            OnPropertyChanged(nameof(HolderDisplay));
            OnPropertyChanged(nameof(OutSinceDisplay));
            OnPropertyChanged(nameof(StockSummary));
            OnPropertyChanged(nameof(ActivitySummary));
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return string.Empty;
        }

        private static string FormatPart(string label, string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : $"{label} {value}";
        }

        private static string JoinParts(params string[] parts)
        {
            var filtered = new List<string>();
            foreach (var part in parts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                    filtered.Add(part);
            }

            return string.Join(" | ", filtered);
        }
    }
}

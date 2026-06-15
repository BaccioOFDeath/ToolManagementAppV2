// ViewModels/ItemDetailsViewModel.cs
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.ViewModels
{
    public class ItemDetailsViewModel : ObservableObject
    {
        readonly IItemService _itemService;
        readonly ICustomerService _customerService;
        readonly IRentalService _rentalService;
        readonly IDialogService _dialogService;

        public ItemModel ItemModel { get; }

        public IRelayCommand CloseCommand { get; }
        public IAsyncRelayCommand EditCommand { get; }
        public IAsyncRelayCommand RentOutCommand { get; }
        public IAsyncRelayCommand ToggleCheckOutCommand { get; }
        public IAsyncRelayCommand OpenRentalHistoryCommand { get; }

        public string CheckOutButtonText => ItemModel.IsCheckedOut ? "Check In" : "Check Out";
        public string StatusText
        {
            get
            {
                if (ItemModel.IsIncomplete)
                    return "Maintenance / Incomplete";
                if (ItemModel.IsCheckedOut)
                    return "Checked Out";
                if (ItemModel.HasRentedStock)
                    return "Rented";
                if (ItemModel.HasNoOnHand)
                    return "Unavailable";
                return "Available";
            }
        }

        public string AvailabilitySummary => ItemModel.IsCheckedOut
            ? "Not available until it is checked back in."
            : ItemModel.HasRentedStock
                ? "Rental stock is currently out with a customer."
                : ItemModel.HasNoOnHand
                    ? "No on-hand stock is available at this location."
                    : "Available for checkout or rental.";

        public string HolderSummary => ItemModel.IsCheckedOut
            ? string.IsNullOrWhiteSpace(ItemModel.CheckedOutBy) ? "Holder not recorded" : ItemModel.CheckedOutBy
            : ItemModel.HasRentedStock ? "Customer rental in progress" : "Shelf / available stock";

        public string CheckedOutSinceText => ItemModel.CheckedOutTime?.ToString("yyyy-MM-dd HH:mm") ?? "Not checked out";
        public string TimeOutText => ItemModel.CheckedOutTime is DateTime checkedOut
            ? FormatElapsed(DateTime.Now - checkedOut)
            : "-";
        public string LastCheckInText => ItemModel.CheckedInTime?.ToString("yyyy-MM-dd HH:mm") ?? "No recent check-in recorded";
        public string StockSummary => $"On hand {ItemModel.QuantityOnHand} | Rented {ItemModel.RentedQuantity}";
        public string UsageSummary => ItemModel.CheckoutCount == 1 ? "1 recorded checkout" : $"{ItemModel.CheckoutCount} recorded checkouts";
        public string UpdatedText => ItemModel.UpdatedAt == default ? "Not recorded" : ItemModel.UpdatedAt.ToString("yyyy-MM-dd HH:mm");
        public string PurchasedText => ItemModel.PurchasedDate?.ToString("yyyy-MM-dd") ?? "Not recorded";
        public string PriceText => ItemModel.Price > 0 ? ItemModel.Price.ToString("C") : "Not recorded";
        public string ConditionSummary
        {
            get
            {
                if (ItemModel.IsIncomplete)
                {
                    var missing = ItemModel.MissingComponentsNotes;
                    var issues = ItemModel.IssuesNotes;
                    if (!string.IsNullOrWhiteSpace(missing) && !string.IsNullOrWhiteSpace(issues))
                        return $"{missing} | {issues}";
                    if (!string.IsNullOrWhiteSpace(missing))
                        return missing;
                    if (!string.IsNullOrWhiteSpace(issues))
                        return issues;
                    return "Marked incomplete";
                }

                if (!string.IsNullOrWhiteSpace(ItemModel.IssuesNotes))
                    return ItemModel.IssuesNotes;

                return "No open condition notes";
            }
        }

        public string NextActionText
        {
            get
            {
                if (ItemModel.IsCheckedOut)
                    return "Check in from this window, then review any waiting rental requests from the rentals page.";
                if (ItemModel.HasRentedStock || ItemModel.HasNoOnHand)
                    return "Open rental history or place a request from the rentals workflow if another user needs this item.";
                return "Check out to a technician, rent to a customer, or open history before handing it out.";
            }
        }

        public ItemDetailsViewModel(ItemModel item, IItemService itemService, ICustomerService customerService, IRentalService rentalService, IDialogService dialogService, Action onClose)
        {
            ItemModel = item;
            _itemService = itemService;
            _customerService = customerService;
            _rentalService = rentalService;
            _dialogService = dialogService;
            CloseCommand = new RelayCommand(onClose);
            EditCommand = new AsyncRelayCommand(EditAsync);
            RentOutCommand = new AsyncRelayCommand(RentOutAsync);
            ToggleCheckOutCommand = new AsyncRelayCommand(ToggleCheckOutAsync);
            OpenRentalHistoryCommand = new AsyncRelayCommand(OpenRentalHistoryAsync);
        }

        async Task EditAsync()
        {
            var clone = CloneItem(ItemModel);
            var updated = await _dialogService.ShowEditItemDialogAsync(clone).ConfigureAwait(false);
            if (updated == null)
                return;

            await _itemService.UpdateItemAsync(updated).ConfigureAwait(false);
            CopyItem(ItemModel, updated);
            RefreshState();
        }

        async Task ToggleCheckOutAsync()
        {
            var result = await _itemService.ToggleItemCheckOutStatusAsync(ItemModel.ItemID).ConfigureAwait(false);
            if (!result)
                return;

            var checkedOut = !ItemModel.IsCheckedOut;
            ItemModel.IsCheckedOut = checkedOut;
            ItemModel.QuantityOnHand += checkedOut ? -1 : 1;

            var refreshed = await _itemService.GetItemByIDAsync(ItemModel.ItemID).ConfigureAwait(false);
            if (refreshed != null)
            {
                CopyItem(ItemModel, refreshed);
            }

            RefreshState();
        }

        async Task RentOutAsync()
        {
            var customers = await _customerService.GetAllCustomersAsync().ConfigureAwait(false);
            var result = _dialogService.ShowRentItemDialog(ItemModel, customers);
            if (result == null)
                return;

            var (customer, dueDate) = result.Value;
            await _rentalService.RentItemAsync(ItemModel.ItemID, customer.CustomerID, DateTime.Today, dueDate).ConfigureAwait(false);

            var refreshed = await _itemService.GetItemByIDAsync(ItemModel.ItemID).ConfigureAwait(false);
            if (refreshed != null)
            {
                CopyItem(ItemModel, refreshed);
            }

            RefreshState();
        }

        async Task OpenRentalHistoryAsync()
        {
            var history = await _rentalService.GetRentalHistoryForItemAsync(ItemModel.ItemID).ConfigureAwait(false);
            _dialogService.ShowRentalHistory(ItemModel, history);
        }

        void RefreshState()
        {
            OnPropertyChanged(nameof(CheckOutButtonText));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(AvailabilitySummary));
            OnPropertyChanged(nameof(HolderSummary));
            OnPropertyChanged(nameof(CheckedOutSinceText));
            OnPropertyChanged(nameof(TimeOutText));
            OnPropertyChanged(nameof(LastCheckInText));
            OnPropertyChanged(nameof(StockSummary));
            OnPropertyChanged(nameof(UsageSummary));
            OnPropertyChanged(nameof(UpdatedText));
            OnPropertyChanged(nameof(PurchasedText));
            OnPropertyChanged(nameof(PriceText));
            OnPropertyChanged(nameof(ConditionSummary));
            OnPropertyChanged(nameof(NextActionText));
        }

        static string FormatElapsed(TimeSpan elapsed)
        {
            if (elapsed.TotalDays >= 1)
                return $"{(int)elapsed.TotalDays}d {elapsed.Hours}h";
            if (elapsed.TotalHours >= 1)
                return $"{(int)elapsed.TotalHours}h {elapsed.Minutes}m";
            return $"{Math.Max(0, (int)elapsed.TotalMinutes)}m";
        }

        static ItemModel CloneItem(ItemModel source)
        {
            return new ItemModel
            {
                ItemID = source.ItemID,
                ItemNumber = source.ItemNumber,
                PartNumber = source.PartNumber,
                Name = source.Name,
                Brand = source.Brand,
                Location = source.Location,
                Price = source.Price,
                QuantityOnHand = source.QuantityOnHand,
                RentedQuantity = source.RentedQuantity,
                Supplier = source.Supplier,
                PurchasedDate = source.PurchasedDate,
                Notes = source.Notes,
                Keywords = source.Keywords,
                IsPowered = source.IsPowered,
                IsRentalItem = source.IsRentalItem,
                IsCheckedOut = source.IsCheckedOut,
                CheckedOutBy = source.CheckedOutBy,
                CheckedOutTime = source.CheckedOutTime,
                CheckedInBy = source.CheckedInBy,
                CheckedInTime = source.CheckedInTime,
                ImagePath = source.ImagePath,
                UpdatedAt = source.UpdatedAt,
                IsIncomplete = source.IsIncomplete,
                MissingComponentsNotes = source.MissingComponentsNotes,
                IssuesNotes = source.IssuesNotes,
                CheckoutCount = source.CheckoutCount
            };
        }

        static void CopyItem(ItemModel target, ItemModel source)
        {
            target.ItemNumber = source.ItemNumber;
            target.PartNumber = source.PartNumber;
            target.Name = source.Name;
            target.Brand = source.Brand;
            target.Location = source.Location;
            target.Price = source.Price;
            target.QuantityOnHand = source.QuantityOnHand;
            target.RentedQuantity = source.RentedQuantity;
            target.Supplier = source.Supplier;
            target.PurchasedDate = source.PurchasedDate;
            target.Notes = source.Notes;
            target.Keywords = source.Keywords;
            target.IsPowered = source.IsPowered;
            target.IsRentalItem = source.IsRentalItem;
            target.IsCheckedOut = source.IsCheckedOut;
            target.CheckedOutBy = source.CheckedOutBy;
            target.CheckedOutTime = source.CheckedOutTime;
            target.CheckedInBy = source.CheckedInBy;
            target.CheckedInTime = source.CheckedInTime;
            target.ImagePath = source.ImagePath;
            target.UpdatedAt = source.UpdatedAt;
            target.IsIncomplete = source.IsIncomplete;
            target.MissingComponentsNotes = source.MissingComponentsNotes;
            target.IssuesNotes = source.IssuesNotes;
            target.CheckoutCount = source.CheckoutCount;
        }
    }
}

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
                ItemModel.CheckedOutBy = refreshed.CheckedOutBy;
                ItemModel.CheckedOutTime = refreshed.CheckedOutTime;
                ItemModel.CheckedInBy = refreshed.CheckedInBy;
                ItemModel.CheckedInTime = refreshed.CheckedInTime;
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
                ImagePath = source.ImagePath
            };
        }

        static void CopyItem(ItemModel target, ItemModel source)
        {
            target.ItemNumber = source.ItemNumber;
            target.PartNumber = source.PartNumber;
            target.Name = source.Name;
            target.Brand = source.Brand;
            target.Location = source.Location;
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
        }
    }
}

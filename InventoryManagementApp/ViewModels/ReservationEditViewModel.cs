using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.ViewModels
{
    public class ReservationEditViewModel : ObservableObject
    {
        readonly IItemService? _itemService;
        CancellationTokenSource? _searchCts;

        public Reservation Reservation { get; }

        public bool IsNew { get; }

        public string Title => IsNew ? "New Reservation" : "Edit Reservation";

        public ObservableCollection<string> StatusOptions { get; }

        public ObservableCollection<ItemModel> ItemSearchResults { get; } = new();

        public string ItemSearchSummary => _itemService == null
            ? "Item lookup is not available in this session."
            : ItemSearchResults.Count == 1
                ? "1 item match"
                : $"{ItemSearchResults.Count} item matches";

        public string SelectedItemSummary => SelectedSearchItem == null
            ? "No lookup item selected."
            : $"{ValueOrNotRecorded(SelectedSearchItem.ItemNumber)} | {ValueOrNotRecorded(SelectedSearchItem.Name)} | {ValueOrNotRecorded(SelectedSearchItem.Location)}";

        string _itemSearchText = string.Empty;
        public string ItemSearchText
        {
            get => _itemSearchText;
            set
            {
                if (SetProperty(ref _itemSearchText, value))
                    SearchItemsCommand.Execute(null);
            }
        }

        ItemModel? _selectedSearchItem;
        public ItemModel? SelectedSearchItem
        {
            get => _selectedSearchItem;
            set
            {
                if (SetProperty(ref _selectedSearchItem, value))
                {
                    ApplySelectedItemCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(SelectedItemSummary));
                }
            }
        }

        public IRelayCommand SaveCommand { get; }

        public IRelayCommand CancelCommand { get; }

        public IAsyncRelayCommand SearchItemsCommand { get; }

        public IRelayCommand ClearItemSearchCommand { get; }

        public IRelayCommand ApplySelectedItemCommand { get; }

        public ReservationEditViewModel(Reservation reservation, bool isNew, Action onSave, Action onCancel, IItemService? itemService = null)
        {
            Reservation = reservation;
            IsNew = isNew;
            _itemService = itemService;
            StatusOptions = new ObservableCollection<string>
            {
                "Pending",
                "Confirmed",
                "Fulfilled",
                "Cancelled"
            };
            SaveCommand = new RelayCommand(onSave);
            CancelCommand = new RelayCommand(onCancel);
            SearchItemsCommand = new AsyncRelayCommand(SearchItemsAsync, () => _itemService != null);
            ClearItemSearchCommand = new RelayCommand(ClearItemSearch, () => !string.IsNullOrWhiteSpace(ItemSearchText));
            ApplySelectedItemCommand = new RelayCommand(ApplySelectedItem, () => SelectedSearchItem != null);
        }

        async Task SearchItemsAsync()
        {
            if (_itemService == null)
                return;

            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = new CancellationTokenSource();
            var cancellationToken = _searchCts.Token;
            var term = ItemSearchText?.Trim() ?? string.Empty;
            ItemSearchResults.Clear();
            SelectedSearchItem = null;

            if (string.IsNullOrWhiteSpace(term))
            {
                OnPropertyChanged(nameof(ItemSearchSummary));
                ClearItemSearchCommand.NotifyCanExecuteChanged();
                return;
            }

            var matches = new List<ItemModel>();
            try
            {
                await foreach (var item in _itemService.SearchItemsAsync(term, new ItemPage(1, 20), SortField.Name, SortDirection.Ascending, cancellationToken: cancellationToken)
                    .WithCancellation(cancellationToken))
                {
                    matches.Add(item);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            foreach (var item in matches.OrderBy(i => i.ItemNumber).ThenBy(i => i.Name))
                ItemSearchResults.Add(item);

            if (ItemSearchResults.Count == 1)
                SelectedSearchItem = ItemSearchResults[0];

            OnPropertyChanged(nameof(ItemSearchSummary));
            ClearItemSearchCommand.NotifyCanExecuteChanged();
        }

        void ClearItemSearch()
        {
            ItemSearchText = string.Empty;
            ItemSearchResults.Clear();
            SelectedSearchItem = null;
            OnPropertyChanged(nameof(ItemSearchSummary));
            ClearItemSearchCommand.NotifyCanExecuteChanged();
        }

        void ApplySelectedItem()
        {
            if (SelectedSearchItem == null)
                return;

            Reservation.ItemNumber = SelectedSearchItem.ItemNumber;
            Reservation.ItemName = SelectedSearchItem.Name;
            OnPropertyChanged(nameof(Reservation));
        }

        static string ValueOrNotRecorded(string? value)
            => string.IsNullOrWhiteSpace(value) ? "Not recorded" : value;
    }
}

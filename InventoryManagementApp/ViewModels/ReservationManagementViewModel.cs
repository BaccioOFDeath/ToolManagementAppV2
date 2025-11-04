using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Reservations;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.ViewModels
{
    public class ReservationManagementViewModel : ObservableObject
    {
        private readonly ReservationService _reservationService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<Reservation> Reservations { get; }
        public ObservableCollection<Reservation> FilteredReservations { get; }

        private Reservation? _selectedReservation;
        public Reservation? SelectedReservation
        {
            get => _selectedReservation;
            set
            {
                if (SetProperty(ref _selectedReservation, value))
                {
                    EditReservationCommand.NotifyCanExecuteChanged();
                    DeleteReservationCommand.NotifyCanExecuteChanged();
                    ConfirmReservationCommand.NotifyCanExecuteChanged();
                    CancelReservationCommand.NotifyCanExecuteChanged();
                    FulfillReservationCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilter();
                }
            }
        }

        private string _selectedFilter = "Active";
        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (SetProperty(ref _selectedFilter, value))
                {
                    ApplyFilter();
                }
            }
        }

        public ObservableCollection<string> FilterOptions { get; }

        public IAsyncRelayCommand LoadReservationsCommand { get; }
        public IAsyncRelayCommand AddReservationCommand { get; }
        public IAsyncRelayCommand EditReservationCommand { get; }
        public IAsyncRelayCommand DeleteReservationCommand { get; }
        public IAsyncRelayCommand ConfirmReservationCommand { get; }
        public IAsyncRelayCommand CancelReservationCommand { get; }
        public IAsyncRelayCommand FulfillReservationCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }

        public ReservationManagementViewModel(
            ReservationService reservationService,
            IDialogService dialogService)
        {
            _reservationService = reservationService;
            _dialogService = dialogService;

            Reservations = new ObservableCollection<Reservation>();
            FilteredReservations = new ObservableCollection<Reservation>();
            FilterOptions = new ObservableCollection<string>
            {
                "All",
                "Active",
                "Pending",
                "Confirmed",
                "Fulfilled",
                "Cancelled",
                "Upcoming (7 days)"
            };

            LoadReservationsCommand = new AsyncRelayCommand(LoadReservationsAsync);
            AddReservationCommand = new AsyncRelayCommand(AddReservationAsync);
            EditReservationCommand = new AsyncRelayCommand(EditReservationAsync, CanEdit);
            DeleteReservationCommand = new AsyncRelayCommand(DeleteReservationAsync, CanDelete);
            ConfirmReservationCommand = new AsyncRelayCommand(ConfirmReservationAsync, CanConfirm);
            CancelReservationCommand = new AsyncRelayCommand(CancelReservationAsync, CanCancel);
            FulfillReservationCommand = new AsyncRelayCommand(FulfillReservationAsync, CanFulfill);
            RefreshCommand = new AsyncRelayCommand(LoadReservationsAsync);
        }

        private async Task LoadReservationsAsync()
        {
            try
            {
                var reservations = await _reservationService.GetAllReservationsAsync();
                Reservations.Clear();
                foreach (var reservation in reservations)
                {
                    Reservations.Add(reservation);
                }
                ApplyFilter();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error loading reservations", ex.Message);
            }
        }

        private async Task AddReservationAsync()
        {
            var newReservation = new Reservation
            {
                ReservationDate = DateTime.Now,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                Quantity = 1,
                Status = "Pending"
            };

            var result = await _dialogService.ShowReservationEditDialogAsync(newReservation, isNew: true);
            if (result)
            {
                try
                {
                    var isAvailable = await _reservationService.CheckAvailabilityAsync(
                        newReservation.ItemID,
                        newReservation.StartDate,
                        newReservation.EndDate,
                        newReservation.Quantity);

                    if (!isAvailable)
                    {
                        var proceed = await _dialogService.ShowConfirmAsync(
                            "Availability Warning",
                            "The requested quantity may not be available for the selected dates. Create reservation anyway?");

                        if (!proceed) return;
                    }

                    var id = await _reservationService.CreateReservationAsync(newReservation);
                    newReservation.ReservationID = id;
                    Reservations.Insert(0, newReservation);
                    ApplyFilter();
                    await _dialogService.ShowInfoAsync("Success", "Reservation created successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error creating reservation", ex.Message);
                }
            }
        }

        private async Task EditReservationAsync()
        {
            if (SelectedReservation == null) return;

            var clone = new Reservation
            {
                ReservationID = SelectedReservation.ReservationID,
                ItemID = SelectedReservation.ItemID,
                CustomerID = SelectedReservation.CustomerID,
                ItemNumber = SelectedReservation.ItemNumber,
                ItemName = SelectedReservation.ItemName,
                CustomerName = SelectedReservation.CustomerName,
                ReservationDate = SelectedReservation.ReservationDate,
                StartDate = SelectedReservation.StartDate,
                EndDate = SelectedReservation.EndDate,
                Quantity = SelectedReservation.Quantity,
                Status = SelectedReservation.Status,
                Notes = SelectedReservation.Notes,
                CreatedByUserID = SelectedReservation.CreatedByUserID,
                CreatedAt = SelectedReservation.CreatedAt,
                RentalID = SelectedReservation.RentalID
            };

            var result = await _dialogService.ShowReservationEditDialogAsync(clone, isNew: false);
            if (result)
            {
                try
                {
                    await _reservationService.UpdateReservationAsync(clone);
                    var index = Reservations.IndexOf(SelectedReservation);
                    Reservations[index] = clone;
                    ApplyFilter();
                    await _dialogService.ShowInfoAsync("Success", "Reservation updated successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error updating reservation", ex.Message);
                }
            }
        }

        private async Task DeleteReservationAsync()
        {
            if (SelectedReservation == null) return;

            var confirmed = await _dialogService.ShowConfirmAsync(
                "Delete Reservation",
                $"Are you sure you want to delete this reservation?");

            if (confirmed)
            {
                try
                {
                    await _reservationService.DeleteReservationAsync(SelectedReservation.ReservationID);
                    Reservations.Remove(SelectedReservation);
                    ApplyFilter();
                    await _dialogService.ShowInfoAsync("Success", "Reservation deleted successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error deleting reservation", ex.Message);
                }
            }
        }

        private async Task ConfirmReservationAsync()
        {
            if (SelectedReservation == null) return;

            try
            {
                await _reservationService.ConfirmReservationAsync(SelectedReservation.ReservationID);
                SelectedReservation.Status = "Confirmed";
                ApplyFilter();
                await _dialogService.ShowInfoAsync("Success", "Reservation confirmed");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error confirming reservation", ex.Message);
            }
        }

        private async Task CancelReservationAsync()
        {
            if (SelectedReservation == null) return;

            var confirmed = await _dialogService.ShowConfirmAsync(
                "Cancel Reservation",
                "Are you sure you want to cancel this reservation?");

            if (confirmed)
            {
                try
                {
                    await _reservationService.CancelReservationAsync(SelectedReservation.ReservationID);
                    SelectedReservation.Status = "Cancelled";
                    ApplyFilter();
                    await _dialogService.ShowInfoAsync("Success", "Reservation cancelled");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error cancelling reservation", ex.Message);
                }
            }
        }

        private async Task FulfillReservationAsync()
        {
            if (SelectedReservation == null) return;

            var rentalIdText = await _dialogService.ShowInputDialogAsync(
                "Fulfill Reservation",
                "Enter the Rental ID that fulfills this reservation:");

            if (!string.IsNullOrWhiteSpace(rentalIdText) && int.TryParse(rentalIdText, out var rentalId))
            {
                try
                {
                    await _reservationService.FulfillReservationAsync(SelectedReservation.ReservationID, rentalId);
                    SelectedReservation.Status = "Fulfilled";
                    SelectedReservation.RentalID = rentalId;
                    ApplyFilter();
                    await _dialogService.ShowInfoAsync("Success", "Reservation marked as fulfilled");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error fulfilling reservation", ex.Message);
                }
            }
        }

        private void ApplyFilter()
        {
            FilteredReservations.Clear();

            var filtered = Reservations.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLowerInvariant();
                filtered = filtered.Where(r =>
                    r.ItemNumber.ToLowerInvariant().Contains(search) ||
                    r.ItemName.ToLowerInvariant().Contains(search) ||
                    r.CustomerName.ToLowerInvariant().Contains(search));
            }

            filtered = SelectedFilter switch
            {
                "Active" => filtered.Where(r => r.IsActive),
                "Pending" => filtered.Where(r => r.Status == "Pending"),
                "Confirmed" => filtered.Where(r => r.Status == "Confirmed"),
                "Fulfilled" => filtered.Where(r => r.Status == "Fulfilled"),
                "Cancelled" => filtered.Where(r => r.Status == "Cancelled"),
                "Upcoming (7 days)" => filtered.Where(r => r.IsActive && r.StartDate <= DateTime.Now.AddDays(7)),
                _ => filtered
            };

            foreach (var reservation in filtered)
            {
                FilteredReservations.Add(reservation);
            }
        }

        private bool CanEdit() => SelectedReservation != null && SelectedReservation.Status != "Fulfilled";

        private bool CanDelete() => SelectedReservation != null;

        private bool CanConfirm() => SelectedReservation != null && SelectedReservation.Status == "Pending";

        private bool CanCancel() => SelectedReservation != null && SelectedReservation.IsActive;

        private bool CanFulfill() => SelectedReservation != null && SelectedReservation.Status == "Confirmed";
    }
}

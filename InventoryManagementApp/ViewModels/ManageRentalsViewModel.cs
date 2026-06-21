using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Reservations;
using InventoryManagementApp.Utilities.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.ViewModels
{
    public class ManageRentalsViewModel : ObservableObject
    {
        private readonly IRentalService _rentalService;
        private readonly IDialogService _dialogService;
        private readonly ReservationService? _reservationService;
        private readonly ILogger<ManageRentalsViewModel> _logger;
        private List<RentalModel> _allRentals = new();

        public ObservableCollection<RentalModel> Rentals { get; } = new();
        public ObservableCollection<RentalModel> ActiveRentals { get; } = new();
        public ObservableCollection<Reservation> PendingRequests { get; } = new();

        public string SearchSummary => $"{Rentals.Count} result{(Rentals.Count == 1 ? string.Empty : "s")} shown";
        public string CheckedOutSummary => $"{ActiveRentals.Count} item{(ActiveRentals.Count == 1 ? string.Empty : "s")} currently checked out";
        public string RequestSummary => $"{PendingRequests.Count} open request{(PendingRequests.Count == 1 ? string.Empty : "s")}";
        public string SelectedRequestSummary => SelectedRequest == null
            ? "Select a request to see customer, holder, and next action."
            : $"{ValueOrNotRecorded(SelectedRequest.ItemNumber)} | {ValueOrNotRecorded(SelectedRequest.CustomerName)} | {SelectedRequest.StatusDisplay}";
        public string SelectedRequestDateLine => SelectedRequest == null
            ? "No request selected."
            : $"Requested {FormatDate(SelectedRequest.ReservationDate)} | Needed {SelectedRequest.StartDate:yyyy-MM-dd} to {SelectedRequest.EndDate:yyyy-MM-dd}";
        public string SelectedRequestHolderLine
        {
            get
            {
                if (SelectedRequest == null)
                    return "Select a request to inspect current availability.";

                var activeRental = FindActiveRentalForRequest(SelectedRequest);
                return activeRental == null
                    ? "No active checkout found for this item. It may be ready to pick or rent."
                    : $"Currently out to {ValueOrNotRecorded(activeRental.CustomerName)}; due back {activeRental.DueDate:yyyy-MM-dd HH:mm}.";
            }
        }
        public string SelectedRequestNextAction
        {
            get
            {
                if (SelectedRequest == null)
                    return "Next action: choose a request from the queue.";
                if (_reservationService == null || SelectedRequest.ReservationID <= 0)
                    return "Next action: open details or print this request; durable status changes are not available in this session.";
                if (string.Equals(SelectedRequest.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                    return "Next action: confirm the hold, contact the current holder, or cancel it if the customer no longer needs it.";
                if (string.Equals(SelectedRequest.Status, "Confirmed", StringComparison.OrdinalIgnoreCase))
                    return "Next action: watch for check-in, pick the item, then complete the rental from the normal rental workflow.";
                return "Next action: review the request details and history before changing it.";
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    ApplyFilterCommand.Execute(null);
            }
        }

        private DateTime? _filterFrom;
        public DateTime? FilterFrom
        {
            get => _filterFrom;
            set
            {
                if (SetProperty(ref _filterFrom, value))
                    ApplyFilterCommand.Execute(null);
            }
        }

        private DateTime? _filterTo;
        public DateTime? FilterTo
        {
            get => _filterTo;
            set
            {
                if (SetProperty(ref _filterTo, value))
                    ApplyFilterCommand.Execute(null);
            }
        }

        public ObservableCollection<string> StatusOptions { get; } = new() { "All", "Rented", "Returned" };

        private string _selectedStatus = "All";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                    ApplyFilterCommand.Execute(null);
            }
        }

        private RentalModel? _selectedRental;
        public RentalModel? SelectedRental
        {
            get => _selectedRental;
            set
            {
                if (SetProperty(ref _selectedRental, value))
                {
                    CheckInCommand.NotifyCanExecuteChanged();
                    ExtendCommand.NotifyCanExecuteChanged();
                    OpenHistoryCommand.NotifyCanExecuteChanged();
                    OpenRentalDetailsCommand.NotifyCanExecuteChanged();
                    PlaceRequestCommand.NotifyCanExecuteChanged();
                    PrintRentalCommand.NotifyCanExecuteChanged();
                    PrintPickingSlipCommand.NotifyCanExecuteChanged();
                    PrintInvoiceCommand.NotifyCanExecuteChanged();
                    DeleteRentalCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private Reservation? _selectedRequest;
        public Reservation? SelectedRequest
        {
            get => _selectedRequest;
            set
            {
                if (SetProperty(ref _selectedRequest, value))
                {
                    OpenRequestDetailsCommand.NotifyCanExecuteChanged();
                    ConfirmRequestCommand.NotifyCanExecuteChanged();
                    CancelRequestCommand.NotifyCanExecuteChanged();
                    PrintRequestCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(SelectedRequestSummary));
                    OnPropertyChanged(nameof(SelectedRequestDateLine));
                    OnPropertyChanged(nameof(SelectedRequestHolderLine));
                    OnPropertyChanged(nameof(SelectedRequestNextAction));
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public IRelayCommand ApplyFilterCommand { get; }
        public IRelayCommand ClearFilterCommand { get; }
        public IAsyncRelayCommand CheckInCommand { get; }
        public IAsyncRelayCommand ExtendCommand { get; }
        public IAsyncRelayCommand OpenHistoryCommand { get; }
        public IRelayCommand OpenRentalDetailsCommand { get; }
        public IAsyncRelayCommand PlaceRequestCommand { get; }
        public IRelayCommand OpenRequestDetailsCommand { get; }
        public IAsyncRelayCommand ConfirmRequestCommand { get; }
        public IAsyncRelayCommand CancelRequestCommand { get; }
        public IRelayCommand PrintRequestCommand { get; }
        public IRelayCommand PrintRentalCommand { get; }
        public IRelayCommand PrintSearchResultsCommand { get; }
        public IRelayCommand PrintCheckedOutCommand { get; }
        public IRelayCommand PrintRequestsCommand { get; }
        public IRelayCommand PrintPickingSlipCommand { get; }
        public IRelayCommand PrintInvoiceCommand { get; }
        public IAsyncRelayCommand DeleteRentalCommand { get; }

        public ManageRentalsViewModel(
            IRentalService rentalService,
            IDialogService dialogService,
            ILogger<ManageRentalsViewModel>? logger = null)
            : this(rentalService, dialogService, null, logger)
        {
        }

        public ManageRentalsViewModel(
            IRentalService rentalService,
            IDialogService dialogService,
            ReservationService? reservationService,
            ILogger<ManageRentalsViewModel>? logger = null)
        {
            _rentalService = rentalService;
            _dialogService = dialogService;
            _reservationService = reservationService ?? TryResolveReservationService();
            _logger = logger ?? NullLogger<ManageRentalsViewModel>.Instance;

            ApplyFilterCommand = new RelayCommand(ApplyFilter);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            CheckInCommand = new AsyncRelayCommand(CheckInAsync, CanReturnSelectedRental);
            ExtendCommand = new AsyncRelayCommand(ExtendAsync, CanReturnSelectedRental);
            OpenHistoryCommand = new AsyncRelayCommand(OpenHistoryAsync, () => SelectedRental != null);
            OpenRentalDetailsCommand = new RelayCommand(OpenRentalDetails, () => SelectedRental != null);
            PlaceRequestCommand = new AsyncRelayCommand(PlaceRequestAsync, CanPlaceRequestForSelectedRental);
            OpenRequestDetailsCommand = new RelayCommand(OpenRequestDetails, () => SelectedRequest != null);
            ConfirmRequestCommand = new AsyncRelayCommand(ConfirmRequestAsync, CanUpdateSelectedRequest);
            CancelRequestCommand = new AsyncRelayCommand(CancelRequestAsync, CanUpdateSelectedRequest);
            PrintRequestCommand = new RelayCommand(PrintRequest, () => SelectedRequest != null);
            PrintRentalCommand = new RelayCommand(PrintRental, () => SelectedRental != null);
            PrintSearchResultsCommand = new RelayCommand(PrintSearchResults);
            PrintCheckedOutCommand = new RelayCommand(PrintCheckedOut);
            PrintRequestsCommand = new RelayCommand(PrintRequests);
            PrintPickingSlipCommand = new RelayCommand(PrintPickingSlip, () => SelectedRental != null);
            PrintInvoiceCommand = new RelayCommand(PrintInvoice, () => SelectedRental != null);
            DeleteRentalCommand = new AsyncRelayCommand(DeleteRentalAsync, () => SelectedRental != null);
        }

        public async Task LoadRentalsAsync()
        {
            var selectedRentalId = SelectedRental?.RentalID;
            IsLoading = true;
            try
            {
                _allRentals = await _rentalService.GetAllRentalsAsync();
                await LoadPendingRequestsAsync();
                RefreshActiveRentals();
                ApplyFilter(selectedRentalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load rentals");
                await _dialogService.ShowInfoAsync($"Failed to load rentals: {ex.Message}", "Error");
            }
            finally
            {
                IsLoading = false;
            }
        }

        async Task LoadPendingRequestsAsync()
        {
            if (_reservationService == null)
            {
                PendingRequests.Clear();
                OnPropertyChanged(nameof(RequestSummary));
                return;
            }

            try
            {
                var selectedRequestId = SelectedRequest?.ReservationID;
                var requests = await _reservationService.GetActiveReservationsAsync();
                PendingRequests.ReplaceRange(requests);
                SelectedRequest = PendingRequests.FirstOrDefault(r => selectedRequestId.HasValue && r.ReservationID == selectedRequestId.Value)
                    ?? PendingRequests.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load open reservations for rentals page");
                PendingRequests.Clear();
            }
            finally
            {
                OnPropertyChanged(nameof(RequestSummary));
            }
        }

        void ApplyFilter() => ApplyFilter(SelectedRental?.RentalID);

        void ApplyFilter(int? selectedRentalId)
        {
            if (FilterFrom.HasValue && FilterTo.HasValue && FilterFrom > FilterTo)
            {
                _ = _dialogService.ShowInfoAsync("\"From\" date cannot be later than \"To\" date.", "Invalid Date Range");
                return;
            }

            IEnumerable<RentalModel> filtered = _allRentals;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var term = SearchText.Trim();
                filtered = filtered.Where(r =>
                    (r.ItemNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.ItemLocation?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.CustomerName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (FilterFrom.HasValue)
                filtered = filtered.Where(r => r.RentalDate >= FilterFrom.Value);
            if (FilterTo.HasValue)
                filtered = filtered.Where(r => r.RentalDate <= FilterTo.Value);
            if (!string.IsNullOrWhiteSpace(SelectedStatus) && SelectedStatus != "All")
                filtered = filtered.Where(r => string.Equals(r.Status, SelectedStatus, StringComparison.OrdinalIgnoreCase));

            Rentals.ReplaceRange(filtered.ToList());
            RestoreSelectedRental(selectedRentalId);
            OnPropertyChanged(nameof(SearchSummary));
        }

        void ClearFilter()
        {
            var selectedRentalId = SelectedRental?.RentalID;
            SearchText = string.Empty;
            FilterFrom = null;
            FilterTo = null;
            SelectedStatus = StatusOptions.First();
            Rentals.ReplaceRange(_allRentals);
            RestoreSelectedRental(selectedRentalId);
            OnPropertyChanged(nameof(SearchSummary));
        }

        void RestoreSelectedRental(int? selectedRentalId)
        {
            if (!selectedRentalId.HasValue)
            {
                if (SelectedRental != null && !Rentals.Contains(SelectedRental))
                    SelectedRental = null;
                return;
            }

            SelectedRental = Rentals.FirstOrDefault(r => r.RentalID == selectedRentalId.Value);
        }

        async Task CheckInAsync()
        {
            if (SelectedRental == null)
                return;

            var returnedRental = SelectedRental;
            try
            {
                IsLoading = true;
                await _rentalService.ReturnItemAsync(returnedRental.RentalID, DateTime.Today);
                await LoadRentalsAsync();
                await NotifyWaitingRequestsAsync(returnedRental.ItemID, returnedRental.ItemNumber);
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to check in rentals.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check in rental {RentalID}", returnedRental.RentalID);
                await RefreshRentalDeskAfterOperationFailureAsync(returnedRental.RentalID);
                await _dialogService.ShowInfoAsync($"Failed to check in rental: {ex.Message} The rental desk has been refreshed so current rental actions match the latest saved state.", "Error");
            }
            finally
            {
                IsLoading = false;
            }
        }

        async Task NotifyWaitingRequestsAsync(int itemId, string itemNumber)
        {
            var waitingRequests = PendingRequests.Where(r => r.ItemID == itemId && r.IsActive).ToList();
            if (waitingRequests.Count == 0)
                return;

            var firstRequest = waitingRequests.OrderBy(r => r.StartDate).First();
            var message = new StringBuilder();
            message.AppendLine($"{waitingRequests.Count} open request{(waitingRequests.Count == 1 ? string.Empty : "s")} is waiting for item {itemNumber}.");
            message.AppendLine();
            message.AppendLine($"Next customer: {ValueOrNotRecorded(firstRequest.CustomerName)}");
            message.AppendLine($"Needed from: {FormatDate(firstRequest.StartDate)}");
            message.AppendLine($"Needed until: {FormatDate(firstRequest.EndDate)}");
            message.AppendLine();
            message.AppendLine("Use the pending requests grid to open the request, contact the customer, or print the queue.");

            await _dialogService.ShowInfoAsync(message.ToString(), "Requests Waiting");
        }

        async Task ExtendAsync()
        {
            if (SelectedRental == null)
                return;

            var rentalToExtend = SelectedRental;
            try
            {
                IsLoading = true;
                var newDueDate = rentalToExtend.DueDate.AddDays(7);
                await _rentalService.ExtendRentalAsync(rentalToExtend.RentalID, newDueDate);
                await LoadRentalsAsync();
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to extend rentals.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extend rental {RentalID}", rentalToExtend.RentalID);
                await RefreshRentalDeskAfterOperationFailureAsync(rentalToExtend.RentalID);
                await _dialogService.ShowInfoAsync($"Failed to extend rental: {ex.Message} The rental desk has been refreshed so current rental actions match the latest saved state.", "Error");
            }
            finally
            {
                IsLoading = false;
            }
        }

        async Task RefreshRentalDeskAfterOperationFailureAsync(int rentalId)
        {
            try
            {
                _allRentals = await _rentalService.GetAllRentalsAsync();
                await LoadPendingRequestsAsync();
                RefreshActiveRentals();
                ApplyFilter(rentalId);
            }
            catch (Exception refreshEx)
            {
                _logger.LogError(refreshEx, "Failed to refresh rentals after operation failure for rental {RentalID}", rentalId);
            }
        }

        async Task OpenHistoryAsync()
        {
            if (SelectedRental == null)
                return;

            List<RentalModel> history;
            try
            {
                history = await _rentalService.GetRentalHistoryForItemAsync(SelectedRental.ItemID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open rental history for item {ItemID}", SelectedRental.ItemID);
                await _dialogService.ShowInfoAsync($"Failed to load rental history: {ex.Message}", "Error");
                return;
            }

            var item = new ItemModel
            {
                ItemID = SelectedRental.ItemID,
                ItemNumber = SelectedRental.ItemNumber,
                Name = SelectedRental.ItemNumber
            };

            _dialogService.ShowRentalHistory(item, history);
        }

        void OpenRentalDetails()
        {
            if (SelectedRental == null)
                return;

            var rental = SelectedRental;
            var details = new StringBuilder();
            details.AppendLine($"Rental #: {rental.RentalID}");
            details.AppendLine($"Item #: {rental.ItemNumber}");
            details.AppendLine($"Location: {ValueOrNotRecorded(rental.ItemLocation)}");
            details.AppendLine($"Status: {ValueOrNotRecorded(rental.Status)}");
            details.AppendLine($"Open requests: {PendingRequests.Count(r => r.ItemID == rental.ItemID && r.IsActive)}");
            details.AppendLine();
            details.AppendLine($"Checked out to: {ValueOrNotRecorded(rental.CustomerName)}");
            details.AppendLine($"Contact: {ValueOrNotRecorded(rental.CustomerContact)}");
            details.AppendLine($"Phone: {ValueOrNotRecorded(rental.CustomerPhone)}");
            details.AppendLine($"Email: {ValueOrNotRecorded(rental.CustomerEmail)}");
            details.AppendLine();
            details.AppendLine($"Checked out: {FormatDate(rental.RentalDate)}");
            details.AppendLine($"Due back: {FormatDate(rental.DueDate)}");
            details.AppendLine($"Returned: {FormatNullableDate(rental.ReturnDate)}");
            details.AppendLine($"Time out: {DescribeRentalAge(rental)}");
            details.AppendLine();
            details.AppendLine(IsRentalActive(rental)
                ? "Next steps: check in when returned, extend if approved, place a request for the next customer, or open history for prior usage."
                : "Next steps: open history to inspect prior usage, review open requests, or print this rental record.");

            _dialogService.ShowInfo(details.ToString(), $"Rental Details - {rental.ItemNumber}");
        }

        async Task PlaceRequestAsync()
        {
            if (SelectedRental == null)
                return;

            var rental = SelectedRental;
            var reservation = new Reservation
            {
                ItemID = rental.ItemID,
                CustomerID = 0,
                ItemNumber = rental.ItemNumber,
                ItemName = rental.ItemNumber,
                CustomerName = string.Empty,
                ReservationDate = DateTime.Now,
                StartDate = rental.DueDate.Date,
                EndDate = rental.DueDate.Date.AddDays(7),
                Quantity = 1,
                Status = "Pending",
                Notes = $"Requested from rental screen while rental #{rental.RentalID} is out to {ValueOrNotRecorded(rental.CustomerName)}. Due back {rental.DueDate:yyyy-MM-dd}."
            };

            try
            {
                var accepted = await _dialogService.ShowReservationEditDialogAsync(reservation, isNew: true);
                if (!accepted)
                    return;

                if (reservation.ReservationDate == default)
                    reservation.ReservationDate = DateTime.Now;
                if (reservation.CreatedAt == default)
                    reservation.CreatedAt = DateTime.Now;
                if (string.IsNullOrWhiteSpace(reservation.Status))
                    reservation.Status = "Pending";
                if (reservation.Quantity < 1)
                    reservation.Quantity = 1;

                var savedReservation = await SaveReservationAsync(reservation);
                PendingRequests.Add(savedReservation);
                SelectedRequest = savedReservation;
                OnPropertyChanged(nameof(RequestSummary));

                var persistenceNote = _reservationService == null
                    ? "Request captured for this rentals screen."
                    : "Request saved to reservations and added to the open request queue.";
                await _dialogService.ShowInfoAsync(persistenceNote, "Request Captured");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to place request for rental {RentalID}", rental.RentalID);
                await _dialogService.ShowInfoAsync($"Failed to place request: {ex.Message}", "Error");
            }
        }

        async Task<Reservation> SaveReservationAsync(Reservation reservation)
        {
            if (_reservationService == null)
                return reservation;

            var reservationId = await _reservationService.CreateReservationAsync(reservation);
            reservation.ReservationID = reservationId;
            reservation.ReservationDate = DateTime.Now;
            reservation.CreatedAt = DateTime.Now;

            return await _reservationService.GetReservationByIdAsync(reservationId) ?? reservation;
        }

        void OpenRequestDetails()
        {
            if (SelectedRequest == null)
                return;

            var request = SelectedRequest;
            var details = new StringBuilder();
            details.AppendLine($"Request #: {(request.ReservationID > 0 ? request.ReservationID.ToString() : "Not saved")}");
            details.AppendLine($"Item #: {ValueOrNotRecorded(request.ItemNumber)}");
            details.AppendLine($"Item: {ValueOrNotRecorded(request.ItemName)}");
            details.AppendLine($"Customer: {ValueOrNotRecorded(request.CustomerName)}");
            details.AppendLine($"Status: {ValueOrNotRecorded(request.Status)}");
            details.AppendLine($"Quantity: {request.Quantity}");
            details.AppendLine();
            details.AppendLine($"Requested: {FormatDate(request.ReservationDate)}");
            details.AppendLine($"Needed from: {FormatDate(request.StartDate)}");
            details.AppendLine($"Needed until: {FormatDate(request.EndDate)}");
            details.AppendLine($"Current holder: {SelectedRequestHolderLine}");
            details.AppendLine();
            details.AppendLine($"Notes: {ValueOrNotRecorded(request.Notes)}");
            details.AppendLine();
            details.AppendLine(SelectedRequestNextAction);

            _dialogService.ShowInfo(details.ToString(), $"Request Details - {request.ItemNumber}");
        }

        async Task ConfirmRequestAsync()
        {
            if (SelectedRequest == null || _reservationService == null)
                return;

            var requestId = SelectedRequest.ReservationID;
            try
            {
                IsLoading = true;
                var updated = await _reservationService.ConfirmReservationAsync(requestId);
                if (!updated)
                {
                    await LoadPendingRequestsAsync();
                    await _dialogService.ShowInfoAsync("The selected request could not be confirmed. It may have been removed or changed by another user. The open request queue has been refreshed.", "Confirm Request");
                    return;
                }

                await LoadPendingRequestsAsync();
                await _dialogService.ShowInfoAsync("Request confirmed and remains in the open request queue.", "Confirm Request");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to confirm request {ReservationID}", requestId);
                await _dialogService.ShowInfoAsync($"Failed to confirm request: {ex.Message}", "Error");
            }
            finally
            {
                IsLoading = false;
            }
        }

        async Task CancelRequestAsync()
        {
            if (SelectedRequest == null || _reservationService == null)
                return;

            var request = SelectedRequest;
            var confirmed = await _dialogService.ShowConfirmAsync("Cancel Request", $"Cancel request #{request.ReservationID} for item {request.ItemNumber}?");
            if (!confirmed)
                return;

            try
            {
                IsLoading = true;
                var updated = await _reservationService.CancelReservationAsync(request.ReservationID);
                if (!updated)
                {
                    await LoadPendingRequestsAsync();
                    await _dialogService.ShowInfoAsync("The selected request could not be cancelled. It may have been removed or changed by another user. The open request queue has been refreshed.", "Cancel Request");
                    return;
                }

                await LoadPendingRequestsAsync();
                await _dialogService.ShowInfoAsync("Request cancelled and removed from the open request queue.", "Cancel Request");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel request {ReservationID}", request.ReservationID);
                await _dialogService.ShowInfoAsync($"Failed to cancel request: {ex.Message}", "Error");
            }
            finally
            {
                IsLoading = false;
            }
        }

        void PrintRequest()
        {
            if (SelectedRequest == null)
                return;

            try
            {
                var request = SelectedRequest;
                var doc = CreateRentalDocument("Request Information");
                var table = CreateKeyValueTable();
                var group = table.RowGroups[0];
                AddKeyValueRow(group, "Request #:", request.ReservationID > 0 ? request.ReservationID.ToString() : "Not saved");
                AddKeyValueRow(group, "Item #:", request.ItemNumber);
                AddKeyValueRow(group, "Item:", request.ItemName);
                AddKeyValueRow(group, "Customer:", request.CustomerName);
                AddKeyValueRow(group, "Status:", request.StatusDisplay);
                AddKeyValueRow(group, "Quantity:", request.Quantity.ToString());
                AddKeyValueRow(group, "Requested:", FormatDate(request.ReservationDate));
                AddKeyValueRow(group, "Needed From:", request.StartDate.ToString("yyyy-MM-dd"));
                AddKeyValueRow(group, "Needed Until:", request.EndDate.ToString("yyyy-MM-dd"));
                AddKeyValueRow(group, "Current Holder:", SelectedRequestHolderLine);
                AddKeyValueRow(group, "Next Action:", SelectedRequestNextAction);
                AddKeyValueRow(group, "Notes:", request.Notes);
                doc.Blocks.Add(table);

                _dialogService.ShowPrintPreview(doc, $"Request {request.ReservationID}", string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to print request {ReservationID}", SelectedRequest?.ReservationID);
                _dialogService.ShowInfo($"Failed to print request: {ex.Message}", "Error");
            }
        }

        void PrintRental()
        {
            if (SelectedRental == null)
                return;
            try
            {
                var doc = CreateRentalDocument("Rental Information");
                var table = CreateKeyValueTable();
                var group = table.RowGroups[0];
                AddKeyValueRow(group, "Rental #:", SelectedRental.RentalID.ToString());
                AddKeyValueRow(group, "Item #:", SelectedRental.ItemNumber);
                AddKeyValueRow(group, "Location:", SelectedRental.ItemLocation);
                AddKeyValueRow(group, "Customer:", SelectedRental.CustomerName);
                AddKeyValueRow(group, "Rental Date:", SelectedRental.RentalDate.ToString("yyyy-MM-dd HH:mm"));
                AddKeyValueRow(group, "Due Date:", SelectedRental.DueDate.ToString("yyyy-MM-dd HH:mm"));
                AddKeyValueRow(group, "Return Date:", SelectedRental.ReturnDate?.ToString("yyyy-MM-dd HH:mm") ?? "N/A");
                AddKeyValueRow(group, "Status:", SelectedRental.Status ?? string.Empty);
                AddKeyValueRow(group, "Open Requests:", PendingRequests.Count(r => r.ItemID == SelectedRental.ItemID && r.IsActive).ToString());
                doc.Blocks.Add(table);

                _dialogService.ShowPrintPreview(doc, $"Rental {SelectedRental.RentalID}", string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to print rental {RentalID}", SelectedRental?.RentalID);
                _dialogService.ShowInfo($"Failed to print rental: {ex.Message}", "Error");
            }
        }

        void PrintSearchResults() => PrintRentalList("Rental Search Results", Rentals, "There are no rental search results to print.");

        void PrintCheckedOut() => PrintRentalList("Currently Checked Out Items", ActiveRentals, "There are no checked-out items to print.");

        void PrintRequests()
        {
            var requests = PendingRequests.ToList();
            if (requests.Count == 0)
            {
                _dialogService.ShowInfo("There are no open requests to print.", "Open Requests");
                return;
            }

            try
            {
                var doc = CreateRentalDocument("Open Requests", fontSize: 11);
                doc.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:yyyy-MM-dd HH:mm} | {requests.Count} request{(requests.Count == 1 ? string.Empty : "s")}"))
                {
                    FontSize = 10,
                    Margin = new Thickness(0, 0, 0, 10)
                });

                var table = new Table { CellSpacing = 0 };
                table.Columns.Add(new TableColumn { Width = new GridLength(80) });
                table.Columns.Add(new TableColumn { Width = new GridLength(100) });
                table.Columns.Add(new TableColumn { Width = new GridLength(140) });
                table.Columns.Add(new TableColumn { Width = new GridLength(150) });
                table.Columns.Add(new TableColumn { Width = new GridLength(95) });
                table.Columns.Add(new TableColumn { Width = new GridLength(95) });
                table.Columns.Add(new TableColumn { Width = new GridLength(90) });

                var group = new TableRowGroup();
                table.RowGroups.Add(group);
                AddPrintRow(group, true, "Request", "Item #", "Item", "Customer", "Needed", "Until", "Status");

                foreach (var request in requests)
                {
                    AddPrintRow(group, false, request.ReservationID.ToString(), request.ItemNumber, request.ItemName, request.CustomerName, request.StartDate.ToString("yyyy-MM-dd"), request.EndDate.ToString("yyyy-MM-dd"), request.Status);
                }

                doc.Blocks.Add(table);
                _dialogService.ShowPrintPreview(doc, "Open Requests", string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to print open requests");
                _dialogService.ShowInfo($"Failed to print open requests: {ex.Message}", "Error");
            }
        }

        void PrintRentalList(string title, IEnumerable<RentalModel> rentals, string emptyMessage)
        {
            var records = rentals.ToList();
            if (records.Count == 0)
            {
                _dialogService.ShowInfo(emptyMessage, title);
                return;
            }

            try
            {
                var doc = CreateRentalDocument(title, fontSize: 11);
                doc.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:yyyy-MM-dd HH:mm} | {records.Count} record{(records.Count == 1 ? string.Empty : "s")}"))
                {
                    FontSize = 10,
                    Margin = new Thickness(0, 0, 0, 10)
                });

                var table = new Table { CellSpacing = 0 };
                table.Columns.Add(new TableColumn { Width = new GridLength(70) });
                table.Columns.Add(new TableColumn { Width = new GridLength(95) });
                table.Columns.Add(new TableColumn { Width = new GridLength(140) });
                table.Columns.Add(new TableColumn { Width = new GridLength(140) });
                table.Columns.Add(new TableColumn { Width = new GridLength(95) });
                table.Columns.Add(new TableColumn { Width = new GridLength(95) });
                table.Columns.Add(new TableColumn { Width = new GridLength(80) });

                var group = new TableRowGroup();
                table.RowGroups.Add(group);
                AddPrintRow(group, true, "Rental", "Item #", "Location", "Checked Out To", "Out", "Due", "Status");

                foreach (var rental in records)
                {
                    AddPrintRow(group, false, rental.RentalID.ToString(), rental.ItemNumber, rental.ItemLocation, rental.CustomerName, rental.RentalDate.ToString("yyyy-MM-dd"), rental.DueDate.ToString("yyyy-MM-dd"), rental.Status ?? string.Empty);
                }

                doc.Blocks.Add(table);
                _dialogService.ShowPrintPreview(doc, title, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to print rental list {Title}", title);
                _dialogService.ShowInfo($"Failed to print rental list: {ex.Message}", "Error");
            }
        }

        static FlowDocument CreateRentalDocument(string title, double fontSize = 16)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(36),
                FontFamily = new System.Windows.Media.FontFamily("Calibri"),
                FontSize = fontSize
            };

            doc.Blocks.Add(new Paragraph(new Bold(new Run(title)))
            {
                FontSize = 20,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            return doc;
        }

        static Table CreateKeyValueTable()
        {
            var table = new Table();
            table.Columns.Add(new TableColumn { Width = new GridLength(150) });
            table.Columns.Add(new TableColumn());
            table.RowGroups.Add(new TableRowGroup());
            return table;
        }

        static void AddKeyValueRow(TableRowGroup group, string label, string value)
        {
            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(label)) { FontWeight = FontWeights.Bold }));
            row.Cells.Add(new TableCell(new Paragraph(new Run(value ?? string.Empty))));
            group.Rows.Add(row);
        }

        static void AddPrintRow(TableRowGroup group, bool isHeader, params string[] values)
        {
            var row = new TableRow();
            foreach (var value in values)
            {
                var paragraph = new Paragraph(new Run(value ?? string.Empty))
                {
                    Margin = new Thickness(3),
                    FontSize = isHeader ? 10 : 9,
                    FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal
                };
                var cell = new TableCell(paragraph)
                {
                    BorderBrush = System.Windows.Media.Brushes.Gray,
                    BorderThickness = new Thickness(0.5),
                    Padding = new Thickness(2)
                };
                row.Cells.Add(cell);
            }
            group.Rows.Add(row);
        }

        void PrintPickingSlip()
        {
            if (SelectedRental == null)
                return;
            try
            {
                var printService = new Services.Printing.RentalPrintingService("Equipment Rentals", "", "");
                var doc = printService.GeneratePickingSlip(SelectedRental);
                _dialogService.ShowPrintPreview(doc, $"Picking Slip - Rental {SelectedRental.RentalID}", string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to print picking slip for rental {RentalID}", SelectedRental?.RentalID);
                _dialogService.ShowInfo($"Failed to print picking slip: {ex.Message}", "Error");
            }
        }

        void PrintInvoice()
        {
            if (SelectedRental == null)
                return;
            try
            {
                var printService = new Services.Printing.RentalPrintingService("Equipment Rentals", "", "");
                var doc = printService.GenerateInvoice(SelectedRental, dailyRate: 25.00m, lateFee: 0);
                _dialogService.ShowPrintPreview(doc, $"Invoice - Rental {SelectedRental.RentalID}", string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to print invoice for rental {RentalID}", SelectedRental?.RentalID);
                _dialogService.ShowInfo($"Failed to print invoice: {ex.Message}", "Error");
            }
        }

        async Task DeleteRentalAsync()
        {
            if (SelectedRental == null)
                return;

            var confirmed = await _dialogService.ShowConfirmAsync("Delete Rental", $"Are you sure you want to delete rental #{SelectedRental.RentalID}?");
            if (!confirmed)
                return;

            var rentalToDelete = SelectedRental;
            try
            {
                IsLoading = true;
                await _rentalService.DeleteRentalAsync(rentalToDelete.RentalID);
                _allRentals.Remove(rentalToDelete);
                SelectedRental = null;
                RefreshActiveRentals();
                ApplyFilter();
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to delete rentals.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete rental {RentalID}", rentalToDelete.RentalID);
                await RefreshRentalDeskAfterOperationFailureAsync(rentalToDelete.RentalID);
                await _dialogService.ShowInfoAsync($"Failed to delete rental: {ex.Message} The rental desk has been refreshed so current rental actions match the latest saved state.", "Error");
            }
            finally
            {
                IsLoading = false;
            }
        }

        void RefreshActiveRentals()
        {
            ActiveRentals.ReplaceRange(_allRentals.Where(IsRentalActive));
            OnPropertyChanged(nameof(CheckedOutSummary));
            OnPropertyChanged(nameof(SelectedRequestHolderLine));
            OnPropertyChanged(nameof(SelectedRequestNextAction));
        }

        bool CanReturnSelectedRental() => SelectedRental != null && IsRentalActive(SelectedRental);

        bool CanPlaceRequestForSelectedRental() => SelectedRental != null && IsRentalActive(SelectedRental);

        bool CanUpdateSelectedRequest() => SelectedRequest != null
            && SelectedRequest.ReservationID > 0
            && SelectedRequest.IsActive
            && _reservationService != null;

        RentalModel? FindActiveRentalForRequest(Reservation? request)
        {
            if (request == null)
                return null;

            return ActiveRentals.FirstOrDefault(r => r.ItemID == request.ItemID);
        }

        static bool IsRentalActive(RentalModel rental)
        {
            return rental.ReturnDate == null && !string.Equals(rental.Status, "Returned", StringComparison.OrdinalIgnoreCase);
        }

        static string ValueOrNotRecorded(string? value) => string.IsNullOrWhiteSpace(value) ? "Not recorded" : value;

        static string FormatDate(DateTime value) => value.ToString("yyyy-MM-dd HH:mm");

        static string FormatNullableDate(DateTime? value) => value?.ToString("yyyy-MM-dd HH:mm") ?? "Not returned yet";

        static string DescribeRentalAge(RentalModel rental)
        {
            var end = rental.ReturnDate ?? DateTime.Now;
            var days = Math.Max(0, (end.Date - rental.RentalDate.Date).Days);
            return days == 1 ? "1 day" : $"{days} days";
        }

        static ReservationService? TryResolveReservationService()
        {
            try
            {
                if (System.Windows.Application.Current is App app)
                    return app.Host.Services.GetService<ReservationService>();
            }
            catch
            {
            }

            return null;
        }
    }
}
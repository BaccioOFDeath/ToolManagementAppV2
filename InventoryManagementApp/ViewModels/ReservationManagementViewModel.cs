using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Reservations;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.ViewModels
{
    public class ReservationManagementViewModel : ObservableObject
    {
        private const int MaxVisibleReservationRows = 500;
        private const int MaxReservationPrintRows = 250;

        private readonly ReservationService _reservationService;
        private readonly IDialogService _dialogService;
        private int _matchedReservationCount;

        public ObservableCollection<Reservation> Reservations { get; }
        public ObservableCollection<Reservation> FilteredReservations { get; }

        public string ReservationResultsSummary
        {
            get
            {
                if (IsLoading)
                {
                    return "Loading reservations...";
                }

                var activeCount = Reservations.Count(r => r.IsActive);
                var matchLabel = MatchingReservationCount == 1 ? "reservation" : "reservations";
                var shown = HasOmittedReservationRows
                    ? $"{VisibleReservationCount} of {MatchingReservationCount} matching {matchLabel} shown"
                    : $"{VisibleReservationCount} of {TotalReservationCount} reservation{(TotalReservationCount == 1 ? string.Empty : "s")} shown";
                var active = $"{activeCount} active";
                var omitted = HasOmittedReservationRows
                    ? $" | {OmittedReservationCount} hidden from live grid"
                    : string.Empty;

                return string.IsNullOrWhiteSpace(SearchText)
                    ? $"{shown} | {active} | filter: {SelectedFilter}{omitted}"
                    : $"{shown} for \"{SearchText.Trim()}\" | {active} | filter: {SelectedFilter}{omitted}";
            }
        }

        public int VisibleReservationCount => FilteredReservations.Count;
        public int TotalReservationCount => Reservations.Count;
        public int MatchingReservationCount => _matchedReservationCount;
        public int OmittedReservationCount => Math.Max(0, MatchingReservationCount - VisibleReservationCount);
        public bool HasOmittedReservationRows => OmittedReservationCount > 0;
        public string ReservationVisibleWindowSummary
        {
            get
            {
                if (IsLoading)
                {
                    return "Rows loading...";
                }

                if (VisibleReservationCount == 0)
                {
                    return IsFilterActive
                        ? "No matching rows"
                        : "No active rows";
                }

                return HasOmittedReservationRows
                    ? $"Showing first {VisibleReservationCount} of {MatchingReservationCount}; {OmittedReservationCount} hidden for responsiveness."
                    : $"Showing all {VisibleReservationCount} matching row{(VisibleReservationCount == 1 ? string.Empty : "s")}.";
            }
        }

        public bool IsFilterActive => !string.IsNullOrWhiteSpace(SearchText) || !string.Equals(SelectedFilter, "Active", StringComparison.Ordinal);

        public string ReservationEmptyTitle => Reservations.Count == 0
            ? "No reservations found"
            : "No reservations match this filter";

        public string ReservationEmptyMessage => Reservations.Count == 0
            ? "Add a hold or refresh when reservation records are available."
            : "Clear search, switch the status filter, or add a new hold to restart the reservation queue.";

        public bool CanPrintReservationDirectory => !IsLoading && FilteredReservations.Count > 0;

        public string ReservationPrintStatus
        {
            get
            {
                if (IsLoading)
                {
                    return "Print paused while reservation rows load";
                }

                if (FilteredReservations.Count == 0)
                {
                    return IsFilterActive
                        ? "No filtered hold rows ready to print"
                        : "No hold rows ready to print";
                }

                var printed = Math.Min(VisibleReservationCount, MaxReservationPrintRows);
                var omitted = Math.Max(0, MatchingReservationCount - printed);
                var filterContext = IsFilterActive ? "filtered" : "active";
                return omitted == 0
                    ? $"Ready to print {printed} {filterContext} hold row{(printed == 1 ? string.Empty : "s")}."
                    : $"Ready to print first {printed} of {MatchingReservationCount} {filterContext} hold rows; {omitted} omitted from preview.";
            }
        }

        public string SelectedReservationTitle => SelectedReservation == null
            ? "No reservation selected"
            : $"Reservation #{SelectedReservation.ReservationID} - {ValueOrNotRecorded(SelectedReservation.ItemNumber)}";

        public string SelectedReservationSubtitle => SelectedReservation == null
            ? "Select or double-click a reservation to see customer, item, timing, and fulfillment guidance."
            : $"{ValueOrNotRecorded(SelectedReservation.CustomerName)} | {SelectedReservation.StatusDisplay} | Qty {SelectedReservation.Quantity}";

        public string SelectedReservationTiming => SelectedReservation == null
            ? "No date range selected."
            : $"Requested {SelectedReservation.StartDate:yyyy-MM-dd} through {SelectedReservation.EndDate:yyyy-MM-dd} | Created {SelectedReservation.CreatedAt:yyyy-MM-dd HH:mm}";

        public string SelectedReservationNextAction
        {
            get
            {
                if (SelectedReservation == null)
                    return "Choose a hold before confirming availability, cancelling, fulfilling with a rental ID, or printing a shelf handoff.";

                return SelectedReservation.Status switch
                {
                    "Pending" => "Confirm the hold when the item is available, or edit the dates/customer before committing stock.",
                    "Confirmed" when SelectedReservation.StartDate.Date <= DateTime.Now.Date => "Collect the item from the shelf, start the rental checkout, then fulfill this reservation with the Rental ID.",
                    "Confirmed" => "Keep this hold staged for the start date, print/copy the handoff, or cancel if the customer no longer needs it.",
                    "Fulfilled" => "Reservation is complete. Use the linked Rental ID for checkout, return, invoice, or history review.",
                    "Cancelled" => "Reservation is cancelled. Keep it for audit history or delete it if it was entered in error.",
                    _ => "Review the hold status, customer, dates, and notes before choosing the next operation."
                };
            }
        }

        public string SelectedReservationShelfChecklist => SelectedReservation == null
            ? "Shelf checklist appears after selecting a reservation."
            : "1. Verify item number and quantity. 2. Check condition and calibration/maintenance flags. 3. Match customer at pickup. 4. Create rental and record the Rental ID here.";

        public string SelectedReservationDetail => SelectedReservation == null
            ? "No reservation selected."
            : CreateReservationHandoffText(SelectedReservation);

        public string SelectedReservationSummary => SelectedReservation == null
            ? "Select a reservation to confirm, cancel, fulfill, print, copy, edit, or delete."
            : $"Ready: #{SelectedReservation.ReservationID} | {ValueOrNotRecorded(SelectedReservation.CustomerName)} | {ValueOrNotRecorded(SelectedReservation.ItemName)} | {SelectedReservation.StartDate:yyyy-MM-dd} to {SelectedReservation.EndDate:yyyy-MM-dd} | {SelectedReservation.StatusDisplay}";

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    NotifyCommandStatesAndSummaries();
                    NotifyReservationListStateChanged();
                }
            }
        }

        private Reservation? _selectedReservation;
        public Reservation? SelectedReservation
        {
            get => _selectedReservation;
            set
            {
                if (SetProperty(ref _selectedReservation, value))
                {
                    NotifyCommandStatesAndSummaries();
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
        public IRelayCommand OpenReservationDetailsCommand { get; }
        public IRelayCommand CopyReservationHandoffCommand { get; }
        public IRelayCommand PrintReservationHandoffCommand { get; }
        public IRelayCommand PrintReservationDirectoryCommand { get; }
        public IRelayCommand ClearReservationSearchCommand { get; }
        public IRelayCommand ShowActiveReservationsCommand { get; }
        public IRelayCommand ShowPendingReservationsCommand { get; }
        public IRelayCommand ShowConfirmedReservationsCommand { get; }
        public IRelayCommand ShowUpcomingReservationsCommand { get; }

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

            LoadReservationsCommand = new AsyncRelayCommand(LoadReservationsAsync, CanRefreshReservations);
            AddReservationCommand = new AsyncRelayCommand(AddReservationAsync, CanInteractWithReservations);
            EditReservationCommand = new AsyncRelayCommand(EditReservationAsync, CanEdit);
            DeleteReservationCommand = new AsyncRelayCommand(DeleteReservationAsync, CanDelete);
            ConfirmReservationCommand = new AsyncRelayCommand(ConfirmReservationAsync, CanConfirm);
            CancelReservationCommand = new AsyncRelayCommand(CancelReservationAsync, CanCancel);
            FulfillReservationCommand = new AsyncRelayCommand(FulfillReservationAsync, CanFulfill);
            RefreshCommand = new AsyncRelayCommand(LoadReservationsAsync, CanRefreshReservations);
            OpenReservationDetailsCommand = new RelayCommand(OpenReservationDetails, CanUseSelectedReservation);
            CopyReservationHandoffCommand = new RelayCommand(CopyReservationHandoff, CanUseSelectedReservation);
            PrintReservationHandoffCommand = new RelayCommand(PrintReservationHandoff, CanUseSelectedReservation);
            PrintReservationDirectoryCommand = new RelayCommand(PrintReservationDirectory, () => CanPrintReservationDirectory);
            ClearReservationSearchCommand = new RelayCommand(ClearReservationSearch, CanInteractWithReservations);
            ShowActiveReservationsCommand = new RelayCommand(() => SelectedFilter = "Active", CanInteractWithReservations);
            ShowPendingReservationsCommand = new RelayCommand(() => SelectedFilter = "Pending", CanInteractWithReservations);
            ShowConfirmedReservationsCommand = new RelayCommand(() => SelectedFilter = "Confirmed", CanInteractWithReservations);
            ShowUpcomingReservationsCommand = new RelayCommand(() => SelectedFilter = "Upcoming (7 days)", CanInteractWithReservations);
        }

        private async Task LoadReservationsAsync()
        {
            if (IsLoading)
            {
                return;
            }

            try
            {
                IsLoading = true;
                var preferredReservationId = SelectedReservation?.ReservationID;
                var reservations = await _reservationService.GetAllReservationsAsync();
                Reservations.Clear();
                foreach (var reservation in reservations)
                {
                    Reservations.Add(reservation);
                }
                ApplyFilter(preferredReservationId);
            }
            catch (Exception ex)
            {
                ClearReservationStateAfterLoadFailure();
                await _dialogService.ShowErrorAsync("Error loading reservations", $"{ex.Message} The reservation list has been cleared until reload succeeds.");
            }
            finally
            {
                IsLoading = false;
                NotifyReservationListStateChanged();
            }
        }

        private async Task<bool> RefreshReservationsAfterOperationFailureAsync(int? preferredReservationId = null)
        {
            try
            {
                var reservations = await _reservationService.GetAllReservationsAsync();
                Reservations.Clear();
                foreach (var reservation in reservations)
                {
                    Reservations.Add(reservation);
                }
                ApplyFilter(preferredReservationId);
                NotifyCommandStatesAndSummaries();
                NotifyReservationListStateChanged();
                return true;
            }
            catch
            {
                ClearReservationStateAfterLoadFailure();
                return false;
            }
        }

        private static string AppendReservationRefreshMessage(string message, bool refreshed) => refreshed
            ? $"{message} The reservation list has been refreshed in case saved state changed before the failure."
            : $"{message} The reservation list could not be refreshed, so visible reservation rows were cleared until reload succeeds.";

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
                    ApplyFilter(id);
                    await _dialogService.ShowInfoAsync("Success", "Reservation created successfully");
                }
                catch (Exception ex)
                {
                    var refreshed = await RefreshReservationsAfterOperationFailureAsync(newReservation.ReservationID > 0 ? newReservation.ReservationID : null);
                    await _dialogService.ShowErrorAsync("Error creating reservation", AppendReservationRefreshMessage(ex.Message, refreshed));
                }
            }
        }

        private async Task EditReservationAsync()
        {
            if (SelectedReservation == null) return;

            var clone = CloneReservation(SelectedReservation);

            var result = await _dialogService.ShowReservationEditDialogAsync(clone, isNew: false);
            if (result)
            {
                try
                {
                    await _reservationService.UpdateReservationAsync(clone);
                    var originalId = SelectedReservation.ReservationID;
                    var index = Reservations.IndexOf(SelectedReservation);
                    if (index >= 0) Reservations[index] = clone;
                    ApplyFilter(originalId);
                    await _dialogService.ShowInfoAsync("Success", "Reservation updated successfully");
                }
                catch (Exception ex)
                {
                    var refreshed = await RefreshReservationsAfterOperationFailureAsync(clone.ReservationID);
                    await _dialogService.ShowErrorAsync("Error updating reservation", AppendReservationRefreshMessage(ex.Message, refreshed));
                }
            }
        }

        private async Task DeleteReservationAsync()
        {
            if (SelectedReservation == null) return;

            var reservation = SelectedReservation;
            var confirmed = await _dialogService.ShowConfirmAsync(
                "Delete Reservation",
                $"Delete reservation #{reservation.ReservationID} for {ValueOrNotRecorded(reservation.CustomerName)}?");

            if (confirmed)
            {
                try
                {
                    await _reservationService.DeleteReservationAsync(reservation.ReservationID);
                    Reservations.Remove(reservation);
                    ApplyFilter();
                    await _dialogService.ShowInfoAsync("Success", "Reservation deleted successfully");
                }
                catch (Exception ex)
                {
                    var refreshed = await RefreshReservationsAfterOperationFailureAsync(reservation.ReservationID);
                    await _dialogService.ShowErrorAsync("Error deleting reservation", AppendReservationRefreshMessage(ex.Message, refreshed));
                }
            }
        }

        private async Task ConfirmReservationAsync()
        {
            if (SelectedReservation == null) return;

            var reservationId = SelectedReservation.ReservationID;
            try
            {
                await _reservationService.ConfirmReservationAsync(reservationId);
                SelectedReservation.Status = "Confirmed";
                ApplyFilter(reservationId);
                await _dialogService.ShowInfoAsync("Success", "Reservation confirmed");
            }
            catch (Exception ex)
            {
                var refreshed = await RefreshReservationsAfterOperationFailureAsync(reservationId);
                await _dialogService.ShowErrorAsync("Error confirming reservation", AppendReservationRefreshMessage(ex.Message, refreshed));
            }
        }

        private async Task CancelReservationAsync()
        {
            if (SelectedReservation == null) return;

            var reservation = SelectedReservation;
            var confirmed = await _dialogService.ShowConfirmAsync(
                "Cancel Reservation",
                $"Cancel reservation #{reservation.ReservationID} for {ValueOrNotRecorded(reservation.CustomerName)}?");

            if (confirmed)
            {
                try
                {
                    await _reservationService.CancelReservationAsync(reservation.ReservationID);
                    reservation.Status = "Cancelled";
                    ApplyFilter(reservation.ReservationID);
                    await _dialogService.ShowInfoAsync("Success", "Reservation cancelled");
                }
                catch (Exception ex)
                {
                    var refreshed = await RefreshReservationsAfterOperationFailureAsync(reservation.ReservationID);
                    await _dialogService.ShowErrorAsync("Error cancelling reservation", AppendReservationRefreshMessage(ex.Message, refreshed));
                }
            }
        }

        private async Task FulfillReservationAsync()
        {
            if (SelectedReservation == null) return;

            var rentalIdText = await _dialogService.ShowInputDialogAsync(
                "Fulfill Reservation",
                "Enter the Rental ID created during checkout for this reservation:");

            if (!string.IsNullOrWhiteSpace(rentalIdText) && int.TryParse(rentalIdText, out var rentalId))
            {
                var reservationId = SelectedReservation.ReservationID;
                try
                {
                    await _reservationService.FulfillReservationAsync(reservationId, rentalId);
                    SelectedReservation.Status = "Fulfilled";
                    SelectedReservation.RentalID = rentalId;
                    ApplyFilter(reservationId);
                    await _dialogService.ShowInfoAsync("Success", "Reservation marked as fulfilled");
                }
                catch (Exception ex)
                {
                    var refreshed = await RefreshReservationsAfterOperationFailureAsync(reservationId);
                    await _dialogService.ShowErrorAsync("Error fulfilling reservation", AppendReservationRefreshMessage(ex.Message, refreshed));
                }
            }
        }

        private void ApplyFilter(int? preferredReservationId = null)
        {
            preferredReservationId ??= SelectedReservation?.ReservationID;

            var filtered = Reservations.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.Trim();
                filtered = filtered.Where(r =>
                    Contains(r.ReservationID.ToString(), search) ||
                    Contains(r.ItemNumber, search) ||
                    Contains(r.ItemName, search) ||
                    Contains(r.CustomerName, search) ||
                    Contains(r.Status, search) ||
                    Contains(r.Notes, search));
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

            var visibleRows = new System.Collections.Generic.List<Reservation>(MaxVisibleReservationRows);
            var matchedCount = 0;
            foreach (var reservation in filtered.OrderBy(r => r.StartDate).ThenBy(r => r.CustomerName).ThenBy(r => r.ReservationID))
            {
                matchedCount++;
                if (visibleRows.Count < MaxVisibleReservationRows)
                {
                    visibleRows.Add(reservation);
                }
            }

            _matchedReservationCount = matchedCount;
            ApplyFilteredReservationWindow(visibleRows);
            SelectBestReservationAfterRefresh(preferredReservationId);
            NotifyReservationListStateChanged();
        }

        private void ApplyFilteredReservationWindow(System.Collections.Generic.IReadOnlyList<Reservation> visibleRows)
        {
            if (FilteredReservations.Count == visibleRows.Count && FilteredReservations.Select((row, index) => ReferenceEquals(row, visibleRows[index])).All(match => match))
            {
                return;
            }

            FilteredReservations.Clear();
            foreach (var reservation in visibleRows)
            {
                FilteredReservations.Add(reservation);
            }
        }

        private void ClearReservationSearch()
        {
            SearchText = string.Empty;
            SelectedFilter = "Active";
            ApplyFilter();
        }

        private void ClearReservationStateAfterLoadFailure()
        {
            Reservations.Clear();
            FilteredReservations.Clear();
            _matchedReservationCount = 0;
            SelectedReservation = null;
            NotifyCommandStatesAndSummaries();
            NotifyReservationListStateChanged();
        }

        private void OpenReservationDetails()
        {
            if (SelectedReservation == null)
                return;

            _dialogService.ShowInfo(CreateReservationHandoffText(SelectedReservation), $"Reservation #{SelectedReservation.ReservationID}");
        }

        private void CopyReservationHandoff()
        {
            if (SelectedReservation == null)
                return;

            try
            {
                System.Windows.Clipboard.SetText(CreateReservationHandoffText(SelectedReservation));
                _dialogService.ShowInfo("Reservation handoff copied to the clipboard.", "Reservation Handoff");
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to copy reservation handoff: {ex.Message}", "Copy Failed");
            }
        }

        private void PrintReservationHandoff()
        {
            if (SelectedReservation == null)
                return;

            try
            {
                var reservation = SelectedReservation;
                var doc = CreateReservationDocument($"Reservation Handoff - #{reservation.ReservationID}");
                var table = CreateKeyValueTable();
                var group = table.RowGroups[0];
                AddKeyValueRow(group, "Reservation #:", reservation.ReservationID.ToString());
                AddKeyValueRow(group, "Status:", reservation.StatusDisplay);
                AddKeyValueRow(group, "Customer:", reservation.CustomerName);
                AddKeyValueRow(group, "Item:", $"{ValueOrNotRecorded(reservation.ItemNumber)} - {ValueOrNotRecorded(reservation.ItemName)}");
                AddKeyValueRow(group, "Quantity:", reservation.Quantity.ToString());
                AddKeyValueRow(group, "Dates:", $"{reservation.StartDate:yyyy-MM-dd} to {reservation.EndDate:yyyy-MM-dd}");
                AddKeyValueRow(group, "Rental ID:", reservation.RentalID?.ToString() ?? "Not fulfilled yet");
                AddKeyValueRow(group, "Notes:", ValueOrNotRecorded(reservation.Notes));
                AddKeyValueRow(group, "Next action:", SelectedReservationNextAction);
                AddKeyValueRow(group, "Shelf checklist:", SelectedReservationShelfChecklist);
                doc.Blocks.Add(table);
                _dialogService.ShowPrintPreview(doc, $"Reservation {reservation.ReservationID}", string.Empty);
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to print reservation handoff: {ex.Message}", "Print Failed");
            }
        }

        private void PrintReservationDirectory()
        {
            if (!CanPrintReservationDirectory)
            {
                _dialogService.ShowInfo("There are no reservations ready to print for the current filter.", "Reservation Directory");
                return;
            }

            try
            {
                var matchedRows = MatchingReservationCount;
                var visibleRows = VisibleReservationCount;
                var hiddenFromGridRows = OmittedReservationCount;
                var printRows = FilteredReservations.Take(MaxReservationPrintRows).ToList();
                var omittedRows = Math.Max(0, matchedRows - printRows.Count);
                var doc = CreateReservationDocument("Reservation Directory", fontSize: 11);
                doc.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:yyyy-MM-dd HH:mm} | Filter: {SelectedFilter} | Search: {ValueOrNotRecorded(SearchText)} | Matched: {matchedRows} | Visible grid: {visibleRows} | Hidden from grid: {hiddenFromGridRows} | Printed: {printRows.Count} | Print omitted: {omittedRows} | {ReservationResultsSummary}"))
                {
                    FontSize = 10,
                    Margin = new Thickness(0, 0, 0, 8)
                });

                if (omittedRows > 0)
                {
                    doc.Blocks.Add(new Paragraph(new Run($"Large reservation preview limited to the first {MaxReservationPrintRows} visible rows to keep print preview responsive. The live grid shows up to {MaxVisibleReservationRows} rows; narrow the status filter or search before printing a full shelf-pick packet."))
                    {
                        FontSize = 10,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(0, 0, 0, 8)
                    });
                }

                var table = new Table { CellSpacing = 0 };
                table.Columns.Add(new TableColumn { Width = new GridLength(0.85, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.1, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.65, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.65, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.0, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.0, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.05, GridUnitType.Star) });

                var group = new TableRowGroup();
                table.RowGroups.Add(group);
                AddPrintRow(group, true, "Hold #", "Item #", "Item", "Customer", "Start", "End", "Status");

                foreach (var reservation in printRows)
                {
                    AddPrintRow(
                        group,
                        false,
                        reservation.ReservationID.ToString(),
                        reservation.ItemNumber,
                        reservation.ItemName,
                        reservation.CustomerName,
                        reservation.StartDate.ToString("yyyy-MM-dd"),
                        reservation.EndDate.ToString("yyyy-MM-dd"),
                        reservation.StatusDisplay);
                }

                doc.Blocks.Add(table);
                doc.Blocks.Add(new Paragraph(new Run("Review pending, confirmed, upcoming, fulfilled, cancelled, linked Rental ID, hidden-from-grid counts, and print-omitted counts before shelf pickup or customer handoff."))
                {
                    FontSize = 10,
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(0, 10, 0, 0)
                });
                _dialogService.ShowPrintPreview(doc, "Reservation Directory", ReservationPrintStatus);
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to print reservation directory: {ex.Message}", "Print Failed");
            }
        }

        private void SelectBestReservationAfterRefresh(int? preferredReservationId = null)
        {
            if (FilteredReservations.Count == 0)
            {
                SelectedReservation = null;
                return;
            }

            SelectedReservation = preferredReservationId.HasValue
                ? FilteredReservations.FirstOrDefault(r => r.ReservationID == preferredReservationId.Value) ?? FilteredReservations.FirstOrDefault()
                : FilteredReservations.FirstOrDefault();
        }

        private void NotifyCommandStatesAndSummaries()
        {
            LoadReservationsCommand.NotifyCanExecuteChanged();
            AddReservationCommand.NotifyCanExecuteChanged();
            EditReservationCommand.NotifyCanExecuteChanged();
            DeleteReservationCommand.NotifyCanExecuteChanged();
            ConfirmReservationCommand.NotifyCanExecuteChanged();
            CancelReservationCommand.NotifyCanExecuteChanged();
            FulfillReservationCommand.NotifyCanExecuteChanged();
            RefreshCommand.NotifyCanExecuteChanged();
            OpenReservationDetailsCommand.NotifyCanExecuteChanged();
            CopyReservationHandoffCommand.NotifyCanExecuteChanged();
            PrintReservationHandoffCommand.NotifyCanExecuteChanged();
            PrintReservationDirectoryCommand.NotifyCanExecuteChanged();
            ClearReservationSearchCommand.NotifyCanExecuteChanged();
            ShowActiveReservationsCommand.NotifyCanExecuteChanged();
            ShowPendingReservationsCommand.NotifyCanExecuteChanged();
            ShowConfirmedReservationsCommand.NotifyCanExecuteChanged();
            ShowUpcomingReservationsCommand.NotifyCanExecuteChanged();
            NotifySelectionSummariesChanged();
            OnPropertyChanged(nameof(ReservationResultsSummary));
            OnPropertyChanged(nameof(ReservationVisibleWindowSummary));
        }

        private void NotifyReservationListStateChanged()
        {
            OnPropertyChanged(nameof(ReservationResultsSummary));
            OnPropertyChanged(nameof(VisibleReservationCount));
            OnPropertyChanged(nameof(TotalReservationCount));
            OnPropertyChanged(nameof(MatchingReservationCount));
            OnPropertyChanged(nameof(OmittedReservationCount));
            OnPropertyChanged(nameof(HasOmittedReservationRows));
            OnPropertyChanged(nameof(ReservationVisibleWindowSummary));
            OnPropertyChanged(nameof(IsFilterActive));
            OnPropertyChanged(nameof(ReservationEmptyTitle));
            OnPropertyChanged(nameof(ReservationEmptyMessage));
            OnPropertyChanged(nameof(CanPrintReservationDirectory));
            OnPropertyChanged(nameof(ReservationPrintStatus));
            PrintReservationDirectoryCommand.NotifyCanExecuteChanged();
        }

        private void NotifySelectionSummariesChanged()
        {
            OnPropertyChanged(nameof(SelectedReservationTitle));
            OnPropertyChanged(nameof(SelectedReservationSubtitle));
            OnPropertyChanged(nameof(SelectedReservationTiming));
            OnPropertyChanged(nameof(SelectedReservationNextAction));
            OnPropertyChanged(nameof(SelectedReservationShelfChecklist));
            OnPropertyChanged(nameof(SelectedReservationDetail));
            OnPropertyChanged(nameof(SelectedReservationSummary));
        }

        private static Reservation CloneReservation(Reservation source) => new()
        {
            ReservationID = source.ReservationID,
            ItemID = source.ItemID,
            CustomerID = source.CustomerID,
            ItemNumber = source.ItemNumber,
            ItemName = source.ItemName,
            CustomerName = source.CustomerName,
            ReservationDate = source.ReservationDate,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            Quantity = source.Quantity,
            Status = source.Status,
            Notes = source.Notes,
            CreatedByUserID = source.CreatedByUserID,
            CreatedAt = source.CreatedAt,
            RentalID = source.RentalID
        };

        private static bool Contains(string? value, string search) =>
            !string.IsNullOrWhiteSpace(value) && value.Contains(search, StringComparison.OrdinalIgnoreCase);

        private static string CreateReservationHandoffText(Reservation reservation)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Reservation #{reservation.ReservationID}");
            builder.AppendLine($"Status: {reservation.StatusDisplay}");
            builder.AppendLine($"Customer: {ValueOrNotRecorded(reservation.CustomerName)}");
            builder.AppendLine($"Item: {ValueOrNotRecorded(reservation.ItemNumber)} - {ValueOrNotRecorded(reservation.ItemName)}");
            builder.AppendLine($"Quantity: {reservation.Quantity}");
            builder.AppendLine($"Dates: {reservation.StartDate:yyyy-MM-dd} to {reservation.EndDate:yyyy-MM-dd}");
            builder.AppendLine($"Rental ID: {(reservation.RentalID.HasValue ? reservation.RentalID.Value.ToString() : "Not fulfilled yet")}");
            builder.AppendLine($"Notes: {ValueOrNotRecorded(reservation.Notes)}");
            builder.AppendLine();
            builder.AppendLine("Shelf handoff: verify item number and quantity, check condition, match customer at pickup, create the rental, then fulfill this reservation with the Rental ID.");
            return builder.ToString();
        }

        private static FlowDocument CreateReservationDocument(string title, double fontSize = 12)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(40),
                ColumnWidth = double.PositiveInfinity,
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = fontSize
            };
            doc.Blocks.Add(new Paragraph(new Run(title))
            {
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            });
            return doc;
        }

        private static Table CreateKeyValueTable()
        {
            var table = new Table { CellSpacing = 0 };
            table.Columns.Add(new TableColumn { Width = new GridLength(135) });
            table.Columns.Add(new TableColumn { Width = new GridLength(520) });
            table.RowGroups.Add(new TableRowGroup());
            return table;
        }

        private static void AddKeyValueRow(TableRowGroup group, string label, string? value)
        {
            var row = new TableRow();
            AddCell(row, label, isHeader: true);
            AddCell(row, ValueOrNotRecorded(value));
            group.Rows.Add(row);
        }

        private static void AddPrintRow(TableRowGroup group, bool isHeader, params string?[] values)
        {
            var row = new TableRow();
            foreach (var value in values)
            {
                AddCell(row, ValueOrNotRecorded(value), isHeader);
            }
            group.Rows.Add(row);
        }

        private static void AddCell(TableRow row, string text, bool isHeader = false)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(text)))
            {
                Padding = new Thickness(4, 3, 4, 3),
                BorderBrush = System.Windows.Media.Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 0, 0.5),
                FontWeight = isHeader ? FontWeights.SemiBold : FontWeights.Normal
            });
        }

        private static string ValueOrNotRecorded(string? value) =>
            string.IsNullOrWhiteSpace(value) ? "Not recorded" : value.Trim();

        private bool CanInteractWithReservations() => !IsLoading;

        private bool CanRefreshReservations() => !IsLoading;

        private bool CanUseSelectedReservation() => !IsLoading && SelectedReservation != null;

        private bool CanEdit() => !IsLoading && SelectedReservation != null && SelectedReservation.Status != "Fulfilled";

        private bool CanDelete() => !IsLoading && SelectedReservation != null;

        private bool CanConfirm() => !IsLoading && SelectedReservation != null && SelectedReservation.Status == "Pending";

        private bool CanCancel() => !IsLoading && SelectedReservation != null && SelectedReservation.IsActive;

        private bool CanFulfill() => !IsLoading && SelectedReservation != null && SelectedReservation.Status == "Confirmed";
    }
}

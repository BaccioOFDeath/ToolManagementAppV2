using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.ViewModels
{
    /// <summary>
    /// View model backing the ManageRentalsPage. Provides filtering and
    /// commands for common rental operations.
    /// </summary>
    public class ManageRentalsViewModel : ObservableObject
    {
        private readonly IRentalService _rentalService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ManageRentalsViewModel> _logger;
        private List<RentalModel> _allRentals = new();

        public ObservableCollection<RentalModel> Rentals { get; } = new();

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

        public ObservableCollection<string> StatusOptions { get; } =
            new ObservableCollection<string> { "All", "Rented", "Returned" };

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
                    PrintRentalCommand.NotifyCanExecuteChanged();
                    PrintPickingSlipCommand.NotifyCanExecuteChanged();
                    PrintInvoiceCommand.NotifyCanExecuteChanged();
                    DeleteRentalCommand.NotifyCanExecuteChanged();
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
        public IRelayCommand PrintRentalCommand { get; }
        public IRelayCommand PrintPickingSlipCommand { get; }
        public IRelayCommand PrintInvoiceCommand { get; }
        public IAsyncRelayCommand DeleteRentalCommand { get; }

        public ManageRentalsViewModel(IRentalService rentalService, IDialogService dialogService, ILogger<ManageRentalsViewModel>? logger = null)
        {
            _rentalService = rentalService;
            _dialogService = dialogService;
            _logger = logger ?? NullLogger<ManageRentalsViewModel>.Instance;

            ApplyFilterCommand = new RelayCommand(ApplyFilter);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            CheckInCommand = new AsyncRelayCommand(CheckInAsync, () => SelectedRental != null);
            ExtendCommand = new AsyncRelayCommand(ExtendAsync, () => SelectedRental != null);
            OpenHistoryCommand = new AsyncRelayCommand(OpenHistoryAsync, () => SelectedRental != null);
            PrintRentalCommand = new RelayCommand(PrintRental, () => SelectedRental != null);
            PrintPickingSlipCommand = new RelayCommand(PrintPickingSlip, () => SelectedRental != null);
            PrintInvoiceCommand = new RelayCommand(PrintInvoice, () => SelectedRental != null);
            DeleteRentalCommand = new AsyncRelayCommand(DeleteRentalAsync, () => SelectedRental != null);
        }

        /// <summary>Loads all rentals from the service.</summary>
        public async Task LoadRentalsAsync()
        {
            IsLoading = true;
            try
            {
                _allRentals = await _rentalService.GetAllRentalsAsync();
                Rentals.ReplaceRange(_allRentals);
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

        void ApplyFilter()
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
                    (r.CustomerName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (FilterFrom.HasValue)
                filtered = filtered.Where(r => r.RentalDate >= FilterFrom.Value);
            if (FilterTo.HasValue)
                filtered = filtered.Where(r => r.RentalDate <= FilterTo.Value);
            if (!string.IsNullOrWhiteSpace(SelectedStatus) && SelectedStatus != "All")
                filtered = filtered.Where(r => string.Equals(r.Status, SelectedStatus, StringComparison.OrdinalIgnoreCase));

            Rentals.ReplaceRange(filtered);
        }

        void ClearFilter()
        {
            SearchText = string.Empty;
            FilterFrom = null;
            FilterTo = null;
            SelectedStatus = StatusOptions.First();
            Rentals.ReplaceRange(_allRentals);
        }

        async Task CheckInAsync()
        {
            if (SelectedRental == null)
                return;
            try
            {
                IsLoading = true;
                await _rentalService.ReturnItemAsync(SelectedRental.RentalID, DateTime.Today);
                await LoadRentalsAsync();
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to check in rentals.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check in rental {RentalID}", SelectedRental.RentalID);
                await _dialogService.ShowInfoAsync($"Failed to check in rental: {ex.Message}", "Error");
            }
            finally
            {
                IsLoading = false;
            }
        }

        async Task ExtendAsync()
        {
            if (SelectedRental == null)
                return;
            try
            {
                IsLoading = true;
                var newDueDate = SelectedRental.DueDate.AddDays(7);
                await _rentalService.ExtendRentalAsync(SelectedRental.RentalID, newDueDate);
                await LoadRentalsAsync();
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to extend rentals.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extend rental {RentalID}", SelectedRental.RentalID);
                await _dialogService.ShowInfoAsync($"Failed to extend rental: {ex.Message}", "Error");
            }
            finally
            {
                IsLoading = false;
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

        void PrintRental()
        {
            if (SelectedRental == null)
                return;
            try
            {
                var doc = new FlowDocument
                {
                    PagePadding = new Thickness(40),
                    FontFamily = new System.Windows.Media.FontFamily("Calibri"),
                    FontSize = 16
                };

                doc.Blocks.Add(new Paragraph(new Bold(new Run("Rental Information")))
                {
                    FontSize = 22,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 20)
                });

                var table = new Table();
                table.Columns.Add(new TableColumn { Width = new GridLength(150) });
                table.Columns.Add(new TableColumn());
                var group = new TableRowGroup();
                table.RowGroups.Add(group);

                void AddRow(string label, string value)
                {
                    var row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph(new Run(label))
                    {
                        FontWeight = FontWeights.Bold
                    }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(value ?? string.Empty))));
                    group.Rows.Add(row);
                }

                AddRow("Rental #:", SelectedRental.RentalID.ToString());
                AddRow("ItemModel #:", SelectedRental.ItemNumber);
                AddRow("Customer:", SelectedRental.CustomerName);
                AddRow("Rental Date:", SelectedRental.RentalDate.ToString("yyyy-MM-dd HH:mm"));
                AddRow("Due Date:", SelectedRental.DueDate.ToString("yyyy-MM-dd HH:mm"));
                AddRow("Return Date:", SelectedRental.ReturnDate?.ToString("yyyy-MM-dd HH:mm") ?? "N/A");
                AddRow("Status:", SelectedRental.Status ?? string.Empty);

                doc.Blocks.Add(table);

                _dialogService.ShowPrintPreview(doc, $"Rental {SelectedRental.RentalID}", string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to print rental {RentalID}", SelectedRental?.RentalID);
                _dialogService.ShowInfo($"Failed to print rental: {ex.Message}", "Error");
            }
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

            var rentalToDelete = SelectedRental;
            try
            {
                IsLoading = true;
                await _rentalService.DeleteRentalAsync(rentalToDelete.RentalID);
                _allRentals.Remove(rentalToDelete);
                Rentals.Remove(rentalToDelete);
                SelectedRental = null;
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to delete rentals.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete rental {RentalID}", rentalToDelete?.RentalID);
                await _dialogService.ShowInfoAsync($"Failed to delete rental: {ex.Message}", "Error");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}


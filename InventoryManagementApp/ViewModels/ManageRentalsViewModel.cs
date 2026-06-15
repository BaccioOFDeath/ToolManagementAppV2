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
using InventoryManagementApp.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.ViewModels
{
    public class ManageRentalsViewModel : ObservableObject
    {
        private readonly IRentalService _rentalService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ManageRentalsViewModel> _logger;
        private List<RentalModel> _allRentals = new();

        public ObservableCollection<RentalModel> Rentals { get; } = new();
        public ObservableCollection<RentalModel> ActiveRentals { get; } = new();

        public string SearchSummary => $"{Rentals.Count} result{(Rentals.Count == 1 ? string.Empty : "s")} shown";
        public string CheckedOutSummary => $"{ActiveRentals.Count} item{(ActiveRentals.Count == 1 ? string.Empty : "s")} currently checked out";

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
        public IRelayCommand OpenRentalDetailsCommand { get; }
        public IRelayCommand PrintRentalCommand { get; }
        public IRelayCommand PrintSearchResultsCommand { get; }
        public IRelayCommand PrintCheckedOutCommand { get; }
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
            CheckInCommand = new AsyncRelayCommand(CheckInAsync, CanReturnSelectedRental);
            ExtendCommand = new AsyncRelayCommand(ExtendAsync, CanReturnSelectedRental);
            OpenHistoryCommand = new AsyncRelayCommand(OpenHistoryAsync, () => SelectedRental != null);
            OpenRentalDetailsCommand = new RelayCommand(OpenRentalDetails, () => SelectedRental != null);
            PrintRentalCommand = new RelayCommand(PrintRental, () => SelectedRental != null);
            PrintSearchResultsCommand = new RelayCommand(PrintSearchResults);
            PrintCheckedOutCommand = new RelayCommand(PrintCheckedOut);
            PrintPickingSlipCommand = new RelayCommand(PrintPickingSlip, () => SelectedRental != null);
            PrintInvoiceCommand = new RelayCommand(PrintInvoice, () => SelectedRental != null);
            DeleteRentalCommand = new AsyncRelayCommand(DeleteRentalAsync, () => SelectedRental != null);
        }

        public async Task LoadRentalsAsync()
        {
            IsLoading = true;
            try
            {
                _allRentals = await _rentalService.GetAllRentalsAsync();
                RefreshActiveRentals();
                ApplyFilter();
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
                    (r.ItemLocation?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.CustomerName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (FilterFrom.HasValue)
                filtered = filtered.Where(r => r.RentalDate >= FilterFrom.Value);
            if (FilterTo.HasValue)
                filtered = filtered.Where(r => r.RentalDate <= FilterTo.Value);
            if (!string.IsNullOrWhiteSpace(SelectedStatus) && SelectedStatus != "All")
                filtered = filtered.Where(r => string.Equals(r.Status, SelectedStatus, StringComparison.OrdinalIgnoreCase));

            Rentals.ReplaceRange(filtered);
            OnPropertyChanged(nameof(SearchSummary));
        }

        void ClearFilter()
        {
            SearchText = string.Empty;
            FilterFrom = null;
            FilterTo = null;
            SelectedStatus = StatusOptions.First();
            Rentals.ReplaceRange(_allRentals);
            OnPropertyChanged(nameof(SearchSummary));
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

        void OpenRentalDetails()
        {
            if (SelectedRental == null)
                return;

            var rental = SelectedRental;
            var details = new StringBuilder();
            details.AppendLine($"Rental #: {rental.RentalID}");
            details.AppendLine($"Item #: {rental.ItemNumber}");
            details.AppendLine($"Location: {ValueOrNotRecorded(rental.ItemLocation)}");
            details.AppendLine($"Status: {rental.Status}");
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
                ? "Next steps: check in when returned, extend if approved, or open history for prior usage."
                : "Next steps: open history to inspect prior usage or print this rental record.");

            _dialogService.ShowInfo(details.ToString(), $"Rental Details - {rental.ItemNumber}");
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
                _logger.LogError(ex, "Failed to delete rental {RentalID}", rentalToDelete?.RentalID);
                await _dialogService.ShowInfoAsync($"Failed to delete rental: {ex.Message}", "Error");
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
        }

        bool CanReturnSelectedRental() => SelectedRental != null && IsRentalActive(SelectedRental);

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
    }
}

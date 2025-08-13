using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.ViewModels
{
    /// <summary>
    /// View model backing the ManageRentalsPage. Provides filtering and
    /// commands for common rental operations.
    /// </summary>
    public class ManageRentalsViewModel : ObservableObject
    {
        private readonly IRentalService _rentalService;
        private readonly IDialogService _dialogService;
        private List<RentalModel> _allRentals = new();

        public ObservableCollection<RentalModel> Rentals { get; } = new();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        private DateTime? _filterFrom;
        public DateTime? FilterFrom
        {
            get => _filterFrom;
            set => SetProperty(ref _filterFrom, value);
        }

        private DateTime? _filterTo;
        public DateTime? FilterTo
        {
            get => _filterTo;
            set => SetProperty(ref _filterTo, value);
        }

        public ObservableCollection<string> StatusOptions { get; } =
            new ObservableCollection<string> { "All", "Rented", "Returned" };

        private string _selectedStatus = "All";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
        }

        private RentalModel _selectedRental;
        public RentalModel SelectedRental
        {
            get => _selectedRental;
            set
            {
                if (SetProperty(ref _selectedRental, value))
                {
                    ((RelayCommand)CheckInCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)ExtendCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)OpenHistoryCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)PrintRentalCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)DeleteRentalCommand).NotifyCanExecuteChanged();
                }
            }
        }

        public IRelayCommand ApplyFilterCommand { get; }
        public IRelayCommand ClearFilterCommand { get; }
        public IRelayCommand OpenFilterWindowCommand { get; }
        public IRelayCommand CloseCommand { get; }
        public IRelayCommand CheckInCommand { get; }
        public IRelayCommand ExtendCommand { get; }
        public IRelayCommand OpenHistoryCommand { get; }
        public IRelayCommand PrintRentalCommand { get; }
        public IRelayCommand DeleteRentalCommand { get; }

        public ManageRentalsViewModel(IRentalService rentalService, IDialogService dialogService)
        {
            _rentalService = rentalService;
            _dialogService = dialogService;

            ApplyFilterCommand = new RelayCommand(ApplyFilter);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            OpenFilterWindowCommand = new RelayCommand(OpenFilterWindow);
            CloseCommand = new RelayCommand(CloseFilterWindow);
            CheckInCommand = new RelayCommand(CheckIn, () => SelectedRental != null);
            ExtendCommand = new RelayCommand(Extend, () => SelectedRental != null);
            OpenHistoryCommand = new RelayCommand(OpenHistory, () => SelectedRental != null);
            PrintRentalCommand = new RelayCommand(PrintRental, () => SelectedRental != null);
            DeleteRentalCommand = new RelayCommand(DeleteRental, () => SelectedRental != null);
        }

        /// <summary>Loads all rentals from the service.</summary>
        public void LoadRentals()
        {
            _allRentals = _rentalService.GetAllRentals();
            Rentals.ReplaceRange(_allRentals);
        }

        void ApplyFilter()
        {
            IEnumerable<RentalModel> filtered = _allRentals;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var term = SearchText.Trim();
                filtered = filtered.Where(r =>
                    (r.ToolNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
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

        void OpenFilterWindow()
        {
            _dialogService.ShowRentalsFilter(this);
        }

        void CloseFilterWindow()
        {
            if (System.Windows.Application.Current == null) return;
            var window = System.Windows.Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);
            window?.Close();
        }

        void CheckIn()
        {
            if (SelectedRental == null)
                return;

            _rentalService.ReturnTool(SelectedRental.RentalID, DateTime.Today);
            LoadRentals();
        }

        void Extend()
        {
            if (SelectedRental == null)
                return;

            var newDueDate = SelectedRental.DueDate.AddDays(7);
            _rentalService.ExtendRental(SelectedRental.RentalID, newDueDate);
            LoadRentals();
        }

        void OpenHistory()
        {
            if (SelectedRental == null)
                return;

            var history = _rentalService.GetRentalHistoryForTool(SelectedRental.ToolID);
            var tool = new ToolModel
            {
                ToolID = SelectedRental.ToolID,
                ToolNumber = SelectedRental.ToolNumber,
                NameDescription = SelectedRental.ToolNumber
            };
            _dialogService.ShowRentalHistory(tool, history);
        }

        void PrintRental()
        {
            if (SelectedRental == null)
                return;
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
            AddRow("Tool #:", SelectedRental.ToolNumber);
            AddRow("Customer:", SelectedRental.CustomerName);
            AddRow("Rental Date:", SelectedRental.RentalDate.ToString("yyyy-MM-dd HH:mm"));
            AddRow("Due Date:", SelectedRental.DueDate.ToString("yyyy-MM-dd HH:mm"));
            AddRow("Return Date:", SelectedRental.ReturnDate?.ToString("yyyy-MM-dd HH:mm") ?? "N/A");
            AddRow("Status:", SelectedRental.Status ?? string.Empty);

            doc.Blocks.Add(table);

            _dialogService.ShowPrintPreview(doc, $"Rental {SelectedRental.RentalID}", string.Empty);
        }

        void DeleteRental()
        {
            if (SelectedRental == null)
                return;
            _rentalService.DeleteRental(SelectedRental.RentalID);
            _allRentals.Remove(SelectedRental);
            Rentals.Remove(SelectedRental);
        }
    }
}


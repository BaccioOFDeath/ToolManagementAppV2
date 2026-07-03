using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.ViewModels.Rental
{
    public class RentalHistoryViewModel : ObservableObject, IDisposable
    {
        private readonly List<RentalHistorySearchRow> _allHistory;
        private readonly ILogger<RentalHistoryViewModel> _logger;
        private readonly IDialogService _dialogService;
        private CancellationTokenSource? _searchCts;
        private bool _disposed;

        public ObservableCollection<RentalModel> History { get; }
        public string ItemDisplayName { get; }
        public string WindowSummary => _allHistory.Count == 1
            ? "1 rental record loaded"
            : $"{_allHistory.Count} rental records loaded";
        public string ResultsSummary => History.Count == _allHistory.Count
            ? WindowSummary
            : $"{History.Count} of {_allHistory.Count} records shown";
        public string SearchStatus => IsFiltering
            ? "Searching rental history..."
            : HasActiveSearch
                ? $"{ResultsSummary} for \"{AppliedSearchText}\""
                : WindowSummary;
        public bool HasActiveSearch => !string.IsNullOrWhiteSpace(AppliedSearchText);
        public bool HasNoResults => History.Count == 0;
        public bool CanExportHistory => History.Count > 0 && !IsFiltering;
        public string EmptyStateTitle => HasActiveSearch ? "No matching rental records" : "No rental history records";
        public string EmptyStateMessage => HasActiveSearch
            ? "Clear the search or try a rental number, item number, customer, location, status, or date."
            : "Previous rental activity for this item will appear here once records exist.";
        public string ExportSummary => CanExportHistory
            ? $"Export {History.Count} visible record(s) to CSV."
            : IsFiltering
                ? "Wait for search to finish before exporting."
                : "No visible rental records to export.";
        public string SelectedEntrySummary => SelectedEntry == null
            ? "Select a rental row to see holder, dates, and status. Double-click any row for details."
            : $"Rental #{SelectedEntry.RentalID} | {SelectedEntry.ItemNumber} | {SelectedEntry.CustomerName} | {SelectedEntry.Status}";

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    OnPropertyChanged(nameof(SearchPrompt));
            }
        }

        private string _appliedSearchText = string.Empty;
        public string AppliedSearchText
        {
            get => _appliedSearchText;
            private set
            {
                if (SetProperty(ref _appliedSearchText, value))
                {
                    OnPropertyChanged(nameof(HasActiveSearch));
                    OnPropertyChanged(nameof(SearchStatus));
                    OnPropertyChanged(nameof(EmptyStateTitle));
                    OnPropertyChanged(nameof(EmptyStateMessage));
                }
            }
        }

        private bool _isFiltering;
        public bool IsFiltering
        {
            get => _isFiltering;
            private set
            {
                if (SetProperty(ref _isFiltering, value))
                {
                    SearchCommand.NotifyCanExecuteChanged();
                    ExportCsvCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(SearchStatus));
                    OnPropertyChanged(nameof(CanExportHistory));
                    OnPropertyChanged(nameof(ExportSummary));
                }
            }
        }

        private RentalModel? _selectedEntry;
        public RentalModel? SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                if (SetProperty(ref _selectedEntry, value))
                {
                    OnPropertyChanged(nameof(SelectedEntrySummary));
                    OpenDetailsCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public string SearchPrompt => string.IsNullOrWhiteSpace(SearchText)
            ? "Find by rental #, item, customer, location, status, or date"
            : $"Find \"{SearchText.Trim()}\"";

        public IAsyncRelayCommand SearchCommand { get; }
        public IRelayCommand ClearSearchCommand { get; }
        public IRelayCommand OpenDetailsCommand { get; }
        public IRelayCommand ExportCsvCommand { get; }
        public IRelayCommand CloseCommand { get; }

        public RentalHistoryViewModel(ItemModel? item, IEnumerable<RentalModel>? history, IDialogService dialogService, ILogger<RentalHistoryViewModel>? logger = null)
        {
            ItemDisplayName = item != null
                ? $"{item.ItemNumber} - {item.Name}"
                : "Rental History";

            _allHistory = (history ?? Enumerable.Empty<RentalModel>())
                .OrderByDescending(r => r.RentalDate)
                .ThenByDescending(r => r.RentalID)
                .Select(r => new RentalHistorySearchRow(r))
                .ToList();
            History = new ObservableCollection<RentalModel>(_allHistory.Select(r => r.Rental));
            _logger = logger ?? NullLogger<RentalHistoryViewModel>.Instance;
            _dialogService = dialogService;

            SearchCommand = new AsyncRelayCommand(ExecuteSearchAsync, () => !IsFiltering);
            ClearSearchCommand = new RelayCommand(ClearSearch);
            OpenDetailsCommand = new RelayCommand(OpenDetails, () => SelectedEntry != null);
            ExportCsvCommand = new RelayCommand(ExportCsv, () => CanExportHistory);
            CloseCommand = new RelayCommand(CloseWindow);
        }

        async Task ExecuteSearchAsync()
        {
            ThrowIfDisposed();

            var term = NormalizeSearchTerm(SearchText);
            _searchCts?.Cancel();
            var cts = new CancellationTokenSource();
            _searchCts = cts;
            IsFiltering = true;

            try
            {
                var results = await Task.Run(() => BuildFilteredHistory(term, cts.Token), cts.Token);
                if (cts.IsCancellationRequested)
                    return;

                var previousSelectionId = SelectedEntry?.RentalID;
                History.ReplaceRange(results);
                AppliedSearchText = term;
                RestoreSelection(previousSelectionId);
                NotifyHistoryViewChanged();
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Rental history search canceled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to filter rental history for term {SearchTerm}", term);
                _dialogService.ShowInfo($"Failed to filter rental history: {ex.Message}", "Rental History Search");
            }
            finally
            {
                if (ReferenceEquals(_searchCts, cts))
                {
                    _searchCts = null;
                    IsFiltering = false;
                }
                cts.Dispose();
            }
        }

        void ClearSearch()
        {
            ThrowIfDisposed();

            _searchCts?.Cancel();
            SearchText = string.Empty;
            AppliedSearchText = string.Empty;
            var previousSelectionId = SelectedEntry?.RentalID;
            History.ReplaceRange(_allHistory.Select(r => r.Rental));
            RestoreSelection(previousSelectionId);
            NotifyHistoryViewChanged();
        }

        List<RentalModel> BuildFilteredHistory(string term, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(term))
                return _allHistory.Select(r => r.Rental).ToList();

            return _allHistory
                .Where(r =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return r.Matches(term);
                })
                .Select(r => r.Rental)
                .ToList();
        }

        void RestoreSelection(int? previousSelectionId)
        {
            SelectedEntry = previousSelectionId.HasValue
                ? History.FirstOrDefault(r => r.RentalID == previousSelectionId.Value) ?? History.FirstOrDefault()
                : History.FirstOrDefault();
        }

        void NotifyHistoryViewChanged()
        {
            OnPropertyChanged(nameof(ResultsSummary));
            OnPropertyChanged(nameof(SearchStatus));
            OnPropertyChanged(nameof(HasNoResults));
            OnPropertyChanged(nameof(CanExportHistory));
            OnPropertyChanged(nameof(EmptyStateTitle));
            OnPropertyChanged(nameof(EmptyStateMessage));
            OnPropertyChanged(nameof(ExportSummary));
            ExportCsvCommand.NotifyCanExecuteChanged();
        }

        void OpenDetails()
        {
            if (SelectedEntry == null)
                return;

            var returned = SelectedEntry.ReturnDate?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "Not returned";
            var details = new StringBuilder()
                .AppendLine($"Rental #: {SelectedEntry.RentalID}")
                .AppendLine($"Item #: {SelectedEntry.ItemNumber}")
                .AppendLine($"Location: {SelectedEntry.ItemLocation}")
                .AppendLine($"Customer: {SelectedEntry.CustomerName}")
                .AppendLine($"Checked out: {SelectedEntry.RentalDate:yyyy-MM-dd HH:mm}")
                .AppendLine($"Due back: {SelectedEntry.DueDate:yyyy-MM-dd HH:mm}")
                .AppendLine($"Returned: {returned}")
                .AppendLine($"Status: {SelectedEntry.Status}")
                .AppendLine()
                .AppendLine(SearchStatus)
                .ToString();

            _dialogService.ShowInfo(details, "Rental History Details");
        }

        void ExportCsv()
        {
            if (!CanExportHistory)
                return;

            string? path = null;

            if (System.Windows.Application.Current != null)
            {
                try
                {
                    var dlg = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "CSV Files|*.csv",
                        FileName = BuildExportFileName()
                    };
                    if (dlg.ShowDialog() == true)
                        path = dlg.FileName;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to show save file dialog for rental history export");
                }
            }

            path ??= Path.Combine(Environment.CurrentDirectory, BuildExportFileName());

            var sb = new StringBuilder();
            sb.AppendLine("RentalID,ItemNumber,ItemLocation,CustomerName,RentalDate,DueDate,ReturnDate,Status,FilteredView");
            foreach (var r in History)
            {
                sb.AppendLine(string.Join(',',
                    r.RentalID.ToString(CultureInfo.InvariantCulture),
                    Escape(r.ItemNumber),
                    Escape(r.ItemLocation),
                    Escape(r.CustomerName),
                    r.RentalDate.ToString("o", CultureInfo.InvariantCulture),
                    r.DueDate.ToString("o", CultureInfo.InvariantCulture),
                    r.ReturnDate?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty,
                    Escape(r.Status),
                    Escape(SearchStatus)));
            }

            try
            {
                File.WriteAllText(path, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
                _dialogService.ShowInfo($"Exported {History.Count} rental record(s) to {path}.", "Rental History Export");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export rental history to {Path}", path);
                _dialogService.ShowInfo($"Failed to export rental history: {ex.Message}", "Error");
            }
        }

        string BuildExportFileName()
        {
            var suffix = HasActiveSearch ? "_filtered" : string.Empty;
            return $"rental_history{suffix}_{DateTime.Now:yyyyMMdd_HHmm}.csv";
        }

        static string NormalizeSearchTerm(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        static string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var escaped = value.Replace("\"", "\"\"");
            return escaped.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0
                ? $"\"{escaped}\""
                : escaped;
        }

        void CloseWindow()
        {
            if (System.Windows.Application.Current == null) return;
            var window = System.Windows.Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);
            if (window != null)
                window.Close();
        }

        void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RentalHistoryViewModel));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _disposed = true;
        }

        private sealed class RentalHistorySearchRow
        {
            public RentalHistorySearchRow(RentalModel rental)
            {
                Rental = rental;
                SearchText = string.Join(' ',
                    rental.RentalID.ToString(CultureInfo.InvariantCulture),
                    rental.ItemNumber,
                    rental.ItemLocation,
                    rental.CustomerName,
                    rental.Status,
                    rental.RentalDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    rental.RentalDate.ToString("MMM d yyyy", CultureInfo.InvariantCulture),
                    rental.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    rental.DueDate.ToString("MMM d yyyy", CultureInfo.InvariantCulture),
                    rental.ReturnDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    rental.ReturnDate?.ToString("MMM d yyyy", CultureInfo.InvariantCulture));
            }

            public RentalModel Rental { get; }
            private string SearchText { get; }

            public bool Matches(string term) => SearchText.Contains(term, StringComparison.OrdinalIgnoreCase);
        }
    }
}
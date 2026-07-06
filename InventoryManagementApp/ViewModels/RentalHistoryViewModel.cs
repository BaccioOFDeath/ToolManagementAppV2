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
        internal const int MaxVisibleHistoryRows = 500;

        private readonly List<RentalHistorySearchRow> _allHistory;
        private readonly ILogger<RentalHistoryViewModel> _logger;
        private readonly IDialogService _dialogService;
        private CancellationTokenSource? _searchCts;
        private bool _disposed;
        private int _matchedHistoryCount;

        public ObservableCollection<RentalModel> History { get; }
        public string ItemDisplayName { get; }
        public string WindowSummary => _allHistory.Count == 1
            ? "1 rental record loaded"
            : $"{_allHistory.Count} rental records loaded";
        public string ResultsSummary
        {
            get
            {
                if (HasOmittedHistoryRows)
                    return HasActiveSearch
                        ? $"{History.Count} of {_matchedHistoryCount} matching records shown"
                        : $"{History.Count} of {_matchedHistoryCount} loaded records shown";

                if (HasActiveSearch)
                    return $"{History.Count} of {_allHistory.Count} records shown";

                return WindowSummary;
            }
        }
        public string SearchStatus
        {
            get
            {
                if (IsExportingCsv)
                    return $"Exporting {History.Count} visible rental record(s) to CSV...";

                if (IsFiltering)
                    return "Searching rental history...";

                if (HasOmittedHistoryRows)
                    return HasActiveSearch
                        ? $"Showing first {History.Count} matches for \"{AppliedSearchText}\"; refine search to review {OmittedHistoryCount} omitted row(s)."
                        : $"Showing first {History.Count} rental records; search to narrow {OmittedHistoryCount} omitted row(s).";

                return HasActiveSearch
                    ? $"{ResultsSummary} for \"{AppliedSearchText}\""
                    : WindowSummary;
            }
        }
        public int VisibleHistoryCount => History.Count;
        public int TotalHistoryCount => _allHistory.Count;
        public int OmittedHistoryCount => Math.Max(0, _matchedHistoryCount - History.Count);
        public bool HasOmittedHistoryRows => OmittedHistoryCount > 0;
        public bool HasActiveSearch => !string.IsNullOrWhiteSpace(AppliedSearchText);
        public bool HasNoResults => History.Count == 0;
        public bool IsHistoryBusy => IsFiltering || IsExportingCsv;
        public bool IsEmptyStateVisible => HasNoResults && !IsHistoryBusy;
        public bool CanOpenDetails => SelectedEntry != null && !IsHistoryBusy;
        public bool CanExportHistory => History.Count > 0 && !IsHistoryBusy;
        public bool CanClearSearch => !IsHistoryBusy && (HasActiveSearch || !string.IsNullOrWhiteSpace(SearchText));
        public bool IsHistoryActionReady => !IsHistoryBusy;
        public string HistoryBusyStatus => IsExportingCsv
            ? $"Preparing a CSV export for {History.Count} visible rental record(s). The dialog remains responsive while the file is written."
            : "Searching rental history off the UI path...";
        public string EmptyStateTitle => HasActiveSearch ? "No matching rental records" : "No rental history records";
        public string EmptyStateMessage => HasActiveSearch
            ? "Clear the search or try a rental number, item number, customer, location, status, or date."
            : "Previous rental activity for this item will appear here once records exist.";
        public string ExportSummary
        {
            get
            {
                if (IsExportingCsv)
                    return $"Exporting {History.Count} visible record(s); actions are paused until the CSV is ready.";

                if (IsFiltering)
                    return "Wait for search to finish before exporting.";

                if (History.Count == 0)
                    return "No visible rental records to export.";

                return HasOmittedHistoryRows
                    ? $"Export {History.Count} visible record(s); {OmittedHistoryCount} row(s) are omitted from the current grid for responsiveness."
                    : $"Export {History.Count} visible record(s) to CSV.";
            }
        }
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
                {
                    OnPropertyChanged(nameof(SearchPrompt));
                    OnPropertyChanged(nameof(CanClearSearch));
                    ClearSearchCommand.NotifyCanExecuteChanged();
                }
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
                    OnPropertyChanged(nameof(CanClearSearch));
                    ClearSearchCommand.NotifyCanExecuteChanged();
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
                    NotifyBusyStateChanged();
            }
        }

        private bool _isExportingCsv;
        public bool IsExportingCsv
        {
            get => _isExportingCsv;
            private set
            {
                if (SetProperty(ref _isExportingCsv, value))
                    NotifyBusyStateChanged();
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
                    OnPropertyChanged(nameof(CanOpenDetails));
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
        public IAsyncRelayCommand ExportCsvCommand { get; }
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
            _matchedHistoryCount = _allHistory.Count;
            History = new ObservableCollection<RentalModel>(_allHistory.Take(MaxVisibleHistoryRows).Select(r => r.Rental));
            _logger = logger ?? NullLogger<RentalHistoryViewModel>.Instance;
            _dialogService = dialogService;

            SearchCommand = new AsyncRelayCommand(ExecuteSearchAsync, () => !IsHistoryBusy);
            ClearSearchCommand = new RelayCommand(ClearSearch, () => CanClearSearch);
            OpenDetailsCommand = new RelayCommand(OpenDetails, () => CanOpenDetails);
            ExportCsvCommand = new AsyncRelayCommand(ExportCsvAsync, () => CanExportHistory);
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
                History.ReplaceRange(results.VisibleRows);
                _matchedHistoryCount = results.MatchedCount;
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
            if (IsHistoryBusy)
                return;

            SearchText = string.Empty;
            AppliedSearchText = string.Empty;
            var previousSelectionId = SelectedEntry?.RentalID;
            _matchedHistoryCount = _allHistory.Count;
            History.ReplaceRange(_allHistory.Take(MaxVisibleHistoryRows).Select(r => r.Rental));
            RestoreSelection(previousSelectionId);
            NotifyHistoryViewChanged();
        }

        FilteredHistoryResult BuildFilteredHistory(string term, CancellationToken cancellationToken)
        {
            var visibleRows = new List<RentalModel>(MaxVisibleHistoryRows);
            var matchedCount = 0;

            foreach (var row in _allHistory)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!string.IsNullOrWhiteSpace(term) && !row.Matches(term))
                    continue;

                matchedCount++;
                if (visibleRows.Count < MaxVisibleHistoryRows)
                    visibleRows.Add(row.Rental);
            }

            return new FilteredHistoryResult(visibleRows, matchedCount);
        }

        void RestoreSelection(int? previousSelectionId)
        {
            SelectedEntry = previousSelectionId.HasValue
                ? History.FirstOrDefault(r => r.RentalID == previousSelectionId.Value) ?? History.FirstOrDefault()
                : History.FirstOrDefault();
        }

        void NotifyBusyStateChanged()
        {
            SearchCommand.NotifyCanExecuteChanged();
            ClearSearchCommand.NotifyCanExecuteChanged();
            OpenDetailsCommand.NotifyCanExecuteChanged();
            ExportCsvCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(SearchStatus));
            OnPropertyChanged(nameof(IsHistoryBusy));
            OnPropertyChanged(nameof(HistoryBusyStatus));
            OnPropertyChanged(nameof(IsEmptyStateVisible));
            OnPropertyChanged(nameof(CanOpenDetails));
            OnPropertyChanged(nameof(CanExportHistory));
            OnPropertyChanged(nameof(CanClearSearch));
            OnPropertyChanged(nameof(IsHistoryActionReady));
            OnPropertyChanged(nameof(ExportSummary));
        }

        void NotifyHistoryViewChanged()
        {
            OnPropertyChanged(nameof(ResultsSummary));
            OnPropertyChanged(nameof(SearchStatus));
            OnPropertyChanged(nameof(VisibleHistoryCount));
            OnPropertyChanged(nameof(TotalHistoryCount));
            OnPropertyChanged(nameof(OmittedHistoryCount));
            OnPropertyChanged(nameof(HasOmittedHistoryRows));
            OnPropertyChanged(nameof(HasNoResults));
            OnPropertyChanged(nameof(IsHistoryBusy));
            OnPropertyChanged(nameof(HistoryBusyStatus));
            OnPropertyChanged(nameof(IsEmptyStateVisible));
            OnPropertyChanged(nameof(CanOpenDetails));
            OnPropertyChanged(nameof(CanExportHistory));
            OnPropertyChanged(nameof(CanClearSearch));
            OnPropertyChanged(nameof(IsHistoryActionReady));
            OnPropertyChanged(nameof(EmptyStateTitle));
            OnPropertyChanged(nameof(EmptyStateMessage));
            OnPropertyChanged(nameof(ExportSummary));
            SearchCommand.NotifyCanExecuteChanged();
            ClearSearchCommand.NotifyCanExecuteChanged();
            OpenDetailsCommand.NotifyCanExecuteChanged();
            ExportCsvCommand.NotifyCanExecuteChanged();
        }

        void OpenDetails()
        {
            if (!CanOpenDetails || SelectedEntry == null)
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

        async Task ExportCsvAsync()
        {
            ThrowIfDisposed();

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

            var visibleRows = History.ToList();
            var filteredView = SearchStatus;
            IsExportingCsv = true;

            try
            {
                var csv = await Task.Run(() => BuildCsv(visibleRows, filteredView));
                await File.WriteAllTextAsync(path, csv, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
                _dialogService.ShowInfo($"Exported {visibleRows.Count} rental record(s) to {path}.", "Rental History Export");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export rental history to {Path}", path);
                _dialogService.ShowInfo($"Failed to export rental history: {ex.Message}", "Error");
            }
            finally
            {
                IsExportingCsv = false;
            }
        }

        static string BuildCsv(IReadOnlyList<RentalModel> rows, string filteredView)
        {
            var sb = new StringBuilder();
            sb.AppendLine("RentalID,ItemNumber,ItemLocation,CustomerName,RentalDate,DueDate,ReturnDate,Status,FilteredView");
            foreach (var r in rows)
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
                    Escape(filteredView)));
            }

            return sb.ToString();
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
            _searchCts = null;
            _disposed = true;
        }

        private sealed record FilteredHistoryResult(IReadOnlyList<RentalModel> VisibleRows, int MatchedCount);

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

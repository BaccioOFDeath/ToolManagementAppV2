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
using InventoryManagementApp.Services.Maintenance;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.ViewModels
{
    public class MaintenanceManagementViewModel : ObservableObject
    {
        private const int MaxMaintenancePrintRows = 250;

        private readonly MaintenanceService _maintenanceService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<MaintenanceRecord> MaintenanceRecords { get; }
        public ObservableCollection<MaintenanceRecord> FilteredMaintenanceRecords { get; }

        public string MaintenanceResultsSummary => IsLoading
            ? "Loading maintenance records..."
            : $"{FilteredMaintenanceRecords.Count} of {MaintenanceRecords.Count} maintenance record{(MaintenanceRecords.Count == 1 ? string.Empty : "s")} shown";

        public string MaintenanceBacklogSummary
        {
            get
            {
                if (IsLoading)
                {
                    return "Refreshing maintenance backlog";
                }

                var scheduled = MaintenanceRecords.Count(r => IsScheduled(r));
                var overdue = MaintenanceRecords.Count(r => r.IsOverdue);
                var upcoming = MaintenanceRecords.Count(r => IsUpcoming(r));
                var completed = MaintenanceRecords.Count(r => IsCompleted(r));
                return $"{overdue} overdue | {upcoming} upcoming | {scheduled} scheduled | {completed} completed";
            }
        }

        public bool IsFilterActive => !string.IsNullOrWhiteSpace(SearchText) || !string.Equals(SelectedFilter, "All", StringComparison.Ordinal);

        public string MaintenanceEmptyTitle => MaintenanceRecords.Count == 0
            ? "No maintenance work recorded"
            : "No maintenance work matches this filter";

        public string MaintenanceEmptyMessage => MaintenanceRecords.Count == 0
            ? "Add a work order or refresh the schedule when service history is available."
            : "Clear search, switch the filter, or add a new work order to restart the technician queue.";

        public bool CanPrintMaintenanceList => !IsLoading && FilteredMaintenanceRecords.Count > 0;

        public string MaintenancePrintStatus
        {
            get
            {
                if (IsLoading)
                {
                    return "Print paused while maintenance rows load";
                }

                if (FilteredMaintenanceRecords.Count == 0)
                {
                    return IsFilterActive
                        ? "No filtered rows ready to print"
                        : "No maintenance rows ready to print";
                }

                var visible = FilteredMaintenanceRecords.Count;
                var printed = Math.Min(visible, MaxMaintenancePrintRows);
                var omitted = Math.Max(0, visible - printed);
                var filterContext = IsFilterActive ? "filtered" : "all rows";
                return omitted == 0
                    ? $"Ready to print {printed} {filterContext} row{(printed == 1 ? string.Empty : "s")}."
                    : $"Ready to print first {printed} of {visible} {filterContext} rows; {omitted} omitted from preview.";
            }
        }

        public string SelectedRecordSummary => SelectedRecord == null
            ? "Select or double-click maintenance work to review it, complete it, copy the technician handoff, edit, print, or delete."
            : $"{ValueOrNotRecorded(SelectedRecord.ItemNumber)} | {ValueOrNotRecorded(SelectedRecord.ItemName)} | {SelectedRecord.StatusDisplay} | scheduled {SelectedRecord.ScheduledDate:yyyy-MM-dd}";

        public string SelectedMaintenanceDetail => SelectedRecord == null
            ? "No work selected. Choose a maintenance row or create new work before taking an action."
            : $"{ValueOrNotRecorded(SelectedRecord.MaintenanceType)} for {ValueOrNotRecorded(SelectedRecord.ItemNumber)} - {ValueOrNotRecorded(SelectedRecord.ItemName)}. Description: {ValueOrNotRecorded(SelectedRecord.Description)}. Notes: {ValueOrNotRecorded(SelectedRecord.Notes)}";

        public string SelectedMaintenanceTimingSummary => SelectedRecord == null
            ? "No schedule selected."
            : $"Scheduled {SelectedRecord.ScheduledDate:yyyy-MM-dd}. Completed {FormatDate(SelectedRecord.CompletedDate)}. Performed by {ValueOrNotRecorded(SelectedRecord.PerformedBy)}. Cost {SelectedRecord.Cost:C}.";

        public string SelectedMaintenanceNextAction
        {
            get
            {
                if (SelectedRecord == null)
                {
                    return "Add a maintenance record or select existing work to see the next operational step.";
                }

                if (SelectedRecord.IsOverdue)
                {
                    return "This work is overdue. Hold the item from issue, complete or reschedule the job, then print or copy the updated record for the bench file.";
                }

                if (IsScheduled(SelectedRecord))
                {
                    return "Confirm the item is on the shelf or staged for service, perform the work, mark it complete, and print or copy the record for handoff.";
                }

                if (IsCompleted(SelectedRecord))
                {
                    return "This work is complete. Review the notes, print the record if needed, or edit the entry if follow-up details are missing.";
                }

                return "Review the work details, update the status, and keep the record with the item before it is released.";
            }
        }

        public string SelectedMaintenanceBenchChecklist => SelectedRecord == null
            ? "Select work first, then verify the item tag, confirm service notes, complete or edit the record, and print/copy the handoff before releasing the item."
            : "Verify item tag and location, capture who performed the work, record cost/notes, mark complete when finished, and keep the printed or copied handoff with the item.";

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    NotifyCommandStatesAndSummaries();
                    NotifyMaintenanceListStateChanged();
                }
            }
        }

        private MaintenanceRecord? _selectedRecord;
        public MaintenanceRecord? SelectedRecord
        {
            get => _selectedRecord;
            set
            {
                if (SetProperty(ref _selectedRecord, value))
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

        private string _selectedFilter = "All";
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

        public IAsyncRelayCommand LoadMaintenanceCommand { get; }
        public IAsyncRelayCommand AddMaintenanceCommand { get; }
        public IAsyncRelayCommand EditMaintenanceCommand { get; }
        public IAsyncRelayCommand DeleteMaintenanceCommand { get; }
        public IAsyncRelayCommand CompleteMaintenanceCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }
        public IRelayCommand OpenMaintenanceDetailsCommand { get; }
        public IRelayCommand PrintMaintenanceListCommand { get; }
        public IRelayCommand PrintSelectedMaintenanceCommand { get; }
        public IRelayCommand CopySelectedMaintenanceCommand { get; }
        public IRelayCommand ClearSearchCommand { get; }
        public IRelayCommand ShowOverdueCommand { get; }
        public IRelayCommand ShowUpcomingCommand { get; }
        public IRelayCommand ShowScheduledCommand { get; }

        public MaintenanceManagementViewModel(
            MaintenanceService maintenanceService,
            IDialogService dialogService)
        {
            _maintenanceService = maintenanceService;
            _dialogService = dialogService;

            MaintenanceRecords = new ObservableCollection<MaintenanceRecord>();
            FilteredMaintenanceRecords = new ObservableCollection<MaintenanceRecord>();
            FilterOptions = new ObservableCollection<string>
            {
                "All",
                "Scheduled",
                "Completed",
                "Overdue",
                "Upcoming (30 days)"
            };

            LoadMaintenanceCommand = new AsyncRelayCommand(LoadMaintenanceAsync, CanRefreshMaintenance);
            AddMaintenanceCommand = new AsyncRelayCommand(AddMaintenanceAsync, CanInteractWithMaintenanceList);
            EditMaintenanceCommand = new AsyncRelayCommand(EditMaintenanceAsync, CanEditOrDelete);
            DeleteMaintenanceCommand = new AsyncRelayCommand(DeleteMaintenanceAsync, CanEditOrDelete);
            CompleteMaintenanceCommand = new AsyncRelayCommand(CompleteMaintenanceAsync, CanComplete);
            RefreshCommand = new AsyncRelayCommand(LoadMaintenanceAsync, CanRefreshMaintenance);
            OpenMaintenanceDetailsCommand = new RelayCommand(OpenMaintenanceDetails, CanEditOrDelete);
            PrintMaintenanceListCommand = new RelayCommand(PrintMaintenanceList, () => CanPrintMaintenanceList);
            PrintSelectedMaintenanceCommand = new RelayCommand(PrintSelectedMaintenance, CanEditOrDelete);
            CopySelectedMaintenanceCommand = new RelayCommand(CopySelectedMaintenance, CanEditOrDelete);
            ClearSearchCommand = new RelayCommand(ClearSearch, CanInteractWithMaintenanceList);
            ShowOverdueCommand = new RelayCommand(() => SelectedFilter = "Overdue", CanInteractWithMaintenanceList);
            ShowUpcomingCommand = new RelayCommand(() => SelectedFilter = "Upcoming (30 days)", CanInteractWithMaintenanceList);
            ShowScheduledCommand = new RelayCommand(() => SelectedFilter = "Scheduled", CanInteractWithMaintenanceList);
        }

        private async Task LoadMaintenanceAsync()
        {
            if (IsLoading)
            {
                return;
            }

            try
            {
                IsLoading = true;
                var selectedId = SelectedRecord?.MaintenanceID;
                var records = await _maintenanceService.GetAllMaintenanceRecordsAsync();
                MaintenanceRecords.Clear();
                foreach (var record in records)
                {
                    MaintenanceRecords.Add(record);
                }
                ApplyFilter(selectedId);
            }
            catch (Exception ex)
            {
                ClearMaintenanceStateAfterLoadFailure();
                await _dialogService.ShowErrorAsync("Error loading maintenance records", $"{ex.Message} Maintenance rows were cleared until reload succeeds.");
            }
            finally
            {
                IsLoading = false;
                NotifyMaintenanceListStateChanged();
            }
        }

        private async Task AddMaintenanceAsync()
        {
            var newRecord = new MaintenanceRecord
            {
                ScheduledDate = DateTime.Now.AddDays(7),
                MaintenanceType = "Routine",
                Status = "Scheduled"
            };

            var result = await _dialogService.ShowMaintenanceEditDialogAsync(newRecord, isNew: true);
            if (result)
            {
                try
                {
                    var id = await _maintenanceService.CreateMaintenanceRecordAsync(newRecord);
                    newRecord.MaintenanceID = id;
                    MaintenanceRecords.Insert(0, newRecord);
                    ApplyFilter(newRecord.MaintenanceID);
                    await _dialogService.ShowInfoAsync("Success", "Maintenance record created successfully");
                }
                catch (Exception ex)
                {
                    await RefreshMaintenanceAfterMutationFailureAsync(
                        newRecord.MaintenanceID > 0 ? newRecord.MaintenanceID : null,
                        "Error creating maintenance record",
                        $"{ex.Message} Maintenance rows were refreshed from saved data.");
                }
            }
        }

        private async Task EditMaintenanceAsync()
        {
            if (SelectedRecord == null) return;

            var clone = new MaintenanceRecord
            {
                MaintenanceID = SelectedRecord.MaintenanceID,
                ItemID = SelectedRecord.ItemID,
                ItemNumber = SelectedRecord.ItemNumber,
                ItemName = SelectedRecord.ItemName,
                ScheduledDate = SelectedRecord.ScheduledDate,
                CompletedDate = SelectedRecord.CompletedDate,
                MaintenanceType = SelectedRecord.MaintenanceType,
                Description = SelectedRecord.Description,
                PerformedBy = SelectedRecord.PerformedBy,
                Cost = SelectedRecord.Cost,
                Status = SelectedRecord.Status,
                Notes = SelectedRecord.Notes,
                UserID = SelectedRecord.UserID,
                CreatedAt = SelectedRecord.CreatedAt
            };

            var result = await _dialogService.ShowMaintenanceEditDialogAsync(clone, isNew: false);
            if (result)
            {
                try
                {
                    await _maintenanceService.UpdateMaintenanceRecordAsync(clone);
                    var index = MaintenanceRecords.IndexOf(SelectedRecord);
                    if (index >= 0) MaintenanceRecords[index] = clone;
                    ApplyFilter(clone.MaintenanceID);
                    await _dialogService.ShowInfoAsync("Success", "Maintenance record updated successfully");
                }
                catch (Exception ex)
                {
                    await RefreshMaintenanceAfterMutationFailureAsync(
                        clone.MaintenanceID,
                        "Error updating maintenance record",
                        $"{ex.Message} Maintenance rows were refreshed from saved data.");
                }
            }
        }

        private async Task DeleteMaintenanceAsync()
        {
            if (SelectedRecord == null) return;

            var confirmed = await _dialogService.ShowConfirmAsync(
                "Delete Maintenance Record",
                $"Delete maintenance for {ValueOrNotRecorded(SelectedRecord.ItemName)} scheduled {SelectedRecord.ScheduledDate:yyyy-MM-dd}?");

            if (confirmed)
            {
                var deletedRecord = SelectedRecord;
                try
                {
                    await _maintenanceService.DeleteMaintenanceRecordAsync(deletedRecord.MaintenanceID);
                    MaintenanceRecords.Remove(deletedRecord);
                    ApplyFilter();
                    await _dialogService.ShowInfoAsync("Success", "Maintenance record deleted successfully");
                }
                catch (Exception ex)
                {
                    await RefreshMaintenanceAfterMutationFailureAsync(
                        deletedRecord.MaintenanceID,
                        "Error deleting maintenance record",
                        $"{ex.Message} Maintenance rows were refreshed from saved data.",
                        clearSelectionWhenAffectedRecordIsGone: true);
                }
            }
        }

        private async Task CompleteMaintenanceAsync()
        {
            if (SelectedRecord == null) return;

            var performedBy = await _dialogService.ShowInputDialogAsync(
                "Complete Maintenance",
                "Enter the name of the person who performed the maintenance:");

            if (!string.IsNullOrWhiteSpace(performedBy))
            {
                var completedId = SelectedRecord.MaintenanceID;
                try
                {
                    await _maintenanceService.CompleteMaintenanceAsync(
                        completedId,
                        performedBy,
                        "");
                    SelectedRecord.Status = "Completed";
                    SelectedRecord.CompletedDate = DateTime.Now;
                    SelectedRecord.PerformedBy = performedBy;
                    ApplyFilter(completedId);
                    NotifyCommandStatesAndSummaries();
                    await _dialogService.ShowInfoAsync("Success", "Maintenance marked as completed");
                }
                catch (Exception ex)
                {
                    await RefreshMaintenanceAfterMutationFailureAsync(
                        completedId,
                        "Error completing maintenance",
                        $"{ex.Message} Maintenance rows were refreshed from saved data.");
                }
            }
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
            SelectedFilter = "All";
            ApplyFilter();
        }

        private void ClearMaintenanceStateAfterLoadFailure()
        {
            MaintenanceRecords.Clear();
            FilteredMaintenanceRecords.Clear();
            SelectedRecord = null;
            NotifyCommandStatesAndSummaries();
            NotifyMaintenanceListStateChanged();
        }

        private async Task RefreshMaintenanceAfterMutationFailureAsync(
            int? preferredMaintenanceId,
            string title,
            string message,
            bool clearSelectionWhenAffectedRecordIsGone = false)
        {
            try
            {
                var records = await _maintenanceService.GetAllMaintenanceRecordsAsync();
                MaintenanceRecords.Clear();
                foreach (var record in records)
                {
                    MaintenanceRecords.Add(record);
                }

                ApplyFilter(preferredMaintenanceId);
                if (clearSelectionWhenAffectedRecordIsGone
                    && preferredMaintenanceId.HasValue
                    && MaintenanceRecords.All(r => r.MaintenanceID != preferredMaintenanceId.Value))
                {
                    SelectedRecord = null;
                }

                NotifyCommandStatesAndSummaries();
                NotifyMaintenanceListStateChanged();
                await _dialogService.ShowErrorAsync(title, message);
            }
            catch (Exception refreshEx)
            {
                ClearMaintenanceStateAfterLoadFailure();
                await _dialogService.ShowErrorAsync(title, $"{message} Recovery refresh also failed: {refreshEx.Message} Maintenance rows were cleared until reload succeeds.");
            }
        }

        private void ApplyFilter(int? preferredMaintenanceId = null)
        {
            preferredMaintenanceId ??= SelectedRecord?.MaintenanceID;
            FilteredMaintenanceRecords.Clear();

            var filtered = MaintenanceRecords.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.Trim().ToLowerInvariant();
                filtered = filtered.Where(r =>
                    Searchable(r.ItemNumber).Contains(search) ||
                    Searchable(r.ItemName).Contains(search) ||
                    Searchable(r.MaintenanceType).Contains(search) ||
                    Searchable(r.Description).Contains(search) ||
                    Searchable(r.PerformedBy).Contains(search) ||
                    Searchable(r.Notes).Contains(search));
            }

            filtered = SelectedFilter switch
            {
                "Scheduled" => filtered.Where(r => IsScheduled(r) && r.ScheduledDate >= DateTime.Now),
                "Completed" => filtered.Where(IsCompleted),
                "Overdue" => filtered.Where(r => r.IsOverdue),
                "Upcoming (30 days)" => filtered.Where(IsUpcoming),
                _ => filtered
            };

            var filteredList = filtered
                .OrderBy(r => IsCompleted(r) ? 1 : 0)
                .ThenBy(r => r.ScheduledDate)
                .ThenBy(r => Searchable(r.ItemNumber))
                .ToList();

            foreach (var record in filteredList)
            {
                FilteredMaintenanceRecords.Add(record);
            }

            SelectedRecord = FilteredMaintenanceRecords.FirstOrDefault(r => r.MaintenanceID == preferredMaintenanceId)
                ?? FilteredMaintenanceRecords.FirstOrDefault();

            NotifyMaintenanceListStateChanged();
        }

        private void OpenMaintenanceDetails()
        {
            if (SelectedRecord == null) return;

            var record = SelectedRecord;
            var details = new StringBuilder();
            details.AppendLine($"Maintenance #: {record.MaintenanceID}");
            details.AppendLine($"Item: {ValueOrNotRecorded(record.ItemNumber)} - {ValueOrNotRecorded(record.ItemName)}");
            details.AppendLine($"Type: {ValueOrNotRecorded(record.MaintenanceType)}");
            details.AppendLine($"Status: {record.StatusDisplay}");
            details.AppendLine();
            details.AppendLine($"Scheduled: {record.ScheduledDate:yyyy-MM-dd}");
            details.AppendLine($"Completed: {FormatDate(record.CompletedDate)}");
            details.AppendLine($"Performed by: {ValueOrNotRecorded(record.PerformedBy)}");
            details.AppendLine($"Cost: {record.Cost:C}");
            details.AppendLine();
            details.AppendLine($"Description: {ValueOrNotRecorded(record.Description)}");
            details.AppendLine($"Notes: {ValueOrNotRecorded(record.Notes)}");
            details.AppendLine();
            details.AppendLine(SelectedMaintenanceNextAction);

            _dialogService.ShowInfo(details.ToString(), $"Maintenance Details - {ValueOrNotRecorded(record.ItemNumber)}");
        }

        private void CopySelectedMaintenance()
        {
            if (SelectedRecord == null) return;

            var record = SelectedRecord;
            var handoff = new StringBuilder();
            handoff.AppendLine("Maintenance handoff");
            handoff.AppendLine($"Item: {ValueOrNotRecorded(record.ItemNumber)} - {ValueOrNotRecorded(record.ItemName)}");
            handoff.AppendLine($"Type: {ValueOrNotRecorded(record.MaintenanceType)}");
            handoff.AppendLine($"Status: {record.StatusDisplay}");
            handoff.AppendLine($"Scheduled: {record.ScheduledDate:yyyy-MM-dd}");
            handoff.AppendLine($"Completed: {FormatDate(record.CompletedDate)}");
            handoff.AppendLine($"Performed by: {ValueOrNotRecorded(record.PerformedBy)}");
            handoff.AppendLine($"Description: {ValueOrNotRecorded(record.Description)}");
            handoff.AppendLine($"Notes: {ValueOrNotRecorded(record.Notes)}");
            handoff.AppendLine($"Next action: {SelectedMaintenanceNextAction}");

            try
            {
                System.Windows.Clipboard.SetText(handoff.ToString());
                _dialogService.ShowInfo("Maintenance handoff copied to the clipboard.", "Copied");
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Unable to copy maintenance handoff: {ex.Message}", "Copy Failed");
            }
        }

        private void PrintMaintenanceList()
        {
            if (!CanPrintMaintenanceList)
            {
                _dialogService.ShowInfo("There are no maintenance records ready to print.", "Maintenance Report");
                return;
            }

            try
            {
                var visibleRows = FilteredMaintenanceRecords.Count;
                var printRows = FilteredMaintenanceRecords.Take(MaxMaintenancePrintRows).ToList();
                var omittedRows = Math.Max(0, visibleRows - printRows.Count);
                var doc = CreateMaintenanceDocument("Maintenance Schedule", fontSize: 11);
                doc.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:yyyy-MM-dd HH:mm} | Filter: {SelectedFilter} | Search: {ValueOrNotRecorded(SearchText)} | Visible: {visibleRows} | Printed: {printRows.Count} | Omitted: {omittedRows} | {MaintenanceBacklogSummary}"))
                {
                    FontSize = 10,
                    Margin = new Thickness(0, 0, 0, 8)
                });

                if (omittedRows > 0)
                {
                    doc.Blocks.Add(new Paragraph(new Run($"Large schedule preview limited to the first {MaxMaintenancePrintRows} visible rows to keep print preview responsive. Clear filters or export smaller packets for full handoff review."))
                    {
                        FontSize = 10,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(0, 0, 0, 8)
                    });
                }

                var table = new Table { CellSpacing = 0 };
                table.Columns.Add(new TableColumn { Width = new GridLength(1.05, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.65, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.25, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.05, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.0, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.25, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.05, GridUnitType.Star) });

                var group = new TableRowGroup();
                table.RowGroups.Add(group);
                AddPrintRow(group, true, "Item #", "Name", "Type", "Scheduled", "Status", "Technician", "Completed");
                foreach (var record in printRows)
                {
                    AddPrintRow(group, false, record.ItemNumber, record.ItemName, record.MaintenanceType, record.ScheduledDate.ToString("yyyy-MM-dd"), record.StatusDisplay, record.PerformedBy, FormatDate(record.CompletedDate));
                }

                doc.Blocks.Add(table);
                doc.Blocks.Add(new Paragraph(new Run("Review overdue rows, technician assignment, completed dates, and omitted-row counts before handing this schedule to the bench."))
                {
                    FontSize = 10,
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(0, 10, 0, 0)
                });
                _dialogService.ShowPrintPreview(doc, "Maintenance Schedule", MaintenancePrintStatus);
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to print maintenance report: {ex.Message}", "Print Failed");
            }
        }

        private void PrintSelectedMaintenance()
        {
            if (SelectedRecord == null) return;

            try
            {
                var record = SelectedRecord;
                var doc = CreateMaintenanceDocument($"Maintenance Record - {ValueOrNotRecorded(record.ItemNumber)}");
                var table = CreateKeyValueTable();
                var group = table.RowGroups[0];
                AddKeyValueRow(group, "Maintenance #:", record.MaintenanceID.ToString());
                AddKeyValueRow(group, "Item:", $"{ValueOrNotRecorded(record.ItemNumber)} - {ValueOrNotRecorded(record.ItemName)}");
                AddKeyValueRow(group, "Type:", record.MaintenanceType);
                AddKeyValueRow(group, "Status:", record.StatusDisplay);
                AddKeyValueRow(group, "Scheduled:", record.ScheduledDate.ToString("yyyy-MM-dd"));
                AddKeyValueRow(group, "Completed:", FormatDate(record.CompletedDate));
                AddKeyValueRow(group, "Performed by:", record.PerformedBy);
                AddKeyValueRow(group, "Cost:", record.Cost.ToString("C"));
                AddKeyValueRow(group, "Description:", record.Description);
                AddKeyValueRow(group, "Notes:", record.Notes);
                AddKeyValueRow(group, "Next action:", SelectedMaintenanceNextAction);
                doc.Blocks.Add(table);

                _dialogService.ShowPrintPreview(doc, $"Maintenance {record.MaintenanceID}", SelectedMaintenanceNextAction);
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to print maintenance record: {ex.Message}", "Print Failed");
            }
        }

        private bool CanInteractWithMaintenanceList() => !IsLoading;

        private bool CanRefreshMaintenance() => !IsLoading;

        private bool CanEditOrDelete() => !IsLoading && SelectedRecord != null;

        private bool CanComplete() => !IsLoading && SelectedRecord != null && IsScheduled(SelectedRecord);

        private void NotifyCommandStatesAndSummaries()
        {
            LoadMaintenanceCommand.NotifyCanExecuteChanged();
            AddMaintenanceCommand.NotifyCanExecuteChanged();
            EditMaintenanceCommand.NotifyCanExecuteChanged();
            DeleteMaintenanceCommand.NotifyCanExecuteChanged();
            CompleteMaintenanceCommand.NotifyCanExecuteChanged();
            RefreshCommand.NotifyCanExecuteChanged();
            OpenMaintenanceDetailsCommand.NotifyCanExecuteChanged();
            PrintMaintenanceListCommand.NotifyCanExecuteChanged();
            PrintSelectedMaintenanceCommand.NotifyCanExecuteChanged();
            CopySelectedMaintenanceCommand.NotifyCanExecuteChanged();
            ClearSearchCommand.NotifyCanExecuteChanged();
            ShowOverdueCommand.NotifyCanExecuteChanged();
            ShowUpcomingCommand.NotifyCanExecuteChanged();
            ShowScheduledCommand.NotifyCanExecuteChanged();
            OnSelectedRecordSummariesChanged();
            OnPropertyChanged(nameof(MaintenanceBacklogSummary));
            OnPropertyChanged(nameof(MaintenanceResultsSummary));
        }

        private void NotifyMaintenanceListStateChanged()
        {
            OnPropertyChanged(nameof(MaintenanceResultsSummary));
            OnPropertyChanged(nameof(MaintenanceBacklogSummary));
            OnPropertyChanged(nameof(IsFilterActive));
            OnPropertyChanged(nameof(MaintenanceEmptyTitle));
            OnPropertyChanged(nameof(MaintenanceEmptyMessage));
            OnPropertyChanged(nameof(CanPrintMaintenanceList));
            OnPropertyChanged(nameof(MaintenancePrintStatus));
            PrintMaintenanceListCommand.NotifyCanExecuteChanged();
        }

        private void OnSelectedRecordSummariesChanged()
        {
            OnPropertyChanged(nameof(SelectedRecordSummary));
            OnPropertyChanged(nameof(SelectedMaintenanceDetail));
            OnPropertyChanged(nameof(SelectedMaintenanceTimingSummary));
            OnPropertyChanged(nameof(SelectedMaintenanceNextAction));
            OnPropertyChanged(nameof(SelectedMaintenanceBenchChecklist));
        }

        private static bool IsScheduled(MaintenanceRecord record) =>
            string.Equals(record.Status, "Scheduled", StringComparison.OrdinalIgnoreCase);

        private static bool IsCompleted(MaintenanceRecord record) =>
            string.Equals(record.Status, "Completed", StringComparison.OrdinalIgnoreCase);

        private static bool IsUpcoming(MaintenanceRecord record) =>
            IsScheduled(record) && record.ScheduledDate >= DateTime.Now && record.ScheduledDate <= DateTime.Now.AddDays(30);

        private static string Searchable(string? value) => value?.ToLowerInvariant() ?? string.Empty;

        private static FlowDocument CreateMaintenanceDocument(string title, double fontSize = 16)
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

        private static Table CreateKeyValueTable()
        {
            var table = new Table();
            table.Columns.Add(new TableColumn { Width = new GridLength(150) });
            table.Columns.Add(new TableColumn());
            table.RowGroups.Add(new TableRowGroup());
            return table;
        }

        private static void AddKeyValueRow(TableRowGroup group, string label, string? value)
        {
            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(label)) { FontWeight = FontWeights.Bold }));
            row.Cells.Add(new TableCell(new Paragraph(new Run(ValueOrNotRecorded(value)))));
            group.Rows.Add(row);
        }

        private static void AddPrintRow(TableRowGroup group, bool isHeader, params string?[] values)
        {
            var row = new TableRow();
            foreach (var value in values)
            {
                var paragraph = new Paragraph(new Run(ValueOrNotRecorded(value)))
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

        private static string FormatDate(DateTime? value) => value?.ToString("yyyy-MM-dd") ?? "Not recorded";

        private static string ValueOrNotRecorded(string? value) => string.IsNullOrWhiteSpace(value) ? "Not recorded" : value;
    }
}
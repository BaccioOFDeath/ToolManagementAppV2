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
using InventoryManagementApp.Services.Calibration;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.ViewModels
{
    public class CalibrationManagementViewModel : ObservableObject
    {
        private readonly CalibrationService _calibrationService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<CalibrationRecord> CalibrationRecords { get; }
        public ObservableCollection<CalibrationRecord> FilteredCalibrationRecords { get; }

        public string CalibrationResultsSummary => $"{FilteredCalibrationRecords.Count} of {CalibrationRecords.Count} calibration record{(CalibrationRecords.Count == 1 ? string.Empty : "s")} shown";
        public string CalibrationBacklogSummary
        {
            get
            {
                var overdue = CalibrationRecords.Count(r => r.IsOverdue);
                var dueSoon = CalibrationRecords.Count(r => r.IsDueSoon);
                var current = CalibrationRecords.Count(r => IsCurrent(r));
                return $"{overdue} overdue | {dueSoon} due soon | {current} current";
            }
        }

        public string SelectedRecordSummary => SelectedRecord == null
            ? "Select or double-click a calibration row to review certificate details, copy the shelf handoff, print the record, edit, or delete."
            : $"{ValueOrNotRecorded(SelectedRecord.ItemNumber)} | {ValueOrNotRecorded(SelectedRecord.ItemName)} | {SelectedRecord.StatusDisplay} | due {SelectedRecord.NextCalibrationDue:yyyy-MM-dd}";

        public string SelectedCalibrationDetail => SelectedRecord == null
            ? "No calibration selected. Choose a certificate row or add a record before taking an action."
            : $"Certificate {ValueOrNotRecorded(SelectedRecord.CertificateNumber)} for {ValueOrNotRecorded(SelectedRecord.ItemNumber)} - {ValueOrNotRecorded(SelectedRecord.ItemName)}. Standard: {ValueOrNotRecorded(SelectedRecord.Standard)}. Result: {ValueOrNotRecorded(SelectedRecord.Result)}. Notes: {ValueOrNotRecorded(SelectedRecord.Notes)}";

        public string SelectedCalibrationTimingSummary => SelectedRecord == null
            ? "No due date selected."
            : $"Calibrated {SelectedRecord.CalibrationDate:yyyy-MM-dd}. Next due {SelectedRecord.NextCalibrationDue:yyyy-MM-dd}. Calibrated by {ValueOrNotRecorded(SelectedRecord.CalibratedBy)}. Cost {SelectedRecord.Cost:C}.";

        public string SelectedCalibrationNextAction
        {
            get
            {
                if (SelectedRecord == null)
                {
                    return "Add a calibration record or select an existing certificate to see the next operational step.";
                }

                if (SelectedRecord.IsOverdue)
                {
                    return "This item is overdue. Hold it from issue, renew calibration, then print or copy the updated certificate handoff before returning it to the shelf.";
                }

                if (SelectedRecord.IsDueSoon)
                {
                    return "Calibration is due soon. Stage the item for renewal, confirm the certificate details, and print or copy the handoff for the technician queue.";
                }

                return "Calibration is current. Verify the item tag, certificate number, and standard before releasing or renting the item.";
            }
        }

        public string SelectedCalibrationBenchChecklist => SelectedRecord == null
            ? "Select a certificate first, then verify the item tag, certificate number, due date, result, and shelf release status before the tool is issued."
            : "Verify item tag and location, confirm certificate number and standard, check due date/result, update missing notes if needed, and keep the printed or copied handoff with the tool.";

        private CalibrationRecord? _selectedRecord;
        public CalibrationRecord? SelectedRecord
        {
            get => _selectedRecord;
            set
            {
                if (SetProperty(ref _selectedRecord, value))
                {
                    EditCalibrationCommand.NotifyCanExecuteChanged();
                    DeleteCalibrationCommand.NotifyCanExecuteChanged();
                    OpenCalibrationDetailsCommand.NotifyCanExecuteChanged();
                    PrintSelectedCalibrationCommand.NotifyCanExecuteChanged();
                    CopySelectedCalibrationCommand.NotifyCanExecuteChanged();
                    OnSelectedRecordSummariesChanged();
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

        public IAsyncRelayCommand LoadCalibrationCommand { get; }
        public IAsyncRelayCommand AddCalibrationCommand { get; }
        public IAsyncRelayCommand EditCalibrationCommand { get; }
        public IAsyncRelayCommand DeleteCalibrationCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }
        public IRelayCommand OpenCalibrationDetailsCommand { get; }
        public IRelayCommand PrintCalibrationListCommand { get; }
        public IRelayCommand PrintSelectedCalibrationCommand { get; }
        public IRelayCommand CopySelectedCalibrationCommand { get; }
        public IRelayCommand ClearSearchCommand { get; }
        public IRelayCommand ShowOverdueCommand { get; }
        public IRelayCommand ShowDueSoonCommand { get; }
        public IRelayCommand ShowCurrentCommand { get; }

        public CalibrationManagementViewModel(
            CalibrationService calibrationService,
            IDialogService dialogService)
        {
            _calibrationService = calibrationService;
            _dialogService = dialogService;

            CalibrationRecords = new ObservableCollection<CalibrationRecord>();
            FilteredCalibrationRecords = new ObservableCollection<CalibrationRecord>();
            FilterOptions = new ObservableCollection<string>
            {
                "All",
                "Current",
                "Due Soon",
                "Overdue"
            };

            LoadCalibrationCommand = new AsyncRelayCommand(LoadCalibrationAsync);
            AddCalibrationCommand = new AsyncRelayCommand(AddCalibrationAsync);
            EditCalibrationCommand = new AsyncRelayCommand(EditCalibrationAsync, CanEditOrDelete);
            DeleteCalibrationCommand = new AsyncRelayCommand(DeleteCalibrationAsync, CanEditOrDelete);
            RefreshCommand = new AsyncRelayCommand(LoadCalibrationAsync);
            OpenCalibrationDetailsCommand = new RelayCommand(OpenCalibrationDetails, CanEditOrDelete);
            PrintCalibrationListCommand = new RelayCommand(PrintCalibrationList);
            PrintSelectedCalibrationCommand = new RelayCommand(PrintSelectedCalibration, CanEditOrDelete);
            CopySelectedCalibrationCommand = new RelayCommand(CopySelectedCalibration, CanEditOrDelete);
            ClearSearchCommand = new RelayCommand(ClearSearch);
            ShowOverdueCommand = new RelayCommand(() => SelectedFilter = "Overdue");
            ShowDueSoonCommand = new RelayCommand(() => SelectedFilter = "Due Soon");
            ShowCurrentCommand = new RelayCommand(() => SelectedFilter = "Current");
        }

        private async Task LoadCalibrationAsync()
        {
            try
            {
                var selectedId = SelectedRecord?.CalibrationID;
                var records = await _calibrationService.GetAllCalibrationRecordsAsync();
                CalibrationRecords.Clear();
                foreach (var record in records)
                {
                    CalibrationRecords.Add(record);
                }
                ApplyFilter(selectedId);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error loading calibration records", ex.Message);
            }
        }

        private async Task AddCalibrationAsync()
        {
            var newRecord = new CalibrationRecord
            {
                CalibrationDate = DateTime.Now,
                NextCalibrationDue = DateTime.Now.AddYears(1),
                Result = "Pass"
            };

            var result = await _dialogService.ShowCalibrationEditDialogAsync(newRecord, isNew: true);
            if (result)
            {
                try
                {
                    var id = await _calibrationService.CreateCalibrationRecordAsync(newRecord);
                    newRecord.CalibrationID = id;
                    CalibrationRecords.Insert(0, newRecord);
                    ApplyFilter(newRecord.CalibrationID);
                    await _dialogService.ShowInfoAsync("Success", "Calibration record created successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error creating calibration record", ex.Message);
                }
            }
        }

        private async Task EditCalibrationAsync()
        {
            if (SelectedRecord == null) return;

            var clone = new CalibrationRecord
            {
                CalibrationID = SelectedRecord.CalibrationID,
                ItemID = SelectedRecord.ItemID,
                ItemNumber = SelectedRecord.ItemNumber,
                ItemName = SelectedRecord.ItemName,
                CalibrationDate = SelectedRecord.CalibrationDate,
                NextCalibrationDue = SelectedRecord.NextCalibrationDue,
                CalibratedBy = SelectedRecord.CalibratedBy,
                CertificateNumber = SelectedRecord.CertificateNumber,
                Standard = SelectedRecord.Standard,
                Result = SelectedRecord.Result,
                Cost = SelectedRecord.Cost,
                Notes = SelectedRecord.Notes,
                UserID = SelectedRecord.UserID,
                CreatedAt = SelectedRecord.CreatedAt
            };

            var result = await _dialogService.ShowCalibrationEditDialogAsync(clone, isNew: false);
            if (result)
            {
                try
                {
                    await _calibrationService.UpdateCalibrationRecordAsync(clone);
                    var index = CalibrationRecords.IndexOf(SelectedRecord);
                    if (index >= 0) CalibrationRecords[index] = clone;
                    ApplyFilter(clone.CalibrationID);
                    await _dialogService.ShowInfoAsync("Success", "Calibration record updated successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error updating calibration record", ex.Message);
                }
            }
        }

        private async Task DeleteCalibrationAsync()
        {
            if (SelectedRecord == null) return;

            var confirmed = await _dialogService.ShowConfirmAsync(
                "Delete Calibration Record",
                $"Delete calibration certificate {ValueOrNotRecorded(SelectedRecord.CertificateNumber)} for {ValueOrNotRecorded(SelectedRecord.ItemName)} due {SelectedRecord.NextCalibrationDue:yyyy-MM-dd}?");

            if (confirmed)
            {
                try
                {
                    var deletedRecord = SelectedRecord;
                    await _calibrationService.DeleteCalibrationRecordAsync(deletedRecord.CalibrationID);
                    CalibrationRecords.Remove(deletedRecord);
                    ApplyFilter();
                    await _dialogService.ShowInfoAsync("Success", "Calibration record deleted successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error deleting calibration record", ex.Message);
                }
            }
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
            SelectedFilter = "All";
            ApplyFilter();
        }

        private void ApplyFilter(int? preferredCalibrationId = null)
        {
            preferredCalibrationId ??= SelectedRecord?.CalibrationID;
            FilteredCalibrationRecords.Clear();

            var filtered = CalibrationRecords.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.Trim().ToLowerInvariant();
                filtered = filtered.Where(r =>
                    Searchable(r.ItemNumber).Contains(search) ||
                    Searchable(r.ItemName).Contains(search) ||
                    Searchable(r.CertificateNumber).Contains(search) ||
                    Searchable(r.CalibratedBy).Contains(search) ||
                    Searchable(r.Standard).Contains(search) ||
                    Searchable(r.Result).Contains(search) ||
                    Searchable(r.Notes).Contains(search));
            }

            filtered = SelectedFilter switch
            {
                "Current" => filtered.Where(IsCurrent),
                "Due Soon" => filtered.Where(r => r.IsDueSoon),
                "Overdue" => filtered.Where(r => r.IsOverdue),
                _ => filtered
            };

            var filteredList = filtered
                .OrderBy(r => IsCurrent(r) ? 1 : 0)
                .ThenBy(r => r.NextCalibrationDue)
                .ThenBy(r => Searchable(r.ItemNumber))
                .ToList();

            foreach (var record in filteredList)
            {
                FilteredCalibrationRecords.Add(record);
            }

            SelectedRecord = FilteredCalibrationRecords.FirstOrDefault(r => r.CalibrationID == preferredCalibrationId)
                ?? FilteredCalibrationRecords.FirstOrDefault();

            OnPropertyChanged(nameof(CalibrationResultsSummary));
            OnPropertyChanged(nameof(CalibrationBacklogSummary));
        }

        private void OpenCalibrationDetails()
        {
            if (SelectedRecord == null) return;

            var record = SelectedRecord;
            var details = new StringBuilder();
            details.AppendLine($"Calibration #: {record.CalibrationID}");
            details.AppendLine($"Item: {ValueOrNotRecorded(record.ItemNumber)} - {ValueOrNotRecorded(record.ItemName)}");
            details.AppendLine($"Status: {record.StatusDisplay}");
            details.AppendLine();
            details.AppendLine($"Calibrated: {record.CalibrationDate:yyyy-MM-dd}");
            details.AppendLine($"Next due: {record.NextCalibrationDue:yyyy-MM-dd}");
            details.AppendLine($"Calibrated by: {ValueOrNotRecorded(record.CalibratedBy)}");
            details.AppendLine($"Certificate: {ValueOrNotRecorded(record.CertificateNumber)}");
            details.AppendLine($"Standard: {ValueOrNotRecorded(record.Standard)}");
            details.AppendLine($"Result: {ValueOrNotRecorded(record.Result)}");
            details.AppendLine($"Cost: {record.Cost:C}");
            details.AppendLine();
            details.AppendLine($"Notes: {ValueOrNotRecorded(record.Notes)}");
            details.AppendLine();
            details.AppendLine(SelectedCalibrationNextAction);

            _dialogService.ShowInfo(details.ToString(), $"Calibration Details - {ValueOrNotRecorded(record.ItemNumber)}");
        }

        private void CopySelectedCalibration()
        {
            if (SelectedRecord == null) return;

            var record = SelectedRecord;
            var handoff = new StringBuilder();
            handoff.AppendLine("Calibration handoff");
            handoff.AppendLine($"Item: {ValueOrNotRecorded(record.ItemNumber)} - {ValueOrNotRecorded(record.ItemName)}");
            handoff.AppendLine($"Status: {record.StatusDisplay}");
            handoff.AppendLine($"Certificate: {ValueOrNotRecorded(record.CertificateNumber)}");
            handoff.AppendLine($"Standard: {ValueOrNotRecorded(record.Standard)}");
            handoff.AppendLine($"Result: {ValueOrNotRecorded(record.Result)}");
            handoff.AppendLine($"Calibrated: {record.CalibrationDate:yyyy-MM-dd}");
            handoff.AppendLine($"Next due: {record.NextCalibrationDue:yyyy-MM-dd}");
            handoff.AppendLine($"Calibrated by: {ValueOrNotRecorded(record.CalibratedBy)}");
            handoff.AppendLine($"Notes: {ValueOrNotRecorded(record.Notes)}");
            handoff.AppendLine($"Next action: {SelectedCalibrationNextAction}");

            try
            {
                Clipboard.SetText(handoff.ToString());
                _dialogService.ShowInfo("Calibration handoff copied to the clipboard.", "Copied");
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Unable to copy calibration handoff: {ex.Message}", "Copy Failed");
            }
        }

        private void PrintCalibrationList()
        {
            if (FilteredCalibrationRecords.Count == 0)
            {
                _dialogService.ShowInfo("There are no calibration records to print.", "Calibration Report");
                return;
            }

            try
            {
                var doc = CreateCalibrationDocument("Calibration Due Report", fontSize: 11);
                doc.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:yyyy-MM-dd HH:mm} | Filter: {SelectedFilter} | Search: {ValueOrNotRecorded(SearchText)} | {CalibrationResultsSummary} | {CalibrationBacklogSummary}"))
                {
                    FontSize = 10,
                    Margin = new Thickness(0, 0, 0, 10)
                });

                var table = new Table { CellSpacing = 0 };
                table.Columns.Add(new TableColumn { Width = new GridLength(90) });
                table.Columns.Add(new TableColumn { Width = new GridLength(150) });
                table.Columns.Add(new TableColumn { Width = new GridLength(90) });
                table.Columns.Add(new TableColumn { Width = new GridLength(90) });
                table.Columns.Add(new TableColumn { Width = new GridLength(105) });
                table.Columns.Add(new TableColumn { Width = new GridLength(105) });
                table.Columns.Add(new TableColumn { Width = new GridLength(80) });

                var group = new TableRowGroup();
                table.RowGroups.Add(group);
                AddPrintRow(group, true, "Item #", "Name", "Calibrated", "Next Due", "By", "Certificate", "Status");
                foreach (var record in FilteredCalibrationRecords)
                {
                    AddPrintRow(group, false, record.ItemNumber, record.ItemName, record.CalibrationDate.ToString("yyyy-MM-dd"), record.NextCalibrationDue.ToString("yyyy-MM-dd"), record.CalibratedBy, record.CertificateNumber, record.StatusDisplay);
                }

                doc.Blocks.Add(table);
                _dialogService.ShowPrintPreview(doc, "Calibration Due Report", string.Empty);
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to print calibration report: {ex.Message}", "Print Failed");
            }
        }

        private void PrintSelectedCalibration()
        {
            if (SelectedRecord == null) return;

            try
            {
                var record = SelectedRecord;
                var doc = CreateCalibrationDocument($"Calibration Certificate - {ValueOrNotRecorded(record.ItemNumber)}");
                var table = CreateKeyValueTable();
                var group = table.RowGroups[0];
                AddKeyValueRow(group, "Calibration #:", record.CalibrationID.ToString());
                AddKeyValueRow(group, "Item:", $"{ValueOrNotRecorded(record.ItemNumber)} - {ValueOrNotRecorded(record.ItemName)}");
                AddKeyValueRow(group, "Status:", record.StatusDisplay);
                AddKeyValueRow(group, "Calibrated:", record.CalibrationDate.ToString("yyyy-MM-dd"));
                AddKeyValueRow(group, "Next due:", record.NextCalibrationDue.ToString("yyyy-MM-dd"));
                AddKeyValueRow(group, "Calibrated by:", record.CalibratedBy);
                AddKeyValueRow(group, "Certificate:", record.CertificateNumber);
                AddKeyValueRow(group, "Standard:", record.Standard);
                AddKeyValueRow(group, "Result:", record.Result);
                AddKeyValueRow(group, "Cost:", record.Cost.ToString("C"));
                AddKeyValueRow(group, "Notes:", record.Notes);
                AddKeyValueRow(group, "Next action:", SelectedCalibrationNextAction);
                doc.Blocks.Add(table);

                _dialogService.ShowPrintPreview(doc, $"Calibration {record.CalibrationID}", string.Empty);
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to print calibration record: {ex.Message}", "Print Failed");
            }
        }

        private bool CanEditOrDelete() => SelectedRecord != null;

        private void OnSelectedRecordSummariesChanged()
        {
            OnPropertyChanged(nameof(SelectedRecordSummary));
            OnPropertyChanged(nameof(SelectedCalibrationDetail));
            OnPropertyChanged(nameof(SelectedCalibrationTimingSummary));
            OnPropertyChanged(nameof(SelectedCalibrationNextAction));
            OnPropertyChanged(nameof(SelectedCalibrationBenchChecklist));
        }

        private static bool IsCurrent(CalibrationRecord record) => !record.IsOverdue && !record.IsDueSoon;

        private static string Searchable(string? value) => value?.ToLowerInvariant() ?? string.Empty;

        private static FlowDocument CreateCalibrationDocument(string title, double fontSize = 16)
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

        private static string ValueOrNotRecorded(string? value) => string.IsNullOrWhiteSpace(value) ? "Not recorded" : value;
    }
}

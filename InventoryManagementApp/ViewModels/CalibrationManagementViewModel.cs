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
        public string SelectedRecordSummary => SelectedRecord == null
            ? "Select or double-click a calibration row to view certificate details, print the record, edit, or delete."
            : $"{ValueOrNotRecorded(SelectedRecord.ItemNumber)} | {ValueOrNotRecorded(SelectedRecord.ItemName)} | {SelectedRecord.StatusDisplay} | due {SelectedRecord.NextCalibrationDue:yyyy-MM-dd}";

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
                    OnPropertyChanged(nameof(SelectedRecordSummary));
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
        }

        private async Task LoadCalibrationAsync()
        {
            try
            {
                var records = await _calibrationService.GetAllCalibrationRecordsAsync();
                CalibrationRecords.Clear();
                foreach (var record in records)
                {
                    CalibrationRecords.Add(record);
                }
                ApplyFilter();
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
                    ApplyFilter();
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
                    SelectedRecord = clone;
                    ApplyFilter();
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
                $"Delete calibration certificate {ValueOrNotRecorded(SelectedRecord.CertificateNumber)} for {ValueOrNotRecorded(SelectedRecord.ItemName)}?");

            if (confirmed)
            {
                try
                {
                    await _calibrationService.DeleteCalibrationRecordAsync(SelectedRecord.CalibrationID);
                    CalibrationRecords.Remove(SelectedRecord);
                    SelectedRecord = null;
                    ApplyFilter();
                    await _dialogService.ShowInfoAsync("Success", "Calibration record deleted successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error deleting calibration record", ex.Message);
                }
            }
        }

        private void ApplyFilter()
        {
            FilteredCalibrationRecords.Clear();

            var filtered = CalibrationRecords.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLowerInvariant();
                filtered = filtered.Where(r =>
                    r.ItemNumber.ToLowerInvariant().Contains(search) ||
                    r.ItemName.ToLowerInvariant().Contains(search) ||
                    r.CertificateNumber.ToLowerInvariant().Contains(search) ||
                    r.CalibratedBy.ToLowerInvariant().Contains(search));
            }

            filtered = SelectedFilter switch
            {
                "Current" => filtered.Where(r => !r.IsOverdue && !r.IsDueSoon),
                "Due Soon" => filtered.Where(r => r.IsDueSoon),
                "Overdue" => filtered.Where(r => r.IsOverdue),
                _ => filtered
            };

            foreach (var record in filtered)
            {
                FilteredCalibrationRecords.Add(record);
            }

            OnPropertyChanged(nameof(CalibrationResultsSummary));
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
            details.AppendLine(record.IsOverdue
                ? "Next action: treat this item as blocked until calibration is renewed or the record is updated."
                : "Next action: print the certificate sheet, edit certificate details, or review due-soon work from this page.");

            _dialogService.ShowInfo(details.ToString(), $"Calibration Details - {ValueOrNotRecorded(record.ItemNumber)}");
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
                doc.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:yyyy-MM-dd HH:mm} | Filter: {SelectedFilter} | Search: {ValueOrNotRecorded(SearchText)} | {CalibrationResultsSummary}"))
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
                doc.Blocks.Add(table);

                _dialogService.ShowPrintPreview(doc, $"Calibration {record.CalibrationID}", string.Empty);
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to print calibration record: {ex.Message}", "Print Failed");
            }
        }

        private bool CanEditOrDelete() => SelectedRecord != null;

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

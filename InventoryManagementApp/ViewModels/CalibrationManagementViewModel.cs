using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
                    CalibrationRecords[index] = clone;
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
                $"Are you sure you want to delete this calibration record?");

            if (confirmed)
            {
                try
                {
                    await _calibrationService.DeleteCalibrationRecordAsync(SelectedRecord.CalibrationID);
                    CalibrationRecords.Remove(SelectedRecord);
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
        }

        private bool CanEditOrDelete() => SelectedRecord != null;
    }
}

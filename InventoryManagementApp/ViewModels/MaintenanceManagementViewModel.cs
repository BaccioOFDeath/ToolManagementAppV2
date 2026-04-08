using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Maintenance;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.ViewModels
{
    public class MaintenanceManagementViewModel : ObservableObject
    {
        private readonly MaintenanceService _maintenanceService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<MaintenanceRecord> MaintenanceRecords { get; }
        public ObservableCollection<MaintenanceRecord> FilteredMaintenanceRecords { get; }

        private MaintenanceRecord? _selectedRecord;
        public MaintenanceRecord? SelectedRecord
        {
            get => _selectedRecord;
            set
            {
                if (SetProperty(ref _selectedRecord, value))
                {
                    EditMaintenanceCommand.NotifyCanExecuteChanged();
                    DeleteMaintenanceCommand.NotifyCanExecuteChanged();
                    CompleteMaintenanceCommand.NotifyCanExecuteChanged();
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

            LoadMaintenanceCommand = new AsyncRelayCommand(LoadMaintenanceAsync);
            AddMaintenanceCommand = new AsyncRelayCommand(AddMaintenanceAsync);
            EditMaintenanceCommand = new AsyncRelayCommand(EditMaintenanceAsync, CanEditOrDelete);
            DeleteMaintenanceCommand = new AsyncRelayCommand(DeleteMaintenanceAsync, CanEditOrDelete);
            CompleteMaintenanceCommand = new AsyncRelayCommand(CompleteMaintenanceAsync, CanComplete);
            RefreshCommand = new AsyncRelayCommand(LoadMaintenanceAsync);
        }

        private async Task LoadMaintenanceAsync()
        {
            try
            {
                var records = await _maintenanceService.GetAllMaintenanceRecordsAsync();
                MaintenanceRecords.Clear();
                foreach (var record in records)
                {
                    MaintenanceRecords.Add(record);
                }
                ApplyFilter();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error loading maintenance records", ex.Message);
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
                    ApplyFilter();
                    await _dialogService.ShowInfoAsync("Success", "Maintenance record created successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error creating maintenance record", ex.Message);
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
                    ApplyFilter();
                    await _dialogService.ShowInfoAsync("Success", "Maintenance record updated successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error updating maintenance record", ex.Message);
                }
            }
        }

        private async Task DeleteMaintenanceAsync()
        {
            if (SelectedRecord == null) return;

            var confirmed = await _dialogService.ShowConfirmAsync(
                "Delete Maintenance Record",
                $"Are you sure you want to delete this maintenance record?");

            if (confirmed)
            {
                try
                {
                    await _maintenanceService.DeleteMaintenanceRecordAsync(SelectedRecord.MaintenanceID);
                    MaintenanceRecords.Remove(SelectedRecord);
                    ApplyFilter();
                    await _dialogService.ShowInfoAsync("Success", "Maintenance record deleted successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error deleting maintenance record", ex.Message);
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
                try
                {
                    await _maintenanceService.CompleteMaintenanceAsync(
                        SelectedRecord.MaintenanceID,
                        performedBy,
                        "");
                    SelectedRecord.Status = "Completed";
                    SelectedRecord.CompletedDate = DateTime.Now;
                    SelectedRecord.PerformedBy = performedBy;
                    ApplyFilter();
                    EditMaintenanceCommand.NotifyCanExecuteChanged();
                    DeleteMaintenanceCommand.NotifyCanExecuteChanged();
                    CompleteMaintenanceCommand.NotifyCanExecuteChanged();
                    await _dialogService.ShowInfoAsync("Success", "Maintenance marked as completed");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error completing maintenance", ex.Message);
                }
            }
        }

        private void ApplyFilter()
        {
            FilteredMaintenanceRecords.Clear();

            var filtered = MaintenanceRecords.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLowerInvariant();
                filtered = filtered.Where(r =>
                    r.ItemNumber.ToLowerInvariant().Contains(search) ||
                    r.ItemName.ToLowerInvariant().Contains(search) ||
                    r.MaintenanceType.ToLowerInvariant().Contains(search) ||
                    r.Description.ToLowerInvariant().Contains(search));
            }

            filtered = SelectedFilter switch
            {
                "Scheduled" => filtered.Where(r => r.Status == "Scheduled" && r.ScheduledDate >= DateTime.Now),
                "Completed" => filtered.Where(r => r.Status == "Completed"),
                "Overdue" => filtered.Where(r => r.IsOverdue),
                "Upcoming (30 days)" => filtered.Where(r => r.Status == "Scheduled" && r.ScheduledDate >= DateTime.Now && r.ScheduledDate <= DateTime.Now.AddDays(30)),
                _ => filtered
            };

            foreach (var record in filtered)
            {
                FilteredMaintenanceRecords.Add(record);
            }
        }

        private bool CanEditOrDelete() => SelectedRecord != null;

        private bool CanComplete() => SelectedRecord != null && SelectedRecord.Status == "Scheduled";
    }
}

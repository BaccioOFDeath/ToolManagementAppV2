using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Vehicles;

namespace InventoryManagementApp.ViewModels
{
    /// <summary>
    /// Coordinates vehicle intake, compliance holds, and dismantling task tracking for SDAutoOS workflows.
    /// </summary>
    public class VehicleManagementViewModel : ObservableObject
    {
        private readonly VehicleIntakeService _vehicleService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<VehicleIntake> Vehicles { get; } = new();
        public ObservableCollection<VehicleIntake> FilteredVehicles { get; } = new();
        public ObservableCollection<DismantlingTask> SelectedVehicleTasks { get; } = new();

        private VehicleIntake? _selectedVehicle;
        public VehicleIntake? SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                if (SetProperty(ref _selectedVehicle, value))
                {
                    _ = LoadTasksAsync(value);
                    UpdateCommandStates();
                }
            }
        }

        private DismantlingTask? _selectedTask;
        public DismantlingTask? SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (SetProperty(ref _selectedTask, value))
                {
                    UpdateCommandStates();
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

        public ObservableCollection<string> FilterOptions { get; } = new()
        {
            "All",
            "Received",
            "OnHold",
            "Dismantling",
            "Completed"
        };

        public IAsyncRelayCommand LoadVehiclesCommand { get; }
        public IAsyncRelayCommand AddVehicleCommand { get; }
        public IAsyncRelayCommand<string?> UpdateStatusCommand { get; }
        public IAsyncRelayCommand AddTaskCommand { get; }
        public IAsyncRelayCommand StartTaskCommand { get; }
        public IAsyncRelayCommand CompleteTaskCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }

        public VehicleManagementViewModel(VehicleIntakeService vehicleService, IDialogService dialogService)
        {
            _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            LoadVehiclesCommand = new AsyncRelayCommand(LoadVehiclesAsync);
            RefreshCommand = new AsyncRelayCommand(LoadVehiclesAsync);
            AddVehicleCommand = new AsyncRelayCommand(AddVehicleAsync);
            UpdateStatusCommand = new AsyncRelayCommand<string?>(SetStatusAsync, _ => SelectedVehicle != null);
            AddTaskCommand = new AsyncRelayCommand(AddTaskAsync, () => SelectedVehicle != null);
            StartTaskCommand = new AsyncRelayCommand(StartTaskAsync, CanStartTask);
            CompleteTaskCommand = new AsyncRelayCommand(CompleteTaskAsync, CanCompleteTask);
        }

        private async Task LoadVehiclesAsync()
        {
            try
            {
                var vehicles = await _vehicleService.GetAllVehiclesAsync();
                Vehicles.Clear();
                foreach (var vehicle in vehicles)
                {
                    Vehicles.Add(vehicle);
                }
                ApplyFilter();
                if (SelectedVehicle != null)
                {
                    await LoadTasksAsync(SelectedVehicle);
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error loading vehicles", ex.Message);
            }
        }

        private async Task AddVehicleAsync()
        {
            var vin = await _dialogService.ShowInputDialogAsync("Log Vehicle Intake", "Enter VIN for the arriving vehicle:");
            if (string.IsNullOrWhiteSpace(vin))
                return;

            var stockNumber = await _dialogService.ShowInputDialogAsync("Stock Number", "Enter stock number (optional):");
            var vehicle = new VehicleIntake
            {
                Vin = vin.Trim(),
                StockNumber = stockNumber ?? string.Empty,
                IntakeDate = DateTime.Today,
                Status = "Received"
            };

            try
            {
                await _vehicleService.CreateVehicleAsync(vehicle);
                Vehicles.Insert(0, vehicle);
                ApplyFilter();
                SelectedVehicle = vehicle;
                await _dialogService.ShowInfoAsync("Vehicle logged", "Vehicle intake saved and ready for dismantling scheduling.");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Unable to log vehicle", ex.Message);
            }
        }

        private async Task SetStatusAsync(string? status)
        {
            if (SelectedVehicle == null || string.IsNullOrWhiteSpace(status))
                return;

            try
            {
                await _vehicleService.UpdateStatusAsync(SelectedVehicle.VehicleID, status);
                SelectedVehicle.Status = status;
                ApplyFilter();
                await _dialogService.ShowInfoAsync("Status updated", $"Vehicle moved to {status}.");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Unable to update status", ex.Message);
            }
        }

        private async Task AddTaskAsync()
        {
            if (SelectedVehicle == null)
                return;

            var partName = await _dialogService.ShowInputDialogAsync("Add dismantling task", "Enter the part to harvest or inspect:");
            if (string.IsNullOrWhiteSpace(partName))
                return;

            var partTag = await _dialogService.ShowInputDialogAsync("Part tag", "Enter part tag or barcode (optional):");

            var task = new DismantlingTask
            {
                VehicleID = SelectedVehicle.VehicleID,
                PartName = partName,
                PartTag = partTag ?? string.Empty,
                Status = "Pending",
                ContainsHazmat = false
            };

            try
            {
                await _vehicleService.CreateDismantlingTaskAsync(task);
                SelectedVehicleTasks.Insert(0, task);
                await _dialogService.ShowInfoAsync("Task added", "Dismantling task queued for this vehicle.");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Unable to add task", ex.Message);
            }
        }

        private async Task StartTaskAsync()
        {
            if (SelectedTask == null)
                return;

            var technician = await _dialogService.ShowInputDialogAsync("Assign technician", "Enter the technician starting this task:");
            if (string.IsNullOrWhiteSpace(technician))
                return;

            try
            {
                await _vehicleService.StartTaskAsync(SelectedTask.TaskID, technician);
                SelectedTask.Status = "InProgress";
                SelectedTask.Technician = technician;
                SelectedTask.StartedAt = DateTime.Now;
                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Unable to start task", ex.Message);
            }
        }

        private async Task CompleteTaskAsync()
        {
            if (SelectedTask == null)
                return;

            var grade = await _dialogService.ShowInputDialogAsync("Condition grade", "Enter grade or damage notes for the part:");
            if (grade == null)
                return;

            try
            {
                await _vehicleService.CompleteTaskAsync(SelectedTask.TaskID, grade, SelectedTask.Notes);
                SelectedTask.Status = "Completed";
                SelectedTask.ConditionGrade = grade;
                SelectedTask.CompletedAt = DateTime.Now;
                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Unable to complete task", ex.Message);
            }
        }

        private async Task LoadTasksAsync(VehicleIntake? vehicle)
        {
            SelectedVehicleTasks.Clear();
            if (vehicle == null)
                return;

            try
            {
                var tasks = await _vehicleService.GetTasksForVehicleAsync(vehicle.VehicleID);
                foreach (var task in tasks)
                {
                    SelectedVehicleTasks.Add(task);
                }
                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Unable to load dismantling tasks", ex.Message);
            }
        }

        private void ApplyFilter()
        {
            FilteredVehicles.Clear();
            var filtered = Vehicles.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLowerInvariant();
                filtered = filtered.Where(v =>
                    v.Vin.ToLowerInvariant().Contains(search) ||
                    v.StockNumber.ToLowerInvariant().Contains(search) ||
                    v.Make.ToLowerInvariant().Contains(search) ||
                    v.Model.ToLowerInvariant().Contains(search));
            }

            filtered = SelectedFilter switch
            {
                "Received" => filtered.Where(v => v.Status.Equals("Received", StringComparison.OrdinalIgnoreCase)),
                "OnHold" => filtered.Where(v => v.Status.Equals("OnHold", StringComparison.OrdinalIgnoreCase)),
                "Dismantling" => filtered.Where(v => v.Status.Equals("Dismantling", StringComparison.OrdinalIgnoreCase)),
                "Completed" => filtered.Where(v => v.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)),
                _ => filtered
            };

            foreach (var vehicle in filtered)
            {
                FilteredVehicles.Add(vehicle);
            }
        }

        private bool CanStartTask() => SelectedTask != null && SelectedTask.Status == "Pending";

        private bool CanCompleteTask() => SelectedTask != null && (SelectedTask.Status == "Pending" || SelectedTask.Status == "InProgress");

        private void UpdateCommandStates()
        {
            UpdateStatusCommand.NotifyCanExecuteChanged();
            AddTaskCommand.NotifyCanExecuteChanged();
            StartTaskCommand.NotifyCanExecuteChanged();
            CompleteTaskCommand.NotifyCanExecuteChanged();
        }
    }
}

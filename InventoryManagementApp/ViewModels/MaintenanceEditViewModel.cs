using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.ViewModels
{
    public class MaintenanceEditViewModel : ObservableObject
    {
        public MaintenanceRecord MaintenanceRecord { get; }

        public bool IsNew { get; }

        public string Title => IsNew ? "Schedule Maintenance" : "Edit Maintenance";

        public ObservableCollection<string> StatusOptions { get; }

        public ObservableCollection<string> MaintenanceTypeOptions { get; }

        public IRelayCommand SaveCommand { get; }

        public IRelayCommand CancelCommand { get; }

        public MaintenanceEditViewModel(MaintenanceRecord record, bool isNew, Action onSave, Action onCancel)
        {
            MaintenanceRecord = record;
            IsNew = isNew;
            StatusOptions = new ObservableCollection<string>
            {
                "Scheduled",
                "In Progress",
                "Completed",
                "Cancelled"
            };
            MaintenanceTypeOptions = new ObservableCollection<string>
            {
                "Routine",
                "Calibration",
                "Repair",
                "Inspection"
            };
            SaveCommand = new RelayCommand(onSave);
            CancelCommand = new RelayCommand(onCancel);
        }
    }
}

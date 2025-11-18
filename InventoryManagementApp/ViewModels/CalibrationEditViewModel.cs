using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.ViewModels
{
    public class CalibrationEditViewModel : ObservableObject
    {
        public CalibrationRecord CalibrationRecord { get; }

        public bool IsNew { get; }

        public string Title => IsNew ? "Log Calibration" : "Edit Calibration";

        public ObservableCollection<string> ResultOptions { get; }

        public IRelayCommand SaveCommand { get; }

        public IRelayCommand CancelCommand { get; }

        public CalibrationEditViewModel(CalibrationRecord record, bool isNew, Action onSave, Action onCancel)
        {
            CalibrationRecord = record;
            IsNew = isNew;
            ResultOptions = new ObservableCollection<string>
            {
                "Pass",
                "Fail",
                "Pending"
            };
            SaveCommand = new RelayCommand(onSave);
            CancelCommand = new RelayCommand(onCancel);
        }
    }
}

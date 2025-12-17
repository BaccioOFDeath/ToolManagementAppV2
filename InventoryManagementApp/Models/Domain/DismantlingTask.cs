using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace InventoryManagementApp.Models.Domain
{
    public class DismantlingTask : ObservableObject
    {
        private int _taskID;
        public int TaskID
        {
            get => _taskID;
            set => SetProperty(ref _taskID, value);
        }

        private int _vehicleID;
        public int VehicleID
        {
            get => _vehicleID;
            set => SetProperty(ref _vehicleID, value);
        }

        private string _partName = string.Empty;
        public string PartName
        {
            get => _partName;
            set => SetProperty(ref _partName, value);
        }

        private string _partTag = string.Empty;
        public string PartTag
        {
            get => _partTag;
            set => SetProperty(ref _partTag, value);
        }

        private string _conditionGrade = string.Empty;
        public string ConditionGrade
        {
            get => _conditionGrade;
            set => SetProperty(ref _conditionGrade, value);
        }

        private string _technician = string.Empty;
        public string Technician
        {
            get => _technician;
            set => SetProperty(ref _technician, value);
        }

        private DateTime? _startedAt;
        public DateTime? StartedAt
        {
            get => _startedAt;
            set => SetProperty(ref _startedAt, value);
        }

        private DateTime? _completedAt;
        public DateTime? CompletedAt
        {
            get => _completedAt;
            set => SetProperty(ref _completedAt, value);
        }

        private string _status = "Pending";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        private bool _containsHazmat;
        public bool ContainsHazmat
        {
            get => _containsHazmat;
            set => SetProperty(ref _containsHazmat, value);
        }

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        public bool IsCompleted => Status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
        public bool IsInProgress => Status.Equals("InProgress", StringComparison.OrdinalIgnoreCase);
    }
}

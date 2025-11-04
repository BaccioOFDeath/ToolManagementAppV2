using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace InventoryManagementApp.Models.Domain
{
    public class MaintenanceRecord : ObservableObject
    {
        private int _maintenanceID;
        public int MaintenanceID
        {
            get => _maintenanceID;
            set => SetProperty(ref _maintenanceID, value);
        }

        private int _itemID;
        public int ItemID
        {
            get => _itemID;
            set => SetProperty(ref _itemID, value);
        }

        private string _itemNumber = string.Empty;
        public string ItemNumber
        {
            get => _itemNumber;
            set => SetProperty(ref _itemNumber, value);
        }

        private string _itemName = string.Empty;
        public string ItemName
        {
            get => _itemName;
            set => SetProperty(ref _itemName, value);
        }

        private DateTime _scheduledDate;
        public DateTime ScheduledDate
        {
            get => _scheduledDate;
            set => SetProperty(ref _scheduledDate, value);
        }

        private DateTime? _completedDate;
        public DateTime? CompletedDate
        {
            get => _completedDate;
            set => SetProperty(ref _completedDate, value);
        }

        private string _maintenanceType = string.Empty;
        public string MaintenanceType
        {
            get => _maintenanceType;
            set => SetProperty(ref _maintenanceType, value);
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private string _performedBy = string.Empty;
        public string PerformedBy
        {
            get => _performedBy;
            set => SetProperty(ref _performedBy, value);
        }

        private decimal _cost;
        public decimal Cost
        {
            get => _cost;
            set => SetProperty(ref _cost, value);
        }

        private string _status = "Scheduled";
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

        private int _userID;
        public int UserID
        {
            get => _userID;
            set => SetProperty(ref _userID, value);
        }

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        public bool IsOverdue => Status == "Scheduled" && ScheduledDate < DateTime.Now;

        public bool IsCompleted => Status == "Completed";

        public string StatusDisplay => IsOverdue ? "Overdue" : Status;
    }
}

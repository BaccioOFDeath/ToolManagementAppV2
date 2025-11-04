using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace InventoryManagementApp.Models.Domain
{
    public class CalibrationRecord : ObservableObject
    {
        private int _calibrationID;
        public int CalibrationID
        {
            get => _calibrationID;
            set => SetProperty(ref _calibrationID, value);
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

        private DateTime _calibrationDate;
        public DateTime CalibrationDate
        {
            get => _calibrationDate;
            set => SetProperty(ref _calibrationDate, value);
        }

        private DateTime _nextCalibrationDue;
        public DateTime NextCalibrationDue
        {
            get => _nextCalibrationDue;
            set => SetProperty(ref _nextCalibrationDue, value);
        }

        private string _calibratedBy = string.Empty;
        public string CalibratedBy
        {
            get => _calibratedBy;
            set => SetProperty(ref _calibratedBy, value);
        }

        private string _certificateNumber = string.Empty;
        public string CertificateNumber
        {
            get => _certificateNumber;
            set => SetProperty(ref _certificateNumber, value);
        }

        private string _standard = string.Empty;
        public string Standard
        {
            get => _standard;
            set => SetProperty(ref _standard, value);
        }

        private string _result = string.Empty;
        public string Result
        {
            get => _result;
            set => SetProperty(ref _result, value);
        }

        private decimal _cost;
        public decimal Cost
        {
            get => _cost;
            set => SetProperty(ref _cost, value);
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

        public bool IsDueSoon => (NextCalibrationDue - DateTime.Now).TotalDays <= 30 && (NextCalibrationDue - DateTime.Now).TotalDays > 0;

        public bool IsOverdue => NextCalibrationDue < DateTime.Now;

        public string StatusDisplay
        {
            get
            {
                if (IsOverdue) return "Overdue";
                if (IsDueSoon) return "Due Soon";
                return "Current";
            }
        }
    }
}

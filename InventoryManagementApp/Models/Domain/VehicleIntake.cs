using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace InventoryManagementApp.Models.Domain
{
    public class VehicleIntake : ObservableObject
    {
        private int _vehicleID;
        public int VehicleID
        {
            get => _vehicleID;
            set => SetProperty(ref _vehicleID, value);
        }

        private string _vin = string.Empty;
        public string Vin
        {
            get => _vin;
            set => SetProperty(ref _vin, value);
        }

        private string _stockNumber = string.Empty;
        public string StockNumber
        {
            get => _stockNumber;
            set => SetProperty(ref _stockNumber, value);
        }

        private int? _year;
        public int? Year
        {
            get => _year;
            set => SetProperty(ref _year, value);
        }

        private string _make = string.Empty;
        public string Make
        {
            get => _make;
            set => SetProperty(ref _make, value);
        }

        private string _model = string.Empty;
        public string Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        private string _trim = string.Empty;
        public string Trim
        {
            get => _trim;
            set => SetProperty(ref _trim, value);
        }

        private DateTime _intakeDate = DateTime.Today;
        public DateTime IntakeDate
        {
            get => _intakeDate;
            set => SetProperty(ref _intakeDate, value);
        }

        private string _status = "Received";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private string _location = string.Empty;
        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        private int? _mileage;
        public int? Mileage
        {
            get => _mileage;
            set => SetProperty(ref _mileage, value);
        }

        private string _fuelType = string.Empty;
        public string FuelType
        {
            get => _fuelType;
            set => SetProperty(ref _fuelType, value);
        }

        private string _driveTrain = string.Empty;
        public string DriveTrain
        {
            get => _driveTrain;
            set => SetProperty(ref _driveTrain, value);
        }

        private string _disposition = string.Empty;
        public string Disposition
        {
            get => _disposition;
            set => SetProperty(ref _disposition, value);
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        private string _complianceHoldReason = string.Empty;
        public string ComplianceHoldReason
        {
            get => _complianceHoldReason;
            set => SetProperty(ref _complianceHoldReason, value);
        }

        private int _createdByUserID;
        public int CreatedByUserID
        {
            get => _createdByUserID;
            set => SetProperty(ref _createdByUserID, value);
        }

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        private DateTime _updatedAt;
        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set => SetProperty(ref _updatedAt, value);
        }

        public bool IsOnHold => !string.IsNullOrWhiteSpace(ComplianceHoldReason);
        public bool IsReadyForDismantling =>
            (Status.Equals("Dismantling", StringComparison.OrdinalIgnoreCase) ||
             Status.Equals("Received", StringComparison.OrdinalIgnoreCase)) &&
             !IsOnHold;
        public string DisplayName => string.IsNullOrWhiteSpace(StockNumber) ? Vin : $"{StockNumber} ({Vin})";
    }
}

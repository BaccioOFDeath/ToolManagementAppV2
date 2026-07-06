// ViewModels/CustomerEditViewModel.cs
using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace InventoryManagementApp.ViewModels
{
    public class CustomerEditViewModel : ObservableObject
    {
        public CustomerModel Customer { get; }

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        private readonly Action _onSave;

        private bool _isSaving;
        public bool IsSaving
        {
            get => _isSaving;
            private set
            {
                if (SetProperty(ref _isSaving, value))
                    NotifySaveStateChanged();
            }
        }

        public bool CanEditCustomer => !IsSaving;

        public bool CanSaveCustomer => !IsSaving && HasRequiredCustomerDetails();

        private string _validationMessage = string.Empty;
        public string ValidationMessage
        {
            get => _validationMessage;
            private set
            {
                if (SetProperty(ref _validationMessage, value))
                {
                    OnPropertyChanged(nameof(HasValidationMessage));
                    OnPropertyChanged(nameof(StatusMessage));
                }
            }
        }

        public bool HasValidationMessage => !string.IsNullOrWhiteSpace(ValidationMessage);

        public string SaveReadinessText
        {
            get
            {
                if (IsSaving)
                    return "Saving customer profile - directory actions are paused until the update finishes.";

                var company = (Customer.Company ?? string.Empty).Trim();
                var contact = (Customer.Contact ?? string.Empty).Trim();
                var phone = (Customer.Phone ?? string.Empty).Trim();
                var mobile = (Customer.Mobile ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(company))
                    return "Enter a company name before saving this customer profile.";

                if (string.IsNullOrWhiteSpace(contact))
                    return "Enter a primary contact before saving this customer profile.";

                if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(mobile))
                    return "Enter a phone or mobile number before saving this customer profile.";

                return "Customer profile ready - save applies account, contact, phone, email, and service address updates.";
            }
        }

        public string StatusMessage => HasValidationMessage
            ? ValidationMessage
            : SaveReadinessText;

        public CustomerEditViewModel(CustomerModel customer, Action onSave, Action onCancel)
        {
            Customer = customer;
            _onSave = onSave;
            Customer.PropertyChanged += Customer_PropertyChanged;
            SaveCommand = new RelayCommand(Save, () => CanSaveCustomer);
            CancelCommand = new RelayCommand(onCancel, () => CanEditCustomer);
        }

        void Customer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Customer.Company)
                || e.PropertyName == nameof(Customer.Contact)
                || e.PropertyName == nameof(Customer.Phone)
                || e.PropertyName == nameof(Customer.Mobile)
                || e.PropertyName == nameof(Customer.Email)
                || e.PropertyName == nameof(Customer.Address))
            {
                if (HasValidationMessage)
                    ValidationMessage = string.Empty;

                NotifySaveStateChanged();
            }
        }

        void Save()
        {
            if (!ValidateForSave())
                return;

            IsSaving = true;
            try
            {
                _onSave();
            }
            finally
            {
                IsSaving = false;
            }
        }

        bool ValidateForSave()
        {
            var company = (Customer.Company ?? string.Empty).Trim();
            var contact = (Customer.Contact ?? string.Empty).Trim();
            var phone = (Customer.Phone ?? string.Empty).Trim();
            var mobile = (Customer.Mobile ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(company))
            {
                ValidationMessage = "Company is required before this customer can be saved.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(contact))
            {
                ValidationMessage = "Contact is required before this customer can be saved.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(mobile))
            {
                ValidationMessage = "Enter a phone or mobile number before this customer can be saved.";
                return false;
            }

            Customer.Company = company;
            Customer.Contact = contact;
            Customer.Phone = phone;
            Customer.Mobile = mobile;
            Customer.Email = (Customer.Email ?? string.Empty).Trim();
            Customer.Address = (Customer.Address ?? string.Empty).Trim();
            ValidationMessage = string.Empty;
            return true;
        }

        bool HasRequiredCustomerDetails()
        {
            var company = (Customer.Company ?? string.Empty).Trim();
            var contact = (Customer.Contact ?? string.Empty).Trim();
            var phone = (Customer.Phone ?? string.Empty).Trim();
            var mobile = (Customer.Mobile ?? string.Empty).Trim();

            return !string.IsNullOrWhiteSpace(company)
                && !string.IsNullOrWhiteSpace(contact)
                && (!string.IsNullOrWhiteSpace(phone) || !string.IsNullOrWhiteSpace(mobile));
        }

        void NotifySaveStateChanged()
        {
            OnPropertyChanged(nameof(IsSaving));
            OnPropertyChanged(nameof(CanEditCustomer));
            OnPropertyChanged(nameof(CanSaveCustomer));
            OnPropertyChanged(nameof(SaveReadinessText));
            OnPropertyChanged(nameof(StatusMessage));
            SaveCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();
        }
    }
}
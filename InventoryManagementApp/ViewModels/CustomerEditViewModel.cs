// ViewModels/CustomerEditViewModel.cs
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace InventoryManagementApp.ViewModels
{
    public class CustomerEditViewModel : ObservableObject
    {
        public CustomerModel Customer { get; }

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        private string _validationMessage = string.Empty;
        public string ValidationMessage
        {
            get => _validationMessage;
            private set
            {
                if (SetProperty(ref _validationMessage, value))
                    OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public string StatusMessage => string.IsNullOrWhiteSpace(ValidationMessage)
            ? "Save updates customer lookup, rental handoffs, and printed customer sheets; cancel returns without applying profile edits."
            : ValidationMessage;

        public CustomerEditViewModel(CustomerModel customer, Action onSave, Action onCancel)
        {
            Customer = customer;
            SaveCommand = new RelayCommand(() =>
            {
                if (!ValidateForSave())
                    return;

                onSave();
            });
            CancelCommand = new RelayCommand(onCancel);
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
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.ViewModels
{
    public class RentalManagementViewModel : ObservableObject
    {
        private readonly ICustomerService _customerService;

        public ObservableCollection<CustomerModel> Customers { get; } = new();

        private CustomerModel _selectedCustomer;
        public CustomerModel SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                    ((RelayCommand)UpdateCustomerCommand).NotifyCanExecuteChanged();
            }
        }

        public string NewCustomerName { get => _newCustomerName; set => SetProperty(ref _newCustomerName, value); }
        string _newCustomerName;
        public string NewCustomerEmail { get => _newCustomerEmail; set => SetProperty(ref _newCustomerEmail, value); }
        string _newCustomerEmail;
        public string NewCustomerContact { get => _newCustomerContact; set => SetProperty(ref _newCustomerContact, value); }
        string _newCustomerContact;
        public string NewCustomerPhone { get => _newCustomerPhone; set => SetProperty(ref _newCustomerPhone, value); }
        string _newCustomerPhone;
        public string NewCustomerMobile { get => _newCustomerMobile; set => SetProperty(ref _newCustomerMobile, value); }
        string _newCustomerMobile;
        public string NewCustomerAddress { get => _newCustomerAddress; set => SetProperty(ref _newCustomerAddress, value); }
        string _newCustomerAddress;

        private string _customerSearchTerm;
        public string CustomerSearchTerm
        {
            get => _customerSearchTerm;
            set => SetProperty(ref _customerSearchTerm, value);
        }

        public IRelayCommand AddCustomerCommand { get; }
        public IRelayCommand UpdateCustomerCommand { get; }
        public IRelayCommand SearchCustomersCommand { get; }
        public IRelayCommand DeleteCustomerCommand { get; }

        public RentalManagementViewModel(ICustomerService customerService)
        {
            _customerService = customerService;
            AddCustomerCommand = new RelayCommand(AddCustomer);
            UpdateCustomerCommand = new RelayCommand(UpdateCustomer, () => SelectedCustomer != null);
            SearchCustomersCommand = new RelayCommand(SearchCustomers);
            DeleteCustomerCommand = new RelayCommand(DeleteCustomer);
        }

        public void LoadCustomers()
        {
            var all = _customerService.GetAllCustomers();
            Customers.ReplaceRange(all);
        }

        void AddCustomer()
        {
            _customerService.AddCustomer(new CustomerModel
            {
                Company = NewCustomerName,
                Email = NewCustomerEmail,
                Contact = NewCustomerContact,
                Phone = NewCustomerPhone,
                Mobile = NewCustomerMobile,
                Address = NewCustomerAddress
            });
            LoadCustomers();
            NewCustomerName = string.Empty;
            NewCustomerEmail = string.Empty;
            NewCustomerContact = string.Empty;
            NewCustomerPhone = string.Empty;
            NewCustomerMobile = string.Empty;
            NewCustomerAddress = string.Empty;
        }

        void UpdateCustomer()
        {
            if (SelectedCustomer == null) return;

            SelectedCustomer.Company = NewCustomerName;
            SelectedCustomer.Email = NewCustomerEmail;
            SelectedCustomer.Contact = NewCustomerContact;
            SelectedCustomer.Phone = NewCustomerPhone;
            SelectedCustomer.Mobile = NewCustomerMobile;
            SelectedCustomer.Address = NewCustomerAddress;

            _customerService.UpdateCustomer(SelectedCustomer);
            LoadCustomers();
        }

        void SearchCustomers()
        {
            var all = _customerService.GetAllCustomers();
            if (!string.IsNullOrWhiteSpace(CustomerSearchTerm))
            {
                all = all.Where(c =>
                    (c.Company?.Contains(CustomerSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Email?.Contains(CustomerSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Contact?.Contains(CustomerSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Phone?.Contains(CustomerSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Mobile?.Contains(CustomerSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Address?.Contains(CustomerSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }
            Customers.ReplaceRange(all);
        }

        void DeleteCustomer()
        {
            if (SelectedCustomer == null)
                return;

            _customerService.DeleteCustomer(SelectedCustomer.CustomerID);
            SearchCustomers();
            SelectedCustomer = null;
        }
    }
}

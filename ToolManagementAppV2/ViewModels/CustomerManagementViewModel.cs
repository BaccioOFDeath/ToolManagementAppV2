using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Extensions;
using ToolManagementAppV2.Views;

namespace ToolManagementAppV2.ViewModels
{
    public class CustomerManagementViewModel : ObservableObject
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
                {
                    ((RelayCommand)UpdateCustomerCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)DeleteCustomerCommand).NotifyCanExecuteChanged();

                    if (value != null)
                    {
                        NewCustomerName = value.Company;
                        NewCustomerEmail = value.Email;
                        NewCustomerContact = value.Contact;
                        NewCustomerPhone = value.Phone;
                        NewCustomerMobile = value.Mobile;
                        NewCustomerAddress = value.Address;
                    }
                    else
                    {
                        NewCustomerName = string.Empty;
                        NewCustomerEmail = string.Empty;
                        NewCustomerContact = string.Empty;
                        NewCustomerPhone = string.Empty;
                        NewCustomerMobile = string.Empty;
                        NewCustomerAddress = string.Empty;
                    }
                }
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

        public Func<CustomerModel?> AddCustomerDialog { get; set; }

        public CustomerManagementViewModel(ICustomerService customerService)
        {
        
            _customerService = customerService;
            AddCustomerDialog = DefaultAddCustomerDialog;
            AddCustomerCommand = new RelayCommand(AddCustomer);
            UpdateCustomerCommand = new RelayCommand(UpdateCustomer, () => SelectedCustomer != null);
            SearchCustomersCommand = new RelayCommand(SearchCustomers);
            DeleteCustomerCommand = new RelayCommand(DeleteCustomer, () => SelectedCustomer != null);
        }

        public void LoadCustomers()
        {
            var all = _customerService.GetAllCustomers();
            Customers.ReplaceRange(all);
        }

        void AddCustomer()
        {
            var customer = AddCustomerDialog?.Invoke();
            if (customer == null) return;

            _customerService.AddCustomer(customer);
            LoadCustomers();
            NewCustomerName = string.Empty;
            NewCustomerEmail = string.Empty;
            NewCustomerContact = string.Empty;
            NewCustomerPhone = string.Empty;
            NewCustomerMobile = string.Empty;
            NewCustomerAddress = string.Empty;
        }

        CustomerModel? DefaultAddCustomerDialog()
        {
            var customer = new CustomerModel();
            CustomerEditWindow win = null!;
            win = new CustomerEditWindow(customer,
                onSave: () => win.DialogResult = true,
                onCancel: () => win.DialogResult = false);
            try { win.Owner = System.Windows.Application.Current?.MainWindow; } catch { }
            try { return win.ShowDialog() == true ? customer : null; } catch { return null; }
        }

        void UpdateCustomer()
        {
            if (SelectedCustomer == null) return;
            var updated = new CustomerModel
            {
                CustomerID = SelectedCustomer.CustomerID,
                Company = NewCustomerName,
                Email = NewCustomerEmail,
                Contact = NewCustomerContact,
                Phone = NewCustomerPhone,
                Mobile = NewCustomerMobile,
                Address = NewCustomerAddress
            };

            _customerService.UpdateCustomer(updated);
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

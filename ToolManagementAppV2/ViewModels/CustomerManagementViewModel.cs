using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.ViewModels
{
    public class CustomerManagementViewModel : ObservableObject
    {
        private readonly ICustomerService _customerService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<CustomerModel> Customers { get; } = new();

        private CustomerModel _selectedCustomer;
        public CustomerModel SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    ((AsyncRelayCommand)UpdateCustomerCommand).NotifyCanExecuteChanged();
                    ((AsyncRelayCommand)DeleteCustomerCommand).NotifyCanExecuteChanged();

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
                        ClearNewCustomerFields();
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

        public IAsyncRelayCommand AddCustomerCommand { get; }
        public IAsyncRelayCommand UpdateCustomerCommand { get; }
        public IAsyncRelayCommand SearchCustomersCommand { get; }
        public IAsyncRelayCommand DeleteCustomerCommand { get; }

        public CustomerManagementViewModel(ICustomerService customerService, IDialogService dialogService)
        {

            _customerService = customerService;
            _dialogService = dialogService;
            AddCustomerCommand = new AsyncRelayCommand(AddCustomerAsync);
            UpdateCustomerCommand = new AsyncRelayCommand(UpdateCustomerAsync, () => SelectedCustomer != null);
            SearchCustomersCommand = new AsyncRelayCommand(SearchCustomersAsync);
            DeleteCustomerCommand = new AsyncRelayCommand(DeleteCustomerAsync, () => SelectedCustomer != null);
        }

        public async Task LoadCustomersAsync()
        {
            var all = await _customerService.GetAllCustomersAsync();
            Customers.ReplaceRange(all);
        }

        async Task AddCustomerAsync()
        {
            var customer = _dialogService.ShowAddCustomerDialog();
            if (customer == null) return;

            try
            {
                await _customerService.AddCustomerAsync(customer);
                await LoadCustomersAsync();
                ClearNewCustomerFields();
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to add customers.", "Unauthorized");
            }
        }

        async Task UpdateCustomerAsync()
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

            try
            {
                await _customerService.UpdateCustomerAsync(updated);
                await LoadCustomersAsync();
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to update customers.", "Unauthorized");
            }
        }

        async Task SearchCustomersAsync()
        {
            var all = await _customerService.GetAllCustomersAsync();
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

        private void ClearNewCustomerFields()
        {
            NewCustomerName = string.Empty;
            NewCustomerEmail = string.Empty;
            NewCustomerContact = string.Empty;
            NewCustomerPhone = string.Empty;
            NewCustomerMobile = string.Empty;
            NewCustomerAddress = string.Empty;
        }

        async Task DeleteCustomerAsync()
        {
            if (SelectedCustomer == null)
                return;

            try
            {
                await _customerService.DeleteCustomerAsync(SelectedCustomer.CustomerID);
                await SearchCustomersAsync();
                SelectedCustomer = null;
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to delete customers.", "Unauthorized");
            }
        }
    }
}

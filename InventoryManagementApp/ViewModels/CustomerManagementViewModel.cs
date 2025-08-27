using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Utilities.Extensions;

#nullable enable

namespace InventoryManagementApp.ViewModels
{
    public class CustomerManagementViewModel : ObservableObject
    {
        private readonly ICustomerService? _customerService;
        private readonly IDialogService? _dialogService;

        public ObservableCollection<CustomerModel> Customers { get; } = new();

        private CustomerModel? _selectedCustomer;
        public CustomerModel? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    ((AsyncRelayCommand)UpdateCustomerCommand).NotifyCanExecuteChanged();
                    ((AsyncRelayCommand)DeleteCustomerCommand).NotifyCanExecuteChanged();
                    ((AsyncRelayCommand)EditCustomerCommand).NotifyCanExecuteChanged();

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
        string _newCustomerName = string.Empty;
        public string NewCustomerEmail { get => _newCustomerEmail; set => SetProperty(ref _newCustomerEmail, value); }
        string _newCustomerEmail = string.Empty;
        public string NewCustomerContact { get => _newCustomerContact; set => SetProperty(ref _newCustomerContact, value); }
        string _newCustomerContact = string.Empty;
        public string NewCustomerPhone { get => _newCustomerPhone; set => SetProperty(ref _newCustomerPhone, value); }
        string _newCustomerPhone = string.Empty;
        public string NewCustomerMobile { get => _newCustomerMobile; set => SetProperty(ref _newCustomerMobile, value); }
        string _newCustomerMobile = string.Empty;
        public string NewCustomerAddress { get => _newCustomerAddress; set => SetProperty(ref _newCustomerAddress, value); }
        string _newCustomerAddress = string.Empty;

        private string _customerSearchTerm = string.Empty;
        public string CustomerSearchTerm
        {
            get => _customerSearchTerm;
            set => SetProperty(ref _customerSearchTerm, value);
        }

        public IAsyncRelayCommand AddCustomerCommand { get; }
        public IAsyncRelayCommand UpdateCustomerCommand { get; }
        public IAsyncRelayCommand SearchCustomersCommand { get; }
        public IAsyncRelayCommand DeleteCustomerCommand { get; }
        public IAsyncRelayCommand EditCustomerCommand { get; }
        public IAsyncRelayCommand<CustomerModel> EditCustomerFromRowCommand { get; }
        public IAsyncRelayCommand<CustomerModel> DeleteCustomerFromRowCommand { get; }
        public IAsyncRelayCommand ClearCustomerSearchCommand { get; }

        public CustomerManagementViewModel(ICustomerService customerService, IDialogService dialogService)
        {

            _customerService = customerService;
            _dialogService = dialogService;
            AddCustomerCommand = new AsyncRelayCommand(AddCustomerAsync);
            UpdateCustomerCommand = new AsyncRelayCommand(UpdateCustomerAsync, () => SelectedCustomer != null);
            SearchCustomersCommand = new AsyncRelayCommand(SearchCustomersAsync);
            DeleteCustomerCommand = new AsyncRelayCommand(() => DeleteCustomerAsync(), () => SelectedCustomer != null);
            EditCustomerCommand = new AsyncRelayCommand(() => EditCustomerAsync(SelectedCustomer), () => SelectedCustomer != null);
            EditCustomerFromRowCommand = new AsyncRelayCommand<CustomerModel>(EditCustomerAsync);
            DeleteCustomerFromRowCommand = new AsyncRelayCommand<CustomerModel>(c => DeleteCustomerAsync(c));
            ClearCustomerSearchCommand = new AsyncRelayCommand(ClearCustomerSearchAsync);
        }

        public async Task LoadCustomersAsync()
        {
            if (_customerService == null) return;
            var all = await _customerService.GetAllCustomersAsync();
            Customers.ReplaceRange(all);
        }

        async Task AddCustomerAsync()
        {
            var customer = _dialogService?.ShowAddCustomerDialog();
            if (customer == null || _customerService == null || _dialogService == null) return;

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
            if (SelectedCustomer == null || _customerService == null || _dialogService == null) return;
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
            if (_customerService == null) return;
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

        async Task ClearCustomerSearchAsync()
        {
            CustomerSearchTerm = string.Empty;
            await LoadCustomersAsync();
        }

        async Task DeleteCustomerAsync(CustomerModel? customer = null)
        {
            customer ??= SelectedCustomer;
            if (customer == null || _customerService == null || _dialogService == null)
                return;

            try
            {
                await _customerService.DeleteCustomerAsync(customer.CustomerID);
                await SearchCustomersAsync();
                if (ReferenceEquals(SelectedCustomer, customer)) SelectedCustomer = null;
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to delete customers.", "Unauthorized");
            }
        }

        public async Task EditCustomerAsync(CustomerModel? customer)
        {
            if (customer == null || _dialogService == null || _customerService == null) return;
            var edited = _dialogService.ShowEditCustomerDialog(customer);
            if (edited == null) return;
            try
            {
                await _customerService.UpdateCustomerAsync(edited);
                await LoadCustomersAsync();
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to update customers.", "Unauthorized");
            }
        }
    }
}

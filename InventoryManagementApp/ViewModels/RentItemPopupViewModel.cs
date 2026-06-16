using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.ViewModels.Rental
{
    public class RentItemPopupViewModel : ObservableObject
    {
        readonly ICustomerService _customerService;
        readonly IDialogService _dialogService;

        public ObservableCollection<CustomerModel> Customers { get; }
        public ObservableCollection<CustomerModel> FilteredCustomers { get; }
        
        CustomerModel? _selectedCustomer;
        public CustomerModel? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                    CheckOutCommand.NotifyCanExecuteChanged();
            }
        }

        private string _customerSearchText = string.Empty;
        public string CustomerSearchText
        {
            get => _customerSearchText;
            set
            {
                if (SetProperty(ref _customerSearchText, value))
                {
                    FilterCustomers();
                }
            }
        }

        private int _rentalDays = 7;
        public int RentalDays
        {
            get => _rentalDays;
            set
            {
                if (SetProperty(ref _rentalDays, value))
                {
                    SelectedDueDate = DateTime.Today.AddDays(value);
                }
            }
        }

        private DateTime _selectedDueDate = DateTime.Today.AddDays(7);
        public DateTime SelectedDueDate
        {
            get => _selectedDueDate;
            set => SetProperty(ref _selectedDueDate, value);
        }

        public event EventHandler? RequestClose;

        public CustomerModel? SelectedCustomerResult { get; private set; }
        public DateTime SelectedDueDateResult { get; private set; }

        public IRelayCommand CheckOutCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public IAsyncRelayCommand AddCustomerCommand { get; }
        public IRelayCommand SetRentalDaysCommand { get; }

        public RentItemPopupViewModel(ItemModel item, IEnumerable<CustomerModel> customers, ICustomerService customerService, IDialogService dialogService)
        {
            _customerService = customerService;
            _dialogService = dialogService;
            Customers = new ObservableCollection<CustomerModel>(customers);
            FilteredCustomers = new ObservableCollection<CustomerModel>(customers);
            CheckOutCommand = new RelayCommand(Confirm, CanConfirm);
            CancelCommand = new RelayCommand(Cancel);
            AddCustomerCommand = new AsyncRelayCommand(AddCustomerAsync);
            SetRentalDaysCommand = new RelayCommand<string>(SetRentalDays);
        }

        bool CanConfirm() => SelectedCustomer != null;

        void Confirm()
        {
            SelectedCustomerResult = SelectedCustomer;
            SelectedDueDateResult = SelectedDueDate;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        void Cancel()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        async Task AddCustomerAsync()
        {
            var customer = _dialogService.ShowAddCustomerDialog();
            if (customer == null) return;
            await _customerService.AddCustomerAsync(customer);
            Customers.Add(customer);
            FilteredCustomers.Add(customer);
            SelectedCustomer = customer;
        }

        void SetRentalDays(string? days)
        {
            if (int.TryParse(days, out var d))
            {
                RentalDays = d;
            }
        }

        void FilterCustomers()
        {
            FilteredCustomers.Clear();
            
            if (string.IsNullOrWhiteSpace(CustomerSearchText))
            {
                foreach (var customer in Customers)
                {
                    FilteredCustomers.Add(customer);
                }
            }
            else
            {
                var searchTerm = CustomerSearchText.ToLowerInvariant();
                var matches = Customers.Where(c =>
                    (c.Company?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                    (c.Contact?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                    (c.Email?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                    (c.Phone?.ToLowerInvariant().Contains(searchTerm) ?? false));
                
                foreach (var customer in matches)
                {
                    FilteredCustomers.Add(customer);
                }
            }
        }
    }
}

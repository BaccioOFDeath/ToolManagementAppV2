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

        public string CustomerCountSummary => FilteredCustomers.Count == Customers.Count
            ? $"{Customers.Count} customer{(Customers.Count == 1 ? string.Empty : "s")} available"
            : $"{FilteredCustomers.Count} of {Customers.Count} customer{(Customers.Count == 1 ? string.Empty : "s")} shown";

        public string SelectedCustomerSummary => SelectedCustomer == null
            ? "No customer selected"
            : $"{ValueOrNotRecorded(SelectedCustomer.Company)} | {ValueOrNotRecorded(SelectedCustomer.Contact)}";

        public string SelectedCustomerContactLine => SelectedCustomer == null
            ? "Search or select a customer before confirming the rental."
            : $"Email: {ValueOrNotRecorded(SelectedCustomer.Email)} | Phone: {ValueOrNotRecorded(SelectedCustomer.Phone)} | Mobile: {ValueOrNotRecorded(SelectedCustomer.Mobile)}";

        public string SelectedCustomerAddressLine => SelectedCustomer == null
            ? "Customer details will appear here for final advisor review."
            : $"Address: {ValueOrNotRecorded(SelectedCustomer.Address)}";

        public string SelectedCustomerActionHint => SelectedCustomer == null
            ? "Next action: choose the customer collecting this item."
            : $"Next action: confirm the rental for {ValueOrNotRecorded(SelectedCustomer.Company)} due back {SelectedDueDate:yyyy-MM-dd}.";

        CustomerModel? _selectedCustomer;
        public CustomerModel? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    CheckOutCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(SelectedCustomerSummary));
                    OnPropertyChanged(nameof(SelectedCustomerContactLine));
                    OnPropertyChanged(nameof(SelectedCustomerAddressLine));
                    OnPropertyChanged(nameof(SelectedCustomerActionHint));
                }
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
            set
            {
                if (SetProperty(ref _selectedDueDate, value))
                    OnPropertyChanged(nameof(SelectedCustomerActionHint));
            }
        }

        public event EventHandler? RequestClose;

        public CustomerModel? SelectedCustomerResult { get; private set; }
        public DateTime SelectedDueDateResult { get; private set; }

        public IRelayCommand CheckOutCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public IAsyncRelayCommand AddCustomerCommand { get; }
        public IRelayCommand SetRentalDaysCommand { get; }
        public IRelayCommand ClearCustomerSearchCommand { get; }

        public RentItemPopupViewModel(ItemModel item, IEnumerable<CustomerModel> customers, ICustomerService customerService, IDialogService dialogService)
        {
            _customerService = customerService;
            _dialogService = dialogService;
            Customers = new ObservableCollection<CustomerModel>(customers.OrderBy(c => c.Company).ThenBy(c => c.Contact));
            FilteredCustomers = new ObservableCollection<CustomerModel>(Customers);
            CheckOutCommand = new RelayCommand(Confirm, CanConfirm);
            CancelCommand = new RelayCommand(Cancel);
            AddCustomerCommand = new AsyncRelayCommand(AddCustomerAsync);
            SetRentalDaysCommand = new RelayCommand<string>(SetRentalDays);
            ClearCustomerSearchCommand = new RelayCommand(ClearCustomerSearch, () => !string.IsNullOrWhiteSpace(CustomerSearchText));
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
            CustomerSearchText = string.Empty;
            FilterCustomers();
            SelectedCustomer = customer;
        }

        void SetRentalDays(string? days)
        {
            if (int.TryParse(days, out var d))
            {
                RentalDays = d;
            }
        }

        void ClearCustomerSearch()
        {
            CustomerSearchText = string.Empty;
        }

        void FilterCustomers()
        {
            var selected = SelectedCustomer;
            FilteredCustomers.Clear();

            IEnumerable<CustomerModel> matches;
            if (string.IsNullOrWhiteSpace(CustomerSearchText))
            {
                matches = Customers;
            }
            else
            {
                var searchTerm = CustomerSearchText.Trim();
                matches = Customers.Where(c =>
                    Contains(c.Company, searchTerm) ||
                    Contains(c.Contact, searchTerm) ||
                    Contains(c.Email, searchTerm) ||
                    Contains(c.Phone, searchTerm) ||
                    Contains(c.Mobile, searchTerm) ||
                    Contains(c.Address, searchTerm));
            }

            foreach (var customer in matches.OrderBy(c => c.Company).ThenBy(c => c.Contact))
            {
                FilteredCustomers.Add(customer);
            }

            if (selected != null && !FilteredCustomers.Contains(selected))
                SelectedCustomer = null;
            else if (SelectedCustomer == null && FilteredCustomers.Count == 1)
                SelectedCustomer = FilteredCustomers[0];

            OnPropertyChanged(nameof(CustomerCountSummary));
            ClearCustomerSearchCommand.NotifyCanExecuteChanged();
        }

        static bool Contains(string? value, string searchTerm)
            => !string.IsNullOrWhiteSpace(value) && value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);

        static string ValueOrNotRecorded(string? value)
            => string.IsNullOrWhiteSpace(value) ? "Not recorded" : value;
    }
}

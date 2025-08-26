using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
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
        CustomerModel? _selectedCustomer;
        public CustomerModel? SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }
        public DateTime SelectedDueDate { get; set; } = DateTime.Today.AddDays(7);
        public event EventHandler? RequestClose;

        public CustomerModel? SelectedCustomerResult { get; private set; }
        public DateTime SelectedDueDateResult { get; private set; }

        public IRelayCommand CheckOutCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public IAsyncRelayCommand AddCustomerCommand { get; }

        public RentItemPopupViewModel(ItemModel item, IEnumerable<CustomerModel> customers, ICustomerService customerService, IDialogService dialogService)
        {
            _customerService = customerService;
            _dialogService = dialogService;
            Customers = new ObservableCollection<CustomerModel>(customers);
            CheckOutCommand = new RelayCommand(Confirm);
            CancelCommand = new RelayCommand(Cancel);
            AddCustomerCommand = new AsyncRelayCommand(AddCustomerAsync);
        }

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
            SelectedCustomer = customer;
        }
    }
}


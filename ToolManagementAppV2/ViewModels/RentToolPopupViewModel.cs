using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.ViewModels.Rental
{
    public class RentToolPopupViewModel : ObservableObject
    {
        public ObservableCollection<CustomerModel> Customers { get; }
        public CustomerModel? SelectedCustomer { get; set; }
        public DateTime SelectedDueDate { get; set; } = DateTime.Today.AddDays(7);
        public event EventHandler? RequestClose;

        public CustomerModel? SelectedCustomerResult { get; private set; }
        public DateTime SelectedDueDateResult { get; private set; }

        public IRelayCommand CheckOutCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public RentToolPopupViewModel(ItemModel tool, IEnumerable<CustomerModel> customers)
        {
            Customers = new ObservableCollection<CustomerModel>(customers);
            CheckOutCommand = new RelayCommand(Confirm);
            CancelCommand = new RelayCommand(Cancel);
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
    }
}


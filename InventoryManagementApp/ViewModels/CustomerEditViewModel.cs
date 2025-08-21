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

        public CustomerEditViewModel(CustomerModel customer, Action onSave, Action onCancel)
        {
            Customer = customer;
            SaveCommand = new RelayCommand(onSave);
            CancelCommand = new RelayCommand(onCancel);
        }
    }
}

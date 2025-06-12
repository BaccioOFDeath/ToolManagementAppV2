using CommunityToolkit.Mvvm.ComponentModel;

namespace ToolManagementAppV2.ViewModels
{
    internal class CustomerViewModel : ObservableObject
    {
        private CustomerModel _customer;
        public CustomerModel Customer
        {
            get => _customer;
            set => SetProperty(ref _customer, value);
        }

        public CustomerViewModel(CustomerModel customer)
        {
            _customer = customer;
        }

        public string DisplayName => $"{_customer.Company} - {_customer.Email}";
    }
}

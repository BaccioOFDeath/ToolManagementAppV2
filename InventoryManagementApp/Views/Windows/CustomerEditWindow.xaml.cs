// Views/CustomerEditWindow.xaml.cs
using System;
using System.Windows;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class CustomerEditWindow : Window
    {
        public CustomerEditWindow(CustomerModel customer, Action onSave, Action onCancel)
        {
            InitializeComponent();
            this.UseResponsiveDefaultSize(860, 700);
            DataContext = new CustomerEditViewModel(customer, onSave, onCancel);
            this.DisposeDataContextOnUnload();
        }
    }
}

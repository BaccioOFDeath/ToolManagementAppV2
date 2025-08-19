// Views/CustomerEditWindow.xaml.cs
using System;
using System.Windows;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views.Windows
{
    public partial class CustomerEditWindow : Window
    {
        public CustomerEditWindow(CustomerModel customer, Action onSave, Action onCancel)
        {
            InitializeComponent();
            DataContext = new CustomerEditViewModel(customer, onSave, onCancel);
            this.DisposeDataContextOnUnload();
        }
    }
}

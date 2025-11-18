using System;
using System.Windows;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class InputDialogWindow : Window
    {
        public InputDialogViewModel ViewModel => (InputDialogViewModel)DataContext;

        public InputDialogWindow(string title, string message, bool isRequired)
        {
            InitializeComponent();
            Title = title;
            DataContext = new InputDialogViewModel(title, message, isRequired,
                onOk: () => DialogResult = true,
                onCancel: () => DialogResult = false);
            this.DisposeDataContextOnUnload();
        }
    }
}

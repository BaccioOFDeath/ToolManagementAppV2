using System.Windows;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    /// <summary>
    /// Interaction logic for ConfirmDialogWindow.xaml
    /// </summary>
    public partial class ConfirmDialogWindow : Window
    {
        public ConfirmDialogWindow(string message)
        {
            InitializeComponent();
            DataContext = new ConfirmDialogViewModel(message, result => DialogResult = result);
            this.DisposeDataContextOnUnload();
        }

        public ConfirmDialogWindow() : this(string.Empty) { }
    }
}

using System.Windows;
using DeviceManagementApp.ViewModels;
using DeviceManagementApp.Utilities.Extensions;

namespace DeviceManagementApp.Views.Windows
{
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

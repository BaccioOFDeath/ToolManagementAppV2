using System.Windows;
using DeviceManagementApp.ViewModels;
using DeviceManagementApp.Utilities.Extensions;

namespace DeviceManagementApp.Views.Windows
{
    public partial class InfoDialogWindow : Window
    {
        public InfoDialogWindow(string message)
        {
            InitializeComponent();
            DataContext = new InfoDialogViewModel(message, () => DialogResult = true);
            this.DisposeDataContextOnUnload();
        }

        public InfoDialogWindow() : this(string.Empty) { }
    }
}

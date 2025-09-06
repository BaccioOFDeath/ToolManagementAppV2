using System.Windows;
using DeviceManagementApp.ViewModels;

namespace DeviceManagementApp.Views
{
    public partial class AssignDeviceDialog : Window
    {
        public AssignDeviceDialog()
        {
            InitializeComponent();
            DataContext = new AssignDeviceDialogViewModel();
        }

        void Ok_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    }
}

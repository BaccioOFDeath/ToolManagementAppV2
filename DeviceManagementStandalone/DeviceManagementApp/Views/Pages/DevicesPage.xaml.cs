using System.Windows.Controls;
using System.Windows;
using Forms = System.Windows.Forms;
using DeviceManagementApp.ViewModels;

namespace DeviceManagementApp.Views.Pages
{
    public partial class DevicesPage : Page
    {
        public DevicesPage()
        {
            InitializeComponent();
        }

        void BrowseSourceFolder(object sender, RoutedEventArgs e)
        {
            using var dlg = new Forms.FolderBrowserDialog();
            if (dlg.ShowDialog() == Forms.DialogResult.OK && DataContext is DevicesViewModel vm)
                vm.SourceFolder = dlg.SelectedPath;
        }

        void BrowseDestinationFolder(object sender, RoutedEventArgs e)
        {
            using var dlg = new Forms.FolderBrowserDialog();
            if (dlg.ShowDialog() == Forms.DialogResult.OK && DataContext is DevicesViewModel vm)
                vm.DestinationFolder = dlg.SelectedPath;
        }
    }
}

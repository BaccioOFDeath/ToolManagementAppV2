using System.Windows;
using System.Windows.Controls;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class KitManagementPage : Page
    {
        public KitManagementPage()
        {
            InitializeComponent();
            Loaded += KitManagementPage_Loaded;
        }

        private async void KitManagementPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is KitManagementViewModel vm)
            {
                await vm.LoadKitsCommand.ExecuteAsync(null);
            }
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ActivityLogsPage : Page
    {
        public ActivityLogsPage()
        {
            InitializeComponent();
            Loaded += ActivityLogsPage_Loaded;
        }

        private async void ActivityLogsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ActivityLogsViewModel vm)
            {
                await vm.LoadLogsAsync();
            }
        }
    }
}

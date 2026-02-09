using System.Threading;
using System.Windows;
using System.Windows.Controls;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class DashboardPage : Page
    {
        private CancellationTokenSource _loadCts = new();

        public DashboardPage()
        {
            InitializeComponent();
            Loaded += DashboardPage_Loaded;
            Unloaded += DashboardPage_Unloaded;
        }

        private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is DashboardViewModel vm)
            {
                try
                {
                    await vm.LoadAsync(_loadCts.Token);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private void DashboardPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _loadCts.Cancel();
            _loadCts.Dispose();
            _loadCts = new CancellationTokenSource();
        }
    }
}

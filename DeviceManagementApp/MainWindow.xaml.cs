using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.ViewModels;
using DeviceManagementApp.Views.Pages;

namespace DeviceManagementApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var host = ((App)Application.Current).Host;
            var nav = host.Services.GetRequiredService<INavigationService>();
            if (nav is Services.NavigationService ns)
                ns.Frame = MainFrame;
            var vm = host.Services.GetRequiredService<DevicesViewModel>();
            nav.Navigate(new DevicesPage { DataContext = vm });
        }
    }
}

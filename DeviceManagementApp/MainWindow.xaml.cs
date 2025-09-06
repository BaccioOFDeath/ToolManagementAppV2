using System.Windows;
using DeviceManagementApp.Interfaces;

namespace DeviceManagementApp
{
    public partial class MainWindow : Window
    {
        public MainWindow(IMainViewModel vm, INavigationService nav)
        {
            InitializeComponent();
            DataContext = vm;
            if (nav is Services.NavigationService ns)
                ns.Frame = MainFrame;
        }
    }
}

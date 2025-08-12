using System.Windows;

namespace ToolManagementAppV2.Views
{
    /// <summary>
    /// Interaction logic for RentalsFilterWindow.xaml
    /// </summary>
    public partial class RentalsFilterWindow : Window
    {
        public RentalsFilterWindow()
        {
            InitializeComponent();
        }

        void OnClose(object sender, RoutedEventArgs e) => Close();
    }
}

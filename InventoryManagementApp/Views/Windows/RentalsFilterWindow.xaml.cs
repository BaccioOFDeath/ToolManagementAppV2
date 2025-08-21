using System.Windows;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    /// <summary>
    /// Interaction logic for RentalsFilterWindow.xaml
    /// </summary>
    public partial class RentalsFilterWindow : Window
    {
        public RentalsFilterWindow()
        {
            InitializeComponent();
            this.DisposeDataContextOnUnload();
        }
    }
}

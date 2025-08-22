using System.Windows.Controls;
using System.Windows.Input;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ManageItemsPage : Page
    {
        public ManageItemsPage()
        {
            InitializeComponent();
        }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                row.IsSelected = true;
            }
        }
    }
}

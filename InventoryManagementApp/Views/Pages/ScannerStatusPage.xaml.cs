using System.Windows.Controls;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ScannerStatusPage : Page
    {
        public ScannerStatusPage()
        {
            InitializeComponent();
            this.DisposeDataContextOnUnload();
        }
    }
}

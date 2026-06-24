using System.Windows;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class RentItemPopupWindow : Window
    {
        public RentItemPopupWindow()
        {
            InitializeComponent();
            this.UseResponsiveDefaultSize(1040, 760);
            this.DisposeDataContextOnUnload();
        }
    }
}


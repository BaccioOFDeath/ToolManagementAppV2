using System.Windows;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views.Windows
{
    public partial class RentItemPopupWindow : Window
    {
        public RentItemPopupWindow()
        {
            InitializeComponent();
            this.DisposeDataContextOnUnload();
        }
    }
}


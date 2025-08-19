using System.Windows;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views.Windows
{
    public partial class RentToolPopupWindow : Window
    {
        public RentToolPopupWindow()
        {
            InitializeComponent();
            this.DisposeDataContextOnUnload();
        }
    }
}


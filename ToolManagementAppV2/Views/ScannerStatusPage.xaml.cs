using System.Windows.Controls;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views
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

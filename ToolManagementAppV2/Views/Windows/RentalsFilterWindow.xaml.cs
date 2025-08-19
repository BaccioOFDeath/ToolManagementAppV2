using System.Windows;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views.Windows
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

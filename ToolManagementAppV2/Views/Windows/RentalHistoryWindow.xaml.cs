using System.Windows;
using ToolManagementAppV2.ViewModels.Rental;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views.Windows
{
    public partial class RentalHistoryWindow : Window
    {
        public RentalHistoryWindow()
        {
            InitializeComponent();
            this.DisposeDataContextOnUnload();
        }

        public RentalHistoryWindow(RentalHistoryViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}

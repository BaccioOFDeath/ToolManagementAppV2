using System.Windows;
using ToolManagementAppV2.ViewModels.Rental;

namespace ToolManagementAppV2.Views
{
    public partial class RentalHistoryWindow : Window
    {
        public RentalHistoryWindow()
        {
            InitializeComponent();
        }

        public RentalHistoryWindow(RentalHistoryViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}

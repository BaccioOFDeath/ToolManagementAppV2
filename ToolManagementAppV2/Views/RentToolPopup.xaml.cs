using System.Windows;

namespace ToolManagementAppV2.Views
{
    public partial class RentToolPopupWindow : Window
    {
        public RentToolPopupWindow()
        {
            InitializeComponent();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.Rental.RentToolPopupViewModel vm)
                vm.Confirm();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

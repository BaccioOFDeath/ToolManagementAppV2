using System.Windows;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            var vm = new LoginViewModel();
            vm.LoginSucceeded += (_, __) => DialogResult = true;
            DataContext = vm;
        }
    }
}

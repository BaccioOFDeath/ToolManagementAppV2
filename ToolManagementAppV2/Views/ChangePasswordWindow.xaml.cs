// Views/ChangePasswordWindow.xaml.cs
using System.Windows;
using System.Windows.Controls;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2.Views
{
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordViewModel VM => (ChangePasswordViewModel)DataContext;
        public string NewPassword => VM.NewPassword;

        public ChangePasswordWindow()
        {
            InitializeComponent();
            DataContext = new ChangePasswordViewModel(() => DialogResult = true, () => DialogResult = false);
        }

        void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
            => VM.NewPassword = ((PasswordBox)sender).Password;

        void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
            => VM.ConfirmPassword = ((PasswordBox)sender).Password;
    }
}

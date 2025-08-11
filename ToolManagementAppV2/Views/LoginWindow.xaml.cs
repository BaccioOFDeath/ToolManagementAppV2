// Views/LoginWindow.xaml.cs
using System;
using System.Windows;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            if (DataContext == null)
                DataContext = new LoginViewModel(); // optional: your App may set this already

            if (DataContext is LoginViewModel vm)
                vm.LoginSucceeded += OnLoginSucceeded;
        }

        private void OnLoginSucceeded(object? sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}

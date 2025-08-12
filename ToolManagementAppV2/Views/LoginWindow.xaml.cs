// Views/LoginWindow.xaml.cs
using System;
using System.Windows;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Services;

namespace ToolManagementAppV2
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            if (DataContext is not LoginViewModel vm)
            {
                vm = new LoginViewModel(new DialogService());
                DataContext = vm;
            }

            vm.LoginSucceeded += OnLoginSucceeded;
            Closed += (_, __) => vm.LoginSucceeded -= OnLoginSucceeded;
        }

        private void OnLoginSucceeded(object? sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}

// Views/LoginWindow.xaml.cs
using System;
using System.Windows;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2
{
    public partial class LoginWindow : Window
    {
        public LoginWindow(IUserContext? userContext = null, IDialogService? dialogService = null)
        {
            InitializeComponent();

            var vm = new LoginViewModel(dialogService ?? new DialogService(), userContext ?? new ApplicationUserContext());
            DataContext = vm;

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

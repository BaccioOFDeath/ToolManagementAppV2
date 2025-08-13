using System;
using System.Windows;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml.
    /// Provides an <see cref="IUserContext"/> and optional <see cref="IDialogService"/>
    /// to the <see cref="LoginViewModel"/>.
    /// </summary>
    public partial class LoginWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginWindow"/> class.
        /// </summary>
        /// <param name="userContext">Context containing information about the current user.</param>
        /// <param name="dialogService">Optional dialog service used by the view model.</param>
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

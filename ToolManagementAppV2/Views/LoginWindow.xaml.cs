using System;
using System.Windows;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Settings;

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
        /// <param name="userService">Service used for user operations.</param>
        /// <param name="settingsService">Service used for application settings.</param>
        /// <param name="dialogService">Optional dialog service used by the view model.</param>
        public LoginWindow(IUserContext userContext, IUserService userService, ISettingsService settingsService, IDialogService? dialogService = null)
        {
            InitializeComponent();

            var vm = new LoginViewModel(userService, settingsService, dialogService ?? new DialogService(), userContext);
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

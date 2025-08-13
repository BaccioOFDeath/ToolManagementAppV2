// App.xaml.cs
using System;
using System.IO;
using System.Windows;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            base.OnStartup(e);

            // Boot main window and data context FIRST so it shows behind login
            var db = new DatabaseService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tool_inventory.db"));
            var toolService = new ToolService(db);
            var customerService = new CustomerService(db);
            var userContext = new ApplicationUserContext();
            var userService = new UserService(db, userContext);
            var rentalService = new RentalService(db, toolService);
            var activityLogService = new ActivityLogService(db);
            var fileDialogService = new FileDialogService();
            var settingsService = new SettingsService(db);

            var main = new MainWindow
            {
                DataContext = new MainViewModel(toolService, userService, userContext, customerService, rentalService, fileDialogService, activityLogService, settingsService)
            };

            Current.MainWindow = main;
            main.Show(); // stays visible behind login

            var login = new LoginWindow(userContext)
            {
                Owner = main,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var ok = login.ShowDialog() == true;
            if (!ok)
            {
                main.Close();
                return;
            }

            if (main.DataContext is MainViewModel vm)
                vm.RefreshCurrentUser();

            // bring main to front after login
            if (main.WindowState == WindowState.Minimized) main.WindowState = WindowState.Normal;
            main.Activate();
            main.Focus();
        }
    }
}

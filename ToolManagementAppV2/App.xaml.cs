// App.xaml.cs – Use OnExplicitShutdown while showing the login window, then switch after login
using System.Windows;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            base.OnStartup(e);

            var mainWindow = new MainWindow();
            Current.MainWindow = mainWindow;
            mainWindow.Show();
            if (mainWindow.DataContext is MainViewModel vmStartup)
                vmStartup.RefreshCurrentUser();

            var login = new LoginWindow { Owner = mainWindow };
            login.Closed += (s, args) =>
            {
                if (login.DialogResult != true)
                    mainWindow.Close();
                else if (mainWindow.DataContext is MainViewModel vm)
                    vm.RefreshCurrentUser();
            };
            // Display the login window modally so that DialogResult can be set
            // without throwing an InvalidOperationException when the user logs in.
            login.ShowDialog();
        }
    }
}

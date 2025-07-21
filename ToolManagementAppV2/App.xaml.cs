// App.xaml.cs – Use OnExplicitShutdown while showing the login window, then switch after login
using System.Windows;

namespace ToolManagementAppV2
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            base.OnStartup(e);

            var login = new LoginWindow();
            bool? result = login.ShowDialog();

            if (result == true)
            {
                ShutdownMode = ShutdownMode.OnMainWindowClose;
                MainWindow mainWindow = new MainWindow();
                Current.MainWindow = mainWindow;
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }
    }
}

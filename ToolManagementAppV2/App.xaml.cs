// App.xaml.cs – Use OnExplicitShutdown while showing the login window, then switch after login
using System.Windows;

namespace ToolManagementAppV2
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Prevent shutdown when the login window closes
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            base.OnStartup(e);

            LoginWindow login = new LoginWindow();
            bool? loginResult = login.ShowDialog();

            if (loginResult == true)
            {
                // Switch shutdown mode now that we are creating the main window
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

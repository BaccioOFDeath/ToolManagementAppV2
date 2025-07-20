// App.xaml.cs – Use OnExplicitShutdown while showing the login window, then switch after login
using System.Windows;

namespace ToolManagementAppV2
{
    public partial class App : System.Windows.Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            base.OnStartup(e);

            var login = new LoginWindow();
            login.Show();

            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            void Handler(object s, System.EventArgs args)
            {
                tcs.TrySetResult(login.DialogResult == true);
                login.Closed -= Handler;
            }
            login.Closed += Handler;
            bool result = await tcs.Task;

            if (result)
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

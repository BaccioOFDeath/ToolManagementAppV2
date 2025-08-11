// Utilities/Navigation.cs
using System.Windows;

namespace ToolManagementAppV2
{
    public static class Navigation
    {
        public static void ShowMainWindow()
        {
            var current = System.Windows.Application.Current.MainWindow as MainWindow;

            if (current == null || !current.IsLoaded)
            {
                current = new MainWindow();
                System.Windows.Application.Current.MainWindow = current;
            }

            if (!current.IsVisible) current.Show();
            if (current.WindowState == WindowState.Minimized) current.WindowState = WindowState.Normal;

            current.Activate();
            current.Focus();
        }
    }
}

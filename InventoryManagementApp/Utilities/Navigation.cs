// Utilities/Navigation.cs
using System;
using System.Windows;

namespace InventoryManagementApp
{
    public static class Navigation
    {
        public static void ShowMainWindow()
        {
            var current = System.Windows.Application.Current.MainWindow as MainWindow;

            if (current == null)
                throw new InvalidOperationException("Main window is not initialized.");

            if (!current.IsVisible) current.Show();
            if (current.WindowState == WindowState.Minimized) current.WindowState = WindowState.Normal;

            current.Activate();
            current.Focus();
        }
    }
}

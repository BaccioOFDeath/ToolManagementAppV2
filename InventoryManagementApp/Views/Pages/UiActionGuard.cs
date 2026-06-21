using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InventoryManagementApp.Views.Pages
{
    internal static class UiActionGuard
    {
        public static void Run(Page page, string title, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                ShowClickError(page, title, ex);
            }
        }

        public static async void RunAsync(Page page, string title, Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                ShowClickError(page, title, ex);
            }
        }

        private static void ShowClickError(Page page, string title, Exception ex)
        {
            var owner = Window.GetWindow(page);
            var message = $"The action could not be completed.{Environment.NewLine}{ex.Message}";
            if (owner != null)
                MessageBox.Show(owner, message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

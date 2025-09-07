using System.Windows;
using DeviceManagementApp.Interfaces;

namespace DeviceManagementApp.Services
{
    public class DialogService : IDialogService
    {
        public void ShowInfo(string message, string title)
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

        public bool ShowConfirmation(string message, string title)
            => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}

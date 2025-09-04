using System.Threading.Tasks;
using System.Windows;

namespace DeviceManagementApp.Interfaces
{
    public interface IDialogService
    {
        void ShowInfo(string message, string title);
        Task ShowInfoAsync(string message, string title) =>
            Application.Current?.Dispatcher?.InvokeAsync(() => ShowInfo(message, title)).Task
            ?? Task.CompletedTask;
        bool ShowConfirmation(string message, string title);
        Task<bool> ShowConfirmationAsync(string message, string title) =>
            Application.Current?.Dispatcher?.InvokeAsync(() => ShowConfirmation(message, title)).Task
            ?? Task.FromResult(false);
    }
}

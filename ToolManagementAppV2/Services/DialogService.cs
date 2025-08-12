using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Views;

namespace ToolManagementAppV2.Services
{
    public class DialogService : IDialogService
    {
        public void ShowInfo(string message, string title)
        {
            var dialog = new InfoDialogWindow(message) { Title = title };
            dialog.ShowDialog();
        }

        public bool ShowConfirmation(string message, string title)
        {
            var dialog = new ConfirmDialogWindow(message) { Title = title };
            return dialog.ShowDialog() == true;
        }
    }
}

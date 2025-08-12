namespace ToolManagementAppV2.Interfaces
{
    public interface IDialogService
    {
        void ShowInfo(string message, string title);
        bool ShowConfirmation(string message, string title);
    }
}

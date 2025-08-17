namespace ToolManagementAppV2.Interfaces
{
    public interface IFileDialogService
    {
        string? OpenFile(string filter, string? initialDirectory = null);
        string? SaveFile(string filter);
    }
}

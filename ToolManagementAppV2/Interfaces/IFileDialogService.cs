namespace ToolManagementAppV2.Interfaces
{
    public interface IFileDialogService
    {
        string? OpenFile(string filter);
        string? SaveFile(string filter);
    }
}

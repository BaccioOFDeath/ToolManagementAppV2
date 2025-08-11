using Microsoft.Win32;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.Services.Core
{
    public class FileDialogService : IFileDialogService
    {
        public string? OpenFile(string filter)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter
            };
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }
    }
}

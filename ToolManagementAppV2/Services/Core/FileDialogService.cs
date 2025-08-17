using System;
using Microsoft.Win32;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.Services.Core
{
    public class FileDialogService : IFileDialogService
    {
        public string? OpenFile(string filter, string? initialDirectory = null)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter
            };
            if (!string.IsNullOrEmpty(initialDirectory))
            {
                dlg.InitialDirectory = initialDirectory;
            }
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }

        public string? SaveFile(string filter)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = filter
            };
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }
    }
}

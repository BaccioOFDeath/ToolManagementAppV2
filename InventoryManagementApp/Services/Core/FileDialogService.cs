using System;
using System.Windows.Forms;
using Microsoft.Win32;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Services.Core
{
    public class FileDialogService : IFileDialogService
    {
        public string? OpenFile(string filter, string? initialDirectory = null)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter
            };
            if (!string.IsNullOrWhiteSpace(initialDirectory))
            {
                dlg.InitialDirectory = initialDirectory;
            }
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }

        public string? SaveFile(string filter, string? initialDirectory = null)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = filter
            };
            if (!string.IsNullOrWhiteSpace(initialDirectory))
            {
                dlg.InitialDirectory = initialDirectory;
            }
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }

        public string? BrowseFolder(string? initialDirectory = null)
        {
            using var dlg = new FolderBrowserDialog
            {
                SelectedPath = initialDirectory ?? string.Empty,
                ShowNewFolderButton = true
            };
            return dlg.ShowDialog() == DialogResult.OK ? dlg.SelectedPath : null;
        }
    }
}

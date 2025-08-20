// Views/ToolEditWindow.xaml.cs
using System;
using System.Windows;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views.Windows
{
    public partial class ToolEditWindow : Window
    {
        private readonly IFileDialogService _fileDialogService;

        public ToolEditWindow(ItemModel tool, Action onSave, Action onCancel, IFileDialogService fileDialogService)
        {
            InitializeComponent();
            _fileDialogService = fileDialogService;
            DataContext = new ToolEditViewModel(tool, onSave, onCancel, _fileDialogService);
            this.DisposeDataContextOnUnload();
        }
    }
}

// Views/ToolEditWindow.xaml.cs
using System;
using System.Windows;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views
{
    public partial class ToolEditWindow : Window
    {
        public ToolEditWindow(ToolModel tool, Action onSave, Action onCancel)
        {
            InitializeComponent();
            var fileDialog = new FileDialogService();
            DataContext = new ToolEditViewModel(tool, onSave, onCancel, fileDialog);
            this.DisposeDataContextOnUnload();
        }
    }
}

// Views/ToolEditWindow.xaml.cs
using System;
using System.Windows;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2.Views
{
    public partial class ToolEditWindow : Window
    {
        public ToolEditWindow(ToolModel tool, Action onSave, Action onCancel)
        {
            InitializeComponent();
            DataContext = new ToolEditViewModel(tool, onSave, onCancel);
        }
    }
}

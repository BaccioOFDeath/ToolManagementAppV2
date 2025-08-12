// Views/UsersEditWindow.xaml.cs
using System;
using System.Windows;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2.Views
{
    public partial class UsersEditWindow : Window
    {
        public UsersEditWindow(User user, Action onSave, Action onCancel, Action onBrowseAvatar, Action onRemoveAvatar)
        {
            InitializeComponent();
            DataContext = new UsersEditViewModel(user, onSave, onCancel, onBrowseAvatar, onRemoveAvatar);
        }
    }
}

// Views/UsersEditWindow.xaml.cs
using System;
using System.Threading.Tasks;
using System.Windows;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Views.Windows
{
    public partial class UsersEditWindow : Window
    {
        public UsersEditWindow(User user, Func<Task> onSave, Action onCancel, IFileDialogService fileDialogService)
        {
            InitializeComponent();
            this.UseResponsiveDefaultSize(1120, 880);
            DataContext = new UsersEditViewModel(user, fileDialogService, onSave, onCancel);
            this.DisposeDataContextOnUnload();
        }
    }
}

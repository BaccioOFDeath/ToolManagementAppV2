// Views/ItemEditWindow.xaml.cs
using System;
using System.Windows;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class ItemEditWindow : Window
    {
        private readonly IFileDialogService _fileDialogService;

        public ItemEditWindow(ItemModel item, Action onSave, Action onCancel, IFileDialogService fileDialogService)
        {
            InitializeComponent();
            _fileDialogService = fileDialogService;
            DataContext = new ItemEditViewModel(item, onSave, onCancel, _fileDialogService);
            this.DisposeDataContextOnUnload();
        }
    }
}

using System.Windows;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class KitItemEditWindow : Window
    {
        public KitItemEditWindow(KitItem kitItem, bool isNew)
        {
            InitializeComponent();
            DataContext = new KitItemEditViewModel(kitItem, isNew,
                onSave: () => DialogResult = true,
                onCancel: () => DialogResult = false);
            this.DisposeDataContextOnUnload();
        }
    }
}

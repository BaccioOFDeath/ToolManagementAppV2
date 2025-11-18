using System.Windows;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class KitEditWindow : Window
    {
        public KitEditWindow(Kit kit, bool isNew)
        {
            InitializeComponent();
            DataContext = new KitEditViewModel(kit, isNew,
                onSave: () => DialogResult = true,
                onCancel: () => DialogResult = false);
            this.DisposeDataContextOnUnload();
        }
    }
}

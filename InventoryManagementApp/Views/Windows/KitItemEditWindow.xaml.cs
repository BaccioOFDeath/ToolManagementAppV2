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
            this.UseResponsiveDefaultSize(760, 660);
            DataContext = new KitItemEditViewModel(kitItem, isNew,
                onSave: () => DialogResult = true,
                onCancel: () => DialogResult = false);
            this.DisposeDataContextOnUnload();
        }
    }
}

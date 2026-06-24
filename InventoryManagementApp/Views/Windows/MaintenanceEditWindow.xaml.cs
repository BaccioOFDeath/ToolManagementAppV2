using System.Windows;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class MaintenanceEditWindow : Window
    {
        public MaintenanceEditWindow(MaintenanceRecord record, bool isNew)
        {
            InitializeComponent();
            this.UseResponsiveDefaultSize(880, 720);
            DataContext = new MaintenanceEditViewModel(record, isNew,
                onSave: () => DialogResult = true,
                onCancel: () => DialogResult = false);
            this.DisposeDataContextOnUnload();
        }
    }
}

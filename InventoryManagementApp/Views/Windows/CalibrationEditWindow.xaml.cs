using System.Windows;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class CalibrationEditWindow : Window
    {
        public CalibrationEditWindow(CalibrationRecord record, bool isNew)
        {
            InitializeComponent();
            this.UseResponsiveDefaultSize(880, 720);
            DataContext = new CalibrationEditViewModel(record, isNew,
                onSave: () => DialogResult = true,
                onCancel: () => DialogResult = false);
            this.DisposeDataContextOnUnload();
        }
    }
}

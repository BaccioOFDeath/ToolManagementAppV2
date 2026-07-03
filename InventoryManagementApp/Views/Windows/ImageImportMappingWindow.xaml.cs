using System.Windows;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class ImageImportMappingWindow : Window
    {
        public ImageImportMappingWindow()
        {
            InitializeComponent();
            this.UseResponsiveDefaultSize(720, 620);
            DataContext = new ImageImportMappingViewModel(
                () => { DialogResult = true; Close(); },
                () => { DialogResult = false; Close(); });
            this.DisposeDataContextOnUnload();
        }

        public ImageImportMappingViewModel VM => (ImageImportMappingViewModel)DataContext;
    }
}
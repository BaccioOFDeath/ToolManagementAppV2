using System.Windows;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views.Windows
{
    public partial class ImageImportMappingWindow : Window
    {
        public ImageImportMappingWindow()
        {
            InitializeComponent();
            DataContext = new ImageImportMappingViewModel(
                () => { DialogResult = true; Close(); },
                () => { DialogResult = false; Close(); });
            this.DisposeDataContextOnUnload();
        }

        public ImageImportMappingViewModel VM => (ImageImportMappingViewModel)DataContext;
    }
}

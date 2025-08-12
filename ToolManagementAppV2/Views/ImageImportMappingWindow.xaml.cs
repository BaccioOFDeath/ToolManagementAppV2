using System.Windows;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2.Views
{
    public partial class ImageImportMappingWindow : Window
    {
        public ImageImportMappingWindow()
        {
            InitializeComponent();
            DataContext = new ImageImportMappingViewModel(
                () => { DialogResult = true; Close(); },
                () => { DialogResult = false; Close(); });
        }

        public ImageImportMappingViewModel VM => (ImageImportMappingViewModel)DataContext;
    }
}

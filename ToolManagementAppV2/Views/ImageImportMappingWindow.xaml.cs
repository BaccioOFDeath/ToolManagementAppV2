using System.Windows;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2.Views
{
    public partial class ImageImportMappingWindow : Window
    {
        public ImageImportMappingWindow()
        {
            InitializeComponent();
            DataContext = new ImageImportMappingViewModel();
        }

        public ImageImportMappingViewModel VM => (ImageImportMappingViewModel)DataContext;

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

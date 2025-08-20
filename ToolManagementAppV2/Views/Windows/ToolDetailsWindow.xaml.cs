// Views/ToolDetailsWindow.xaml.cs
using System.Windows;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views.Windows
{
    public partial class ToolDetailsWindow : Window
    {
        public ToolDetailsWindow(ItemModel tool)
        {
            InitializeComponent();
            DataContext = new ToolDetailsViewModel(tool, () => Close());
            this.DisposeDataContextOnUnload();
        }
    }
}

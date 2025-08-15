// Views/ToolDetailsWindow.xaml.cs
using System.Windows;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views
{
    public partial class ToolDetailsWindow : Window
    {
        public ToolDetailsWindow(ToolModel tool)
        {
            InitializeComponent();
            DataContext = new ToolDetailsViewModel(tool, () => Close());
            this.DisposeDataContextOnUnload();
        }
    }
}

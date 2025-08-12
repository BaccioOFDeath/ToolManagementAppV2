// Views/ToolDetailsWindow.xaml.cs
using System.Windows;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2.Views
{
    public partial class ToolDetailsWindow : Window
    {
        public ToolDetailsWindow(ToolModel tool)
        {
            InitializeComponent();
            DataContext = new ToolDetailsViewModel(tool, () => Close());
        }
    }
}

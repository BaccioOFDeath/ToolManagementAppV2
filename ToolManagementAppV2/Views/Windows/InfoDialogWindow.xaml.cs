using System.Windows;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views.Windows
{
    /// <summary>
    /// Interaction logic for InfoDialogWindow.xaml
    /// </summary>
    public partial class InfoDialogWindow : Window
    {
        public InfoDialogWindow(string message)
        {
            InitializeComponent();
            DataContext = new InfoDialogViewModel(message, () => DialogResult = true);
            this.DisposeDataContextOnUnload();
        }

        public InfoDialogWindow() : this(string.Empty) { }
    }
}

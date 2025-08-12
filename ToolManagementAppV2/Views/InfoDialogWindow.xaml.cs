using System.Windows;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2.Views
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
        }

        public InfoDialogWindow() : this(string.Empty) { }
    }
}

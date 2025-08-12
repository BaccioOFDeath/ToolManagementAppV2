using System.Windows;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2.Views
{
    /// <summary>
    /// Interaction logic for ConfirmDialogWindow.xaml
    /// </summary>
    public partial class ConfirmDialogWindow : Window
    {
        public ConfirmDialogWindow(string message)
        {
            InitializeComponent();
            DataContext = new ConfirmDialogViewModel(message, result => DialogResult = result);
        }

        public ConfirmDialogWindow() : this(string.Empty) { }
    }
}

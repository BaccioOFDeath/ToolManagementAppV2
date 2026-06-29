using System.Windows;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
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
            this.UseResponsiveDefaultSize(760, 520);
            this.DisposeDataContextOnUnload();
        }

        public InfoDialogWindow() : this(string.Empty) { }
    }
}

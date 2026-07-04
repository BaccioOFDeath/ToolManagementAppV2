using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    /// <summary>
    /// Interaction logic for PrintLabelWindow.xaml
    /// </summary>
    public partial class PrintLabelWindow : Window
    {
        private readonly IDialogService _dialogService;

        public PrintLabelWindow(IDialogService dialogService)
        {
            InitializeComponent();
            this.UseResponsiveDefaultSize(760, 520);
            _dialogService = dialogService;
            DataContext = new PrintLabelViewModel(_dialogService, () => Close());
            this.DisposeDataContextOnUnload();
        }
    }
}
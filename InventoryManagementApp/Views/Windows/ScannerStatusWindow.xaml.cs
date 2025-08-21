using System.Windows;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class ScannerStatusWindow : Window
    {
        private readonly IScannerService _scannerService;
        private readonly IDialogService _dialogService;

        public ScannerStatusWindow(IScannerService scannerService, IDialogService dialogService)
        {
            InitializeComponent();
            _scannerService = scannerService;
            _dialogService = dialogService;
            DataContext = new ScannerStatusViewModel(_scannerService, _dialogService);
            this.DisposeDataContextOnUnload();
        }
    }
}

using System.Windows;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views
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

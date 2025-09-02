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
        private readonly ISettingsService _settingsService;
        private readonly IScannerGroupService _groupService;

        public ScannerStatusWindow(IScannerService scannerService, IDialogService dialogService, ISettingsService settingsService, IScannerGroupService groupService)
        {
            InitializeComponent();
            _scannerService = scannerService;
            _dialogService = dialogService;
            _settingsService = settingsService;
            _groupService = groupService;
            DataContext = new ScannerStatusViewModel(_scannerService, _dialogService, _settingsService, _groupService);
            this.DisposeDataContextOnUnload();
        }
    }
}

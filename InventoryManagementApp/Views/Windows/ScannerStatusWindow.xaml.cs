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
        private readonly IDeviceService _deviceService;
        private readonly IDeviceGroupService _groupService;
        private readonly IDeviceFileService _fileService;
        private readonly IScannerRuleService _ruleService;

        public ScannerStatusWindow(IScannerService scannerService, IDialogService dialogService, IDeviceService deviceService, IDeviceGroupService groupService, IDeviceFileService fileService, IScannerRuleService ruleService)
        {
            InitializeComponent();
            _scannerService = scannerService;
            _dialogService = dialogService;
            _deviceService = deviceService;
            _groupService = groupService;
            _fileService = fileService;
            _ruleService = ruleService;
            DataContext = new ScannerStatusViewModel(_scannerService, _dialogService, _deviceService, _groupService, _fileService, _ruleService);
            this.DisposeDataContextOnUnload();
        }
    }
}

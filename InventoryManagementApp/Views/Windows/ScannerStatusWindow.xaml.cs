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
        private readonly IDeviceGroupService _groupService;
        private readonly IScannerFileService _fileService;
        private readonly IScannerRuleService _ruleService;

        public ScannerStatusWindow(IScannerService scannerService, IDialogService dialogService, ISettingsService settingsService, IDeviceGroupService groupService, IScannerFileService fileService, IScannerRuleService ruleService)
        {
            InitializeComponent();
            _scannerService = scannerService;
            _dialogService = dialogService;
            _settingsService = settingsService;
            _groupService = groupService;
            _fileService = fileService;
            _ruleService = ruleService;
            DataContext = new ScannerStatusViewModel(_scannerService, _dialogService, _settingsService, _groupService, _fileService, _ruleService);
            this.DisposeDataContextOnUnload();
        }
    }
}

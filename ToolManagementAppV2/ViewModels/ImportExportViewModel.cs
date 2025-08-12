using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.ViewModels
{
    public class ImportExportViewModel : ObservableObject
    {
        private readonly IToolService _toolService;
        private readonly ICustomerService _customerService;
        private readonly IFileDialogService _fileDialogService;

        public IRelayCommand ImportToolsCommand { get; }
        public IRelayCommand ExportToolsCommand { get; }
        public IRelayCommand ImportCustomersCommand { get; }
        public IRelayCommand ExportCustomersCommand { get; }

        public ImportExportViewModel(IToolService toolService,
                                     ICustomerService customerService,
                                     IFileDialogService fileDialogService)
        {
            _toolService = toolService;
            _customerService = customerService;
            _fileDialogService = fileDialogService;
            ImportToolsCommand = new RelayCommand(ImportTools);
            ExportToolsCommand = new RelayCommand(ExportTools);
            ImportCustomersCommand = new RelayCommand(ImportCustomers);
            ExportCustomersCommand = new RelayCommand(ExportCustomers);
        }

        void ImportTools()
        {
            var path = _fileDialogService.OpenFile("CSV Files|*.csv");
            if (!string.IsNullOrEmpty(path))
                _toolService.ImportToolsFromCsv(path, new Dictionary<string, string>());
        }

        void ExportTools()
        {
            var path = _fileDialogService.OpenFile("CSV Files|*.csv");
            if (!string.IsNullOrEmpty(path))
                _toolService.ExportToolsToCsv(path);
        }

        void ImportCustomers()
        {
            var path = _fileDialogService.OpenFile("CSV Files|*.csv");
            if (!string.IsNullOrEmpty(path))
                _customerService.ImportCustomersFromCsv(path, new Dictionary<string, string>());
        }

        void ExportCustomers()
        {
            var path = _fileDialogService.OpenFile("CSV Files|*.csv");
            if (!string.IsNullOrEmpty(path))
                _customerService.ExportCustomersToCsv(path);
        }
    }
}

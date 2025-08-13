using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using ToolManagementAppV2.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ToolManagementAppV2.ViewModels
{
    public class ImportExportViewModel : ObservableObject
    {
        private readonly IToolService _toolService;
        private readonly ICustomerService _customerService;
        private readonly IFileDialogService _fileDialogService;
        private readonly ILogger<ImportExportViewModel> _logger;

        public IRelayCommand ImportToolsCommand { get; }
        public IRelayCommand ExportToolsCommand { get; }
        public IRelayCommand ImportCustomersCommand { get; }
        public IRelayCommand ExportCustomersCommand { get; }

        public ObservableCollection<string> ImportExportLogs { get; } = new();

        public ImportExportViewModel(IToolService toolService,
                                     ICustomerService customerService,
                                     IFileDialogService fileDialogService,
                                     ILogger<ImportExportViewModel>? logger = null)
        {
            _toolService = toolService;
            _customerService = customerService;
            _fileDialogService = fileDialogService;
            _logger = logger ?? NullLogger<ImportExportViewModel>.Instance;
            ImportToolsCommand = new RelayCommand(ImportTools);
            ExportToolsCommand = new RelayCommand(ExportTools);
            ImportCustomersCommand = new RelayCommand(ImportCustomers);
            ExportCustomersCommand = new RelayCommand(ExportCustomers);
        }

        void ImportTools()
        {
            var path = _fileDialogService.OpenFile("CSV Files|*.csv");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                _toolService.ImportToolsFromCsv(path, new Dictionary<string, string>());
                ImportExportLogs.Add($"Successfully imported tools from {path}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import tools from {Path}", path);
                ImportExportLogs.Add($"Failed to import tools from {path}: {ex.Message}");
            }
        }

        void ExportTools()
        {
            var path = _fileDialogService.SaveFile("CSV Files|*.csv");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                _toolService.ExportToolsToCsv(path);
                ImportExportLogs.Add($"Successfully exported tools to {path}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export tools to {Path}", path);
                ImportExportLogs.Add($"Failed to export tools to {path}: {ex.Message}");
            }
        }

        void ImportCustomers()
        {
            var path = _fileDialogService.OpenFile("CSV Files|*.csv");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                _customerService.ImportCustomersFromCsv(path, new Dictionary<string, string>());
                ImportExportLogs.Add($"Successfully imported customers from {path}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import customers from {Path}", path);
                ImportExportLogs.Add($"Failed to import customers from {path}: {ex.Message}");
            }
        }

        void ExportCustomers()
        {
            var path = _fileDialogService.SaveFile("CSV Files|*.csv");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                _customerService.ExportCustomersToCsv(path);
                ImportExportLogs.Add($"Successfully exported customers to {path}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export customers to {Path}", path);
                ImportExportLogs.Add($"Failed to export customers to {path}: {ex.Message}");
            }
        }
    }
}

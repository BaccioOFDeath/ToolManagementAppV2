using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.ImportExport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ToolManagementAppV2.Utilities.IO;

namespace ToolManagementAppV2.ViewModels
{
    public class ImportExportViewModel : ObservableObject
    {
        private readonly IToolService _toolService;
        private readonly ICustomerService _customerService;
        private readonly IFileDialogService _fileDialogService;
        private readonly IDatabaseBackupService _databaseService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ImportExportViewModel> _logger;

        public IAsyncRelayCommand ImportToolsCommand { get; }
        public IAsyncRelayCommand ExportToolsCommand { get; }
        public IAsyncRelayCommand ImportCustomersCommand { get; }
        public IAsyncRelayCommand ExportCustomersCommand { get; }

        /// <summary>
        /// Command that triggers an asynchronous database backup.
        /// </summary>
        /// <remarks>
        /// The backup work executes off the UI thread via <see cref="BackupDatabaseAsync"/>,
        /// keeping the interface responsive while the operation runs.
        /// </remarks>
        public IAsyncRelayCommand BackupDatabaseCommand { get; }

        public ObservableCollection<string> ImportExportLogs { get; } = new();

        public ImportExportViewModel(IToolService toolService,
                                     ICustomerService customerService,
                                     IFileDialogService fileDialogService,
                                     IDatabaseBackupService databaseService,
                                     IDialogService dialogService,
                                     ILogger<ImportExportViewModel>? logger = null)
        {
            _toolService = toolService;
            _customerService = customerService;
            _fileDialogService = fileDialogService;
            _databaseService = databaseService;
            _dialogService = dialogService;
            _logger = logger ?? NullLogger<ImportExportViewModel>.Instance;
            ImportToolsCommand = new AsyncRelayCommand(ct => ImportToolsAsync(ct));
            ExportToolsCommand = new AsyncRelayCommand(ct => ExportToolsAsync(ct));
            ImportCustomersCommand = new AsyncRelayCommand(ct => ImportCustomersAsync(ct));
            ExportCustomersCommand = new AsyncRelayCommand(ct => ExportCustomersAsync(ct));
            BackupDatabaseCommand = new AsyncRelayCommand(ct => BackupDatabaseAsync(ct));
        }

        async Task ImportToolsAsync(CancellationToken cancellationToken)
        {
            var path = _fileDialogService.OpenFile("CSV Files|*.csv");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                var headers = await CsvHelperUtil.ReadHeadersAsync(path);
                var properties = typeof(ToolImportDto).GetProperties().Select(p => p.Name);
                var map = _dialogService.ShowImportMapping(headers, properties);
                if (map == null)
                    return;
                await _dialogService.ShowInfoAsync("Importing tools...", "Import Tools");
                await _toolService.ImportToolsFromCsvAsync(path, map, cancellationToken);
                ImportExportLogs.Add($"Successfully imported tools from {path}.");
                await _dialogService.ShowInfoAsync($"Successfully imported tools from {path}.", "Import Tools");
            }
            catch (OperationCanceledException)
            {
                ImportExportLogs.Add("Tool import was cancelled.");
                await _dialogService.ShowInfoAsync("Tool import was cancelled.", "Import Tools");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import tools from {Path}", path);
                ImportExportLogs.Add($"Failed to import tools from {path}: {ex.Message}");
                await _dialogService.ShowInfoAsync($"Failed to import tools from {path}: {ex.Message}", "Import Tools");
            }
        }

        async Task ExportToolsAsync(CancellationToken cancellationToken)
        {
            var path = _fileDialogService.SaveFile("CSV Files|*.csv");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                await _toolService.ExportToolsToCsvAsync(path, cancellationToken);
                ImportExportLogs.Add($"Successfully exported tools to {path}.");
            }
            catch (OperationCanceledException)
            {
                ImportExportLogs.Add("Tool export was cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export tools to {Path}", path);
                ImportExportLogs.Add($"Failed to export tools to {path}: {ex.Message}");
            }
        }

        async Task ImportCustomersAsync(CancellationToken cancellationToken)
        {
            var path = _fileDialogService.OpenFile("CSV Files|*.csv");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                var headers = await CsvHelperUtil.ReadHeadersAsync(path);
                var properties = typeof(CustomerImportDto).GetProperties().Select(p => p.Name);
                var map = _dialogService.ShowImportMapping(headers, properties);
                if (map == null)
                    return;
                var result = await _customerService.ImportCustomersFromCsvAsync(path, map, cancellationToken);
                ImportExportLogs.Add($"Successfully imported customers from {path}. Imported {result.ImportedCount} customers.");
                foreach (var msg in result.SkippedRows)
                    ImportExportLogs.Add($"Skipped {msg}");
            }
            catch (OperationCanceledException)
            {
                ImportExportLogs.Add("Customer import was cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import customers from {Path}", path);
                ImportExportLogs.Add($"Failed to import customers from {path}: {ex.Message}");
            }
        }

        async Task ExportCustomersAsync(CancellationToken cancellationToken)
        {
            var path = _fileDialogService.SaveFile("CSV Files|*.csv");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                await _customerService.ExportCustomersToCsvAsync(path, cancellationToken);
                ImportExportLogs.Add($"Successfully exported customers to {path}.");
            }
            catch (OperationCanceledException)
            {
                ImportExportLogs.Add("Customer export was cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export customers to {Path}", path);
                ImportExportLogs.Add($"Failed to export customers to {path}: {ex.Message}");
            }
        }

        /// <summary>
        /// Prompts the user for a destination file and backs up the database asynchronously.
        /// </summary>
        /// <remarks>
        /// The backup is performed using asynchronous I/O, allowing the UI thread to remain responsive
        /// while the file copy completes in the background.
        /// </remarks>
        /// <returns>A <see cref="Task"/> representing the asynchronous backup operation.</returns>
        async Task BackupDatabaseAsync(CancellationToken cancellationToken)
        {
            var path = _fileDialogService.SaveFile("SQLite Database|*.db");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                await _databaseService.BackupDatabaseAsync(path, cancellationToken);
                ImportExportLogs.Add($"Successfully backed up database to {path}.");
            }
            catch (OperationCanceledException)
            {
                ImportExportLogs.Add("Database backup was cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to backup database to {Path}", path);
                ImportExportLogs.Add($"Failed to backup database to {path}: {ex.Message}");
            }
        }
    }
}

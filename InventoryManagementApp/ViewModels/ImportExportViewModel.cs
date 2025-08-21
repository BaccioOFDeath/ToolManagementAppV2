using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.ImportExport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Utilities.IO;
using InventoryManagementApp.Utilities.Helpers;

namespace InventoryManagementApp.ViewModels
{
    public class ImportExportViewModel : ObservableObject
    {
        private readonly IItemService _itemService;
        private readonly ICustomerService _customerService;
        private readonly IFileDialogService _fileDialogService;
        private readonly IDatabaseBackupService _databaseService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ImportExportViewModel> _logger;

        public IAsyncRelayCommand ImportItemsCommand { get; }
        public IRelayCommand CancelImportItemsCommand { get; }
        public IAsyncRelayCommand ExportItemsCommand { get; }
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

        public ImportExportViewModel(IItemService itemService,
                                     ICustomerService customerService,
                                     IFileDialogService fileDialogService,
                                     IDatabaseBackupService databaseService,
                                     IDialogService dialogService,
                                     ILogger<ImportExportViewModel>? logger = null)
        {
            _itemService = itemService;
            _customerService = customerService;
            _fileDialogService = fileDialogService;
            _databaseService = databaseService;
            _dialogService = dialogService;
            _logger = logger ?? NullLogger<ImportExportViewModel>.Instance;
            ImportItemsCommand = new AsyncRelayCommand(ct => ImportItemsAsync(ct));
            CancelImportItemsCommand = new RelayCommand(() => ImportItemsCommand.Cancel());
            ExportItemsCommand = new AsyncRelayCommand(ct => ExportItemsAsync(ct));
            ImportCustomersCommand = new AsyncRelayCommand(ct => ImportCustomersAsync(ct));
            ExportCustomersCommand = new AsyncRelayCommand(ct => ExportCustomersAsync(ct));
            BackupDatabaseCommand = new AsyncRelayCommand(ct => BackupDatabaseAsync(ct));
        }

        async Task ImportItemsAsync(CancellationToken cancellationToken)
        {
            var path = _fileDialogService.OpenFile("CSV Files|*.csv", AppContext.BaseDirectory);
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                var headers = await CsvHelperUtil.ReadHeadersAsync(path);
                var properties = typeof(ItemImportDto).GetProperties().Select(p => p.Name);
                var map = _dialogService.ShowImportMapping(headers, properties);
                if (map == null)
                    return;
                var plural = LabelProvider.Instance.ItemLabelPlural;
                await _dialogService.ShowInfoAsync($"Importing {plural}...", $"Import {plural}");
                await _itemService.ImportItemsFromCsvAsync(path, map, cancellationToken);
                ImportExportLogs.Add($"Successfully imported {plural} from {path}.");
                await _dialogService.ShowInfoAsync($"Successfully imported {plural} from {path}.", $"Import {plural}");
            }
            catch (OperationCanceledException)
            {
                var singular = LabelProvider.Instance.ItemLabelSingular;
                var plural = LabelProvider.Instance.ItemLabelPlural;
                ImportExportLogs.Add($"{singular} import was cancelled.");
                await _dialogService.ShowInfoAsync($"{singular} import was cancelled.", $"Import {plural}");
            }
            catch (Exception ex)
            {
                var plural = LabelProvider.Instance.ItemLabelPlural;
                _logger.LogError(ex, "Failed to import {ItemLabelPlural} from {Path}", plural, path);
                ImportExportLogs.Add($"Failed to import {plural} from {path}: {ex.Message}");
                await _dialogService.ShowInfoAsync($"Failed to import {plural} from {path}: {ex.Message}", $"Import {plural}");
            }
        }

        async Task ExportItemsAsync(CancellationToken cancellationToken)
        {
            var path = _fileDialogService.SaveFile("CSV Files|*.csv");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                var plural = LabelProvider.Instance.ItemLabelPlural;
                await _itemService.ExportItemsToCsvAsync(path, cancellationToken);
                ImportExportLogs.Add($"Successfully exported {plural} to {path}.");
            }
            catch (OperationCanceledException)
            {
                var singular = LabelProvider.Instance.ItemLabelSingular;
                ImportExportLogs.Add($"{singular} export was cancelled.");
            }
            catch (Exception ex)
            {
                var plural = LabelProvider.Instance.ItemLabelPlural;
                _logger.LogError(ex, "Failed to export {ItemLabelPlural} to {Path}", plural, path);
                ImportExportLogs.Add($"Failed to export {plural} to {path}: {ex.Message}");
            }
        }

        async Task ImportCustomersAsync(CancellationToken cancellationToken)
        {
            var path = _fileDialogService.OpenFile("CSV Files|*.csv", AppContext.BaseDirectory);
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

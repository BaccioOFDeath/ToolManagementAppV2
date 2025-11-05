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
using InventoryManagementApp.Models.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Utilities.IO;
using InventoryManagementApp.Utilities.Helpers;
using InventoryManagementApp.Services.ImportExport;

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
        private readonly IUserContext _userContext;

        public IAsyncRelayCommand ImportItemsCommand { get; }
        public IRelayCommand CancelImportItemsCommand { get; }
        public IAsyncRelayCommand ExportItemsCommand { get; }
        public IAsyncRelayCommand ImportCustomersCommand { get; }
        public IAsyncRelayCommand ExportCustomersCommand { get; }
        public IAsyncRelayCommand OpenImageImportMappingWindowCommand { get; }

        public bool IsCurrentUserAdmin => _userContext.IsAdmin;

        /// <summary>
        /// Command that triggers an asynchronous database backup.
        /// </summary>
        /// <remarks>
        /// The backup work executes off the UI thread via <see cref="BackupDatabaseAsync"/>,
        /// keeping the interface responsive while the operation runs.
        /// </remarks>
        public IAsyncRelayCommand BackupDatabaseCommand { get; }

        public ObservableCollection<string> ImportExportLogs { get; } = new();

        // Available import/export formats
        private readonly List<IDataImporter<ItemModel>> _itemImporters;
        private readonly List<IDataExporter<ItemModel>> _itemExporters;
        private readonly List<IDataImporter<Customer>> _customerImporters;
        private readonly List<IDataExporter<Customer>> _customerExporters;

        public ImportExportViewModel(IItemService itemService,
                                     ICustomerService customerService,
                                     IFileDialogService fileDialogService,
                                     IDatabaseBackupService databaseService,
                                     IDialogService dialogService,
                                     IAsyncRelayCommand? openImageImportMappingWindowCommand = null,
                                     IUserContext? userContext = null,
                                     ILogger<ImportExportViewModel>? logger = null)
        {
            _itemService = itemService;
            _customerService = customerService;
            _fileDialogService = fileDialogService;
            _databaseService = databaseService;
            _dialogService = dialogService;
            _logger = logger ?? NullLogger<ImportExportViewModel>.Instance;
            OpenImageImportMappingWindowCommand = openImageImportMappingWindowCommand ?? new AsyncRelayCommand(ct => Task.CompletedTask);
            _userContext = userContext ?? new DummyUserContext();
            _userContext.UserChanged += (_, _) => OnPropertyChanged(nameof(IsCurrentUserAdmin));
            
            // Initialize importers and exporters
            _itemImporters = new List<IDataImporter<ItemModel>>
            {
                new ItemJsonImporter(),
                new ItemXmlImporter()
            };
            _itemExporters = new List<IDataExporter<ItemModel>>
            {
                new ItemCsvExporter(),
                new ItemJsonExporter(),
                new ItemXmlExporter()
            };
            _customerImporters = new List<IDataImporter<Customer>>
            {
                new CustomerJsonImporter(),
                new CustomerXmlImporter()
            };
            _customerExporters = new List<IDataExporter<Customer>>
            {
                new CustomerCsvExporter(),
                new CustomerJsonExporter(),
                new CustomerXmlExporter()
            };
            
            ImportItemsCommand = new AsyncRelayCommand(ct => ImportItemsAsync(ct));
            CancelImportItemsCommand = new RelayCommand(() => ImportItemsCommand.Cancel());
            ExportItemsCommand = new AsyncRelayCommand(ct => ExportItemsAsync(ct));
            ImportCustomersCommand = new AsyncRelayCommand(ct => ImportCustomersAsync(ct));
            ExportCustomersCommand = new AsyncRelayCommand(ct => ExportCustomersAsync(ct));
            BackupDatabaseCommand = new AsyncRelayCommand(ct => BackupDatabaseAsync(ct));
        }

        private sealed class DummyUserContext : IUserContext
        {
            User? _currentUser;
            public User? CurrentUser
            {
                get => _currentUser;
                set
                {
                    if (_currentUser == value) return;
                    _currentUser = value;
                    UserChanged?.Invoke(this, value);
                }
            }

            public event EventHandler<User?>? UserChanged;
            public bool IsAdmin => false;
            public string UserName => string.Empty;
            public string Role => string.Empty;
        }

        async Task ImportItemsAsync(CancellationToken cancellationToken)
        {
            // Build combined file filter for all supported formats
            // Note: CSV is handled separately from _itemImporters because it requires
            // an interactive mapping dialog, whereas JSON/XML use direct import
            var filters = new List<string> { "CSV Files|*.csv" };
            filters.AddRange(_itemImporters.Select(i => i.FileFilter));
            var combinedFilter = string.Join("|", filters) + "|All Files|*.*";
            
            var path = _fileDialogService.OpenFile(combinedFilter, AppContext.BaseDirectory);
            if (string.IsNullOrWhiteSpace(path)) return;
            
            try
            {
                var extension = Path.GetExtension(path).ToLowerInvariant();
                var plural = LabelProvider.Instance.ItemLabelPlural;
                
                List<int> skippedRows;
                
                // Check if it's CSV (requires mapping) or other format (direct import)
                if (extension == ".csv")
                {
                    // Use existing CSV import with mapping
                    var headers = await CsvHelperUtil.ReadHeadersAsync(path);
                    var properties = typeof(ItemImportDto).GetProperties().Select(p => p.Name);
                    var map = _dialogService.ShowImportMapping(
                        headers,
                        properties,
                        new[] { nameof(ItemImportDto.ItemNumber), nameof(ItemImportDto.Name) });
                    if (map == null)
                        return;
                    
                    if (!map.TryGetValue(nameof(ItemImportDto.ItemNumber), out var itemNumberHeader) || string.IsNullOrWhiteSpace(itemNumberHeader))
                    {
                        var singular = LabelProvider.Instance.ItemLabelSingular;
                        var errorMessage = $"Mapping for {singular} number is required.";
                        ImportExportLogs.Add(errorMessage);
                        _logger.LogWarning("Import aborted: missing {ItemLabelSingular} number mapping", singular);
                        await _dialogService.ShowInfoAsync(errorMessage, $"Import {plural}");
                        return;
                    }
                    
                    await _dialogService.ShowInfoAsync($"Importing {plural}...", $"Import {plural}");
                    skippedRows = await _itemService.ImportItemsFromCsvAsync(path, map, cancellationToken);
                }
                else
                {
                    // Find appropriate importer
                    var importer = _itemImporters.FirstOrDefault(i => i.FileExtension.Equals(extension, StringComparison.OrdinalIgnoreCase));
                    if (importer == null)
                    {
                        var errorMessage = $"No importer found for file type: {extension}";
                        ImportExportLogs.Add(errorMessage);
                        await _dialogService.ShowInfoAsync(errorMessage, $"Import {plural}");
                        return;
                    }
                    
                    await _dialogService.ShowInfoAsync($"Importing {plural} from {importer.FormatName}...", $"Import {plural}");
                    skippedRows = await _itemService.ImportItemsAsync(path, importer, cancellationToken);
                }
                
                var successMessage = $"Successfully imported {plural} from {path}.";
                ImportExportLogs.Add(successMessage);
                if (skippedRows.Any())
                {
                    var skippedMessage = $"Skipped rows: {string.Join(", ", skippedRows)}";
                    ImportExportLogs.Add(skippedMessage);
                    successMessage += $" {skippedMessage}";
                }
                await _dialogService.ShowInfoAsync(successMessage, $"Import {plural}");
            }
            catch (OperationCanceledException)
            {
                var plural = LabelProvider.Instance.ItemLabelPlural;
                ImportExportLogs.Add($"{plural} import was cancelled.");
                await _dialogService.ShowInfoAsync($"{plural} import was cancelled.", $"Import {plural}");
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
            // Build combined file filter for all supported formats
            var filters = _itemExporters.Select(e => e.FileFilter);
            var combinedFilter = string.Join("|", filters);
            
            var path = _fileDialogService.SaveFile(combinedFilter);
            if (string.IsNullOrWhiteSpace(path)) return;
            
            try
            {
                var extension = Path.GetExtension(path).ToLowerInvariant();
                var plural = LabelProvider.Instance.ItemLabelPlural;
                
                // Find appropriate exporter
                var exporter = _itemExporters.FirstOrDefault(e => e.FileExtension.Equals(extension, StringComparison.OrdinalIgnoreCase));
                if (exporter == null)
                {
                    var errorMessage = $"No exporter found for file type: {extension}";
                    ImportExportLogs.Add(errorMessage);
                    return;
                }
                
                await _itemService.ExportItemsAsync(path, exporter, cancellationToken);
                ImportExportLogs.Add($"Successfully exported {plural} to {path} ({exporter.FormatName} format).");
            }
            catch (OperationCanceledException)
            {
                var plural = LabelProvider.Instance.ItemLabelPlural;
                ImportExportLogs.Add($"{plural} export was cancelled.");
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
            // Build combined file filter for all supported formats
            // Note: CSV is handled separately from _customerImporters because it requires
            // an interactive mapping dialog, whereas JSON/XML use direct import
            var filters = new List<string> { "CSV Files|*.csv" };
            filters.AddRange(_customerImporters.Select(i => i.FileFilter));
            var combinedFilter = string.Join("|", filters) + "|All Files|*.*";
            
            var path = _fileDialogService.OpenFile(combinedFilter, AppContext.BaseDirectory);
            if (string.IsNullOrWhiteSpace(path)) return;
            
            try
            {
                var extension = Path.GetExtension(path).ToLowerInvariant();
                
                if (extension == ".csv")
                {
                    // Use existing CSV import with mapping
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
                else
                {
                    // Find appropriate importer
                    var importer = _customerImporters.FirstOrDefault(i => i.FileExtension.Equals(extension, StringComparison.OrdinalIgnoreCase));
                    if (importer == null)
                    {
                        var errorMessage = $"No importer found for file type: {extension}";
                        ImportExportLogs.Add(errorMessage);
                        await _dialogService.ShowInfoAsync(errorMessage, "Import Customers");
                        return;
                    }
                    
                    var importedCount = await _customerService.ImportCustomersAsync(path, importer, cancellationToken);
                    ImportExportLogs.Add($"Successfully imported {importedCount} customers from {path} ({importer.FormatName} format).");
                }
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
            // Build combined file filter for all supported formats
            var filters = _customerExporters.Select(e => e.FileFilter);
            var combinedFilter = string.Join("|", filters);
            
            var path = _fileDialogService.SaveFile(combinedFilter);
            if (string.IsNullOrWhiteSpace(path)) return;
            
            try
            {
                var extension = Path.GetExtension(path).ToLowerInvariant();
                
                // Find appropriate exporter
                var exporter = _customerExporters.FirstOrDefault(e => e.FileExtension.Equals(extension, StringComparison.OrdinalIgnoreCase));
                if (exporter == null)
                {
                    var errorMessage = $"No exporter found for file type: {extension}";
                    ImportExportLogs.Add(errorMessage);
                    return;
                }
                
                await _customerService.ExportCustomersAsync(path, exporter, cancellationToken);
                ImportExportLogs.Add($"Successfully exported customers to {path} ({exporter.FormatName} format).");
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
            if (string.IsNullOrWhiteSpace(path)) return;
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

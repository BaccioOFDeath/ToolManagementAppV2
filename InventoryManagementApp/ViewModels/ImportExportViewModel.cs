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
using InventoryManagementApp.Services.Settings;

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
        private readonly RentalConfigurationService? _rentalConfigService;

        public IAsyncRelayCommand ImportItemsCommand { get; }
        public IRelayCommand CancelImportItemsCommand { get; }
        public IAsyncRelayCommand ExportItemsCommand { get; }
        public IAsyncRelayCommand ImportCustomersCommand { get; }
        public IAsyncRelayCommand ExportCustomersCommand { get; }
        public IAsyncRelayCommand OpenImageImportMappingWindowCommand { get; }
        public IRelayCommand ClearImportExportLogsCommand { get; }

        public bool CanImportImages => _userContext.IsAdmin || _userContext.CurrentUser?.HasPermission(User.PermissionImportExport) == true;
        public bool IsCurrentUserAdmin => CanImportImages;
        public bool HasLogEntries => ImportExportLogs.Count > 0;
        public string LogSummary => HasLogEntries
            ? $"{ImportExportLogs.Count} operation log entr{(ImportExportLogs.Count == 1 ? "y" : "ies")} recorded this session."
            : "No import, export, image, or backup operations have been run in this session.";

        public string ItemDataSummary =>
            $"Import {LabelProvider.Instance.ItemLabelPlural} from CSV with mapping, JSON, or XML. Export the current item catalog to CSV, JSON, or XML.";

        public string CustomerDataSummary =>
            "Import customers from mapped CSV, JSON, or XML. Export the customer directory to CSV, JSON, or XML for advisor handoff or cleanup.";

        public string ImageImportSummary => CanImportImages
            ? $"Image import is available for matching photos to {LabelProvider.Instance.ItemLabelPlural}."
            : $"Image import requires the {User.PermissionLabels[User.PermissionImportExport]} permission. Ask an admin to grant it before mapping photos to {LabelProvider.Instance.ItemLabelPlural}.";

        public string BackupSummary =>
            "Create a database backup before large imports, bulk cleanup, or workstation changes.";

        private string? _selectedImportExportLog;
        public string? SelectedImportExportLog
        {
            get => _selectedImportExportLog;
            set
            {
                if (SetProperty(ref _selectedImportExportLog, value))
                {
                    OnPropertyChanged(nameof(SelectedLogTitle));
                    OnPropertyChanged(nameof(SelectedLogDetail));
                }
            }
        }

        public string SelectedLogTitle => string.IsNullOrWhiteSpace(SelectedImportExportLog)
            ? "No operation selected"
            : "Selected operation";

        public string SelectedLogDetail => string.IsNullOrWhiteSpace(SelectedImportExportLog)
            ? "Run an import, export, image import, or backup action. Select a log row to copy or print the exact result."
            : SelectedImportExportLog;

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
                                     RentalConfigurationService? rentalConfigService = null,
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
            _userContext.UserChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(CanImportImages));
                OnPropertyChanged(nameof(IsCurrentUserAdmin));
                OnPropertyChanged(nameof(ImageImportSummary));
            };
            _rentalConfigService = rentalConfigService;
            ImportExportLogs.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(HasLogEntries));
                OnPropertyChanged(nameof(LogSummary));
            };
            
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
            ClearImportExportLogsCommand = new RelayCommand(ClearImportExportLogs, () => HasLogEntries);
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

        void AddLog(string message)
        {
            ImportExportLogs.Add(message);
            SelectedImportExportLog = message;
            ClearImportExportLogsCommand.NotifyCanExecuteChanged();
        }

        void ClearImportExportLogs()
        {
            ImportExportLogs.Clear();
            SelectedImportExportLog = null;
            ClearImportExportLogsCommand.NotifyCanExecuteChanged();
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
                    {
                        var message = $"{plural} import mapping was cancelled.";
                        AddLog(message);
                        await _dialogService.ShowInfoAsync(message, $"Import {plural}");
                        return;
                    }
                    
                    if (!map.TryGetValue(nameof(ItemImportDto.ItemNumber), out var itemNumberHeader) || string.IsNullOrWhiteSpace(itemNumberHeader))
                    {
                        var singular = LabelProvider.Instance.ItemLabelSingular;
                        var errorMessage = $"Mapping for {singular} number is required.";
                        AddLog(errorMessage);
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
                        AddLog(errorMessage);
                        await _dialogService.ShowInfoAsync(errorMessage, $"Import {plural}");
                        return;
                    }
                    
                    await _dialogService.ShowInfoAsync($"Importing {plural} from {importer.FormatName}...", $"Import {plural}");
                    skippedRows = await _itemService.ImportItemsAsync(path, importer, cancellationToken);
                }
                
                var successMessage = $"Successfully imported {plural} from {path}.";
                AddLog(successMessage);
                if (skippedRows.Any())
                {
                    var skippedMessage = $"Skipped rows: {string.Join(", ", skippedRows)}";
                    AddLog(skippedMessage);
                    successMessage += $" {skippedMessage}";
                }
                await _dialogService.ShowInfoAsync(successMessage, $"Import {plural}");
            }
            catch (OperationCanceledException)
            {
                var plural = LabelProvider.Instance.ItemLabelPlural;
                AddLog($"{plural} import was cancelled.");
                await _dialogService.ShowInfoAsync($"{plural} import was cancelled.", $"Import {plural}");
            }
            catch (Exception ex)
            {
                var plural = LabelProvider.Instance.ItemLabelPlural;
                _logger.LogError(ex, "Failed to import {ItemLabelPlural} from {Path}", plural, path);
                AddLog($"Failed to import {plural} from {path}: {ex.Message}");
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
                    AddLog(errorMessage);
                    await _dialogService.ShowInfoAsync(errorMessage, $"Export {plural}");
                    return;
                }
                
                await _itemService.ExportItemsAsync(path, exporter, cancellationToken);
                var successMessage = $"Successfully exported {plural} to {path} ({exporter.FormatName} format).";
                AddLog(successMessage);
                await _dialogService.ShowInfoAsync(successMessage, $"Export {plural}");
            }
            catch (OperationCanceledException)
            {
                var plural = LabelProvider.Instance.ItemLabelPlural;
                var message = $"{plural} export was cancelled.";
                AddLog(message);
                await _dialogService.ShowInfoAsync(message, $"Export {plural}");
            }
            catch (Exception ex)
            {
                var plural = LabelProvider.Instance.ItemLabelPlural;
                _logger.LogError(ex, "Failed to export {ItemLabelPlural} to {Path}", plural, path);
                var failureMessage = $"Failed to export {plural} to {path}: {ex.Message}";
                AddLog(failureMessage);
                await _dialogService.ShowInfoAsync(failureMessage, $"Export {plural}");
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
                    {
                        const string message = "Customer import mapping was cancelled.";
                        AddLog(message);
                        await _dialogService.ShowInfoAsync(message, "Import Customers");
                        return;
                    }
                    var result = await _customerService.ImportCustomersFromCsvAsync(path, map, cancellationToken);
                    var successMessage = $"Successfully imported customers from {path}. Imported {result.ImportedCount} customers.";
                    AddLog(successMessage);
                    if (result.SkippedRows.Any())
                        successMessage += $" {result.SkippedRows.Count} skipped row{(result.SkippedRows.Count == 1 ? "" : "s")} were recorded in the run log.";
                    foreach (var msg in result.SkippedRows)
                        AddLog($"Skipped {msg}");
                    await _dialogService.ShowInfoAsync(successMessage, "Import Customers");
                }
                else
                {
                    // Find appropriate importer
                    var importer = _customerImporters.FirstOrDefault(i => i.FileExtension.Equals(extension, StringComparison.OrdinalIgnoreCase));
                    if (importer == null)
                    {
                        var errorMessage = $"No importer found for file type: {extension}";
                        AddLog(errorMessage);
                        await _dialogService.ShowInfoAsync(errorMessage, "Import Customers");
                        return;
                    }
                    
                    var importedCount = await _customerService.ImportCustomersAsync(path, importer, cancellationToken);
                    var successMessage = $"Successfully imported {importedCount} customers from {path} ({importer.FormatName} format).";
                    AddLog(successMessage);
                    await _dialogService.ShowInfoAsync(successMessage, "Import Customers");
                }
            }
            catch (OperationCanceledException)
            {
                const string message = "Customer import was cancelled.";
                AddLog(message);
                await _dialogService.ShowInfoAsync(message, "Import Customers");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import customers from {Path}", path);
                var failureMessage = $"Failed to import customers from {path}: {ex.Message}";
                AddLog(failureMessage);
                await _dialogService.ShowInfoAsync(failureMessage, "Import Customers");
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
                    AddLog(errorMessage);
                    await _dialogService.ShowInfoAsync(errorMessage, "Export Customers");
                    return;
                }
                
                await _customerService.ExportCustomersAsync(path, exporter, cancellationToken);
                var successMessage = $"Successfully exported customers to {path} ({exporter.FormatName} format).";
                AddLog(successMessage);
                await _dialogService.ShowInfoAsync(successMessage, "Export Customers");
            }
            catch (OperationCanceledException)
            {
                const string message = "Customer export was cancelled.";
                AddLog(message);
                await _dialogService.ShowInfoAsync(message, "Export Customers");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export customers to {Path}", path);
                var failureMessage = $"Failed to export customers to {path}: {ex.Message}";
                AddLog(failureMessage);
                await _dialogService.ShowInfoAsync(failureMessage, "Export Customers");
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
            var initialDirectory = _rentalConfigService == null
                ? null
                : await _rentalConfigService.GetBackupDirectoryAsync(cancellationToken).ConfigureAwait(false);
            var path = _fileDialogService.SaveFile("SQLite Database|*.db", initialDirectory);
            if (string.IsNullOrWhiteSpace(path)) return;
            try
            {
                await _databaseService.BackupDatabaseAsync(path, cancellationToken);
                var successMessage = $"Successfully backed up database to {path}.";
                AddLog(successMessage);
                await _dialogService.ShowInfoAsync(successMessage, "Database Backup");
            }
            catch (OperationCanceledException)
            {
                const string message = "Database backup was cancelled.";
                AddLog(message);
                await _dialogService.ShowInfoAsync(message, "Database Backup");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to backup database to {Path}", path);
                var failureMessage = $"Failed to backup database to {path}: {ex.Message}";
                AddLog(failureMessage);
                await _dialogService.ShowInfoAsync(failureMessage, "Database Backup");
            }
        }
    }
}
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
        private const int MaxVisibleImportExportLogRows = 500;
        private const int MaxSelectedLogDetailCharacters = 1800;

        private readonly IItemService _itemService;
        private readonly ICustomerService _customerService;
        private readonly IFileDialogService _fileDialogService;
        private readonly IDatabaseBackupService _databaseService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ImportExportViewModel> _logger;
        private readonly IUserContext _userContext;
        private readonly RentalConfigurationService? _rentalConfigService;
        private readonly IAsyncRelayCommand _openImageImportMappingWindowCommand;
        private int _activeDataOperationCount;
        private int _omittedImportExportLogCount;
        private string? _currentDataOperation;

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
        public int VisibleImportExportLogCount => ImportExportLogs.Count;
        public int OmittedImportExportLogCount => _omittedImportExportLogCount;
        public bool HasOmittedImportExportLogs => _omittedImportExportLogCount > 0;
        public bool IsDataOperationBusy => _activeDataOperationCount > 0;
        public bool IsDataOperationReady => !IsDataOperationBusy;
        public bool CanOpenImageImportMapping => CanImportImages && !IsDataOperationBusy;
        public bool CanReviewSelectedLog => !IsDataOperationBusy && !string.IsNullOrWhiteSpace(SelectedImportExportLog);
        public bool CanPrintImportExportLogs => !IsDataOperationBusy && HasLogEntries;
        public string ActiveDataOperationName => ValueOrDefault(_currentDataOperation, "Data operation");
        public string DataOperationStatus => IsDataOperationBusy
            ? $"{ActiveDataOperationName} running"
            : "Data desk ready";
        public string DataOperationSummary => IsDataOperationBusy
            ? "Finish or cancel the current data operation before starting another import, export, backup, restore, image mapping, copy, or print handoff."
            : "Ready for the next import, export, backup, restore, image mapping, copy, or print handoff.";
        public string LogSummary
        {
            get
            {
                if (!HasLogEntries)
                    return "No import, export, image, or backup operations have been run in this session.";

                var visibleCount = VisibleImportExportLogCount;
                var totalCount = visibleCount + OmittedImportExportLogCount;
                var entryText = totalCount == 1 ? "entry" : "entries";
                if (HasOmittedImportExportLogs)
                {
                    return $"{visibleCount} visible of {totalCount} operation log {entryText} are available this session. {OmittedImportExportLogCount} older entr{(OmittedImportExportLogCount == 1 ? "y was" : "ies were")} kept out of the grid for responsiveness.";
                }

                return $"{visibleCount} operation log entr{(visibleCount == 1 ? "y" : "ies")} recorded this session.";
            }
        }

        public string ItemDataSummary =>
            $"Import {LabelProvider.Instance.ItemLabelPlural} from CSV with mapping, JSON, or XML. Export the current item catalog to CSV, JSON, or XML.";

        public string CustomerDataSummary =>
            "Import customers from mapped CSV, JSON, or XML. Export the customer directory to CSV, JSON, or XML for advisor handoff or cleanup.";

        public string ImageImportSummary
        {
            get
            {
                if (IsDataOperationBusy)
                    return $"Image mapping is paused while {ActiveDataOperationName.ToLowerInvariant()} is running so heavy data workflows do not overlap.";

                return CanImportImages
                    ? $"Image import is available for matching photos to {LabelProvider.Instance.ItemLabelPlural}."
                    : $"Image import requires the {User.PermissionLabels[User.PermissionImportExport]} permission. Ask an admin to grant it before mapping photos to {LabelProvider.Instance.ItemLabelPlural}.";
            }
        }

        public string BackupSummary => IsDataOperationBusy
            ? $"Backup and restore are paused while {ActiveDataOperationName.ToLowerInvariant()} is running."
            : "Create or restore a full recovery package containing the database and app assets.";

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
                    OnPropertyChanged(nameof(CanReviewSelectedLog));
                }
            }
        }

        public string SelectedLogTitle => string.IsNullOrWhiteSpace(SelectedImportExportLog)
            ? "No operation selected"
            : "Selected operation";

        public string SelectedLogDetail => string.IsNullOrWhiteSpace(SelectedImportExportLog)
            ? "Run an import, export, image import, or backup action. Select a log row to copy or print the exact result."
            : BuildSelectedLogDetailPreview(SelectedImportExportLog);

        /// <summary>
        /// Command that triggers an asynchronous database backup.
        /// </summary>
        /// <remarks>
        /// The backup work executes off the UI thread via <see cref="BackupDatabaseAsync"/>,
        /// keeping the interface responsive while the operation runs.
        /// </remarks>
        public IAsyncRelayCommand BackupDatabaseCommand { get; }
        public IAsyncRelayCommand RestoreBackupCommand { get; }

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
            _openImageImportMappingWindowCommand = openImageImportMappingWindowCommand ?? new AsyncRelayCommand(ct => Task.CompletedTask);
            _userContext = userContext ?? new DummyUserContext();
            _userContext.UserChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(CanImportImages));
                OnPropertyChanged(nameof(IsCurrentUserAdmin));
                OnPropertyChanged(nameof(CanOpenImageImportMapping));
                OnPropertyChanged(nameof(ImageImportSummary));
                OpenImageImportMappingWindowCommand.NotifyCanExecuteChanged();
            };
            _rentalConfigService = rentalConfigService;
            ImportExportLogs.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(HasLogEntries));
                OnPropertyChanged(nameof(VisibleImportExportLogCount));
                OnPropertyChanged(nameof(LogSummary));
                OnPropertyChanged(nameof(CanPrintImportExportLogs));
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
            
            ImportItemsCommand = new AsyncRelayCommand(ct => ImportItemsAsync(ct), CanStartDataOperation);
            CancelImportItemsCommand = new RelayCommand(() => ImportItemsCommand.Cancel(), () => ImportItemsCommand.IsRunning);
            ExportItemsCommand = new AsyncRelayCommand(ct => ExportItemsAsync(ct), CanStartDataOperation);
            ImportCustomersCommand = new AsyncRelayCommand(ct => ImportCustomersAsync(ct), CanStartDataOperation);
            ExportCustomersCommand = new AsyncRelayCommand(ct => ExportCustomersAsync(ct), CanStartDataOperation);
            BackupDatabaseCommand = new AsyncRelayCommand(ct => BackupDatabaseAsync(ct), CanStartDataOperation);
            RestoreBackupCommand = new AsyncRelayCommand(ct => RestoreBackupAsync(ct), CanStartDataOperation);
            OpenImageImportMappingWindowCommand = new AsyncRelayCommand(ct => OpenImageImportMappingAsync(ct), () => CanOpenImageImportMapping);
            ClearImportExportLogsCommand = new RelayCommand(ClearImportExportLogs, () => HasLogEntries && !IsDataOperationBusy);
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

        bool CanStartDataOperation() => !IsDataOperationBusy;

        bool TryBeginDataOperation(string operationName)
        {
            if (IsDataOperationBusy)
                return false;

            _activeDataOperationCount++;
            _currentDataOperation = operationName;
            NotifyDataOperationStateChanged();
            return true;
        }

        void EndDataOperation()
        {
            if (_activeDataOperationCount > 0)
                _activeDataOperationCount--;

            if (_activeDataOperationCount == 0)
                _currentDataOperation = null;

            NotifyDataOperationStateChanged();
        }

        void NotifyDataOperationStateChanged()
        {
            OnPropertyChanged(nameof(IsDataOperationBusy));
            OnPropertyChanged(nameof(IsDataOperationReady));
            OnPropertyChanged(nameof(CanOpenImageImportMapping));
            OnPropertyChanged(nameof(CanReviewSelectedLog));
            OnPropertyChanged(nameof(CanPrintImportExportLogs));
            OnPropertyChanged(nameof(ActiveDataOperationName));
            OnPropertyChanged(nameof(DataOperationStatus));
            OnPropertyChanged(nameof(DataOperationSummary));
            OnPropertyChanged(nameof(ImageImportSummary));
            OnPropertyChanged(nameof(BackupSummary));
            ImportItemsCommand.NotifyCanExecuteChanged();
            CancelImportItemsCommand.NotifyCanExecuteChanged();
            ExportItemsCommand.NotifyCanExecuteChanged();
            ImportCustomersCommand.NotifyCanExecuteChanged();
            ExportCustomersCommand.NotifyCanExecuteChanged();
            BackupDatabaseCommand.NotifyCanExecuteChanged();
            RestoreBackupCommand.NotifyCanExecuteChanged();
            OpenImageImportMappingWindowCommand.NotifyCanExecuteChanged();
            ClearImportExportLogsCommand.NotifyCanExecuteChanged();
        }

        void AddLog(string message)
        {
            while (ImportExportLogs.Count >= MaxVisibleImportExportLogRows)
            {
                ImportExportLogs.RemoveAt(0);
                _omittedImportExportLogCount++;
            }

            ImportExportLogs.Add(message);
            SelectedImportExportLog = message;
            OnPropertyChanged(nameof(OmittedImportExportLogCount));
            OnPropertyChanged(nameof(HasOmittedImportExportLogs));
            OnPropertyChanged(nameof(LogSummary));
            ClearImportExportLogsCommand.NotifyCanExecuteChanged();
        }

        void ClearImportExportLogs()
        {
            if (IsDataOperationBusy)
                return;

            ImportExportLogs.Clear();
            _omittedImportExportLogCount = 0;
            SelectedImportExportLog = null;
            OnPropertyChanged(nameof(OmittedImportExportLogCount));
            OnPropertyChanged(nameof(HasOmittedImportExportLogs));
            OnPropertyChanged(nameof(LogSummary));
            ClearImportExportLogsCommand.NotifyCanExecuteChanged();
        }

        async Task CancelFileSelectionAsync(string message, string title)
        {
            AddLog(message);
            await _dialogService.ShowInfoAsync(message, title);
        }

        async Task OpenImageImportMappingAsync(CancellationToken cancellationToken)
        {
            if (!CanOpenImageImportMapping)
                return;

            if (!_openImageImportMappingWindowCommand.CanExecute(null))
            {
                const string message = "Image mapping is not available from the current data desk state.";
                AddLog(message);
                await _dialogService.ShowInfoAsync(message, "Image Mapping");
                return;
            }

            AddLog("Opening image mapping workspace...");
            await _openImageImportMappingWindowCommand.ExecuteAsync(null);
        }

        async Task ImportItemsAsync(CancellationToken cancellationToken)
        {
            if (!TryBeginDataOperation($"{LabelProvider.Instance.ItemLabelPlural} import"))
                return;

            try
            {
                // Build combined file filter for all supported formats
                // Note: CSV is handled separately from _itemImporters because it requires
                // an interactive mapping dialog, whereas JSON/XML use direct import
                var filters = new List<string> { "CSV Files|*.csv" };
                filters.AddRange(_itemImporters.Select(i => i.FileFilter));
                var combinedFilter = string.Join("|", filters) + "|All Files|*.*";
                
                var path = _fileDialogService.OpenFile(combinedFilter, AppContext.BaseDirectory);
                if (string.IsNullOrWhiteSpace(path))
                {
                    var plural = LabelProvider.Instance.ItemLabelPlural;
                    await CancelFileSelectionAsync($"{plural} import file selection was cancelled.", $"Import {plural}");
                    return;
                }

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
                        
                        AddLog($"Importing {plural} from {path}...");
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
                        
                        AddLog($"Importing {plural} from {path} ({importer.FormatName} format)...");
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
            finally
            {
                EndDataOperation();
            }
        }

        async Task ExportItemsAsync(CancellationToken cancellationToken)
        {
            if (!TryBeginDataOperation($"{LabelProvider.Instance.ItemLabelPlural} export"))
                return;

            try
            {
                // Build combined file filter for all supported formats
                var filters = _itemExporters.Select(e => e.FileFilter);
                var combinedFilter = string.Join("|", filters);
                
                var path = _fileDialogService.SaveFile(combinedFilter);
                if (string.IsNullOrWhiteSpace(path))
                {
                    var plural = LabelProvider.Instance.ItemLabelPlural;
                    await CancelFileSelectionAsync($"{plural} export destination selection was cancelled.", $"Export {plural}");
                    return;
                }

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
            finally
            {
                EndDataOperation();
            }
        }

        async Task ImportCustomersAsync(CancellationToken cancellationToken)
        {
            if (!TryBeginDataOperation("Customer import"))
                return;

            try
            {
                // Build combined file filter for all supported formats
                // Note: CSV is handled separately from _customerImporters because it requires
                // an interactive mapping dialog, whereas JSON/XML use direct import
                var filters = new List<string> { "CSV Files|*.csv" };
                filters.AddRange(_customerImporters.Select(i => i.FileFilter));
                var combinedFilter = string.Join("|", filters) + "|All Files|*.*";
                
                var path = _fileDialogService.OpenFile(combinedFilter, AppContext.BaseDirectory);
                if (string.IsNullOrWhiteSpace(path))
                {
                    await CancelFileSelectionAsync("Customer import file selection was cancelled.", "Import Customers");
                    return;
                }

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
            finally
            {
                EndDataOperation();
            }
        }

        async Task ExportCustomersAsync(CancellationToken cancellationToken)
        {
            if (!TryBeginDataOperation("Customer export"))
                return;

            try
            {
                // Build combined file filter for all supported formats
                var filters = _customerExporters.Select(e => e.FileFilter);
                var combinedFilter = string.Join("|", filters);
                
                var path = _fileDialogService.SaveFile(combinedFilter);
                if (string.IsNullOrWhiteSpace(path))
                {
                    await CancelFileSelectionAsync("Customer export destination selection was cancelled.", "Export Customers");
                    return;
                }

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
            finally
            {
                EndDataOperation();
            }
        }

        /// <summary>
        /// Prompts the user for a destination file and backs up the database plus app assets asynchronously.
        /// </summary>
        /// <remarks>
        /// The backup is performed using asynchronous I/O, allowing the UI thread to remain responsive
        /// while the file copy completes in the background.
        /// </remarks>
        /// <returns>A <see cref="Task"/> representing the asynchronous backup operation.</returns>
        async Task BackupDatabaseAsync(CancellationToken cancellationToken)
        {
            if (!TryBeginDataOperation("Full backup"))
                return;

            try
            {
                string? path = null;

                try
                {
                    var initialDirectory = _rentalConfigService == null
                        ? null
                        : await _rentalConfigService.GetBackupDirectoryAsync(cancellationToken).ConfigureAwait(false);
                    path = _fileDialogService.SaveFile("Inventory Backup Package|*.inventory-backup.zip|Zip Files|*.zip", initialDirectory);
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        await CancelFileSelectionAsync("Full backup destination selection was cancelled.", "Full Backup");
                        return;
                    }

                    await _databaseService.BackupApplicationAsync(path, cancellationToken);
                    var successMessage = $"Successfully created full backup package at {path}.";
                    AddLog(successMessage);
                    await _dialogService.ShowInfoAsync(successMessage, "Full Backup");
                }
                catch (OperationCanceledException)
                {
                    const string message = "Full backup was cancelled.";
                    AddLog(message);
                    await _dialogService.ShowInfoAsync(message, "Full Backup");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create full backup package at {Path}", path);
                    var failureMessage = string.IsNullOrWhiteSpace(path)
                        ? $"Failed to start full backup: {ex.Message}"
                        : $"Failed to create full backup package at {path}: {ex.Message}";
                    AddLog(failureMessage);
                    await _dialogService.ShowInfoAsync(failureMessage, "Full Backup");
                }
            }
            finally
            {
                EndDataOperation();
            }
        }

        async Task RestoreBackupAsync(CancellationToken cancellationToken)
        {
            if (!TryBeginDataOperation("Restore backup"))
                return;

            try
            {
                string? path = null;

                try
                {
                    var initialDirectory = _rentalConfigService == null
                        ? null
                        : await _rentalConfigService.GetBackupDirectoryAsync(cancellationToken).ConfigureAwait(false);
                    path = _fileDialogService.OpenFile("Inventory Backup Package|*.inventory-backup.zip;*.zip|All Files|*.*", initialDirectory);
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        await CancelFileSelectionAsync("Backup package selection was cancelled.", "Restore Backup");
                        return;
                    }

                    var confirmed = await _dialogService.ShowConfirmationAsync(
                        "Restoring a backup will replace the current database and app assets. A safety backup of the current data will be created first. Continue?",
                        "Restore Backup").ConfigureAwait(false);
                    if (!confirmed)
                    {
                        const string cancelledMessage = "Restore backup was cancelled before changes were made.";
                        AddLog(cancelledMessage);
                        await _dialogService.ShowInfoAsync(cancelledMessage, "Restore Backup").ConfigureAwait(false);
                        return;
                    }

                    var safetyBackupDirectory = initialDirectory ?? Path.GetDirectoryName(path) ?? AppContext.BaseDirectory;
                    var safetyBackupPath = await _databaseService.RestoreApplicationBackupAsync(path, safetyBackupDirectory, cancellationToken).ConfigureAwait(false);
                    var successMessage = $"Successfully restored backup package from {path}. Safety backup created at {safetyBackupPath}. Restart the app before continuing work.";
                    AddLog(successMessage);
                    await _dialogService.ShowInfoAsync(successMessage, "Restore Backup").ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    const string message = "Restore backup was cancelled.";
                    AddLog(message);
                    await _dialogService.ShowInfoAsync(message, "Restore Backup").ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to restore backup package from {Path}", path);
                    var failureMessage = string.IsNullOrWhiteSpace(path)
                        ? $"Failed to start backup restore: {ex.Message}"
                        : $"Failed to restore backup package from {path}: {ex.Message}";
                    AddLog(failureMessage);
                    await _dialogService.ShowInfoAsync(failureMessage, "Restore Backup").ConfigureAwait(false);
                }
            }
            finally
            {
                EndDataOperation();
            }
        }

        private static string BuildSelectedLogDetailPreview(string? value)
        {
            var text = ValueOrDefault(value, "Not recorded");
            if (text.Length <= MaxSelectedLogDetailCharacters)
                return text;

            var visibleText = text.Substring(0, MaxSelectedLogDetailCharacters).TrimEnd();
            var omittedCharacters = text.Length - visibleText.Length;
            return string.Join(Environment.NewLine,
                visibleText,
                string.Empty,
                $"... {omittedCharacters:N0} characters omitted from this inline preview. Use Copy Result or Open Log Detail for the complete operation text.");
        }

        private static string ValueOrDefault(string? value, string fallback) =>
            string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
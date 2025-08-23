using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Services.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Utilities.Helpers;
using InventoryManagementApp.Models;
using InventoryManagementApp.Messages;

#nullable enable

namespace InventoryManagementApp.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        readonly IFileDialogService _fileDialog;
        readonly ISettingsService _settingsService;
        readonly IDialogService _dialogService;
        readonly ILogger<SettingsViewModel> _logger;
        public ObservableCollection<ItemDetailOption> ItemDetailOptions { get; } = new();
        bool _bulkUpdating;

        public SettingsViewModel(IFileDialogService fileDialog, ISettingsService settingsService, IDialogService dialogService, ILogger<SettingsViewModel>? logger = null)
        {
            _fileDialog = fileDialog;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _logger = logger ?? NullLogger<SettingsViewModel>.Instance;

            ThemeOptions = new ObservableCollection<string> { "Light", "Dark" };
            _theme = ThemeOptions[0];
            _itemLabelSingular = LabelProvider.Instance.ItemLabelSingular;
            _itemLabelPlural = LabelProvider.Instance.ItemLabelPlural;
            TestDbCommand = new RelayCommand(() =>
            {
                var success = TestDbConnection(out var message);
                try
                {
                    _dialogService.ShowInfo(message, "Database Connection");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to display info dialog.");
                }
            });
            BrowseCompanyLogoCommand = new RelayCommand(BrowseCompanyLogo);
            SaveCompanyLogoCommand = new AsyncRelayCommand(SaveCompanyLogoAsync);
            SelectAllItemDisplayCommand = new RelayCommand(() =>
            {
                _bulkUpdating = true;
                foreach (var o in ItemDetailOptions) o.IsVisible = true;
                _bulkUpdating = false;
                _ = SaveVisibilityAsync();
            });
            SelectNoneItemDisplayCommand = new RelayCommand(() =>
            {
                _bulkUpdating = true;
                foreach (var o in ItemDetailOptions) o.IsVisible = false;
                _bulkUpdating = false;
                _ = SaveVisibilityAsync();
            });
        }

        bool _initialized;
        public async Task InitializeAsync()
        {
            if (_initialized) return;
            var logoPath = await _settingsService.GetSettingAsync("CompanyLogoPath").ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(logoPath))
                _companyLogoPath = logoPath;
            var appName = await _settingsService.GetSettingAsync("ApplicationName").ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(appName))
                _applicationName = appName;
            _passwordIterations = await _settingsService.GetPasswordIterationsAsync().ConfigureAwait(false);
            _autoLogoutMinutes = await _settingsService.GetAutoLogoutMinutesAsync().ConfigureAwait(false);
            var vis = await _settingsService.GetItemDetailVisibilityAsync().ConfigureAwait(false);
            foreach (var field in Enum.GetValues<ItemDetailField>())
            {
                var option = new ItemDetailOption(field, vis.TryGetValue(field, out var v) ? v : true);
                option.PropertyChanged += ItemDetailOption_PropertyChanged;
                ItemDetailOptions.Add(option);
            }
            OnPropertyChanged(nameof(CompanyLogoPath));
            OnPropertyChanged(nameof(ApplicationName));
            OnPropertyChanged(nameof(PasswordIterations));
            OnPropertyChanged(nameof(AutoLogoutMinutes));
            _initialized = true;
        }

        private string _applicationName = string.Empty;
        public string ApplicationName
        {
            get => _applicationName;
            set
            {
                if (SetProperty(ref _applicationName, value))
                {
                    _ = SaveApplicationNameAsync(value);
                }
            }
        }

        async Task SaveApplicationNameAsync(string value)
        {
            try
            {
                await _settingsService.SaveSettingAsync("ApplicationName", value).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized to change settings.");
                _dialogService.ShowInfo("You are not authorized to change settings.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save application name.");
            }
        }

        private string _companyLogoPath = string.Empty;
        public string CompanyLogoPath
        {
            get => _companyLogoPath;
            set => SetProperty(ref _companyLogoPath, value);
        }

        private string _itemLabelSingular = string.Empty;
        public string ItemLabelSingular
        {
            get => _itemLabelSingular;
            set
            {
                if (SetProperty(ref _itemLabelSingular, value))
                {
                    _ = SaveItemLabelSingularAsync(value);
                }
            }
        }

        async Task SaveItemLabelSingularAsync(string value)
        {
            try
            {
                await _settingsService.SaveItemLabelSingularAsync(value).ConfigureAwait(false);
                LabelProvider.Instance.UpdateLabels(value, ItemLabelPlural);
                WeakReferenceMessenger.Default.Send(new ItemSettingsChangedMessage());
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized to change settings.");
                _dialogService.ShowInfo("You are not authorized to change settings.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save item label singular.");
            }
        }

        private string _itemLabelPlural = string.Empty;
        public string ItemLabelPlural
        {
            get => _itemLabelPlural;
            set
            {
                if (SetProperty(ref _itemLabelPlural, value))
                {
                    _ = SaveItemLabelPluralAsync(value);
                }
            }
        }

        async Task SaveItemLabelPluralAsync(string value)
        {
            try
            {
                await _settingsService.SaveItemLabelPluralAsync(value).ConfigureAwait(false);
                LabelProvider.Instance.UpdateLabels(ItemLabelSingular, value);
                WeakReferenceMessenger.Default.Send(new ItemSettingsChangedMessage());
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized to change settings.");
                _dialogService.ShowInfo("You are not authorized to change settings.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save item label plural.");
            }
        }

        private int _defaultRentalDuration;
        public int DefaultRentalDuration
        {
            get => _defaultRentalDuration;
            set => SetProperty(ref _defaultRentalDuration, value);
        }

        private string _connectionString = string.Empty;
        public string ConnectionString
        {
            get => _connectionString;
            set => SetProperty(ref _connectionString, value);
        }

        private string _theme = string.Empty;
        public string Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }

        private int _passwordIterations;
        const int MaxPasswordIterations = 1_000_000;
        public int PasswordIterations
        {
            get => _passwordIterations;
            set => _ = SetPasswordIterationsAsync(value);
        }

        async Task SetPasswordIterationsAsync(int value, CancellationToken token = default)
        {
            if (value <= 0) return;
            var newValue = value;
            if (value > MaxPasswordIterations)
            {
                newValue = MaxPasswordIterations;
                try
                {
                    _dialogService.ShowInfo($"Password iterations cannot exceed {MaxPasswordIterations} and have been set to {MaxPasswordIterations}.", "Password Iterations");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to display info dialog.");
                }
            }
            if (SetProperty(ref _passwordIterations, newValue))
            {
                try
                {
                    await _settingsService.SavePasswordIterationsAsync(newValue, token).ConfigureAwait(false);
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogWarning(ex, "Unauthorized to change password iterations.");
                    _dialogService.ShowInfo("You are not authorized to change settings.", "Unauthorized");
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogInformation(ex, "Saving password iterations was canceled.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save password iterations.");
                }
            }
        }

        private int _autoLogoutMinutes;
        public int AutoLogoutMinutes
        {
            get => _autoLogoutMinutes;
            set => _ = SetAutoLogoutMinutesAsync(value);
        }

        async Task SetAutoLogoutMinutesAsync(int value, CancellationToken token = default)
        {
            if (value < 0) return;
            if (SetProperty(ref _autoLogoutMinutes, value))
            {
                try
                {
                    await _settingsService.SaveAutoLogoutMinutesAsync(value, token).ConfigureAwait(false);
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogWarning(ex, "Unauthorized to change auto logout minutes.");
                    _dialogService.ShowInfo("You are not authorized to change settings.", "Unauthorized");
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogInformation(ex, "Saving auto logout minutes was canceled.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save auto logout minutes.");
                }
            }
        }

        void ItemDetailOption_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemDetailOption.IsVisible) && !_bulkUpdating)
            {
                _ = SaveVisibilityAsync();
            }
        }

        async Task SaveVisibilityAsync()
        {
            try
            {
                var dict = ItemDetailOptions.ToDictionary(o => o.Field, o => o.IsVisible);
                await _settingsService.SaveItemDetailVisibilityAsync(dict).ConfigureAwait(false);
                WeakReferenceMessenger.Default.Send(new ItemSettingsChangedMessage());
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized to change settings.");
                _dialogService.ShowInfo("You are not authorized to change settings.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save item detail visibility.");
            }
        }

        public ObservableCollection<string> ThemeOptions { get; }

        public IRelayCommand TestDbCommand { get; }
        public IRelayCommand BrowseCompanyLogoCommand { get; }
        public IAsyncRelayCommand SaveCompanyLogoCommand { get; }
        public IRelayCommand SelectAllItemDisplayCommand { get; }
        public IRelayCommand SelectNoneItemDisplayCommand { get; }

        internal bool TestDbConnection(out string message)
        {
            try
            {
                using var db = new DatabaseService(ConnectionString);
                using var conn = db.CreateConnection();
                message = "Connection successful.";
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection test failed");
                message = $"Connection failed: {ex.Message}";
                return false;
            }
        }

        void BrowseCompanyLogo()
        {
            var path = _fileDialog.OpenFile("Image Files|*.png;*.jpg;*.jpeg;*.bmp");
            if (!string.IsNullOrWhiteSpace(path))
                CompanyLogoPath = path;
        }

        async Task SaveCompanyLogoAsync(CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(CompanyLogoPath))
            {
                _dialogService.ShowInfo("Selected logo path is invalid.", "Invalid Path");
                return;
            }

            string baseDir = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            string fullInputPath;

            try
            {
                fullInputPath = Path.GetFullPath(CompanyLogoPath);
            }
            catch
            {
                _dialogService.ShowInfo("Selected logo path is invalid.", "Invalid Path");
                return;
            }

            if (!File.Exists(fullInputPath))
            {
                _dialogService.ShowInfo("Selected logo path is invalid.", "Invalid Path");
                return;
            }

            string relativePath;
            try
            {
                if (!fullInputPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
                {
                    var assetsDir = Path.Combine(baseDir, "Assets", "CompanyLogo");
                    Directory.CreateDirectory(assetsDir);
                    var fileName = Path.GetFileName(fullInputPath);
                    var destPath = Path.Combine(assetsDir, fileName);
                    File.Copy(fullInputPath, destPath, true);
                    relativePath = Path.GetRelativePath(baseDir, destPath);
                }
                else
                {
                    relativePath = Path.GetRelativePath(baseDir, fullInputPath);
                }

                CompanyLogoPath = relativePath;

                try
                {
                    await _settingsService.SaveSettingAsync("CompanyLogoPath", relativePath, token).ConfigureAwait(false);
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogWarning(ex, "Unauthorized to change settings.");
                    _dialogService.ShowInfo("You are not authorized to change settings.", "Unauthorized");
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogInformation(ex, "Saving company logo was canceled.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save company logo path.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to copy company logo.");
                _dialogService.ShowInfo("Failed to save company logo.", "Error");
            }
        }
    }
}


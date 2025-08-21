using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Services.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Utilities.Helpers;

namespace InventoryManagementApp.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        readonly IFileDialogService _fileDialog;
        readonly ISettingsService _settingsService;
        readonly IDialogService _dialogService;
        readonly ILogger<SettingsViewModel> _logger;

        public SettingsViewModel(IFileDialogService fileDialog, ISettingsService settingsService, IDialogService dialogService, ILogger<SettingsViewModel>? logger = null)
        {
            _fileDialog = fileDialog;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _logger = logger ?? NullLogger<SettingsViewModel>.Instance;

            var logoPath = _settingsService.GetSettingAsync("CompanyLogoPath").GetAwaiter().GetResult();
            if (!string.IsNullOrWhiteSpace(logoPath))
                CompanyLogoPath = logoPath;

            var appName = _settingsService.GetSettingAsync("ApplicationName").GetAwaiter().GetResult();
            if (!string.IsNullOrWhiteSpace(appName))
                _applicationName = appName;

            ThemeOptions = new ObservableCollection<string> { "Light", "Dark" };
            _theme = ThemeOptions[0];
            _passwordIterations = _settingsService.GetPasswordIterationsAsync().GetAwaiter().GetResult();
            _autoLogoutMinutes = _settingsService.GetAutoLogoutMinutesAsync().GetAwaiter().GetResult();
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
        }

        private string _applicationName;
        public string ApplicationName
        {
            get => _applicationName;
            set
            {
                if (SetProperty(ref _applicationName, value))
                {
                    try
                    {
                        _settingsService.SaveSettingAsync("ApplicationName", value).GetAwaiter().GetResult();
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
            }
        }

        private string _companyLogoPath;
        public string CompanyLogoPath
        {
            get => _companyLogoPath;
            set => SetProperty(ref _companyLogoPath, value);
        }

        private string _itemLabelSingular;
        public string ItemLabelSingular
        {
            get => _itemLabelSingular;
            set
            {
                if (SetProperty(ref _itemLabelSingular, value))
                {
                    try
                    {
                        _settingsService.SaveItemLabelSingularAsync(value).GetAwaiter().GetResult();
                        LabelProvider.Instance.UpdateLabels(value, ItemLabelPlural);
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
            }
        }

        private string _itemLabelPlural;
        public string ItemLabelPlural
        {
            get => _itemLabelPlural;
            set
            {
                if (SetProperty(ref _itemLabelPlural, value))
                {
                    try
                    {
                        _settingsService.SaveItemLabelPluralAsync(value).GetAwaiter().GetResult();
                        LabelProvider.Instance.UpdateLabels(ItemLabelSingular, value);
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
            }
        }

        private int _defaultRentalDuration;
        public int DefaultRentalDuration
        {
            get => _defaultRentalDuration;
            set => SetProperty(ref _defaultRentalDuration, value);
        }

        private string _connectionString;
        public string ConnectionString
        {
            get => _connectionString;
            set => SetProperty(ref _connectionString, value);
        }

        private string _theme;
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
            set => SetPasswordIterationsAsync(value).GetAwaiter().GetResult();
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
            set => SetAutoLogoutMinutesAsync(value).GetAwaiter().GetResult();
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

        public ObservableCollection<string> ThemeOptions { get; }

        public IRelayCommand TestDbCommand { get; }
        public IRelayCommand BrowseCompanyLogoCommand { get; }
        public IAsyncRelayCommand SaveCompanyLogoCommand { get; }

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


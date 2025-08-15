using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Utilities.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ToolManagementAppV2.ViewModels
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

            var logoPath = _settingsService.GetSetting("CompanyLogoPath");
            if (!string.IsNullOrWhiteSpace(logoPath))
                CompanyLogoPath = logoPath;

            ThemeOptions = new ObservableCollection<string> { "Light", "Dark" };
            _theme = ThemeOptions[0];
            _passwordIterations = _settingsService.GetPasswordIterations();
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
            SaveCompanyLogoCommand = new RelayCommand(SaveCompanyLogo);
        }

        private string _applicationName;
        public string ApplicationName
        {
            get => _applicationName;
            set => SetProperty(ref _applicationName, value);
        }

        private string _companyLogoPath;
        public string CompanyLogoPath
        {
            get => _companyLogoPath;
            set => SetProperty(ref _companyLogoPath, value);
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
        public int PasswordIterations
        {
            get => _passwordIterations;
            set
            {
                if (value <= 0) return;
                if (SetProperty(ref _passwordIterations, value))
                    _settingsService.SavePasswordIterations(value);
            }
        }

        public ObservableCollection<string> ThemeOptions { get; }

        public IRelayCommand TestDbCommand { get; }
        public IRelayCommand BrowseCompanyLogoCommand { get; }
        public IRelayCommand SaveCompanyLogoCommand { get; }

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

        void SaveCompanyLogo()
        {
            var full = PathHelper.GetAbsolutePath(CompanyLogoPath);
            if (string.IsNullOrEmpty(full))
            {
                _dialogService.ShowInfo("Selected logo path is invalid.", "Invalid Path");
                return;
            }

            _settingsService.SaveSetting("CompanyLogoPath", full);
        }
    }
}


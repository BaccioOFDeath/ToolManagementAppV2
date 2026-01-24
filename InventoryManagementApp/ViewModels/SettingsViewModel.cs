using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Settings;
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
        readonly IThemeService _themeService;
        readonly RentalConfigurationService? _rentalConfigService;
        readonly ILogger<SettingsViewModel> _logger;
        public ObservableCollection<ItemDetailOption> ItemDetailOptions { get; } = new();
        public ObservableCollection<string> FromEmailOptions { get; } = new();
        public ObservableCollection<string> SmsProviders { get; }
        bool _bulkUpdating;

        public SettingsViewModel(IFileDialogService fileDialog, ISettingsService settingsService, IDialogService dialogService, IThemeService themeService, RentalConfigurationService? rentalConfigService = null, ILogger<SettingsViewModel>? logger = null)
        {
            _fileDialog = fileDialog;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _themeService = themeService;
            _rentalConfigService = rentalConfigService;
            _logger = logger ?? NullLogger<SettingsViewModel>.Instance;

            ThemeOptions = new ObservableCollection<string> { "Light", "Dark" };
            _theme = ThemeOptions[0];
            _itemLabelSingular = LabelProvider.Instance.ItemLabelSingular;
            _itemLabelPlural = LabelProvider.Instance.ItemLabelPlural;
            
            // Email provider options
            EmailProviders = new ObservableCollection<string> { "Custom", "Gmail", "Outlook/Office 365", "Yahoo", "iCloud" };
            _selectedEmailProvider = EmailProviders[0];

            SmsProviders = new ObservableCollection<string> { "None", "Twilio", "Vonage", "AWS SNS", "Custom" };
            _selectedSmsProvider = SmsProviders[0];
            
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
            SaveEmailSettingsCommand = new AsyncRelayCommand(SaveEmailSettingsAsync);
            TestEmailCommand = new AsyncRelayCommand(TestEmailConnectionAsync);
            AddFromEmailCommand = new RelayCommand(AddFromEmailOption);
            RemoveFromEmailCommand = new RelayCommand(RemoveFromEmailOption);
            BrowseBackupDirectoryCommand = new RelayCommand(BrowseBackupDirectory);
            SaveBackupSettingsCommand = new AsyncRelayCommand(SaveBackupSettingsAsync);
            SaveMessagingSettingsCommand = new AsyncRelayCommand(SaveMessagingSettingsAsync);
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
            var theme = await _settingsService.GetThemeAsync().ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(theme) && ThemeOptions.Contains(theme))
                _theme = theme;
            _themeService.ApplyTheme(_theme);
            _passwordIterations = await _settingsService.GetPasswordIterationsAsync().ConfigureAwait(false);
            _autoLogoutMinutes = await _settingsService.GetAutoLogoutMinutesAsync().ConfigureAwait(false);
            _itemCardSize = await _settingsService.GetItemCardSizeAsync().ConfigureAwait(false);
            var vis = await _settingsService.GetItemDetailVisibilityAsync().ConfigureAwait(false);
            foreach (var field in Enum.GetValues<ItemDetailField>())
            {
                var option = new ItemDetailOption(field, vis.TryGetValue(field, out var v) ? v : true);
                option.PropertyChanged += ItemDetailOption_PropertyChanged;
                ItemDetailOptions.Add(option);
            }
            OnPropertyChanged(nameof(CompanyLogoPath));
            OnPropertyChanged(nameof(ApplicationName));
            OnPropertyChanged(nameof(Theme));
            OnPropertyChanged(nameof(PasswordIterations));
            OnPropertyChanged(nameof(AutoLogoutMinutes));
            OnPropertyChanged(nameof(ItemCardSize));
            
            // Load email settings if service is available
            if (_rentalConfigService != null)
            {
                try
                {
                    _emailEnabled = await _rentalConfigService.GetEmailEnabledAsync().ConfigureAwait(false);
                    _smtpHost = await _rentalConfigService.GetSmtpHostAsync().ConfigureAwait(false);
                    _smtpPort = await _rentalConfigService.GetSmtpPortAsync().ConfigureAwait(false);
                    _smtpUsername = await _rentalConfigService.GetSmtpUsernameAsync().ConfigureAwait(false);
                    _smtpPassword = await _rentalConfigService.GetSmtpPasswordAsync().ConfigureAwait(false);
                    _fromEmail = await _rentalConfigService.GetFromEmailAsync().ConfigureAwait(false);
                    _fromName = await _rentalConfigService.GetFromNameAsync().ConfigureAwait(false);
                    _enableSsl = await _rentalConfigService.GetEnableSslAsync().ConfigureAwait(false);
                    var fromEmailOptions = await _rentalConfigService.GetFromEmailOptionsAsync().ConfigureAwait(false);
                    _backupDirectory = await _rentalConfigService.GetBackupDirectoryAsync().ConfigureAwait(false);
                    _selectedSmsProvider = await _rentalConfigService.GetSmsProviderAsync().ConfigureAwait(false);
                    _smsApiKey = await _rentalConfigService.GetSmsApiKeyAsync().ConfigureAwait(false);
                    _smsSender = await _rentalConfigService.GetSmsSenderAsync().ConfigureAwait(false);

                    FromEmailOptions.Clear();
                    foreach (var option in fromEmailOptions)
                    {
                        FromEmailOptions.Add(option);
                    }
                    if (!string.IsNullOrWhiteSpace(_fromEmail) && !FromEmailOptions.Contains(_fromEmail))
                    {
                        FromEmailOptions.Insert(0, _fromEmail);
                    }
                    _selectedFromEmail = FromEmailOptions.FirstOrDefault(email => email.Equals(_fromEmail, StringComparison.OrdinalIgnoreCase))
                        ?? FromEmailOptions.FirstOrDefault()
                        ?? _fromEmail;
                    if (!string.IsNullOrWhiteSpace(_selectedFromEmail))
                    {
                        _fromEmail = _selectedFromEmail;
                    }
                    
                    OnPropertyChanged(nameof(EmailEnabled));
                    OnPropertyChanged(nameof(SmtpHost));
                    OnPropertyChanged(nameof(SmtpPort));
                    OnPropertyChanged(nameof(SmtpUsername));
                    OnPropertyChanged(nameof(SmtpPassword));
                    OnPropertyChanged(nameof(FromEmail));
                    OnPropertyChanged(nameof(FromName));
                    OnPropertyChanged(nameof(EnableSsl));
                    OnPropertyChanged(nameof(SelectedFromEmail));
                    OnPropertyChanged(nameof(BackupDirectory));
                    OnPropertyChanged(nameof(SelectedSmsProvider));
                    OnPropertyChanged(nameof(SmsApiKey));
                    OnPropertyChanged(nameof(SmsSender));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load email settings.");
                }
            }
            
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
            set
            {
                if (SetProperty(ref _theme, value))
                {
                    _ = SetThemeAsync(value);
                }
            }
        }

        async Task SetThemeAsync(string value, CancellationToken token = default)
        {
            try
            {
                await _settingsService.SaveThemeAsync(value, token).ConfigureAwait(false);
                _themeService.ApplyTheme(value);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized to change settings.");
                _dialogService.ShowInfo("You are not authorized to change settings.", "Unauthorized");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "Saving theme was canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save theme.");
            }
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

        private double _itemCardSize = 1.0;
        public double ItemCardSize
        {
            get => _itemCardSize;
            set
            {
                if (SetProperty(ref _itemCardSize, value))
                {
                    if (_initialized)
                        _ = SetItemCardSizeAsync(value);
                }
            }
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

        async Task SetItemCardSizeAsync(double value, CancellationToken token = default)
        {
            var clamped = Math.Clamp(value, 0.8, 1.3);
            if (Math.Abs(clamped - value) > 0.0001)
            {
                _itemCardSize = clamped;
                OnPropertyChanged(nameof(ItemCardSize));
            }

            try
            {
                await _settingsService.SaveItemCardSizeAsync(clamped, token).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized to change item card size.");
                _dialogService.ShowInfo("You are not authorized to change settings.", "Unauthorized");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "Saving item card size was canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save item card size.");
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
        public IRelayCommand AddFromEmailCommand { get; }
        public IRelayCommand RemoveFromEmailCommand { get; }

        public IRelayCommand TestDbCommand { get; }
        public IRelayCommand BrowseCompanyLogoCommand { get; }
        public IAsyncRelayCommand SaveCompanyLogoCommand { get; }
        public IRelayCommand SelectAllItemDisplayCommand { get; }
        public IRelayCommand SelectNoneItemDisplayCommand { get; }
        public IRelayCommand BrowseBackupDirectoryCommand { get; }
        public IAsyncRelayCommand SaveBackupSettingsCommand { get; }
        public IAsyncRelayCommand SaveMessagingSettingsCommand { get; }

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

        // Email Configuration Properties and Commands
        public ObservableCollection<string> EmailProviders { get; }
        
        private string _selectedEmailProvider = "Custom";
        public string SelectedEmailProvider
        {
            get => _selectedEmailProvider;
            set
            {
                if (SetProperty(ref _selectedEmailProvider, value))
                {
                    ApplyEmailProviderTemplate(value);
                }
            }
        }

        private bool _emailEnabled;
        public bool EmailEnabled
        {
            get => _emailEnabled;
            set => SetProperty(ref _emailEnabled, value);
        }

        private string _smtpHost = "";
        public string SmtpHost
        {
            get => _smtpHost;
            set => SetProperty(ref _smtpHost, value);
        }

        private int _smtpPort = 587;
        public int SmtpPort
        {
            get => _smtpPort;
            set => SetProperty(ref _smtpPort, value);
        }

        private string _smtpUsername = "";
        public string SmtpUsername
        {
            get => _smtpUsername;
            set => SetProperty(ref _smtpUsername, value);
        }

        private string _smtpPassword = "";
        public string SmtpPassword
        {
            get => _smtpPassword;
            set => SetProperty(ref _smtpPassword, value);
        }

        private string _fromEmail = "";
        public string FromEmail
        {
            get => _fromEmail;
            set => SetProperty(ref _fromEmail, value);
        }

        private string _selectedFromEmail = "";
        public string SelectedFromEmail
        {
            get => _selectedFromEmail;
            set
            {
                if (SetProperty(ref _selectedFromEmail, value))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        FromEmail = value;
                        EnsureFromEmailOption(value);
                    }
                }
            }
        }

        private string _newFromEmail = "";
        public string NewFromEmail
        {
            get => _newFromEmail;
            set => SetProperty(ref _newFromEmail, value);
        }

        private string _fromName = "";
        public string FromName
        {
            get => _fromName;
            set => SetProperty(ref _fromName, value);
        }

        private bool _enableSsl = true;
        public bool EnableSsl
        {
            get => _enableSsl;
            set => SetProperty(ref _enableSsl, value);
        }

        public IAsyncRelayCommand SaveEmailSettingsCommand { get; }
        public IAsyncRelayCommand TestEmailCommand { get; }

        private void ApplyEmailProviderTemplate(string provider)
        {
            switch (provider)
            {
                case "Gmail":
                    SmtpHost = "smtp.gmail.com";
                    SmtpPort = 587;
                    EnableSsl = true;
                    break;
                case "Outlook/Office 365":
                    SmtpHost = "smtp-mail.outlook.com";
                    SmtpPort = 587;
                    EnableSsl = true;
                    break;
                case "Yahoo":
                    SmtpHost = "smtp.mail.yahoo.com";
                    SmtpPort = 587;
                    EnableSsl = true;
                    break;
                case "iCloud":
                    SmtpHost = "smtp.mail.me.com";
                    SmtpPort = 587;
                    EnableSsl = true;
                    break;
                case "Custom":
                    // Don't change values for custom
                    break;
            }
        }

        private async Task SaveEmailSettingsAsync(CancellationToken token = default)
        {
            if (_rentalConfigService == null)
            {
                _dialogService.ShowInfo("Email configuration service not available.", "Error");
                return;
            }

            try
            {
                await SaveFromEmailOptionsAsync(token).ConfigureAwait(false);
                await _rentalConfigService.SetEmailEnabledAsync(EmailEnabled, token);
                await _rentalConfigService.SetSmtpHostAsync(SmtpHost, token);
                await _rentalConfigService.SetSmtpPortAsync(SmtpPort, token);
                await _rentalConfigService.SetSmtpUsernameAsync(SmtpUsername, token);
                await _rentalConfigService.SetSmtpPasswordAsync(SmtpPassword, token);
                await _rentalConfigService.SetFromEmailAsync(FromEmail, token);
                await _rentalConfigService.SetFromNameAsync(FromName, token);
                await _rentalConfigService.SetEnableSslAsync(EnableSsl, token);

                _dialogService.ShowInfo("Email settings saved successfully. Restart the application for changes to take effect.", "Settings Saved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save email settings.");
                _dialogService.ShowInfo("Failed to save email settings. Please try again.", "Error");
            }
        }

        private void AddFromEmailOption()
        {
            if (string.IsNullOrWhiteSpace(NewFromEmail))
            {
                _dialogService.ShowInfo("Enter an email address to add.", "Email Address");
                return;
            }

            try
            {
                _ = new MailAddress(NewFromEmail);
            }
            catch (FormatException)
            {
                _dialogService.ShowInfo("Enter a valid email address.", "Invalid Email");
                return;
            }

            EnsureFromEmailOption(NewFromEmail);
            SelectedFromEmail = NewFromEmail.Trim();
            NewFromEmail = string.Empty;
            _ = SaveFromEmailOptionsAsync();
        }

        private void RemoveFromEmailOption()
        {
            if (string.IsNullOrWhiteSpace(SelectedFromEmail))
            {
                _dialogService.ShowInfo("Select an email address to remove.", "Email Address");
                return;
            }

            var toRemove = SelectedFromEmail;
            if (FromEmailOptions.Remove(toRemove))
            {
                SelectedFromEmail = FromEmailOptions.FirstOrDefault() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(SelectedFromEmail))
                {
                    FromEmail = SelectedFromEmail;
                }
                _ = SaveFromEmailOptionsAsync();
            }
        }

        private void EnsureFromEmailOption(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var trimmed = value.Trim();
            if (!FromEmailOptions.Any(option => option.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                FromEmailOptions.Add(trimmed);
            }
        }

        private async Task SaveFromEmailOptionsAsync(CancellationToken token = default)
        {
            if (_rentalConfigService == null)
            {
                return;
            }

            var options = FromEmailOptions.ToList();
            if (!string.IsNullOrWhiteSpace(FromEmail))
            {
                options.Add(FromEmail);
            }
            await _rentalConfigService.SetFromEmailOptionsAsync(options, token).ConfigureAwait(false);
        }

        private async Task TestEmailConnectionAsync(CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(SmtpHost) || SmtpHost.Contains("example.com", StringComparison.OrdinalIgnoreCase))
            {
                _dialogService.ShowInfo("Please enter a valid SMTP host.", "Invalid Configuration");
                return;
            }

            if (string.IsNullOrWhiteSpace(SmtpUsername) || string.IsNullOrWhiteSpace(SmtpPassword))
            {
                _dialogService.ShowInfo("Please enter SMTP username and password.", "Invalid Configuration");
                return;
            }

            try
            {
                // Test the connection using System.Net.Mail.SmtpClient
                using var client = new System.Net.Mail.SmtpClient(SmtpHost, SmtpPort)
                {
                    EnableSsl = EnableSsl,
                    Credentials = new System.Net.NetworkCredential(SmtpUsername, SmtpPassword),
                    Timeout = 10000
                };

                // Try to send a test (but don't actually send)
                await Task.Run(() =>
                {
                    // Just verify credentials - this will throw if authentication fails
                    client.Send(new System.Net.Mail.MailMessage(
                        FromEmail,
                        FromEmail,
                        "Test Connection",
                        "This is a test message to verify SMTP configuration."));
                }, token);

                _dialogService.ShowInfo("Email configuration is valid and test email sent successfully!", "Connection Successful");
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                _logger.LogWarning(ex, "SMTP test failed.");
                _dialogService.ShowInfo($"SMTP connection failed: {ex.Message}\n\nPlease verify your settings and try again.", "Connection Failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email test failed.");
                _dialogService.ShowInfo($"Email test failed: {ex.Message}", "Test Failed");
            }
        }

        private string _backupDirectory = "";
        public string BackupDirectory
        {
            get => _backupDirectory;
            set => SetProperty(ref _backupDirectory, value);
        }

        private string _selectedSmsProvider = "None";
        public string SelectedSmsProvider
        {
            get => _selectedSmsProvider;
            set => SetProperty(ref _selectedSmsProvider, value);
        }

        private string _smsApiKey = "";
        public string SmsApiKey
        {
            get => _smsApiKey;
            set => SetProperty(ref _smsApiKey, value);
        }

        private string _smsSender = "";
        public string SmsSender
        {
            get => _smsSender;
            set => SetProperty(ref _smsSender, value);
        }

        private void BrowseBackupDirectory()
        {
            var path = _fileDialog.BrowseFolder(BackupDirectory);
            if (!string.IsNullOrWhiteSpace(path))
            {
                BackupDirectory = path;
            }
        }

        private async Task SaveBackupSettingsAsync(CancellationToken token = default)
        {
            if (_rentalConfigService == null)
            {
                _dialogService.ShowInfo("Backup configuration service not available.", "Error");
                return;
            }

            if (string.IsNullOrWhiteSpace(BackupDirectory))
            {
                _dialogService.ShowInfo("Backup directory is required.", "Invalid Directory");
                return;
            }

            try
            {
                Directory.CreateDirectory(BackupDirectory);
                await _rentalConfigService.SetBackupDirectoryAsync(BackupDirectory, token).ConfigureAwait(false);
                _dialogService.ShowInfo("Backup settings saved successfully.", "Settings Saved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save backup settings.");
                _dialogService.ShowInfo("Failed to save backup settings. Please try again.", "Error");
            }
        }

        private async Task SaveMessagingSettingsAsync(CancellationToken token = default)
        {
            if (_rentalConfigService == null)
            {
                _dialogService.ShowInfo("Messaging configuration service not available.", "Error");
                return;
            }

            try
            {
                await _rentalConfigService.SetSmsProviderAsync(SelectedSmsProvider, token).ConfigureAwait(false);
                await _rentalConfigService.SetSmsApiKeyAsync(SmsApiKey, token).ConfigureAwait(false);
                await _rentalConfigService.SetSmsSenderAsync(SmsSender, token).ConfigureAwait(false);
                _dialogService.ShowInfo("Messaging settings saved successfully.", "Settings Saved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save messaging settings.");
                _dialogService.ShowInfo("Failed to save messaging settings. Please try again.", "Error");
            }
        }
    }
}

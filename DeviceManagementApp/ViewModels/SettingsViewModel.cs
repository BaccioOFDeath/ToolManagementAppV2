using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using DeviceManagementApp.Resources;

namespace DeviceManagementApp.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        readonly ISettingsService _settingsService;
        readonly IDialogService _dialogService;
        bool _bulkUpdating;

        public ObservableCollection<ItemDetailOption> ItemDetailOptions { get; } = new();

        public SettingsViewModel(ISettingsService settingsService, IDialogService dialogService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            ThemeOptions = new ObservableCollection<string> { "Light", "Dark" };
            TestDbCommand = new RelayCommand(() =>
            {
                var success = TestDbConnection(out var message);
                _dialogService.ShowInfo(message, "Database Connection");
            });
            BrowseCompanyLogoCommand = new RelayCommand(() => _dialogService.ShowInfo("Browsing not implemented.", "Settings"));
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
            ConnectionString = await _settingsService.GetSettingAsync("ConnectionString").ConfigureAwait(false) ?? string.Empty;
            ApplicationName = await _settingsService.GetSettingAsync("ApplicationName").ConfigureAwait(false) ?? string.Empty;
            Theme = await _settingsService.GetThemeAsync().ConfigureAwait(false) ?? ThemeOptions[0];
            PasswordIterations = await _settingsService.GetPasswordIterationsAsync().ConfigureAwait(false);
            AutoLogoutMinutes = await _settingsService.GetAutoLogoutMinutesAsync().ConfigureAwait(false);
            ItemLabelSingular = await _settingsService.GetItemLabelSingularAsync().ConfigureAwait(false);
            ItemLabelPlural = await _settingsService.GetItemLabelPluralAsync().ConfigureAwait(false);
            CompanyLogoPath = await _settingsService.GetSettingAsync("CompanyLogoPath").ConfigureAwait(false) ?? string.Empty;
            var vis = await _settingsService.GetItemDetailVisibilityAsync().ConfigureAwait(false);
            foreach (var field in Enum.GetValues<ItemDetailField>())
            {
                var option = new ItemDetailOption(field, vis.TryGetValue(field, out var v) ? v : true);
                option.PropertyChanged += ItemDetailOption_PropertyChanged;
                ItemDetailOptions.Add(option);
            }
            _initialized = true;
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
                    _ = _settingsService.SaveThemeAsync(value);
            }
        }

        private int _passwordIterations;
        public int PasswordIterations
        {
            get => _passwordIterations;
            set
            {
                if (SetProperty(ref _passwordIterations, value))
                    _ = _settingsService.SavePasswordIterationsAsync(value);
            }
        }

        private int _autoLogoutMinutes;
        public int AutoLogoutMinutes
        {
            get => _autoLogoutMinutes;
            set
            {
                if (SetProperty(ref _autoLogoutMinutes, value))
                    _ = _settingsService.SaveAutoLogoutMinutesAsync(value);
            }
        }

        private string _itemLabelSingular = string.Empty;
        public string ItemLabelSingular
        {
            get => _itemLabelSingular;
            set
            {
                if (SetProperty(ref _itemLabelSingular, value))
                {
                    _ = _settingsService.SaveItemLabelSingularAsync(value);
                    LabelProvider.Instance.UpdateLabels(LabelProvider.Instance.PageLabel, LabelProvider.Instance.TooltipLabel, value, ItemLabelPlural);
                }
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
                    _ = _settingsService.SaveItemLabelPluralAsync(value);
                    LabelProvider.Instance.UpdateLabels(LabelProvider.Instance.PageLabel, LabelProvider.Instance.TooltipLabel, ItemLabelSingular, value);
                }
            }
        }

        private string _companyLogoPath = string.Empty;
        public string CompanyLogoPath
        {
            get => _companyLogoPath;
            set
            {
                if (SetProperty(ref _companyLogoPath, value))
                    _ = _settingsService.SaveSettingAsync("CompanyLogoPath", value);
            }
        }

        private string _applicationName = string.Empty;
        public string ApplicationName
        {
            get => _applicationName;
            set
            {
                if (SetProperty(ref _applicationName, value))
                    _ = _settingsService.SaveSettingAsync("ApplicationName", value);
            }
        }

        void ItemDetailOption_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemDetailOption.IsVisible) && !_bulkUpdating)
                _ = SaveVisibilityAsync();
        }

        async Task SaveVisibilityAsync()
        {
            var dict = ItemDetailOptions.ToDictionary(o => o.Field, o => o.IsVisible);
            await _settingsService.SaveItemDetailVisibilityAsync(dict).ConfigureAwait(false);
        }

        bool TestDbConnection(out string message)
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
                message = $"Connection failed: {ex.Message}";
                return false;
            }
        }

        async Task SaveCompanyLogoAsync(CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(CompanyLogoPath))
            {
                _dialogService.ShowInfo("Selected logo path is invalid.", "Invalid Path");
                return;
            }
            try
            {
                await _settingsService.SaveSettingAsync("CompanyLogoPath", CompanyLogoPath, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to save company logo: {ex.Message}", "Error");
            }
        }

        public ObservableCollection<string> ThemeOptions { get; }
        public IRelayCommand TestDbCommand { get; }
        public IRelayCommand BrowseCompanyLogoCommand { get; }
        public IAsyncRelayCommand SaveCompanyLogoCommand { get; }
        public IRelayCommand SelectAllItemDisplayCommand { get; }
        public IRelayCommand SelectNoneItemDisplayCommand { get; }
    }
}

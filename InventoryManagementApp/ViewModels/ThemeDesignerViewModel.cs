using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementApp.ViewModels
{
    public class ThemeDesignerViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly IThemeService _themeService;
        private readonly IFileDialogService _fileDialogService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ThemeDesignerViewModel> _logger;
        private AppThemeSettings _settings = AppThemeSettings.CreateDefault();
        private string _status = "Theme designer ready.";

        public ThemeDesignerViewModel(
            ISettingsService settingsService,
            IThemeService themeService,
            IFileDialogService fileDialogService,
            IDialogService dialogService,
            ILogger<ThemeDesignerViewModel>? logger = null)
        {
            _settingsService = settingsService;
            _themeService = themeService;
            _fileDialogService = fileDialogService;
            _dialogService = dialogService;
            _logger = logger ?? NullLogger<ThemeDesignerViewModel>.Instance;

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            ResetCommand = new RelayCommand(Reset);
            BrowseBackgroundCommand = new RelayCommand(BrowseBackground);
            ClearBackgroundCommand = new RelayCommand(() => BackgroundImagePath = string.Empty);
            GlassPresetCommand = new RelayCommand(ApplyGlassPreset);
            BorderlessPresetCommand = new RelayCommand(ApplyBorderlessPreset);
            HighContrastPresetCommand = new RelayCommand(ApplyHighContrastPreset);
        }

        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand ResetCommand { get; }
        public IRelayCommand BrowseBackgroundCommand { get; }
        public IRelayCommand ClearBackgroundCommand { get; }
        public IRelayCommand GlassPresetCommand { get; }
        public IRelayCommand BorderlessPresetCommand { get; }
        public IRelayCommand HighContrastPresetCommand { get; }

        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

        public string BaseTheme
        {
            get => _settings.BaseTheme;
            set => SetString(value, (settings, newValue) => settings.BaseTheme = newValue, nameof(BaseTheme));
        }

        public string BackgroundColor
        {
            get => _settings.BackgroundColor;
            set => SetString(value, (settings, newValue) => settings.BackgroundColor = newValue, nameof(BackgroundColor));
        }

        public string SurfaceColor
        {
            get => _settings.SurfaceColor;
            set => SetString(value, (settings, newValue) => settings.SurfaceColor = newValue, nameof(SurfaceColor));
        }

        public string SurfaceAltColor
        {
            get => _settings.SurfaceAltColor;
            set => SetString(value, (settings, newValue) => settings.SurfaceAltColor = newValue, nameof(SurfaceAltColor));
        }

        public string TextColor
        {
            get => _settings.TextColor;
            set => SetString(value, (settings, newValue) => settings.TextColor = newValue, nameof(TextColor));
        }

        public string MutedTextColor
        {
            get => _settings.MutedTextColor;
            set => SetString(value, (settings, newValue) => settings.MutedTextColor = newValue, nameof(MutedTextColor));
        }

        public string AccentColor
        {
            get => _settings.AccentColor;
            set => SetString(value, (settings, newValue) => settings.AccentColor = newValue, nameof(AccentColor));
        }

        public string SuccessColor
        {
            get => _settings.SuccessColor;
            set => SetString(value, (settings, newValue) => settings.SuccessColor = newValue, nameof(SuccessColor));
        }

        public string WarningColor
        {
            get => _settings.WarningColor;
            set => SetString(value, (settings, newValue) => settings.WarningColor = newValue, nameof(WarningColor));
        }

        public string ErrorColor
        {
            get => _settings.ErrorColor;
            set => SetString(value, (settings, newValue) => settings.ErrorColor = newValue, nameof(ErrorColor));
        }

        public string BackgroundImagePath
        {
            get => _settings.BackgroundImagePath;
            set => SetString(value, (settings, newValue) => settings.BackgroundImagePath = newValue, nameof(BackgroundImagePath));
        }

        public double BackgroundOpacity
        {
            get => _settings.BackgroundOpacity;
            set => SetDouble(value, (settings, newValue) => settings.BackgroundOpacity = newValue, nameof(BackgroundOpacity));
        }

        public double SurfaceOpacity
        {
            get => _settings.SurfaceOpacity;
            set => SetDouble(value, (settings, newValue) => settings.SurfaceOpacity = newValue, nameof(SurfaceOpacity));
        }

        public double InputOpacity
        {
            get => _settings.InputOpacity;
            set => SetDouble(value, (settings, newValue) => settings.InputOpacity = newValue, nameof(InputOpacity));
        }

        public double ButtonOpacity
        {
            get => _settings.ButtonOpacity;
            set => SetDouble(value, (settings, newValue) => settings.ButtonOpacity = newValue, nameof(ButtonOpacity));
        }

        public bool BordersVisible
        {
            get => _settings.BordersVisible;
            set => SetBool(value, (settings, newValue) => settings.BordersVisible = newValue, nameof(BordersVisible));
        }

        public bool UseGlassSurfaces
        {
            get => _settings.UseGlassSurfaces;
            set => SetBool(value, (settings, newValue) => settings.UseGlassSurfaces = newValue, nameof(UseGlassSurfaces));
        }

        public double BorderOpacity
        {
            get => _settings.BorderOpacity;
            set => SetDouble(value, (settings, newValue) => settings.BorderOpacity = newValue, nameof(BorderOpacity));
        }

        public double CardCornerRadius
        {
            get => _settings.CardCornerRadius;
            set => SetDouble(value, (settings, newValue) => settings.CardCornerRadius = newValue, nameof(CardCornerRadius));
        }

        public double ButtonCornerRadius
        {
            get => _settings.ButtonCornerRadius;
            set => SetDouble(value, (settings, newValue) => settings.ButtonCornerRadius = newValue, nameof(ButtonCornerRadius));
        }

        public double InputCornerRadius
        {
            get => _settings.InputCornerRadius;
            set => SetDouble(value, (settings, newValue) => settings.InputCornerRadius = newValue, nameof(InputCornerRadius));
        }

        public double ShadowBlurRadius
        {
            get => _settings.ShadowBlurRadius;
            set => SetDouble(value, (settings, newValue) => settings.ShadowBlurRadius = newValue, nameof(ShadowBlurRadius));
        }

        public double ShadowDepth
        {
            get => _settings.ShadowDepth;
            set => SetDouble(value, (settings, newValue) => settings.ShadowDepth = newValue, nameof(ShadowDepth));
        }

        public double ShadowOpacity
        {
            get => _settings.ShadowOpacity;
            set => SetDouble(value, (settings, newValue) => settings.ShadowOpacity = newValue, nameof(ShadowOpacity));
        }

        public double PagePadding
        {
            get => _settings.PagePadding;
            set => SetDouble(value, (settings, newValue) => settings.PagePadding = newValue, nameof(PagePadding));
        }

        public double CardPadding
        {
            get => _settings.CardPadding;
            set => SetDouble(value, (settings, newValue) => settings.CardPadding = newValue, nameof(CardPadding));
        }

        public async Task InitializeAsync(CancellationToken token = default)
        {
            try
            {
                _settings = await _settingsService.GetAppThemeSettingsAsync(token).ConfigureAwait(false);
                _themeService.ApplyCustomTheme(_settings);
                NotifyAllThemePropertiesChanged();
                Status = "Loaded saved app theme.";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load app theme settings.");
                _settings = AppThemeSettings.CreateDefault();
                NotifyAllThemePropertiesChanged();
                Status = "Using default theme settings.";
            }
        }

        private async Task SaveAsync(CancellationToken token = default)
        {
            try
            {
                _settings.Normalize();
                await _settingsService.SaveThemeAsync(_settings.BaseTheme, token).ConfigureAwait(false);
                await _settingsService.SaveAppThemeSettingsAsync(_settings, token).ConfigureAwait(false);
                _themeService.ApplyCustomTheme(_settings);
                NotifyAllThemePropertiesChanged();
                Status = "Theme saved and applied.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save app theme settings.");
                _dialogService.ShowInfo("Theme settings could not be saved. Check color values and try again.", "Theme Settings");
                Status = "Theme save failed.";
            }
        }

        private void Reset()
        {
            _settings = AppThemeSettings.CreateDefault(BaseTheme);
            Preview();
            NotifyAllThemePropertiesChanged();
            Status = "Theme reset to defaults. Save to keep it.";
        }

        private void BrowseBackground()
        {
            var path = _fileDialogService.OpenFile("Image Files|*.png;*.jpg;*.jpeg;*.bmp");
            if (!string.IsNullOrWhiteSpace(path))
                BackgroundImagePath = path;
        }

        private void ApplyGlassPreset()
        {
            UseGlassSurfaces = true;
            SurfaceOpacity = 0.68;
            InputOpacity = 0.72;
            ButtonOpacity = 0.72;
            BordersVisible = true;
            BorderOpacity = 0.42;
            CardCornerRadius = 14;
            ButtonCornerRadius = 12;
            InputCornerRadius = 10;
            ShadowBlurRadius = 18;
            ShadowDepth = 4;
            ShadowOpacity = 0.24;
            Status = "Glass preset previewed. Save to keep it.";
        }

        private void ApplyBorderlessPreset()
        {
            UseGlassSurfaces = false;
            BordersVisible = false;
            BorderOpacity = 0;
            CardCornerRadius = 0;
            ButtonCornerRadius = 0;
            InputCornerRadius = 0;
            ShadowBlurRadius = 0;
            ShadowDepth = 0;
            ShadowOpacity = 0;
            Status = "Borderless preset previewed. Save to keep it.";
        }

        private void ApplyHighContrastPreset()
        {
            _settings = AppThemeSettings.CreateDefault("Dark");
            _settings.BackgroundColor = "#FF000000";
            _settings.SurfaceColor = "#FF111111";
            _settings.SurfaceAltColor = "#FF1F1F1F";
            _settings.TextColor = "#FFFFFFFF";
            _settings.MutedTextColor = "#FFE5E7EB";
            _settings.AccentColor = "#FFFFFF00";
            _settings.BordersVisible = true;
            _settings.BorderOpacity = 1;
            Preview();
            NotifyAllThemePropertiesChanged();
            Status = "High contrast preset previewed. Save to keep it.";
        }

        private void SetString(string value, Action<AppThemeSettings, string> update, string propertyName)
        {
            update(_settings, value ?? string.Empty);
            OnPropertyChanged(propertyName);
            Preview();
        }

        private void SetDouble(double value, Action<AppThemeSettings, double> update, string propertyName)
        {
            update(_settings, value);
            OnPropertyChanged(propertyName);
            Preview();
        }

        private void SetBool(bool value, Action<AppThemeSettings, bool> update, string propertyName)
        {
            update(_settings, value);
            OnPropertyChanged(propertyName);
            Preview();
        }

        private void Preview()
        {
            try
            {
                _settings.Normalize();
                _themeService.ApplyCustomTheme(_settings);
                NotifyAllThemePropertiesChanged();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to preview app theme settings.");
                Status = "Preview failed. Check color values.";
            }
        }

        private void NotifyAllThemePropertiesChanged()
        {
            OnPropertyChanged(nameof(BaseTheme));
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(SurfaceColor));
            OnPropertyChanged(nameof(SurfaceAltColor));
            OnPropertyChanged(nameof(TextColor));
            OnPropertyChanged(nameof(MutedTextColor));
            OnPropertyChanged(nameof(AccentColor));
            OnPropertyChanged(nameof(SuccessColor));
            OnPropertyChanged(nameof(WarningColor));
            OnPropertyChanged(nameof(ErrorColor));
            OnPropertyChanged(nameof(BackgroundImagePath));
            OnPropertyChanged(nameof(BackgroundOpacity));
            OnPropertyChanged(nameof(SurfaceOpacity));
            OnPropertyChanged(nameof(InputOpacity));
            OnPropertyChanged(nameof(ButtonOpacity));
            OnPropertyChanged(nameof(BordersVisible));
            OnPropertyChanged(nameof(UseGlassSurfaces));
            OnPropertyChanged(nameof(BorderOpacity));
            OnPropertyChanged(nameof(CardCornerRadius));
            OnPropertyChanged(nameof(ButtonCornerRadius));
            OnPropertyChanged(nameof(InputCornerRadius));
            OnPropertyChanged(nameof(ShadowBlurRadius));
            OnPropertyChanged(nameof(ShadowDepth));
            OnPropertyChanged(nameof(ShadowOpacity));
            OnPropertyChanged(nameof(PagePadding));
            OnPropertyChanged(nameof(CardPadding));
        }
    }
}

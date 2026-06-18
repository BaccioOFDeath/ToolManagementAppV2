using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementApp.ViewModels
{
    public partial class SettingsViewModel
    {
        private AppThemeSettings _appThemeSettings = AppThemeSettings.CreateDefault();
        private bool _updatingThemeDesigner;
        private IAsyncRelayCommand? _saveAppThemeSettingsCommand;
        private IRelayCommand? _resetAppThemeSettingsCommand;
        private IRelayCommand? _applyGlassThemePresetCommand;
        private IRelayCommand? _applyBorderlessThemePresetCommand;
        private IRelayCommand? _applyHighContrastThemePresetCommand;
        private IRelayCommand? _browseThemeBackgroundCommand;
        private IRelayCommand? _clearThemeBackgroundCommand;

        public ObservableCollection<string> ThemeBackgroundModes { get; } = new() { "Color", "Image" };

        public IAsyncRelayCommand SaveAppThemeSettingsCommand => _saveAppThemeSettingsCommand ??= new AsyncRelayCommand(SaveAppThemeSettingsAsync);
        public IRelayCommand ResetAppThemeSettingsCommand => _resetAppThemeSettingsCommand ??= new RelayCommand(ResetAppThemeSettings);
        public IRelayCommand ApplyGlassThemePresetCommand => _applyGlassThemePresetCommand ??= new RelayCommand(ApplyGlassThemePreset);
        public IRelayCommand ApplyBorderlessThemePresetCommand => _applyBorderlessThemePresetCommand ??= new RelayCommand(ApplyBorderlessThemePreset);
        public IRelayCommand ApplyHighContrastThemePresetCommand => _applyHighContrastThemePresetCommand ??= new RelayCommand(ApplyHighContrastThemePreset);
        public IRelayCommand BrowseThemeBackgroundCommand => _browseThemeBackgroundCommand ??= new RelayCommand(BrowseThemeBackground);
        public IRelayCommand ClearThemeBackgroundCommand => _clearThemeBackgroundCommand ??= new RelayCommand(() => ThemeBackgroundImagePath = string.Empty);

        public string ThemeBackgroundColor
        {
            get => _appThemeSettings.BackgroundColor;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.BackgroundColor = newValue, nameof(ThemeBackgroundColor));
        }

        public string ThemeSurfaceColor
        {
            get => _appThemeSettings.SurfaceColor;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.SurfaceColor = newValue, nameof(ThemeSurfaceColor));
        }

        public string ThemeSurfaceAltColor
        {
            get => _appThemeSettings.SurfaceAltColor;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.SurfaceAltColor = newValue, nameof(ThemeSurfaceAltColor));
        }

        public string ThemeTextColor
        {
            get => _appThemeSettings.TextColor;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.TextColor = newValue, nameof(ThemeTextColor));
        }

        public string ThemeMutedTextColor
        {
            get => _appThemeSettings.MutedTextColor;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.MutedTextColor = newValue, nameof(ThemeMutedTextColor));
        }

        public string ThemeAccentColor
        {
            get => _appThemeSettings.AccentColor;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.AccentColor = newValue, nameof(ThemeAccentColor));
        }

        public string ThemeSuccessColor
        {
            get => _appThemeSettings.SuccessColor;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.SuccessColor = newValue, nameof(ThemeSuccessColor));
        }

        public string ThemeWarningColor
        {
            get => _appThemeSettings.WarningColor;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.WarningColor = newValue, nameof(ThemeWarningColor));
        }

        public string ThemeErrorColor
        {
            get => _appThemeSettings.ErrorColor;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.ErrorColor = newValue, nameof(ThemeErrorColor));
        }

        public string ThemeBackgroundImagePath
        {
            get => _appThemeSettings.BackgroundImagePath;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.BackgroundImagePath = newValue, nameof(ThemeBackgroundImagePath));
        }

        public double ThemeBackgroundOpacity
        {
            get => _appThemeSettings.BackgroundOpacity;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.BackgroundOpacity = newValue, nameof(ThemeBackgroundOpacity));
        }

        public double ThemeSurfaceOpacity
        {
            get => _appThemeSettings.SurfaceOpacity;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.SurfaceOpacity = newValue, nameof(ThemeSurfaceOpacity));
        }

        public double ThemeSurfaceAltOpacity
        {
            get => _appThemeSettings.SurfaceAltOpacity;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.SurfaceAltOpacity = newValue, nameof(ThemeSurfaceAltOpacity));
        }

        public double ThemeInputOpacity
        {
            get => _appThemeSettings.InputOpacity;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.InputOpacity = newValue, nameof(ThemeInputOpacity));
        }

        public double ThemeButtonOpacity
        {
            get => _appThemeSettings.ButtonOpacity;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.ButtonOpacity = newValue, nameof(ThemeButtonOpacity));
        }

        public bool ThemeBordersVisible
        {
            get => _appThemeSettings.BordersVisible;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.BordersVisible = newValue, nameof(ThemeBordersVisible));
        }

        public bool ThemeUseGlassSurfaces
        {
            get => _appThemeSettings.UseGlassSurfaces;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.UseGlassSurfaces = newValue, nameof(ThemeUseGlassSurfaces));
        }

        public double ThemeBorderOpacity
        {
            get => _appThemeSettings.BorderOpacity;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.BorderOpacity = newValue, nameof(ThemeBorderOpacity));
        }

        public double ThemeCardCornerRadius
        {
            get => _appThemeSettings.CardCornerRadius;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.CardCornerRadius = newValue, nameof(ThemeCardCornerRadius));
        }

        public double ThemePanelCornerRadius
        {
            get => _appThemeSettings.PanelCornerRadius;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.PanelCornerRadius = newValue, nameof(ThemePanelCornerRadius));
        }

        public double ThemeButtonCornerRadius
        {
            get => _appThemeSettings.ButtonCornerRadius;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.ButtonCornerRadius = newValue, nameof(ThemeButtonCornerRadius));
        }

        public double ThemeInputCornerRadius
        {
            get => _appThemeSettings.InputCornerRadius;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.InputCornerRadius = newValue, nameof(ThemeInputCornerRadius));
        }

        public double ThemeShadowBlurRadius
        {
            get => _appThemeSettings.ShadowBlurRadius;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.ShadowBlurRadius = newValue, nameof(ThemeShadowBlurRadius));
        }

        public double ThemeShadowDepth
        {
            get => _appThemeSettings.ShadowDepth;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.ShadowDepth = newValue, nameof(ThemeShadowDepth));
        }

        public double ThemeShadowOpacity
        {
            get => _appThemeSettings.ShadowOpacity;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.ShadowOpacity = newValue, nameof(ThemeShadowOpacity));
        }

        public double ThemePagePadding
        {
            get => _appThemeSettings.PagePadding;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.PagePadding = newValue, nameof(ThemePagePadding));
        }

        public double ThemeCardPadding
        {
            get => _appThemeSettings.CardPadding;
            set => UpdateThemeSetting(value, (settings, newValue) => settings.CardPadding = newValue, nameof(ThemeCardPadding));
        }

        private async Task InitializeThemeDesignerAsync(CancellationToken token = default)
        {
            try
            {
                _appThemeSettings = await _settingsService.GetAppThemeSettingsAsync(token).ConfigureAwait(false);
                _themeService.ApplyCustomTheme(_appThemeSettings);
                NotifyThemeDesignerPropertiesChanged();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load custom app theme settings.");
                _appThemeSettings = AppThemeSettings.CreateDefault(Theme);
                _themeService.ApplyCustomTheme(_appThemeSettings);
                NotifyThemeDesignerPropertiesChanged();
            }
        }

        private void ApplyThemeDesignerBaseTheme(string value)
        {
            if (_updatingThemeDesigner)
                return;

            _appThemeSettings.BaseTheme = value;
            _themeService.ApplyCustomTheme(_appThemeSettings);
            _ = SaveAppThemeSettingsAsync();
        }

        private async Task SaveAppThemeSettingsAsync(CancellationToken token = default)
        {
            try
            {
                _appThemeSettings.BaseTheme = Theme;
                _appThemeSettings.Normalize();
                await _settingsService.SaveAppThemeSettingsAsync(_appThemeSettings, token).ConfigureAwait(false);
                _themeService.ApplyCustomTheme(_appThemeSettings);
                NotifyThemeDesignerPropertiesChanged();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized to change app theme settings.");
                _dialogService.ShowInfo("You are not authorized to change theme settings.", "Unauthorized");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "Saving app theme settings was canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save app theme settings.");
                _dialogService.ShowInfo("Theme settings could not be saved. Please check color values and try again.", "Theme Settings");
            }
        }

        private void ResetAppThemeSettings()
        {
            _appThemeSettings = AppThemeSettings.CreateDefault(Theme);
            _themeService.ApplyCustomTheme(_appThemeSettings);
            NotifyThemeDesignerPropertiesChanged();
            _ = SaveAppThemeSettingsAsync();
        }

        private void ApplyGlassThemePreset()
        {
            _appThemeSettings.UseGlassSurfaces = true;
            _appThemeSettings.SurfaceOpacity = 0.68;
            _appThemeSettings.SurfaceAltOpacity = 0.55;
            _appThemeSettings.InputOpacity = 0.72;
            _appThemeSettings.ButtonOpacity = 0.72;
            _appThemeSettings.BordersVisible = true;
            _appThemeSettings.BorderOpacity = 0.42;
            _appThemeSettings.CardCornerRadius = 14;
            _appThemeSettings.PanelCornerRadius = 12;
            _appThemeSettings.ButtonCornerRadius = 12;
            _appThemeSettings.InputCornerRadius = 10;
            _appThemeSettings.ShadowBlurRadius = 18;
            _appThemeSettings.ShadowDepth = 4;
            _appThemeSettings.ShadowOpacity = 0.24;
            _themeService.ApplyCustomTheme(_appThemeSettings);
            NotifyThemeDesignerPropertiesChanged();
        }

        private void ApplyBorderlessThemePreset()
        {
            _appThemeSettings.UseGlassSurfaces = false;
            _appThemeSettings.BordersVisible = false;
            _appThemeSettings.BorderOpacity = 0;
            _appThemeSettings.CardCornerRadius = 0;
            _appThemeSettings.PanelCornerRadius = 0;
            _appThemeSettings.ButtonCornerRadius = 0;
            _appThemeSettings.InputCornerRadius = 0;
            _appThemeSettings.ShadowBlurRadius = 0;
            _appThemeSettings.ShadowDepth = 0;
            _appThemeSettings.ShadowOpacity = 0;
            _themeService.ApplyCustomTheme(_appThemeSettings);
            NotifyThemeDesignerPropertiesChanged();
        }

        private void ApplyHighContrastThemePreset()
        {
            _appThemeSettings = AppThemeSettings.CreateDefault("Dark");
            _appThemeSettings.BackgroundColor = "#FF000000";
            _appThemeSettings.SurfaceColor = "#FF111111";
            _appThemeSettings.SurfaceAltColor = "#FF1F1F1F";
            _appThemeSettings.TextColor = "#FFFFFFFF";
            _appThemeSettings.MutedTextColor = "#FFE5E7EB";
            _appThemeSettings.AccentColor = "#FFFFFF00";
            _appThemeSettings.BordersVisible = true;
            _appThemeSettings.BorderOpacity = 1;
            _updatingThemeDesigner = true;
            Theme = "Dark";
            _updatingThemeDesigner = false;
            _themeService.ApplyCustomTheme(_appThemeSettings);
            NotifyThemeDesignerPropertiesChanged();
            _ = SetThemeAsync(Theme);
        }

        private void BrowseThemeBackground()
        {
            var path = _fileDialog.OpenFile("Image Files|*.png;*.jpg;*.jpeg;*.bmp");
            if (!string.IsNullOrWhiteSpace(path))
                ThemeBackgroundImagePath = path;
        }

        private void UpdateThemeSetting(string value, Action<AppThemeSettings, string> update, string propertyName)
        {
            update(_appThemeSettings, value ?? string.Empty);
            OnPropertyChanged(propertyName);
            ApplyThemeDesignerPreview();
        }

        private void UpdateThemeSetting(double value, Action<AppThemeSettings, double> update, string propertyName)
        {
            update(_appThemeSettings, value);
            OnPropertyChanged(propertyName);
            ApplyThemeDesignerPreview();
        }

        private void UpdateThemeSetting(bool value, Action<AppThemeSettings, bool> update, string propertyName)
        {
            update(_appThemeSettings, value);
            OnPropertyChanged(propertyName);
            ApplyThemeDesignerPreview();
        }

        private void ApplyThemeDesignerPreview()
        {
            try
            {
                _appThemeSettings.BaseTheme = Theme;
                _appThemeSettings.Normalize();
                _themeService.ApplyCustomTheme(_appThemeSettings);
                NotifyThemeDesignerPropertiesChanged();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to preview custom app theme settings.");
            }
        }

        private void NotifyThemeDesignerPropertiesChanged()
        {
            OnPropertyChanged(nameof(ThemeBackgroundColor));
            OnPropertyChanged(nameof(ThemeSurfaceColor));
            OnPropertyChanged(nameof(ThemeSurfaceAltColor));
            OnPropertyChanged(nameof(ThemeTextColor));
            OnPropertyChanged(nameof(ThemeMutedTextColor));
            OnPropertyChanged(nameof(ThemeAccentColor));
            OnPropertyChanged(nameof(ThemeSuccessColor));
            OnPropertyChanged(nameof(ThemeWarningColor));
            OnPropertyChanged(nameof(ThemeErrorColor));
            OnPropertyChanged(nameof(ThemeBackgroundImagePath));
            OnPropertyChanged(nameof(ThemeBackgroundOpacity));
            OnPropertyChanged(nameof(ThemeSurfaceOpacity));
            OnPropertyChanged(nameof(ThemeSurfaceAltOpacity));
            OnPropertyChanged(nameof(ThemeInputOpacity));
            OnPropertyChanged(nameof(ThemeButtonOpacity));
            OnPropertyChanged(nameof(ThemeBordersVisible));
            OnPropertyChanged(nameof(ThemeUseGlassSurfaces));
            OnPropertyChanged(nameof(ThemeBorderOpacity));
            OnPropertyChanged(nameof(ThemeCardCornerRadius));
            OnPropertyChanged(nameof(ThemePanelCornerRadius));
            OnPropertyChanged(nameof(ThemeButtonCornerRadius));
            OnPropertyChanged(nameof(ThemeInputCornerRadius));
            OnPropertyChanged(nameof(ThemeShadowBlurRadius));
            OnPropertyChanged(nameof(ThemeShadowDepth));
            OnPropertyChanged(nameof(ThemeShadowOpacity));
            OnPropertyChanged(nameof(ThemePagePadding));
            OnPropertyChanged(nameof(ThemeCardPadding));
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.ObjectModel;
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

            ThemeOptions = new ObservableCollection<string> { "Light", "Dark" };
            BackgroundStretchOptions = new ObservableCollection<string> { "UniformToFill", "Uniform", "Fill", "None" };
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            ResetCommand = new RelayCommand(Reset);
            BrowseBackgroundCommand = new RelayCommand(BrowseBackground);
            ClearBackgroundCommand = new RelayCommand(() => BackgroundImagePath = string.Empty);
            GlassPresetCommand = new RelayCommand(ApplyGlassPreset);
            BorderlessPresetCommand = new RelayCommand(ApplyBorderlessPreset);
            HighContrastPresetCommand = new RelayCommand(ApplyHighContrastPreset);
        }

        public ObservableCollection<string> ThemeOptions { get; }
        public ObservableCollection<string> BackgroundStretchOptions { get; }
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

        public string NavigationColor
        {
            get => _settings.NavigationColor;
            set => SetString(value, (settings, newValue) => settings.NavigationColor = newValue, nameof(NavigationColor));
        }

        public string InputColor
        {
            get => _settings.InputColor;
            set => SetString(value, (settings, newValue) => settings.InputColor = newValue, nameof(InputColor));
        }

        public string ButtonColor
        {
            get => _settings.ButtonColor;
            set => SetString(value, (settings, newValue) => settings.ButtonColor = newValue, nameof(ButtonColor));
        }

        public string BorderColor
        {
            get => _settings.BorderColor;
            set => SetString(value, (settings, newValue) => settings.BorderColor = newValue, nameof(BorderColor));
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

        public string ShadowColor
        {
            get => _settings.ShadowColor;
            set => SetString(value, (settings, newValue) => settings.ShadowColor = newValue, nameof(ShadowColor));
        }

        public string BackgroundImagePath
        {
            get => _settings.BackgroundImagePath;
            set => SetString(value, (settings, newValue) => settings.BackgroundImagePath = newValue, nameof(BackgroundImagePath));
        }

        public string BackgroundImageStretch
        {
            get => _settings.BackgroundImageStretch;
            set => SetString(value, (settings, newValue) => settings.BackgroundImageStretch = newValue, nameof(BackgroundImageStretch));
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

        public double SurfaceAltOpacity
        {
            get => _settings.SurfaceAltOpacity;
            set => SetDouble(value, (settings, newValue) => settings.SurfaceAltOpacity = newValue, nameof(SurfaceAltOpacity));
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

        public double NavigationOpacity
        {
            get => _settings.NavigationOpacity;
            set => SetDouble(value, (settings, newValue) => settings.NavigationOpacity = newValue, nameof(NavigationOpacity));
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

        public double PanelCornerRadius
        {
            get => _settings.PanelCornerRadius;
            set => SetDouble(value, (settings, newValue) => settings.PanelCornerRadius = newValue, nameof(PanelCornerRadius));
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

        public double ShadowDirection
        {
            get => _settings.ShadowDirection;
            set => SetDouble(value, (settings, newValue) => settings.ShadowDirection = newValue, nameof(ShadowDirection));
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

        public double FontScale
        {
            get => _settings.FontScale;
            set => SetDouble(value, (settings, newValue) => settings.FontScale = newValue, nameof(FontScale));
        }

        public double ControlHeight
        {
            get => _settings.ControlHeight;
            set => SetDouble(value, (settings, newValue) => settings.ControlHeight = newValue, nameof(ControlHeight));
        }

        public double DataGridRowHeight
        {
            get => _settings.DataGridRowHeight;
            set => SetDouble(value, (settings, newValue) => settings.DataGridRowHeight = newValue, nameof(DataGridRowHeight));
        }

        public double DataGridHeaderHeight
        {
            get => _settings.DataGridHeaderHeight;
            set => SetDouble(value, (settings, newValue) => settings.DataGridHeaderHeight = newValue, nameof(DataGridHeaderHeight));
        }

        public double InteractionIntensity
        {
            get => _settings.InteractionIntensity;
            set => SetDouble(value, (settings, newValue) => settings.InteractionIntensity = newValue, nameof(InteractionIntensity));
        }

        public double FocusRingOpacity
        {
            get => _settings.FocusRingOpacity;
            set => SetDouble(value, (settings, newValue) => settings.FocusRingOpacity = newValue, nameof(FocusRingOpacity));
        }

        public double GridLineOpacity
        {
            get => _settings.GridLineOpacity;
            set => SetDouble(value, (settings, newValue) => settings.GridLineOpacity = newValue, nameof(GridLineOpacity));
        }

        public double MotionIntensity
        {
            get => _settings.MotionIntensity;
            set => SetDouble(value, (settings, newValue) => settings.MotionIntensity = newValue, nameof(MotionIntensity));
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
            SurfaceAltOpacity = 0.58;
            InputOpacity = 0.72;
            ButtonOpacity = 0.72;
            NavigationOpacity = 0.76;
            NavigationColor = SurfaceAltColor;
            InputColor = SurfaceColor;
            ButtonColor = SurfaceAltColor;
            BorderColor = AccentColor;
            ShadowColor = "#88000000";
            BordersVisible = true;
            BorderOpacity = 0.42;
            CardCornerRadius = 14;
            PanelCornerRadius = 12;
            ButtonCornerRadius = 12;
            InputCornerRadius = 10;
            ShadowBlurRadius = 18;
            ShadowDepth = 4;
            ShadowOpacity = 0.24;
            ShadowDirection = 270;
            FontScale = 1.02;
            ControlHeight = 30;
            DataGridRowHeight = 34;
            DataGridHeaderHeight = 34;
            InteractionIntensity = 1.1;
            FocusRingOpacity = 0.48;
            GridLineOpacity = 0.24;
            MotionIntensity = 1.1;
            BackgroundImageStretch = "UniformToFill";
            Status = "Glass preset previewed. Save to keep it.";
        }

        private void ApplyBorderlessPreset()
        {
            UseGlassSurfaces = false;
            BordersVisible = false;
            BorderOpacity = 0;
            CardCornerRadius = 0;
            PanelCornerRadius = 0;
            ButtonCornerRadius = 0;
            InputCornerRadius = 0;
            ShadowBlurRadius = 0;
            ShadowDepth = 0;
            ShadowOpacity = 0;
            ShadowDirection = 270;
            NavigationOpacity = 0.92;
            NavigationColor = SurfaceColor;
            InputColor = SurfaceColor;
            ButtonColor = SurfaceAltColor;
            BorderColor = SurfaceColor;
            ShadowColor = "#00000000";
            ControlHeight = 26;
            DataGridRowHeight = 28;
            DataGridHeaderHeight = 28;
            InteractionIntensity = 0.55;
            FocusRingOpacity = 0.38;
            GridLineOpacity = 0;
            MotionIntensity = 0.75;
            Status = "Borderless preset previewed. Save to keep it.";
        }

        private void ApplyHighContrastPreset()
        {
            _settings = AppThemeSettings.CreateDefault("Dark");
            _settings.BackgroundColor = "#FF000000";
            _settings.SurfaceColor = "#FF111111";
            _settings.SurfaceAltColor = "#FF1F1F1F";
            _settings.NavigationColor = "#FF000000";
            _settings.InputColor = "#FF000000";
            _settings.ButtonColor = "#FF1F1F1F";
            _settings.BorderColor = "#FFFFFF00";
            _settings.TextColor = "#FFFFFFFF";
            _settings.MutedTextColor = "#FFE5E7EB";
            _settings.AccentColor = "#FFFFFF00";
            _settings.SuccessColor = "#FF00FF66";
            _settings.WarningColor = "#FFFFFF00";
            _settings.ErrorColor = "#FFFF4D4D";
            _settings.ShadowColor = "#FFFFFFFF";
            _settings.NavigationOpacity = 1;
            _settings.ControlHeight = 32;
            _settings.DataGridRowHeight = 36;
            _settings.DataGridHeaderHeight = 36;
            _settings.BordersVisible = true;
            _settings.BorderOpacity = 1;
            _settings.ShadowDirection = 270;
            _settings.InteractionIntensity = 1.45;
            _settings.FocusRingOpacity = 1;
            _settings.GridLineOpacity = 0.85;
            _settings.MotionIntensity = 0.4;
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
            OnPropertyChanged(nameof(NavigationColor));
            OnPropertyChanged(nameof(InputColor));
            OnPropertyChanged(nameof(ButtonColor));
            OnPropertyChanged(nameof(BorderColor));
            OnPropertyChanged(nameof(TextColor));
            OnPropertyChanged(nameof(MutedTextColor));
            OnPropertyChanged(nameof(AccentColor));
            OnPropertyChanged(nameof(SuccessColor));
            OnPropertyChanged(nameof(WarningColor));
            OnPropertyChanged(nameof(ErrorColor));
            OnPropertyChanged(nameof(ShadowColor));
            OnPropertyChanged(nameof(BackgroundImagePath));
            OnPropertyChanged(nameof(BackgroundImageStretch));
            OnPropertyChanged(nameof(BackgroundOpacity));
            OnPropertyChanged(nameof(SurfaceOpacity));
            OnPropertyChanged(nameof(SurfaceAltOpacity));
            OnPropertyChanged(nameof(InputOpacity));
            OnPropertyChanged(nameof(ButtonOpacity));
            OnPropertyChanged(nameof(NavigationOpacity));
            OnPropertyChanged(nameof(BordersVisible));
            OnPropertyChanged(nameof(UseGlassSurfaces));
            OnPropertyChanged(nameof(BorderOpacity));
            OnPropertyChanged(nameof(CardCornerRadius));
            OnPropertyChanged(nameof(PanelCornerRadius));
            OnPropertyChanged(nameof(ButtonCornerRadius));
            OnPropertyChanged(nameof(InputCornerRadius));
            OnPropertyChanged(nameof(ShadowBlurRadius));
            OnPropertyChanged(nameof(ShadowDepth));
            OnPropertyChanged(nameof(ShadowOpacity));
            OnPropertyChanged(nameof(ShadowDirection));
            OnPropertyChanged(nameof(PagePadding));
            OnPropertyChanged(nameof(CardPadding));
            OnPropertyChanged(nameof(FontScale));
            OnPropertyChanged(nameof(ControlHeight));
            OnPropertyChanged(nameof(DataGridRowHeight));
            OnPropertyChanged(nameof(DataGridHeaderHeight));
            OnPropertyChanged(nameof(InteractionIntensity));
            OnPropertyChanged(nameof(FocusRingOpacity));
            OnPropertyChanged(nameof(GridLineOpacity));
            OnPropertyChanged(nameof(MotionIntensity));
        }
    }
}

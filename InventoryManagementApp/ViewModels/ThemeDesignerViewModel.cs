using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementApp.ViewModels
{
    public class ThemeDesignerViewModel : ObservableObject
    {
        private static readonly JsonSerializerOptions ThemeProfileJsonOptions = new() { WriteIndented = true };
        private const string ThemeProfileDialogFilter = "Theme Profile (*.json)|*.json|All Files (*.*)|*.*";

        private readonly ISettingsService _settingsService;
        private readonly IThemeService _themeService;
        private readonly IFileDialogService _fileDialogService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ThemeDesignerViewModel> _logger;
        private CancellationTokenSource? _previewDebounceCts;
        private AppThemeSettings _settings = AppThemeSettings.CreateDefault();
        private string _status = "Theme designer ready.";
        private const int PreviewDebounceMilliseconds = 140;

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

            ThemeOptions = new ObservableCollection<string> { "Light", "Dark", "VS Code", "VS Code Light" };
            BackgroundStretchOptions = new ObservableCollection<string> { "UniformToFill", "Uniform", "Fill", "None" };
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            ResetCommand = new RelayCommand(Reset);
            BrowseBackgroundCommand = new RelayCommand(BrowseBackground);
            ClearBackgroundCommand = new RelayCommand(() => BackgroundImagePath = string.Empty);
            ImportThemeProfileCommand = new RelayCommand(ImportThemeProfile);
            ExportThemeProfileCommand = new RelayCommand(ExportThemeProfile);
            GlassPresetCommand = new RelayCommand(ApplyGlassPreset);
            TransparentCanvasPresetCommand = new RelayCommand(ApplyTransparentCanvasPreset);
            BorderlessPresetCommand = new RelayCommand(ApplyBorderlessPreset);
            DeepShadowPresetCommand = new RelayCommand(ApplyDeepShadowPreset);
            HighContrastPresetCommand = new RelayCommand(ApplyHighContrastPreset);
        }

        public ObservableCollection<string> ThemeOptions { get; }
        public ObservableCollection<string> BackgroundStretchOptions { get; }
        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand ResetCommand { get; }
        public IRelayCommand BrowseBackgroundCommand { get; }
        public IRelayCommand ClearBackgroundCommand { get; }
        public IRelayCommand ImportThemeProfileCommand { get; }
        public IRelayCommand ExportThemeProfileCommand { get; }
        public IRelayCommand GlassPresetCommand { get; }
        public IRelayCommand TransparentCanvasPresetCommand { get; }
        public IRelayCommand BorderlessPresetCommand { get; }
        public IRelayCommand DeepShadowPresetCommand { get; }
        public IRelayCommand HighContrastPresetCommand { get; }

        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

        public string BaseTheme
        {
            get => _settings.BaseTheme;
            set
            {
                var theme = (value?.IndexOf("VS Code", StringComparison.OrdinalIgnoreCase) >= 0 ||
                             value?.IndexOf("VSCode", StringComparison.OrdinalIgnoreCase) >= 0 ||
                             value?.IndexOf("Visual Studio Code", StringComparison.OrdinalIgnoreCase) >= 0) &&
                            value?.IndexOf("Light", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "VS Code Light"
                    : value?.IndexOf("VS Code", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      value?.IndexOf("VSCode", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      value?.IndexOf("Visual Studio Code", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "VS Code"
                    : string.Equals(value, "Dark", StringComparison.OrdinalIgnoreCase) ? "Dark" : "Light";
                if (string.Equals(_settings.BaseTheme, theme, StringComparison.OrdinalIgnoreCase))
                    return;

                _settings = AppThemeSettings.CreateDefault(theme);
                Preview(immediate: true);
                NotifyAllThemePropertiesChanged();
                Status = $"{theme} theme previewed. Save to keep it.";
            }
        }

        public string BackgroundColor
        {
            get => _settings.BackgroundColor;
            set => SetString(value, (settings, newValue) => settings.BackgroundColor = newValue, nameof(BackgroundColor));
        }

        public string BackgroundOverlayColor
        {
            get => _settings.BackgroundOverlayColor;
            set => SetString(value, (settings, newValue) => settings.BackgroundOverlayColor = newValue, nameof(BackgroundOverlayColor));
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

        public string HoverColor
        {
            get => _settings.HoverColor;
            set => SetString(value, (settings, newValue) => settings.HoverColor = newValue, nameof(HoverColor));
        }

        public string HoverTextColor
        {
            get => _settings.HoverTextColor;
            set => SetString(value, (settings, newValue) => settings.HoverTextColor = newValue, nameof(HoverTextColor));
        }

        public string SelectedColor
        {
            get => _settings.SelectedColor;
            set => SetString(value, (settings, newValue) => settings.SelectedColor = newValue, nameof(SelectedColor));
        }

        public string SelectedTextColor
        {
            get => _settings.SelectedTextColor;
            set => SetString(value, (settings, newValue) => settings.SelectedTextColor = newValue, nameof(SelectedTextColor));
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

        public string FontFamily
        {
            get => _settings.FontFamily;
            set => SetString(value, (settings, newValue) => settings.FontFamily = newValue, nameof(FontFamily));
        }

        public double BackgroundOpacity
        {
            get => _settings.BackgroundOpacity;
            set => SetDouble(value, (settings, newValue) => settings.BackgroundOpacity = newValue, nameof(BackgroundOpacity));
        }

        public double BackgroundOverlayOpacity
        {
            get => _settings.BackgroundOverlayOpacity;
            set => SetDouble(value, (settings, newValue) => settings.BackgroundOverlayOpacity = newValue, nameof(BackgroundOverlayOpacity));
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

        public double HeaderOpacity
        {
            get => _settings.HeaderOpacity;
            set => SetDouble(value, (settings, newValue) => settings.HeaderOpacity = newValue, nameof(HeaderOpacity));
        }

        public double MenuOpacity
        {
            get => _settings.MenuOpacity;
            set => SetDouble(value, (settings, newValue) => settings.MenuOpacity = newValue, nameof(MenuOpacity));
        }

        public double MenuDropDownOpacity
        {
            get => _settings.MenuDropDownOpacity;
            set => SetDouble(value, (settings, newValue) => settings.MenuDropDownOpacity = newValue, nameof(MenuDropDownOpacity));
        }

        public double FooterOpacity
        {
            get => _settings.FooterOpacity;
            set => SetDouble(value, (settings, newValue) => settings.FooterOpacity = newValue, nameof(FooterOpacity));
        }

        public double DialogOpacity
        {
            get => _settings.DialogOpacity;
            set => SetDouble(value, (settings, newValue) => settings.DialogOpacity = newValue, nameof(DialogOpacity));
        }

        public double DisabledOpacity
        {
            get => _settings.DisabledOpacity;
            set => SetDouble(value, (settings, newValue) => settings.DisabledOpacity = newValue, nameof(DisabledOpacity));
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

        public bool EnableSurfaceShadows
        {
            get => _settings.EnableSurfaceShadows;
            set => SetBool(value, (settings, newValue) => settings.EnableSurfaceShadows = newValue, nameof(EnableSurfaceShadows));
        }

        public bool EnableControlShadows
        {
            get => _settings.EnableControlShadows;
            set => SetBool(value, (settings, newValue) => settings.EnableControlShadows = newValue, nameof(EnableControlShadows));
        }

        public double BorderOpacity
        {
            get => _settings.BorderOpacity;
            set => SetDouble(value, (settings, newValue) => settings.BorderOpacity = newValue, nameof(BorderOpacity));
        }

        public double BorderThickness
        {
            get => _settings.BorderThickness;
            set => SetDouble(value, (settings, newValue) => settings.BorderThickness = newValue, nameof(BorderThickness));
        }

        public double ControlBorderThickness
        {
            get => _settings.ControlBorderThickness;
            set => SetDouble(value, (settings, newValue) => settings.ControlBorderThickness = newValue, nameof(ControlBorderThickness));
        }

        public double DividerOpacity
        {
            get => _settings.DividerOpacity;
            set => SetDouble(value, (settings, newValue) => settings.DividerOpacity = newValue, nameof(DividerOpacity));
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

        public double SurfaceShadowScale
        {
            get => _settings.SurfaceShadowScale;
            set => SetDouble(value, (settings, newValue) => settings.SurfaceShadowScale = newValue, nameof(SurfaceShadowScale));
        }

        public double ControlShadowScale
        {
            get => _settings.ControlShadowScale;
            set => SetDouble(value, (settings, newValue) => settings.ControlShadowScale = newValue, nameof(ControlShadowScale));
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

        public double HeadingFontScale
        {
            get => _settings.HeadingFontScale;
            set => SetDouble(value, (settings, newValue) => settings.HeadingFontScale = newValue, nameof(HeadingFontScale));
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
                Preview(immediate: true);
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
                Preview(immediate: true);
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
            Preview(immediate: true);
            NotifyAllThemePropertiesChanged();
            Status = "Theme reset to defaults. Save to keep it.";
        }

        private void BrowseBackground()
        {
            var path = _fileDialogService.OpenFile("Image Files|*.png;*.jpg;*.jpeg;*.bmp");
            if (!string.IsNullOrWhiteSpace(path))
                BackgroundImagePath = path;
        }

        private void ExportThemeProfile()
        {
            var path = _fileDialogService.SaveFile(ThemeProfileDialogFilter);
            if (string.IsNullOrWhiteSpace(path))
                return;

            try
            {
                _settings.Normalize();
                File.WriteAllText(path, JsonSerializer.Serialize(_settings, ThemeProfileJsonOptions));
                NotifyAllThemePropertiesChanged();
                Status = "Theme profile exported.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export app theme profile.");
                _dialogService.ShowInfo("Theme profile could not be exported. Choose a writable location and try again.", "Theme Profile");
                Status = "Theme profile export failed.";
            }
        }

        private void ImportThemeProfile()
        {
            var path = _fileDialogService.OpenFile(ThemeProfileDialogFilter);
            if (string.IsNullOrWhiteSpace(path))
                return;

            try
            {
                var imported = JsonSerializer.Deserialize<AppThemeSettings>(File.ReadAllText(path), ThemeProfileJsonOptions)
                    ?? throw new InvalidDataException("Theme profile did not contain app theme settings.");

                imported.Normalize();
                _settings = imported;
                Preview(immediate: true);
                NotifyAllThemePropertiesChanged();
                Status = "Theme profile imported for preview. Save to keep it.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import app theme profile.");
                _dialogService.ShowInfo("Theme profile could not be imported. Choose a valid theme JSON file and try again.", "Theme Profile");
                Status = "Theme profile import failed.";
            }
        }

        private void ApplyGlassPreset()
        {
            UseGlassSurfaces = true;
            SurfaceOpacity = 0.68;
            SurfaceAltOpacity = 0.58;
            InputOpacity = 0.72;
            ButtonOpacity = 0.72;
            NavigationOpacity = 0.76;
            HeaderOpacity = 0.68;
            MenuOpacity = 0.62;
            MenuDropDownOpacity = 0.88;
            FooterOpacity = 0.58;
            DialogOpacity = 0.82;
            DisabledOpacity = 0.48;
            BackgroundOverlayColor = "#FFFFFFFF";
            BackgroundOverlayOpacity = 0.16;
            NavigationColor = SurfaceAltColor;
            InputColor = SurfaceColor;
            ButtonColor = SurfaceAltColor;
            BorderColor = AccentColor;
            HoverColor = "#4460A5FA";
            HoverTextColor = TextColor;
            SelectedColor = AccentColor;
            SelectedTextColor = "#FFFFFFFF";
            ShadowColor = "#88000000";
            FontFamily = "Segoe UI";
            BordersVisible = true;
            BorderOpacity = 0.42;
            BorderThickness = 1;
            ControlBorderThickness = 1;
            DividerOpacity = 0.6;
            EnableSurfaceShadows = true;
            EnableControlShadows = true;
            CardCornerRadius = 14;
            PanelCornerRadius = 12;
            ButtonCornerRadius = 12;
            InputCornerRadius = 10;
            ShadowBlurRadius = 18;
            ShadowDepth = 4;
            ShadowOpacity = 0.24;
            ShadowDirection = 270;
            SurfaceShadowScale = 1.2;
            ControlShadowScale = 0.85;
            FontScale = 1.02;
            HeadingFontScale = 1.06;
            ControlHeight = 30;
            DataGridRowHeight = 34;
            DataGridHeaderHeight = 34;
            InteractionIntensity = 1.1;
            FocusRingOpacity = 0.48;
            GridLineOpacity = 0.24;
            MotionIntensity = 1.1;
            BackgroundImageStretch = "UniformToFill";
            Preview(immediate: true);
            NotifyAllThemePropertiesChanged();
            Status = "Glass preset previewed. Save to keep it.";
        }

        private void ApplyTransparentCanvasPreset()
        {
            UseGlassSurfaces = true;
            BackgroundOpacity = 1;
            BackgroundOverlayOpacity = 0;
            SurfaceOpacity = 0.18;
            SurfaceAltOpacity = 0.12;
            InputOpacity = 0.24;
            ButtonOpacity = 0.22;
            NavigationOpacity = 0.18;
            HeaderOpacity = 0.16;
            MenuOpacity = 0.16;
            MenuDropDownOpacity = 0.82;
            FooterOpacity = 0.14;
            DialogOpacity = 0.34;
            DisabledOpacity = 0.36;
            BordersVisible = false;
            BorderOpacity = 0;
            BorderThickness = 0;
            ControlBorderThickness = 0;
            DividerOpacity = 0.08;
            EnableSurfaceShadows = false;
            EnableControlShadows = false;
            ShadowBlurRadius = 0;
            ShadowDepth = 0;
            ShadowOpacity = 0;
            SurfaceShadowScale = 0;
            ControlShadowScale = 0;
            CardCornerRadius = 10;
            PanelCornerRadius = 10;
            ButtonCornerRadius = 10;
            InputCornerRadius = 8;
            PagePadding = 8;
            CardPadding = 10;
            InteractionIntensity = 0.75;
            FocusRingOpacity = 0.5;
            GridLineOpacity = 0.06;
            MotionIntensity = 0.9;
            BackgroundImageStretch = "UniformToFill";
            Preview(immediate: true);
            NotifyAllThemePropertiesChanged();
            Status = "Transparent canvas preset previewed. Save to keep it.";
        }

        private void ApplyBorderlessPreset()
        {
            UseGlassSurfaces = false;
            BordersVisible = false;
            BorderOpacity = 0;
            BorderThickness = 0;
            ControlBorderThickness = 0;
            DividerOpacity = 0;
            CardCornerRadius = 0;
            PanelCornerRadius = 0;
            ButtonCornerRadius = 0;
            InputCornerRadius = 0;
            ShadowBlurRadius = 0;
            ShadowDepth = 0;
            ShadowOpacity = 0;
            ShadowDirection = 270;
            SurfaceShadowScale = 0;
            ControlShadowScale = 0;
            HeaderOpacity = 0.96;
            MenuOpacity = 0.92;
            MenuDropDownOpacity = 0.98;
            FooterOpacity = 0.9;
            DialogOpacity = 0.98;
            DisabledOpacity = 0.42;
            BackgroundOverlayOpacity = 0;
            EnableSurfaceShadows = false;
            EnableControlShadows = false;
            NavigationOpacity = 0.92;
            NavigationColor = SurfaceColor;
            InputColor = SurfaceColor;
            ButtonColor = SurfaceAltColor;
            BorderColor = SurfaceColor;
            ShadowColor = "#00000000";
            FontFamily = "Segoe UI";
            HeadingFontScale = 0.96;
            ControlHeight = 26;
            DataGridRowHeight = 28;
            DataGridHeaderHeight = 28;
            InteractionIntensity = 0.55;
            FocusRingOpacity = 0.38;
            GridLineOpacity = 0;
            MotionIntensity = 0.75;
            Preview(immediate: true);
            NotifyAllThemePropertiesChanged();
            Status = "Borderless preset previewed. Save to keep it.";
        }

        private void ApplyDeepShadowPreset()
        {
            UseGlassSurfaces = false;
            SurfaceOpacity = 0.96;
            SurfaceAltOpacity = 0.92;
            InputOpacity = 0.96;
            ButtonOpacity = 0.94;
            NavigationOpacity = 0.96;
            HeaderOpacity = 0.96;
            MenuOpacity = 0.94;
            MenuDropDownOpacity = 0.98;
            FooterOpacity = 0.92;
            DialogOpacity = 0.98;
            BordersVisible = true;
            BorderOpacity = 0.55;
            BorderThickness = 1;
            ControlBorderThickness = 1;
            DividerOpacity = 0.5;
            EnableSurfaceShadows = true;
            EnableControlShadows = true;
            ShadowBlurRadius = 36;
            ShadowDepth = 12;
            ShadowOpacity = 0.45;
            ShadowDirection = 270;
            SurfaceShadowScale = 2.2;
            ControlShadowScale = 1.6;
            CardCornerRadius = 12;
            PanelCornerRadius = 10;
            ButtonCornerRadius = 8;
            InputCornerRadius = 6;
            PagePadding = 10;
            CardPadding = 12;
            InteractionIntensity = 1.25;
            FocusRingOpacity = 0.65;
            GridLineOpacity = 0.32;
            MotionIntensity = 1.15;
            Preview(immediate: true);
            NotifyAllThemePropertiesChanged();
            Status = "Deep shadow preset previewed. Save to keep it.";
        }

        private void ApplyHighContrastPreset()
        {
            _settings = AppThemeSettings.CreateDefault("Dark");
            _settings.BackgroundColor = "#FF000000";
            _settings.BackgroundOverlayColor = "#FF000000";
            _settings.BackgroundOverlayOpacity = 0.2;
            _settings.SurfaceColor = "#FF111111";
            _settings.SurfaceAltColor = "#FF1F1F1F";
            _settings.NavigationColor = "#FF000000";
            _settings.InputColor = "#FF000000";
            _settings.ButtonColor = "#FF1F1F1F";
            _settings.BorderColor = "#FFFFFF00";
            _settings.TextColor = "#FFFFFFFF";
            _settings.MutedTextColor = "#FFE5E7EB";
            _settings.AccentColor = "#FFFFFF00";
            _settings.HoverColor = "#FF333300";
            _settings.HoverTextColor = "#FFFFFFFF";
            _settings.SelectedColor = "#FFFFFF00";
            _settings.SelectedTextColor = "#FF000000";
            _settings.SuccessColor = "#FF00FF66";
            _settings.WarningColor = "#FFFFFF00";
            _settings.ErrorColor = "#FFFF4D4D";
            _settings.ShadowColor = "#FFFFFFFF";
            _settings.FontFamily = "Segoe UI";
            _settings.NavigationOpacity = 1;
            _settings.HeaderOpacity = 1;
            _settings.MenuOpacity = 1;
            _settings.MenuDropDownOpacity = 1;
            _settings.FooterOpacity = 1;
            _settings.DialogOpacity = 1;
            _settings.DisabledOpacity = 0.72;
            _settings.FontScale = 1.08;
            _settings.HeadingFontScale = 1.12;
            _settings.ControlHeight = 32;
            _settings.DataGridRowHeight = 36;
            _settings.DataGridHeaderHeight = 36;
            _settings.BordersVisible = true;
            _settings.BorderOpacity = 1;
            _settings.BorderThickness = 2;
            _settings.ControlBorderThickness = 2;
            _settings.DividerOpacity = 1;
            _settings.EnableSurfaceShadows = false;
            _settings.EnableControlShadows = false;
            _settings.SurfaceShadowScale = 0;
            _settings.ControlShadowScale = 0;
            _settings.ShadowDirection = 270;
            _settings.InteractionIntensity = 1.45;
            _settings.FocusRingOpacity = 1;
            _settings.GridLineOpacity = 0.85;
            _settings.MotionIntensity = 0.4;
            Preview(immediate: true);
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

        private void Preview(bool immediate = false)
        {
            _previewDebounceCts?.Cancel();
            _previewDebounceCts = null;

            if (immediate)
            {
                ApplyPreview();
                return;
            }

            var cts = new CancellationTokenSource();
            _previewDebounceCts = cts;
            _ = PreviewAfterDelayAsync(cts);
        }

        private async Task PreviewAfterDelayAsync(CancellationTokenSource cts)
        {
            try
            {
                await Task.Delay(PreviewDebounceMilliseconds, cts.Token).ConfigureAwait(false);
                if (!cts.IsCancellationRequested)
                    ApplyPreview();
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (ReferenceEquals(_previewDebounceCts, cts))
                    _previewDebounceCts = null;

                cts.Dispose();
            }
        }

        private void ApplyPreview()
        {
            try
            {
                _settings.Normalize();
                _themeService.ApplyCustomTheme(_settings);
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
            OnPropertyChanged(nameof(BackgroundOverlayColor));
            OnPropertyChanged(nameof(SurfaceColor));
            OnPropertyChanged(nameof(SurfaceAltColor));
            OnPropertyChanged(nameof(NavigationColor));
            OnPropertyChanged(nameof(InputColor));
            OnPropertyChanged(nameof(ButtonColor));
            OnPropertyChanged(nameof(BorderColor));
            OnPropertyChanged(nameof(TextColor));
            OnPropertyChanged(nameof(MutedTextColor));
            OnPropertyChanged(nameof(AccentColor));
            OnPropertyChanged(nameof(HoverColor));
            OnPropertyChanged(nameof(HoverTextColor));
            OnPropertyChanged(nameof(SelectedColor));
            OnPropertyChanged(nameof(SelectedTextColor));
            OnPropertyChanged(nameof(SuccessColor));
            OnPropertyChanged(nameof(WarningColor));
            OnPropertyChanged(nameof(ErrorColor));
            OnPropertyChanged(nameof(ShadowColor));
            OnPropertyChanged(nameof(BackgroundImagePath));
            OnPropertyChanged(nameof(BackgroundImageStretch));
            OnPropertyChanged(nameof(FontFamily));
            OnPropertyChanged(nameof(BackgroundOpacity));
            OnPropertyChanged(nameof(BackgroundOverlayOpacity));
            OnPropertyChanged(nameof(SurfaceOpacity));
            OnPropertyChanged(nameof(SurfaceAltOpacity));
            OnPropertyChanged(nameof(InputOpacity));
            OnPropertyChanged(nameof(ButtonOpacity));
            OnPropertyChanged(nameof(NavigationOpacity));
            OnPropertyChanged(nameof(HeaderOpacity));
            OnPropertyChanged(nameof(MenuOpacity));
            OnPropertyChanged(nameof(MenuDropDownOpacity));
            OnPropertyChanged(nameof(FooterOpacity));
            OnPropertyChanged(nameof(DialogOpacity));
            OnPropertyChanged(nameof(DisabledOpacity));
            OnPropertyChanged(nameof(BordersVisible));
            OnPropertyChanged(nameof(UseGlassSurfaces));
            OnPropertyChanged(nameof(EnableSurfaceShadows));
            OnPropertyChanged(nameof(EnableControlShadows));
            OnPropertyChanged(nameof(BorderOpacity));
            OnPropertyChanged(nameof(BorderThickness));
            OnPropertyChanged(nameof(ControlBorderThickness));
            OnPropertyChanged(nameof(DividerOpacity));
            OnPropertyChanged(nameof(CardCornerRadius));
            OnPropertyChanged(nameof(PanelCornerRadius));
            OnPropertyChanged(nameof(ButtonCornerRadius));
            OnPropertyChanged(nameof(InputCornerRadius));
            OnPropertyChanged(nameof(ShadowBlurRadius));
            OnPropertyChanged(nameof(ShadowDepth));
            OnPropertyChanged(nameof(ShadowOpacity));
            OnPropertyChanged(nameof(ShadowDirection));
            OnPropertyChanged(nameof(SurfaceShadowScale));
            OnPropertyChanged(nameof(ControlShadowScale));
            OnPropertyChanged(nameof(PagePadding));
            OnPropertyChanged(nameof(CardPadding));
            OnPropertyChanged(nameof(FontScale));
            OnPropertyChanged(nameof(HeadingFontScale));
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

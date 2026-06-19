using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using Application = System.Windows.Application;

namespace InventoryManagementApp.Services
{
    /// <summary>
    /// Service for managing application theme resources.
    /// </summary>
    public class ThemeService : IThemeService
    {
        private static readonly Uri LightThemeUri = new("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Light.xaml", UriKind.Absolute);
        private static readonly Uri DarkThemeUri = new("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Dark.xaml", UriKind.Absolute);
        private readonly ISettingsService? _settingsService;
        private bool _applyingCustomTheme;

        public ThemeService(ISettingsService? settingsService = null)
        {
            _settingsService = settingsService;
        }

        public void ApplyTheme(string? theme)
        {
            var app = Application.Current;
            if (app is null) return;

            void ApplyOnUiThread()
            {
                var dictionaries = app.Resources.MergedDictionaries;
                var themeUri = string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase) ? DarkThemeUri : LightThemeUri;
                var insertIndex = 0;

                for (int i = dictionaries.Count - 1; i >= 0; i--)
                {
                    if (IsThemeDictionary(dictionaries[i]))
                    {
                        insertIndex = i;
                        dictionaries.RemoveAt(i);
                    }
                }

                dictionaries.Insert(Math.Min(insertIndex, dictionaries.Count), new ResourceDictionary { Source = themeUri });
                RefreshWindows(app);
            }

            InvokeOnUi(app, ApplyOnUiThread);
            ApplySavedCustomTheme(theme);
        }

        public void ApplyCustomTheme(AppThemeSettings? settings)
        {
            var app = Application.Current;
            if (app is null || settings is null) return;

            void ApplyOnUiThread()
            {
                settings.Normalize();
                _applyingCustomTheme = true;
                try
                {
                    ApplyTheme(settings.BaseTheme);
                }
                finally
                {
                    _applyingCustomTheme = false;
                }

                var resources = app.Resources;
                var borderThickness = settings.BordersVisible ? new Thickness(1) : new Thickness(0);
                var subtleBorderThickness = settings.BordersVisible ? new Thickness(0, 0, 0, 1) : new Thickness(0);
                var borderBrush = CreateBrush(settings.BorderColor, settings.BorderOpacity * 0.55);
                var surfaceBrush = CreateBrush(settings.SurfaceColor, settings.UseGlassSurfaces ? Math.Min(settings.SurfaceOpacity, 0.72) : settings.SurfaceOpacity);
                var surfaceAltBrush = CreateBrush(settings.SurfaceAltColor, settings.UseGlassSurfaces ? Math.Min(settings.SurfaceAltOpacity, 0.62) : settings.SurfaceAltOpacity);
                var inputBrush = CreateBrush(settings.InputColor, settings.InputOpacity);
                var buttonBrush = CreateBrush(settings.ButtonColor, settings.ButtonOpacity);
                var navigationBrush = CreateBrush(settings.NavigationColor, settings.UseGlassSurfaces ? Math.Min(settings.NavigationOpacity, 0.74) : settings.NavigationOpacity);
                var hoverOpacity = Math.Clamp(0.08 + (settings.InteractionIntensity * 0.08), 0, 0.28);
                var selectedOpacity = Math.Clamp(0.14 + (settings.InteractionIntensity * 0.13), 0, 0.42);
                var pressedOpacity = Math.Clamp(0.18 + (settings.InteractionIntensity * 0.12), 0, 0.48);
                var bodyFontSize = Math.Round(13 * settings.FontScale, 1);
                var captionFontSize = Math.Round(11 * settings.FontScale, 1);
                var sectionFontSize = Math.Round(14 * settings.FontScale, 1);
                var titleFontSize = Math.Round(18 * settings.FontScale, 1);

                Set(resources, "BackgroundBrush", CreateBackgroundBrush(settings));
                Set(resources, "SurfaceBrush", surfaceBrush);
                Set(resources, "SurfaceAltBrush", surfaceAltBrush);
                Set(resources, "NavigationSurfaceBrush", navigationBrush);
                Set(resources, "NavigationAltSurfaceBrush", CreateBrush(settings.NavigationColor, Math.Min(1, settings.NavigationOpacity * 0.9)));
                Set(resources, "GlassSurfaceBrush", CreateBrush(settings.SurfaceColor, Math.Min(settings.SurfaceOpacity, 0.78)));
                Set(resources, "GlassSurfaceAltBrush", CreateBrush(settings.SurfaceAltColor, Math.Min(settings.SurfaceAltOpacity, 0.68)));
                Set(resources, "TransparentSurfaceBrush", Brushes.Transparent);
                Set(resources, "TextBoxBackgroundBrush", inputBrush);
                Set(resources, "ComboBoxPopupBackgroundBrush", CreateBrush(settings.InputColor, Math.Max(settings.InputOpacity, 0.9)));
                Set(resources, "ForegroundBrush", CreateBrush(settings.TextColor, 1));
                Set(resources, "ForegroundMutedBrush", CreateBrush(settings.MutedTextColor, 1));
                Set(resources, "AccentBrush", CreateBrush(settings.AccentColor, 1));
                Set(resources, "OnAccentForegroundBrush", CreateOnAccentBrush(settings.AccentColor));
                Set(resources, "SuccessBrush", CreateBrush(settings.SuccessColor, 1));
                Set(resources, "WarningBrush", CreateBrush(settings.WarningColor, 1));
                Set(resources, "ErrorBrush", CreateBrush(settings.ErrorColor, 1));
                Set(resources, "BorderBrushBase", borderBrush);
                Set(resources, "BorderBrushAlt", CreateBrush(settings.BorderColor, settings.BorderOpacity * 0.45));
                Set(resources, "BtnBg", buttonBrush);
                Set(resources, "BtnBgHover", CreateBrush(settings.AccentColor, Math.Min(1, settings.ButtonOpacity * hoverOpacity + 0.08)));
                Set(resources, "BtnBorder", settings.BordersVisible ? CreateBrush(settings.BorderColor, settings.BorderOpacity * 0.72) : Brushes.Transparent);
                Set(resources, "BtnFg", CreateBrush(settings.TextColor, 1));
                Set(resources, "FocusVisualStrokeBrush", CreateBrush(settings.AccentColor, settings.FocusRingOpacity));
                Set(resources, "ThemeFocusVisualStrokeThickness", Math.Clamp(1 + settings.FocusRingOpacity * 3, 1, 4));
                Set(resources, "NavButtonPressedBrush", CreateBrush(settings.AccentColor, pressedOpacity));
                Set(resources, "NavButtonHoverBrush", CreateBrush(settings.AccentColor, hoverOpacity));
                Set(resources, "ItemHoverBrush", CreateBrush(settings.AccentColor, hoverOpacity));
                Set(resources, "ItemSelectedBrush", CreateBrush(settings.AccentColor, selectedOpacity));
                Set(resources, "ItemHoverForegroundBrush", CreateBrush(settings.TextColor, 1));
                Set(resources, "ItemSelectedForegroundBrush", CreateBrush(settings.TextColor, 1));
                Set(resources, "DataGridRowBackgroundBrush", CreateBrush(settings.SurfaceColor, Math.Max(settings.SurfaceOpacity, 0.78)));
                Set(resources, "DataGridAlternatingRowBackgroundBrush", CreateBrush(settings.SurfaceAltColor, Math.Max(settings.SurfaceAltOpacity, 0.72)));
                Set(resources, "ThemeGridLineBrush", settings.BordersVisible ? CreateBrush(settings.BorderColor, settings.GridLineOpacity) : Brushes.Transparent);
                Set(resources, "ProgressBarBackgroundBrush", CreateBrush(settings.SurfaceAltColor, Math.Max(settings.SurfaceAltOpacity, 0.65)));
                Set(resources, "ThemeBorderThickness", borderThickness);
                Set(resources, "ThemeSubtleBorderThickness", subtleBorderThickness);
                Set(resources, "ThemeControlBorderThickness", borderThickness);
                Set(resources, "ThemeBorderlessThickness", new Thickness(0));
                Set(resources, "ThemeCardCornerRadius", new CornerRadius(settings.CardCornerRadius));
                Set(resources, "ThemePanelCornerRadius", new CornerRadius(settings.PanelCornerRadius));
                Set(resources, "ThemeButtonCornerRadius", new CornerRadius(settings.ButtonCornerRadius));
                Set(resources, "ThemeInputCornerRadius", new CornerRadius(settings.InputCornerRadius));
                Set(resources, "ThemeFooterCornerRadius", new CornerRadius(settings.PanelCornerRadius));
                Set(resources, "RadiusSmall", new CornerRadius(settings.InputCornerRadius));
                Set(resources, "RadiusMedium", new CornerRadius(settings.CardCornerRadius));
                Set(resources, "RadiusLarge", new CornerRadius(settings.CardCornerRadius));
                Set(resources, "PagePadding", new Thickness(settings.PagePadding));
                Set(resources, "CardPadding", new Thickness(settings.CardPadding));
                Set(resources, "ThemeFontScale", settings.FontScale);
                Set(resources, "ThemeCaptionFontSize", captionFontSize);
                Set(resources, "ThemeBodyFontSize", bodyFontSize);
                Set(resources, "ThemeSectionFontSize", sectionFontSize);
                Set(resources, "ThemeTitleFontSize", titleFontSize);
                Set(resources, "ThemeControlMinHeight", settings.ControlHeight);
                Set(resources, "ThemeDataGridRowHeight", settings.DataGridRowHeight);
                Set(resources, "ThemeDataGridHeaderHeight", settings.DataGridHeaderHeight);
                Set(resources, "ThemeInteractionIntensity", settings.InteractionIntensity);
                Set(resources, "ThemeMotionIntensity", settings.MotionIntensity);
                Set(resources, "ThemeShadowDirection", settings.ShadowDirection);
                Set(resources, "ThemeShadowColorBrush", CreateBrush(settings.ShadowColor, settings.ShadowOpacity));
                Set(resources, "ThemeSurfaceShadow", CreateShadow(settings));
                Set(resources, "ThemeRaisedShadow", CreateShadow(settings, 1.55));
                Set(resources, "ThemeDeepShadow", CreateShadow(settings, 2.35));
                Set(resources, "SubtleSurfaceShadow", CreateShadow(settings));
                Set(resources, "RaisedSurfaceShadow", CreateShadow(settings, 1.55));

                RefreshWindows(app);
            }

            InvokeOnUi(app, ApplyOnUiThread);
        }

        private void ApplySavedCustomTheme(string? theme)
        {
            if (_applyingCustomTheme || _settingsService == null)
                return;

            try
            {
                var settings = _settingsService.GetAppThemeSettingsAsync().GetAwaiter().GetResult();
                if (!string.IsNullOrWhiteSpace(theme))
                    settings.BaseTheme = theme;
                ApplyCustomTheme(settings);
            }
            catch
            {
                // A missing database or invalid saved profile should never prevent the base theme from loading.
            }
        }

        private static void InvokeOnUi(Application app, Action action)
        {
            if (app.Dispatcher.CheckAccess())
            {
                action();
                return;
            }

            app.Dispatcher.Invoke(action, DispatcherPriority.Send);
        }

        private static void RefreshWindows(Application app)
        {
            foreach (Window window in app.Windows)
            {
                if (!window.IsLoaded)
                {
                    continue;
                }

                window.InvalidateVisual();
                window.UpdateLayout();
            }
        }

        private static bool IsThemeDictionary(ResourceDictionary dictionary)
        {
            var source = dictionary.Source?.OriginalString.Replace('\\', '/');
            return source?.EndsWith("Resources/Colors.Light.xaml", StringComparison.OrdinalIgnoreCase) == true ||
                   source?.EndsWith("Resources/Colors.Dark.xaml", StringComparison.OrdinalIgnoreCase) == true;
        }

        private static void Set(ResourceDictionary resources, string key, object value)
        {
            resources[key] = value;
        }

        private static Brush CreateBackgroundBrush(AppThemeSettings settings)
        {
            if (!string.IsNullOrWhiteSpace(settings.BackgroundImagePath) && File.Exists(settings.BackgroundImagePath))
            {
                try
                {
                    return new ImageBrush(new BitmapImage(new Uri(settings.BackgroundImagePath, UriKind.Absolute)))
                    {
                        Stretch = ParseStretch(settings.BackgroundImageStretch),
                        Opacity = settings.BackgroundOpacity
                    };
                }
                catch
                {
                    // Fall back to the configured color if the image cannot be loaded.
                }
            }

            return CreateBrush(settings.BackgroundColor, settings.BackgroundOpacity);
        }

        private static Stretch ParseStretch(string? value)
        {
            if (Enum.TryParse(value, ignoreCase: true, out Stretch stretch))
                return stretch;
            return Stretch.UniformToFill;
        }

        private static SolidColorBrush CreateBrush(string color, double opacity)
        {
            var parsed = (Color)ColorConverter.ConvertFromString(color);
            parsed.A = (byte)Math.Round(255 * Math.Clamp(opacity, 0, 1), MidpointRounding.AwayFromZero);
            var brush = new SolidColorBrush(parsed);
            if (brush.CanFreeze)
                brush.Freeze();
            return brush;
        }

        private static SolidColorBrush CreateOnAccentBrush(string accentColor)
        {
            var color = (Color)ColorConverter.ConvertFromString(accentColor);
            var brightness = (color.R * 0.299) + (color.G * 0.587) + (color.B * 0.114);
            return CreateBrush(brightness > 140 ? "#FF111827" : "#FFFFFFFF", 1);
        }

        private static DropShadowEffect CreateShadow(AppThemeSettings settings, double multiplier = 1)
        {
            var effect = new DropShadowEffect
            {
                BlurRadius = settings.ShadowBlurRadius * multiplier,
                ShadowDepth = settings.ShadowDepth * multiplier,
                Direction = settings.ShadowDirection,
                Opacity = Math.Clamp(settings.ShadowOpacity * multiplier, 0, 1),
                Color = ParseColor(settings.ShadowColor)
            };

            if (effect.CanFreeze)
                effect.Freeze();

            return effect;
        }

        private static Color ParseColor(string value)
        {
            return (Color)ColorConverter.ConvertFromString(value)!;
        }
    }
}

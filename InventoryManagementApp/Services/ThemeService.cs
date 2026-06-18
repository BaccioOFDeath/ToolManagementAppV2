using System;
using System.Globalization;
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
        }

        public void ApplyCustomTheme(AppThemeSettings? settings)
        {
            var app = Application.Current;
            if (app is null || settings is null) return;

            void ApplyOnUiThread()
            {
                settings.Normalize();
                ApplyTheme(settings.BaseTheme);

                var resources = app.Resources;
                var borderThickness = settings.BordersVisible ? new Thickness(1) : new Thickness(0);
                var subtleBorderThickness = settings.BordersVisible ? new Thickness(0, 0, 0, 1) : new Thickness(0);
                var borderBrush = CreateBrush(settings.AccentColor, settings.BorderOpacity * 0.55);
                var surfaceBrush = CreateBrush(settings.SurfaceColor, settings.UseGlassSurfaces ? Math.Min(settings.SurfaceOpacity, 0.72) : settings.SurfaceOpacity);
                var surfaceAltBrush = CreateBrush(settings.SurfaceAltColor, settings.UseGlassSurfaces ? Math.Min(settings.SurfaceAltOpacity, 0.62) : settings.SurfaceAltOpacity);
                var backgroundBrush = CreateBackgroundBrush(settings);

                Set(resources, "BackgroundBrush", backgroundBrush);
                Set(resources, "SurfaceBrush", surfaceBrush);
                Set(resources, "SurfaceAltBrush", surfaceAltBrush);
                Set(resources, "TextBoxBackgroundBrush", CreateBrush(settings.SurfaceColor, settings.InputOpacity));
                Set(resources, "ComboBoxPopupBackgroundBrush", CreateBrush(settings.SurfaceColor, Math.Max(settings.SurfaceOpacity, 0.9)));
                Set(resources, "ForegroundBrush", CreateBrush(settings.TextColor, 1));
                Set(resources, "ForegroundMutedBrush", CreateBrush(settings.MutedTextColor, 1));
                Set(resources, "AccentBrush", CreateBrush(settings.AccentColor, 1));
                Set(resources, "SuccessBrush", CreateBrush(settings.SuccessColor, 1));
                Set(resources, "WarningBrush", CreateBrush(settings.WarningColor, 1));
                Set(resources, "ErrorBrush", CreateBrush(settings.ErrorColor, 1));
                Set(resources, "BorderBrushBase", borderBrush);
                Set(resources, "BorderBrushAlt", CreateBrush(settings.MutedTextColor, settings.BorderOpacity * 0.45));
                Set(resources, "BtnBg", CreateBrush(settings.SurfaceAltColor, settings.ButtonOpacity));
                Set(resources, "BtnBgHover", CreateBrush(settings.AccentColor, Math.Min(1, settings.ButtonOpacity * 0.22 + 0.12)));
                Set(resources, "BtnBorder", settings.BordersVisible ? CreateBrush(settings.AccentColor, settings.BorderOpacity * 0.72) : Brushes.Transparent);
                Set(resources, "BtnFg", CreateBrush(settings.TextColor, 1));
                Set(resources, "ItemHoverBrush", CreateBrush(settings.AccentColor, 0.14));
                Set(resources, "ItemSelectedBrush", CreateBrush(settings.AccentColor, 0.24));
                Set(resources, "ItemHoverForegroundBrush", CreateBrush(settings.TextColor, 1));
                Set(resources, "ItemSelectedForegroundBrush", CreateBrush(settings.TextColor, 1));
                Set(resources, "DataGridRowBackgroundBrush", CreateBrush(settings.SurfaceColor, Math.Max(settings.SurfaceOpacity, 0.78)));
                Set(resources, "DataGridAlternatingRowBackgroundBrush", CreateBrush(settings.SurfaceAltColor, Math.Max(settings.SurfaceAltOpacity, 0.72)));
                Set(resources, "ThemeBorderThickness", borderThickness);
                Set(resources, "ThemeSubtleBorderThickness", subtleBorderThickness);
                Set(resources, "ThemeControlBorderThickness", borderThickness);
                Set(resources, "ThemeCardCornerRadius", new CornerRadius(settings.CardCornerRadius));
                Set(resources, "ThemePanelCornerRadius", new CornerRadius(settings.PanelCornerRadius));
                Set(resources, "ThemeButtonCornerRadius", new CornerRadius(settings.ButtonCornerRadius));
                Set(resources, "ThemeInputCornerRadius", new CornerRadius(settings.InputCornerRadius));
                Set(resources, "ThemeFooterCornerRadius", new CornerRadius(settings.PanelCornerRadius));
                Set(resources, "PagePadding", new Thickness(settings.PagePadding));
                Set(resources, "CardPadding", new Thickness(settings.CardPadding));
                Set(resources, "ThemeSurfaceShadow", CreateShadow(settings));
                Set(resources, "ThemeRaisedShadow", CreateShadow(settings, 1.55));
                Set(resources, "ThemeDeepShadow", CreateShadow(settings, 2.35));

                RefreshWindows(app);
            }

            InvokeOnUi(app, ApplyOnUiThread);
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
                        Stretch = Stretch.UniformToFill,
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

        private static SolidColorBrush CreateBrush(string color, double opacity)
        {
            var parsed = (Color)ColorConverter.ConvertFromString(color);
            parsed.A = (byte)Math.Round(255 * Math.Clamp(opacity, 0, 1), MidpointRounding.AwayFromZero);
            var brush = new SolidColorBrush(parsed);
            if (brush.CanFreeze)
                brush.Freeze();
            return brush;
        }

        private static DropShadowEffect CreateShadow(AppThemeSettings settings, double multiplier = 1)
        {
            return new DropShadowEffect
            {
                BlurRadius = settings.ShadowBlurRadius * multiplier,
                ShadowDepth = settings.ShadowDepth * multiplier,
                Direction = 270,
                Opacity = Math.Clamp(settings.ShadowOpacity * multiplier, 0, 1),
                Color = ParseColor("#66000000")
            };
        }

        private static Color ParseColor(string value)
        {
            return (Color)ColorConverter.ConvertFromString(value)!;
        }
    }
}

using System;

namespace InventoryManagementApp.Models
{
    public class AppThemeSettings
    {
        public string BaseTheme { get; set; } = "Light";
        public string BackgroundColor { get; set; } = "#F4F6F8";
        public string SurfaceColor { get; set; } = "#FFFFFFFF";
        public string SurfaceAltColor { get; set; } = "#FFE8EDF3";
        public string TextColor { get; set; } = "#FF111827";
        public string MutedTextColor { get; set; } = "#FF5B6472";
        public string AccentColor { get; set; } = "#FF2563EB";
        public string SuccessColor { get; set; } = "#FF15803D";
        public string WarningColor { get; set; } = "#FFD97706";
        public string ErrorColor { get; set; } = "#FFDC2626";
        public string BackgroundImagePath { get; set; } = string.Empty;
        public double BackgroundOpacity { get; set; } = 1.0;
        public double SurfaceOpacity { get; set; } = 1.0;
        public double SurfaceAltOpacity { get; set; } = 1.0;
        public double InputOpacity { get; set; } = 1.0;
        public double ButtonOpacity { get; set; } = 1.0;
        public bool BordersVisible { get; set; } = true;
        public double BorderOpacity { get; set; } = 1.0;
        public double CardCornerRadius { get; set; } = 6.0;
        public double PanelCornerRadius { get; set; } = 4.0;
        public double ButtonCornerRadius { get; set; } = 5.0;
        public double InputCornerRadius { get; set; } = 4.0;
        public double ShadowBlurRadius { get; set; } = 7.0;
        public double ShadowDepth { get; set; } = 1.0;
        public double ShadowOpacity { get; set; } = 0.14;
        public double PagePadding { get; set; } = 6.0;
        public double CardPadding { get; set; } = 8.0;
        public bool UseGlassSurfaces { get; set; }

        public static AppThemeSettings CreateDefault(string? baseTheme = null)
        {
            var settings = new AppThemeSettings();
            if (!string.IsNullOrWhiteSpace(baseTheme))
                settings.BaseTheme = NormalizeBaseTheme(baseTheme);

            if (string.Equals(settings.BaseTheme, "Dark", StringComparison.OrdinalIgnoreCase))
            {
                settings.BackgroundColor = "#FF101418";
                settings.SurfaceColor = "#FF1B222A";
                settings.SurfaceAltColor = "#FF252D36";
                settings.TextColor = "#FFF3F4F6";
                settings.MutedTextColor = "#FFB5BDC8";
                settings.AccentColor = "#FF60A5FA";
                settings.SuccessColor = "#FF4ADE80";
                settings.WarningColor = "#FFFBBF24";
                settings.ErrorColor = "#FFF87171";
                settings.ShadowOpacity = 0.35;
            }

            return settings;
        }

        public void Normalize()
        {
            BaseTheme = NormalizeBaseTheme(BaseTheme);
            var defaults = CreateDefault(BaseTheme);
            BackgroundColor = NormalizeColor(BackgroundColor, defaults.BackgroundColor);
            SurfaceColor = NormalizeColor(SurfaceColor, defaults.SurfaceColor);
            SurfaceAltColor = NormalizeColor(SurfaceAltColor, defaults.SurfaceAltColor);
            TextColor = NormalizeColor(TextColor, defaults.TextColor);
            MutedTextColor = NormalizeColor(MutedTextColor, defaults.MutedTextColor);
            AccentColor = NormalizeColor(AccentColor, defaults.AccentColor);
            SuccessColor = NormalizeColor(SuccessColor, defaults.SuccessColor);
            WarningColor = NormalizeColor(WarningColor, defaults.WarningColor);
            ErrorColor = NormalizeColor(ErrorColor, defaults.ErrorColor);
            BackgroundOpacity = Clamp01(BackgroundOpacity);
            SurfaceOpacity = Clamp01(SurfaceOpacity);
            SurfaceAltOpacity = Clamp01(SurfaceAltOpacity);
            InputOpacity = Clamp01(InputOpacity);
            ButtonOpacity = Clamp01(ButtonOpacity);
            BorderOpacity = Clamp01(BorderOpacity);
            CardCornerRadius = Clamp(CardCornerRadius, 0, 32);
            PanelCornerRadius = Clamp(PanelCornerRadius, 0, 32);
            ButtonCornerRadius = Clamp(ButtonCornerRadius, 0, 32);
            InputCornerRadius = Clamp(InputCornerRadius, 0, 32);
            ShadowBlurRadius = Clamp(ShadowBlurRadius, 0, 48);
            ShadowDepth = Clamp(ShadowDepth, 0, 16);
            ShadowOpacity = Clamp01(ShadowOpacity);
            PagePadding = Clamp(PagePadding, 0, 28);
            CardPadding = Clamp(CardPadding, 0, 32);
        }

        private static string NormalizeBaseTheme(string? value)
            => value?.IndexOf("Dark", StringComparison.OrdinalIgnoreCase) >= 0 ? "Dark" : "Light";

        private static double Clamp01(double value) => Clamp(value, 0, 1);

        private static double Clamp(double value, double min, double max)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return min;
            return Math.Min(max, Math.Max(min, value));
        }

        private static string NormalizeColor(string? value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            var trimmed = value.Trim();
            if (!trimmed.StartsWith("#", StringComparison.Ordinal))
                trimmed = "#" + trimmed;

            return trimmed.Length is 7 or 9 ? trimmed.ToUpperInvariant() : fallback;
        }
    }
}

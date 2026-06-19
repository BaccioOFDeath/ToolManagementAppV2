using System;

namespace InventoryManagementApp.Models
{
    public class AppThemeSettings
    {
        public string BaseTheme { get; set; } = "Light";
        public string BackgroundColor { get; set; } = "#F4F6F8";
        public string SurfaceColor { get; set; } = "#FFFFFFFF";
        public string SurfaceAltColor { get; set; } = "#FFE8EDF3";
        public string NavigationColor { get; set; } = "#FFE8EDF3";
        public string InputColor { get; set; } = "#FFFFFFFF";
        public string ButtonColor { get; set; } = "#FFE8EDF3";
        public string BorderColor { get; set; } = "#FF2563EB";
        public string TextColor { get; set; } = "#FF111827";
        public string MutedTextColor { get; set; } = "#FF5B6472";
        public string AccentColor { get; set; } = "#FF2563EB";
        public string SuccessColor { get; set; } = "#FF15803D";
        public string WarningColor { get; set; } = "#FFD97706";
        public string ErrorColor { get; set; } = "#FFDC2626";
        public string ShadowColor { get; set; } = "#66000000";
        public string BackgroundImagePath { get; set; } = string.Empty;
        public string BackgroundImageStretch { get; set; } = "UniformToFill";
        public double BackgroundOpacity { get; set; } = 1.0;
        public double SurfaceOpacity { get; set; } = 1.0;
        public double SurfaceAltOpacity { get; set; } = 1.0;
        public double InputOpacity { get; set; } = 1.0;
        public double ButtonOpacity { get; set; } = 1.0;
        public double NavigationOpacity { get; set; } = 1.0;
        public double HeaderOpacity { get; set; } = 1.0;
        public double MenuOpacity { get; set; } = 1.0;
        public double FooterOpacity { get; set; } = 1.0;
        public double DialogOpacity { get; set; } = 1.0;
        public bool BordersVisible { get; set; } = true;
        public double BorderOpacity { get; set; } = 1.0;
        public double CardCornerRadius { get; set; } = 6.0;
        public double PanelCornerRadius { get; set; } = 4.0;
        public double ButtonCornerRadius { get; set; } = 5.0;
        public double InputCornerRadius { get; set; } = 4.0;
        public double ShadowBlurRadius { get; set; } = 7.0;
        public double ShadowDepth { get; set; } = 1.0;
        public double ShadowOpacity { get; set; } = 0.14;
        public double ShadowDirection { get; set; } = 270.0;
        public double PagePadding { get; set; } = 6.0;
        public double CardPadding { get; set; } = 8.0;
        public double FontScale { get; set; } = 1.0;
        public double ControlHeight { get; set; } = 28.0;
        public double DataGridRowHeight { get; set; } = 30.0;
        public double DataGridHeaderHeight { get; set; } = 30.0;
        public double InteractionIntensity { get; set; } = 1.0;
        public double FocusRingOpacity { get; set; } = 0.55;
        public double GridLineOpacity { get; set; } = 0.42;
        public double MotionIntensity { get; set; } = 1.0;
        public bool UseGlassSurfaces { get; set; }
        public bool EnableSurfaceShadows { get; set; } = true;
        public bool EnableControlShadows { get; set; }

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
                settings.NavigationColor = "#FF252D36";
                settings.InputColor = "#FF1B222A";
                settings.ButtonColor = "#FF252D36";
                settings.BorderColor = "#FF60A5FA";
                settings.TextColor = "#FFF3F4F6";
                settings.MutedTextColor = "#FFB5BDC8";
                settings.AccentColor = "#FF60A5FA";
                settings.SuccessColor = "#FF4ADE80";
                settings.WarningColor = "#FFFBBF24";
                settings.ErrorColor = "#FFF87171";
                settings.ShadowColor = "#99000000";
                settings.ShadowOpacity = 0.35;
                settings.GridLineOpacity = 0.5;
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
            NavigationColor = NormalizeColor(NavigationColor, defaults.NavigationColor);
            InputColor = NormalizeColor(InputColor, defaults.InputColor);
            ButtonColor = NormalizeColor(ButtonColor, defaults.ButtonColor);
            BorderColor = NormalizeColor(BorderColor, defaults.BorderColor);
            TextColor = NormalizeColor(TextColor, defaults.TextColor);
            MutedTextColor = NormalizeColor(MutedTextColor, defaults.MutedTextColor);
            AccentColor = NormalizeColor(AccentColor, defaults.AccentColor);
            SuccessColor = NormalizeColor(SuccessColor, defaults.SuccessColor);
            WarningColor = NormalizeColor(WarningColor, defaults.WarningColor);
            ErrorColor = NormalizeColor(ErrorColor, defaults.ErrorColor);
            ShadowColor = NormalizeColor(ShadowColor, defaults.ShadowColor);
            BackgroundImageStretch = NormalizeBackgroundStretch(BackgroundImageStretch);
            BackgroundOpacity = Clamp01(BackgroundOpacity);
            SurfaceOpacity = Clamp01(SurfaceOpacity);
            SurfaceAltOpacity = Clamp01(SurfaceAltOpacity);
            InputOpacity = Clamp01(InputOpacity);
            ButtonOpacity = Clamp01(ButtonOpacity);
            NavigationOpacity = Clamp01(NavigationOpacity);
            HeaderOpacity = Clamp01(HeaderOpacity);
            MenuOpacity = Clamp01(MenuOpacity);
            FooterOpacity = Clamp01(FooterOpacity);
            DialogOpacity = Clamp01(DialogOpacity);
            BorderOpacity = Clamp01(BorderOpacity);
            CardCornerRadius = Clamp(CardCornerRadius, 0, 32);
            PanelCornerRadius = Clamp(PanelCornerRadius, 0, 32);
            ButtonCornerRadius = Clamp(ButtonCornerRadius, 0, 32);
            InputCornerRadius = Clamp(InputCornerRadius, 0, 32);
            ShadowBlurRadius = Clamp(ShadowBlurRadius, 0, 48);
            ShadowDepth = Clamp(ShadowDepth, 0, 16);
            ShadowOpacity = Clamp01(ShadowOpacity);
            ShadowDirection = Clamp(ShadowDirection, 0, 360);
            PagePadding = Clamp(PagePadding, 0, 28);
            CardPadding = Clamp(CardPadding, 0, 32);
            FontScale = Clamp(FontScale, 0.75, 1.4);
            ControlHeight = Clamp(ControlHeight, 22, 44);
            DataGridRowHeight = Clamp(DataGridRowHeight, 22, 52);
            DataGridHeaderHeight = Clamp(DataGridHeaderHeight, 24, 56);
            InteractionIntensity = Clamp(InteractionIntensity, 0, 2);
            FocusRingOpacity = Clamp01(FocusRingOpacity);
            GridLineOpacity = Clamp01(GridLineOpacity);
            MotionIntensity = Clamp(MotionIntensity, 0, 2);
        }

        private static string NormalizeBaseTheme(string? value)
            => value?.IndexOf("Dark", StringComparison.OrdinalIgnoreCase) >= 0 ? "Dark" : "Light";

        private static string NormalizeBackgroundStretch(string? value)
        {
            if (string.Equals(value, "None", StringComparison.OrdinalIgnoreCase))
                return "None";
            if (string.Equals(value, "Fill", StringComparison.OrdinalIgnoreCase))
                return "Fill";
            if (string.Equals(value, "Uniform", StringComparison.OrdinalIgnoreCase))
                return "Uniform";
            return "UniformToFill";
        }

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

            if (trimmed.Length is not (7 or 9))
                return fallback;

            for (var i = 1; i < trimmed.Length; i++)
            {
                if (!Uri.IsHexDigit(trimmed[i]))
                    return fallback;
            }

            return trimmed.ToUpperInvariant();
        }
    }
}

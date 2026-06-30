using System;
using System.Text.Json.Serialization;

namespace InventoryManagementApp.Models
{
    public class AppThemeSettings
    {
        public string BaseTheme { get; set; } = "Light";
        public string BackgroundColor { get; set; } = "#F4F6F8";
        public string BackgroundOverlayColor { get; set; } = "#00FFFFFF";
        public string SurfaceColor { get; set; } = "#FFFFFFFF";
        public string SurfaceAltColor { get; set; } = "#FFE8EDF3";
        public string NavigationColor { get; set; } = "#FFE8EDF3";
        public string InputColor { get; set; } = "#FFFFFFFF";
        public string ButtonColor { get; set; } = "#FFE8EDF3";
        public string BorderColor { get; set; } = "#FF2563EB";
        public string TextColor { get; set; } = "#FF111827";
        public string MutedTextColor { get; set; } = "#FF5B6472";
        public string AccentColor { get; set; } = "#FF2563EB";
        public string HoverColor { get; set; } = "#FFDBEAFE";
        public string HoverTextColor { get; set; } = "#FF111827";
        public string SelectedColor { get; set; } = "#FF2563EB";
        public string SelectedTextColor { get; set; } = "#FFFFFFFF";
        public string SuccessColor { get; set; } = "#FF15803D";
        public string WarningColor { get; set; } = "#FFD97706";
        public string ErrorColor { get; set; } = "#FFDC2626";
        public string ShadowColor { get; set; } = "#66000000";
        public string DashboardHeaderColor { get; set; } = string.Empty;
        public string SearchHeaderColor { get; set; } = string.Empty;
        public string ManageItemsHeaderColor { get; set; } = string.Empty;
        public string RentalsHeaderColor { get; set; } = string.Empty;
        public string CustomersHeaderColor { get; set; } = string.Empty;
        public string ReservationsHeaderColor { get; set; } = string.Empty;
        public string MaintenanceHeaderColor { get; set; } = string.Empty;
        public string CalibrationHeaderColor { get; set; } = string.Empty;
        public string KitsHeaderColor { get; set; } = string.Empty;
        public string CategoriesHeaderColor { get; set; } = string.Empty;
        public string ReportsHeaderColor { get; set; } = string.Empty;
        public string ActivityLogsHeaderColor { get; set; } = string.Empty;
        public string ImportExportHeaderColor { get; set; } = string.Empty;
        public string UsersHeaderColor { get; set; } = string.Empty;
        public string SettingsHeaderColor { get; set; } = string.Empty;
        public string BackgroundImagePath { get; set; } = string.Empty;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? BackgroundImageFileName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? BackgroundImageContentBase64 { get; set; }
        public string BackgroundImageStretch { get; set; } = "UniformToFill";
        public string FontFamily { get; set; } = "Segoe UI";
        public double BackgroundOpacity { get; set; } = 1.0;
        public double BackgroundOverlayOpacity { get; set; }
        public double SurfaceOpacity { get; set; } = 1.0;
        public double SurfaceAltOpacity { get; set; } = 1.0;
        public double InputOpacity { get; set; } = 1.0;
        public double ButtonOpacity { get; set; } = 1.0;
        public double NavigationOpacity { get; set; } = 1.0;
        public double HeaderOpacity { get; set; } = 1.0;
        public double MenuOpacity { get; set; } = 1.0;
        public double MenuDropDownOpacity { get; set; } = 1.0;
        public double FooterOpacity { get; set; } = 1.0;
        public double DialogOpacity { get; set; } = 1.0;
        public double DisabledOpacity { get; set; } = 0.55;
        public bool BordersVisible { get; set; } = true;
        public double BorderOpacity { get; set; } = 1.0;
        public double BorderThickness { get; set; } = 1.0;
        public double ControlBorderThickness { get; set; } = 1.0;
        public double DividerOpacity { get; set; } = 1.0;
        public double CardCornerRadius { get; set; } = 6.0;
        public double PanelCornerRadius { get; set; } = 4.0;
        public double ButtonCornerRadius { get; set; } = 5.0;
        public double InputCornerRadius { get; set; } = 4.0;
        public double ShadowBlurRadius { get; set; } = 7.0;
        public double ShadowDepth { get; set; } = 1.0;
        public double ShadowOpacity { get; set; } = 0.14;
        public double ShadowDirection { get; set; } = 270.0;
        public double SurfaceShadowScale { get; set; } = 1.0;
        public double ControlShadowScale { get; set; } = 1.0;
        public double PagePadding { get; set; } = 6.0;
        public double CardPadding { get; set; } = 8.0;
        public double FontScale { get; set; } = 1.0;
        public double HeadingFontScale { get; set; } = 1.0;
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
            settings.BaseTheme = string.IsNullOrWhiteSpace(baseTheme)
                ? "SD European Light"
                : NormalizeBaseTheme(baseTheme);

            if (string.Equals(settings.BaseTheme, "SD European Light", StringComparison.OrdinalIgnoreCase))
            {
                settings.BackgroundColor = "#FFF7F8FA";
                settings.BackgroundOverlayColor = "#00FFFFFF";
                settings.SurfaceColor = "#FFFFFFFF";
                settings.SurfaceAltColor = "#FFF1F2F5";
                settings.NavigationColor = "#E8E7E7";
                settings.InputColor = "#FFFFFFFF";
                settings.ButtonColor = "#FFF5B700";
                settings.BorderColor = "#FFE2E4E8";
                settings.TextColor = "#FF1C1C1E";
                settings.MutedTextColor = "#FF6B7280";
                settings.AccentColor = "#FFF5B700";
                settings.HoverColor = "#FFFFF4CC";
                settings.HoverTextColor = "#FF0F0F0F";
                settings.SelectedColor = "#FFF5B700";
                settings.SelectedTextColor = "#FF0F0F0F";
                settings.SuccessColor = "#FF15803D";
                settings.WarningColor = "#FFC99500";
                settings.ErrorColor = "#FFDC2626";
                settings.ShadowColor = "#33000000";
                ApplySdLightPageHeaderColors(settings);
                settings.CardCornerRadius = 7.974110032362456;
                settings.PanelCornerRadius = 0;
                settings.ButtonCornerRadius = 10.252427184466026;
                settings.InputCornerRadius = 5.652996845425871;
                settings.ShadowBlurRadius = 20;
                settings.ShadowDepth = 3;
                settings.ShadowOpacity = 0.11326860841423966;
                settings.ShadowDirection = 269.1482649842255;
                settings.SurfaceShadowScale = 2.9611650485436884;
                settings.ControlShadowScale = 0.9621451104100942;
                settings.PagePadding = 10;
                settings.CardPadding = 12;
                settings.FontFamily = "Inter, Segoe UI";
                settings.BorderThickness = 0.9762841605622696;
                settings.ControlBorderThickness = 0;
                settings.ControlHeight = 22;
                settings.DataGridRowHeight = 50.27760252365941;
                settings.DataGridHeaderHeight = 34;
                settings.HeadingFontScale = 1.0080441640378595;
                settings.GridLineOpacity = 0.49526813880126197;
            }
            else if (string.Equals(settings.BaseTheme, "SD European Dark", StringComparison.OrdinalIgnoreCase))
            {
                settings.BackgroundColor = "#FF0F0F0F";
                settings.BackgroundOverlayColor = "#000F0F0F";
                settings.SurfaceColor = "#FF1C1C1E";
                settings.SurfaceAltColor = "#FF242426";
                settings.NavigationColor = "#FF0F0F0F";
                settings.InputColor = "#FF242426";
                settings.ButtonColor = "#FFF5B700";
                settings.BorderColor = "#FF343438";
                settings.TextColor = "#FFFFFFFF";
                settings.MutedTextColor = "#FFB8BDC7";
                settings.AccentColor = "#FFF5B700";
                settings.HoverColor = "#FF2F2B18";
                settings.HoverTextColor = "#FFFFFFFF";
                settings.SelectedColor = "#FFF5B700";
                settings.SelectedTextColor = "#FF0F0F0F";
                settings.SuccessColor = "#FF4ADE80";
                settings.WarningColor = "#FFF5B700";
                settings.ErrorColor = "#FFF87171";
                settings.ShadowColor = "#99000000";
                ApplySdDarkPageHeaderColors(settings);
                settings.CardCornerRadius = 12;
                settings.PanelCornerRadius = 12;
                settings.ButtonCornerRadius = 20;
                settings.InputCornerRadius = 8;
                settings.ShadowBlurRadius = 24;
                settings.ShadowDepth = 4;
                settings.ShadowOpacity = 0.28;
                settings.PagePadding = 10;
                settings.CardPadding = 12;
                settings.FontFamily = "Inter, Segoe UI";
                settings.ControlHeight = 32;
                settings.DataGridRowHeight = 34;
                settings.DataGridHeaderHeight = 34;
                settings.GridLineOpacity = 0.5;
            }
            else if (string.Equals(settings.BaseTheme, "Dark", StringComparison.OrdinalIgnoreCase))
            {
                settings.BackgroundColor = "#FF101418";
                settings.BackgroundOverlayColor = "#CC101418";
                settings.SurfaceColor = "#FF1B222A";
                settings.SurfaceAltColor = "#FF252D36";
                settings.NavigationColor = "#FF252D36";
                settings.InputColor = "#FF1B222A";
                settings.ButtonColor = "#FF252D36";
                settings.BorderColor = "#FF60A5FA";
                settings.TextColor = "#FFF3F4F6";
                settings.MutedTextColor = "#FFB5BDC8";
                settings.AccentColor = "#FF60A5FA";
                settings.HoverColor = "#FF1E3A5F";
                settings.HoverTextColor = "#FFF3F4F6";
                settings.SelectedColor = "#FF2563EB";
                settings.SelectedTextColor = "#FFFFFFFF";
                settings.SuccessColor = "#FF4ADE80";
                settings.WarningColor = "#FFFBBF24";
                settings.ErrorColor = "#FFF87171";
                settings.ShadowColor = "#99000000";
                settings.DashboardHeaderColor = settings.SurfaceColor;
                settings.SearchHeaderColor = settings.SurfaceColor;
                settings.ManageItemsHeaderColor = settings.SurfaceColor;
                settings.RentalsHeaderColor = settings.SurfaceColor;
                settings.CustomersHeaderColor = settings.SurfaceColor;
                settings.ReservationsHeaderColor = settings.SurfaceColor;
                settings.MaintenanceHeaderColor = settings.SurfaceColor;
                settings.CalibrationHeaderColor = settings.SurfaceColor;
                settings.KitsHeaderColor = settings.SurfaceColor;
                settings.CategoriesHeaderColor = settings.SurfaceColor;
                settings.ReportsHeaderColor = settings.SurfaceColor;
                settings.ActivityLogsHeaderColor = settings.SurfaceColor;
                settings.ImportExportHeaderColor = settings.SurfaceColor;
                settings.UsersHeaderColor = settings.SurfaceColor;
                settings.SettingsHeaderColor = settings.SurfaceColor;
                settings.ShadowOpacity = 0.35;
                settings.GridLineOpacity = 0.5;
            }
            else if (string.Equals(settings.BaseTheme, "VS Code", StringComparison.OrdinalIgnoreCase))
            {
                settings.BackgroundColor = "#FF1E1E1E";
                settings.BackgroundOverlayColor = "#001E1E1E";
                settings.SurfaceColor = "#FF1E1E1E";
                settings.SurfaceAltColor = "#FF252526";
                settings.NavigationColor = "#FF181818";
                settings.InputColor = "#FF313131";
                settings.ButtonColor = "#FF0E639C";
                settings.BorderColor = "#FF2B2B2B";
                settings.TextColor = "#FFCCCCCC";
                settings.MutedTextColor = "#FF858585";
                settings.AccentColor = "#FF007ACC";
                settings.HoverColor = "#FF2A2D2E";
                settings.HoverTextColor = "#FFCCCCCC";
                settings.SelectedColor = "#FF04395E";
                settings.SelectedTextColor = "#FFFFFFFF";
                settings.SuccessColor = "#FF89D185";
                settings.WarningColor = "#FFDCDCAA";
                settings.ErrorColor = "#FFF48771";
                settings.ShadowColor = "#66000000";
                settings.DashboardHeaderColor = settings.SurfaceColor;
                settings.SearchHeaderColor = settings.SurfaceColor;
                settings.ManageItemsHeaderColor = settings.SurfaceColor;
                settings.RentalsHeaderColor = settings.SurfaceColor;
                settings.CustomersHeaderColor = settings.SurfaceColor;
                settings.ReservationsHeaderColor = settings.SurfaceColor;
                settings.MaintenanceHeaderColor = settings.SurfaceColor;
                settings.CalibrationHeaderColor = settings.SurfaceColor;
                settings.KitsHeaderColor = settings.SurfaceColor;
                settings.CategoriesHeaderColor = settings.SurfaceColor;
                settings.ReportsHeaderColor = settings.SurfaceColor;
                settings.ActivityLogsHeaderColor = settings.SurfaceColor;
                settings.ImportExportHeaderColor = settings.SurfaceColor;
                settings.UsersHeaderColor = settings.SurfaceColor;
                settings.SettingsHeaderColor = settings.SurfaceColor;
                settings.SurfaceOpacity = 1;
                settings.SurfaceAltOpacity = 1;
                settings.InputOpacity = 1;
                settings.ButtonOpacity = 1;
                settings.NavigationOpacity = 1;
                settings.HeaderOpacity = 1;
                settings.MenuOpacity = 1;
                settings.MenuDropDownOpacity = 1;
                settings.FooterOpacity = 1;
                settings.DialogOpacity = 1;
                settings.DisabledOpacity = 0.5;
                settings.BorderOpacity = 1;
                settings.DividerOpacity = 1;
                settings.BorderThickness = 1;
                settings.ControlBorderThickness = 1;
                settings.CardCornerRadius = 0;
                settings.PanelCornerRadius = 0;
                settings.ButtonCornerRadius = 2;
                settings.InputCornerRadius = 2;
                settings.ShadowBlurRadius = 0;
                settings.ShadowDepth = 0;
                settings.ShadowOpacity = 0;
                settings.SurfaceShadowScale = 0;
                settings.ControlShadowScale = 0;
                settings.PagePadding = 4;
                settings.CardPadding = 6;
                settings.FontScale = 0.96;
                settings.HeadingFontScale = 0.95;
                settings.ControlHeight = 26;
                settings.DataGridRowHeight = 28;
                settings.DataGridHeaderHeight = 28;
                settings.InteractionIntensity = 1.15;
                settings.FocusRingOpacity = 0.8;
                settings.GridLineOpacity = 0.65;
                settings.MotionIntensity = 0.4;
                settings.EnableSurfaceShadows = false;
                settings.EnableControlShadows = false;
            }
            else if (string.Equals(settings.BaseTheme, "VS Code Light", StringComparison.OrdinalIgnoreCase))
            {
                settings.BackgroundColor = "#FFF3F3F3";
                settings.BackgroundOverlayColor = "#00F3F3F3";
                settings.SurfaceColor = "#FFFFFFFF";
                settings.SurfaceAltColor = "#FFF8F8F8";
                settings.NavigationColor = "#FFF3F3F3";
                settings.InputColor = "#FFFFFFFF";
                settings.ButtonColor = "#FFE5E5E5";
                settings.BorderColor = "#FFE5E5E5";
                settings.TextColor = "#FF333333";
                settings.MutedTextColor = "#FF6A6A6A";
                settings.AccentColor = "#FF007ACC";
                settings.HoverColor = "#FFE8E8E8";
                settings.HoverTextColor = "#FF333333";
                settings.SelectedColor = "#FFADD6FF";
                settings.SelectedTextColor = "#FF000000";
                settings.SuccessColor = "#FF16825D";
                settings.WarningColor = "#FFB89500";
                settings.ErrorColor = "#FFE51400";
                settings.ShadowColor = "#33000000";
                settings.DashboardHeaderColor = settings.SurfaceColor;
                settings.SearchHeaderColor = settings.SurfaceColor;
                settings.ManageItemsHeaderColor = settings.SurfaceColor;
                settings.RentalsHeaderColor = settings.SurfaceColor;
                settings.CustomersHeaderColor = settings.SurfaceColor;
                settings.ReservationsHeaderColor = settings.SurfaceColor;
                settings.MaintenanceHeaderColor = settings.SurfaceColor;
                settings.CalibrationHeaderColor = settings.SurfaceColor;
                settings.KitsHeaderColor = settings.SurfaceColor;
                settings.CategoriesHeaderColor = settings.SurfaceColor;
                settings.ReportsHeaderColor = settings.SurfaceColor;
                settings.ActivityLogsHeaderColor = settings.SurfaceColor;
                settings.ImportExportHeaderColor = settings.SurfaceColor;
                settings.UsersHeaderColor = settings.SurfaceColor;
                settings.SettingsHeaderColor = settings.SurfaceColor;
                settings.SurfaceOpacity = 1;
                settings.SurfaceAltOpacity = 1;
                settings.InputOpacity = 1;
                settings.ButtonOpacity = 1;
                settings.NavigationOpacity = 1;
                settings.HeaderOpacity = 1;
                settings.MenuOpacity = 1;
                settings.MenuDropDownOpacity = 1;
                settings.FooterOpacity = 1;
                settings.DialogOpacity = 1;
                settings.DisabledOpacity = 0.48;
                settings.BorderOpacity = 1;
                settings.DividerOpacity = 1;
                settings.BorderThickness = 1;
                settings.ControlBorderThickness = 1;
                settings.CardCornerRadius = 0;
                settings.PanelCornerRadius = 0;
                settings.ButtonCornerRadius = 2;
                settings.InputCornerRadius = 2;
                settings.ShadowBlurRadius = 0;
                settings.ShadowDepth = 0;
                settings.ShadowOpacity = 0;
                settings.SurfaceShadowScale = 0;
                settings.ControlShadowScale = 0;
                settings.PagePadding = 4;
                settings.CardPadding = 6;
                settings.FontScale = 0.96;
                settings.HeadingFontScale = 0.95;
                settings.ControlHeight = 26;
                settings.DataGridRowHeight = 28;
                settings.DataGridHeaderHeight = 28;
                settings.InteractionIntensity = 1.1;
                settings.FocusRingOpacity = 0.75;
                settings.GridLineOpacity = 0.7;
                settings.MotionIntensity = 0.4;
                settings.EnableSurfaceShadows = false;
                settings.EnableControlShadows = false;
            }

            return settings;
        }

        public void Normalize()
        {
            BaseTheme = NormalizeBaseTheme(BaseTheme);
            var defaults = CreateDefault(BaseTheme);
            BackgroundColor = NormalizeColor(BackgroundColor, defaults.BackgroundColor);
            BackgroundOverlayColor = NormalizeColor(BackgroundOverlayColor, defaults.BackgroundOverlayColor);
            SurfaceColor = NormalizeColor(SurfaceColor, defaults.SurfaceColor);
            SurfaceAltColor = NormalizeColor(SurfaceAltColor, defaults.SurfaceAltColor);
            NavigationColor = NormalizeColor(NavigationColor, defaults.NavigationColor);
            InputColor = NormalizeColor(InputColor, defaults.InputColor);
            ButtonColor = NormalizeColor(ButtonColor, defaults.ButtonColor);
            BorderColor = NormalizeColor(BorderColor, defaults.BorderColor);
            TextColor = NormalizeColor(TextColor, defaults.TextColor);
            MutedTextColor = NormalizeColor(MutedTextColor, defaults.MutedTextColor);
            AccentColor = NormalizeColor(AccentColor, defaults.AccentColor);
            HoverColor = NormalizeColor(HoverColor, defaults.HoverColor);
            HoverTextColor = NormalizeColor(HoverTextColor, defaults.HoverTextColor);
            SelectedColor = NormalizeColor(SelectedColor, defaults.SelectedColor);
            SelectedTextColor = NormalizeColor(SelectedTextColor, defaults.SelectedTextColor);
            SuccessColor = NormalizeColor(SuccessColor, defaults.SuccessColor);
            WarningColor = NormalizeColor(WarningColor, defaults.WarningColor);
            ErrorColor = NormalizeColor(ErrorColor, defaults.ErrorColor);
            ShadowColor = NormalizeColor(ShadowColor, defaults.ShadowColor);
            DashboardHeaderColor = NormalizeColor(DashboardHeaderColor, SurfaceColor);
            SearchHeaderColor = NormalizeColor(SearchHeaderColor, SurfaceColor);
            ManageItemsHeaderColor = NormalizeColor(ManageItemsHeaderColor, SurfaceColor);
            RentalsHeaderColor = NormalizeColor(RentalsHeaderColor, SurfaceColor);
            CustomersHeaderColor = NormalizeColor(CustomersHeaderColor, SurfaceColor);
            ReservationsHeaderColor = NormalizeColor(ReservationsHeaderColor, SurfaceColor);
            MaintenanceHeaderColor = NormalizeColor(MaintenanceHeaderColor, SurfaceColor);
            CalibrationHeaderColor = NormalizeColor(CalibrationHeaderColor, SurfaceColor);
            KitsHeaderColor = NormalizeColor(KitsHeaderColor, SurfaceColor);
            CategoriesHeaderColor = NormalizeColor(CategoriesHeaderColor, SurfaceColor);
            ReportsHeaderColor = NormalizeColor(ReportsHeaderColor, SurfaceColor);
            ActivityLogsHeaderColor = NormalizeColor(ActivityLogsHeaderColor, SurfaceColor);
            ImportExportHeaderColor = NormalizeColor(ImportExportHeaderColor, SurfaceColor);
            UsersHeaderColor = NormalizeColor(UsersHeaderColor, SurfaceColor);
            SettingsHeaderColor = NormalizeColor(SettingsHeaderColor, SurfaceColor);
            BackgroundImageStretch = NormalizeBackgroundStretch(BackgroundImageStretch);
            FontFamily = NormalizeFontFamily(FontFamily, defaults.FontFamily);
            BackgroundOpacity = Clamp01(BackgroundOpacity);
            BackgroundOverlayOpacity = Clamp01(BackgroundOverlayOpacity);
            SurfaceOpacity = Clamp01(SurfaceOpacity);
            SurfaceAltOpacity = Clamp01(SurfaceAltOpacity);
            InputOpacity = Clamp01(InputOpacity);
            ButtonOpacity = Clamp01(ButtonOpacity);
            NavigationOpacity = Clamp01(NavigationOpacity);
            HeaderOpacity = Clamp01(HeaderOpacity);
            MenuOpacity = Clamp01(MenuOpacity);
            MenuDropDownOpacity = Clamp01(MenuDropDownOpacity);
            FooterOpacity = Clamp01(FooterOpacity);
            DialogOpacity = Clamp01(DialogOpacity);
            DisabledOpacity = Clamp(DisabledOpacity, 0.15, 1);
            BorderOpacity = Clamp01(BorderOpacity);
            BorderThickness = Clamp(BorderThickness, 0, 6);
            ControlBorderThickness = Clamp(ControlBorderThickness, 0, 6);
            DividerOpacity = Clamp01(DividerOpacity);
            CardCornerRadius = Clamp(CardCornerRadius, 0, 32);
            PanelCornerRadius = Clamp(PanelCornerRadius, 0, 32);
            ButtonCornerRadius = Clamp(ButtonCornerRadius, 0, 32);
            InputCornerRadius = Clamp(InputCornerRadius, 0, 32);
            ShadowBlurRadius = Clamp(ShadowBlurRadius, 0, 48);
            ShadowDepth = Clamp(ShadowDepth, 0, 16);
            ShadowOpacity = Clamp01(ShadowOpacity);
            ShadowDirection = Clamp(ShadowDirection, 0, 360);
            SurfaceShadowScale = Clamp(SurfaceShadowScale, 0, 3);
            ControlShadowScale = Clamp(ControlShadowScale, 0, 3);
            PagePadding = Clamp(PagePadding, 0, 28);
            CardPadding = Clamp(CardPadding, 0, 32);
            FontScale = Clamp(FontScale, 0.75, 1.4);
            HeadingFontScale = Clamp(HeadingFontScale, 0.75, 1.6);
            ControlHeight = Clamp(ControlHeight, 22, 44);
            DataGridRowHeight = Clamp(DataGridRowHeight, 22, 52);
            DataGridHeaderHeight = Clamp(DataGridHeaderHeight, 24, 56);
            InteractionIntensity = Clamp(InteractionIntensity, 0, 2);
            FocusRingOpacity = Clamp01(FocusRingOpacity);
            GridLineOpacity = Clamp01(GridLineOpacity);
            MotionIntensity = Clamp(MotionIntensity, 0, 2);
        }

        private static string NormalizeBaseTheme(string? value)
        {
            if (value?.IndexOf("SD European", StringComparison.OrdinalIgnoreCase) >= 0 &&
                value?.IndexOf("Dark", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "SD European Dark";
            }

            if (value?.IndexOf("SD European", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "SD European Light";
            }

            if ((value?.IndexOf("VS Code", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 value?.IndexOf("VSCode", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 value?.IndexOf("Visual Studio Code", StringComparison.OrdinalIgnoreCase) >= 0) &&
                value?.IndexOf("Light", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "VS Code Light";
            }

            if (value?.IndexOf("VS Code", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value?.IndexOf("VSCode", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value?.IndexOf("Visual Studio Code", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "VS Code";
            }

            return value?.IndexOf("Dark", StringComparison.OrdinalIgnoreCase) >= 0 ? "Dark" : "Light";
        }

        private static void ApplySdLightPageHeaderColors(AppThemeSettings settings)
        {
            settings.DashboardHeaderColor = "#FFEAF4FF";
            settings.SearchHeaderColor = "#FFEEF7F1";
            settings.ManageItemsHeaderColor = "#FFFFF4D6";
            settings.RentalsHeaderColor = "#FFFFE8CC";
            settings.CustomersHeaderColor = "#FFEAF1FF";
            settings.ReservationsHeaderColor = "#FFF3E8FF";
            settings.MaintenanceHeaderColor = "#FFFFE7E7";
            settings.CalibrationHeaderColor = "#FFE6FAF8";
            settings.KitsHeaderColor = "#FFF1F5D8";
            settings.CategoriesHeaderColor = "#FFEDE7DD";
            settings.ReportsHeaderColor = "#FFE8F0FE";
            settings.ActivityLogsHeaderColor = "#FFF0EBFF";
            settings.ImportExportHeaderColor = "#FFE8F7FF";
            settings.UsersHeaderColor = "#FFF4EAF3";
            settings.SettingsHeaderColor = "#FFECEFF3";
        }

        private static void ApplySdDarkPageHeaderColors(AppThemeSettings settings)
        {
            settings.DashboardHeaderColor = "#FF12324A";
            settings.SearchHeaderColor = "#FF173A2A";
            settings.ManageItemsHeaderColor = "#FF4A3510";
            settings.RentalsHeaderColor = "#FF4A2A14";
            settings.CustomersHeaderColor = "#FF1C2F4F";
            settings.ReservationsHeaderColor = "#FF35264D";
            settings.MaintenanceHeaderColor = "#FF4A1F24";
            settings.CalibrationHeaderColor = "#FF123D3B";
            settings.KitsHeaderColor = "#FF313A18";
            settings.CategoriesHeaderColor = "#FF34302A";
            settings.ReportsHeaderColor = "#FF223049";
            settings.ActivityLogsHeaderColor = "#FF2D2748";
            settings.ImportExportHeaderColor = "#FF143746";
            settings.UsersHeaderColor = "#FF3A2435";
            settings.SettingsHeaderColor = "#FF2B3038";
        }

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

        private static string NormalizeFontFamily(string? value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            var trimmed = value.Trim();
            return trimmed.Length > 80 ? trimmed[..80] : trimmed;
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

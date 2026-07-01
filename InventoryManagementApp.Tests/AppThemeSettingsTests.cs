using System.Text.Json;
using InventoryManagementApp.Models;
using Xunit;

public class AppThemeSettingsTests
{
    [Fact]
    public void Normalize_ClampsExpandedTransparencyAndShadowControls()
    {
        var settings = new AppThemeSettings
        {
            BackgroundOpacity = 1.5,
            BackgroundOverlayOpacity = -0.25,
            SurfaceOpacity = -0.5,
            HeaderOpacity = 4,
            MenuOpacity = double.NaN,
            MenuDropDownOpacity = double.PositiveInfinity,
            FooterOpacity = -2,
            DialogOpacity = 1.2,
            DisabledOpacity = 9,
            BorderThickness = 99,
            ControlBorderThickness = -1,
            SearchBarBorderThickness = 99,
            DividerOpacity = double.PositiveInfinity,
            ShadowBlurRadius = 99,
            ShadowDepth = -4,
            ShadowOpacity = 2,
            ShadowDirection = 999,
            SurfaceShadowScale = 99,
            ControlShadowScale = -5,
            FontScale = 3,
            HeadingFontScale = 9,
            ControlHeight = 2,
            DataGridRowHeight = 100,
            DataGridHeaderHeight = 1,
            InteractionIntensity = -1,
            MotionIntensity = 9
        };

        settings.Normalize();

        Assert.Equal(1, settings.BackgroundOpacity);
        Assert.Equal(0, settings.BackgroundOverlayOpacity);
        Assert.Equal(0, settings.SurfaceOpacity);
        Assert.Equal(1, settings.HeaderOpacity);
        Assert.Equal(0, settings.MenuOpacity);
        Assert.Equal(0, settings.MenuDropDownOpacity);
        Assert.Equal(0, settings.FooterOpacity);
        Assert.Equal(1, settings.DialogOpacity);
        Assert.Equal(1, settings.DisabledOpacity);
        Assert.Equal(6, settings.BorderThickness);
        Assert.Equal(0, settings.ControlBorderThickness);
        Assert.Equal(6, settings.SearchBarBorderThickness);
        Assert.Equal(0, settings.DividerOpacity);
        Assert.Equal(48, settings.ShadowBlurRadius);
        Assert.Equal(0, settings.ShadowDepth);
        Assert.Equal(1, settings.ShadowOpacity);
        Assert.Equal(360, settings.ShadowDirection);
        Assert.Equal(3, settings.SurfaceShadowScale);
        Assert.Equal(0, settings.ControlShadowScale);
        Assert.Equal(1.4, settings.FontScale);
        Assert.Equal(1.6, settings.HeadingFontScale);
        Assert.Equal(22, settings.ControlHeight);
        Assert.Equal(52, settings.DataGridRowHeight);
        Assert.Equal(24, settings.DataGridHeaderHeight);
        Assert.Equal(0, settings.InteractionIntensity);
        Assert.Equal(2, settings.MotionIntensity);
    }

    [Fact]
    public void Normalize_PreservesShadowToggleChoices()
    {
        var settings = AppThemeSettings.CreateDefault();

        settings.EnableSurfaceShadows = false;
        settings.EnableControlShadows = true;
        settings.Normalize();

        Assert.False(settings.EnableSurfaceShadows);
        Assert.True(settings.EnableControlShadows);
    }

    [Fact]
    public void SdEuropeanDefaults_UseDistinctPageHeaderColors()
    {
        var light = AppThemeSettings.CreateDefault("SD European Light");
        var dark = AppThemeSettings.CreateDefault("SD European Dark");

        Assert.Equal("#FFEAF4FF", light.DashboardHeaderColor);
        Assert.Equal("#FFFFE8CC", light.RentalsHeaderColor);
        Assert.Equal("#FFECEFF3", light.SettingsHeaderColor);
        Assert.NotEqual(light.SurfaceColor, light.DashboardHeaderColor);
        Assert.NotEqual(light.RentalsHeaderColor, light.SettingsHeaderColor);

        Assert.Equal("#FF12324A", dark.DashboardHeaderColor);
        Assert.Equal("#FF4A2A14", dark.RentalsHeaderColor);
        Assert.Equal("#FF2B3038", dark.SettingsHeaderColor);
        Assert.NotEqual(dark.SurfaceColor, dark.DashboardHeaderColor);
        Assert.NotEqual(dark.RentalsHeaderColor, dark.SettingsHeaderColor);
    }

    [Fact]
    public void Normalize_AcceptsBareHexAndRejectsMalformedColors()
    {
        var settings = AppThemeSettings.CreateDefault();
        settings.AccentColor = "60a5fa";
        settings.ErrorColor = "not-a-color";
        settings.WarningColor = "#GGGGGG";
        settings.SearchBarBackgroundColor = "112233";
        settings.SearchBarBorderColor = "not-a-color";

        settings.Normalize();

        Assert.Equal("#60A5FA", settings.AccentColor);
        Assert.Equal("#FFDC2626", settings.ErrorColor);
        Assert.Equal("#FFC99500", settings.WarningColor);
        Assert.Equal("#112233", settings.SearchBarBackgroundColor);
        Assert.Equal("#FFF5B700", settings.SearchBarBorderColor);
    }

    [Fact]
    public void JsonRoundTrip_PreservesExpandedThemeSettings()
    {
        var settings = AppThemeSettings.CreateDefault("Dark");
        settings.BackgroundOverlayColor = "#AA112233";
        settings.BackgroundOverlayOpacity = 0.17;
        settings.HeaderOpacity = 0.42;
        settings.MenuOpacity = 0.37;
        settings.MenuDropDownOpacity = 0.73;
        settings.FooterOpacity = 0.31;
        settings.DialogOpacity = 0.88;
        settings.DisabledOpacity = 0.63;
        settings.BorderThickness = 3.5;
        settings.ControlBorderThickness = 2.5;
        settings.SearchBarBackgroundColor = "#FF101010";
        settings.SearchBarBorderColor = "#FFFFCC00";
        settings.SearchBarBorderThickness = 4.5;
        settings.DividerOpacity = 0.44;
        settings.SurfaceShadowScale = 1.8;
        settings.ControlShadowScale = 0.7;
        settings.FontFamily = "Aptos";
        settings.HeadingFontScale = 1.2;
        settings.EnableSurfaceShadows = false;
        settings.EnableControlShadows = true;

        var json = JsonSerializer.Serialize(settings);
        var roundTripped = JsonSerializer.Deserialize<AppThemeSettings>(json)!;

        Assert.Equal("#AA112233", roundTripped.BackgroundOverlayColor);
        Assert.Equal(0.17, roundTripped.BackgroundOverlayOpacity);
        Assert.Equal(0.42, roundTripped.HeaderOpacity);
        Assert.Equal(0.37, roundTripped.MenuOpacity);
        Assert.Equal(0.73, roundTripped.MenuDropDownOpacity);
        Assert.Equal(0.31, roundTripped.FooterOpacity);
        Assert.Equal(0.88, roundTripped.DialogOpacity);
        Assert.Equal(0.63, roundTripped.DisabledOpacity);
        Assert.Equal(3.5, roundTripped.BorderThickness);
        Assert.Equal(2.5, roundTripped.ControlBorderThickness);
        Assert.Equal("#FF101010", roundTripped.SearchBarBackgroundColor);
        Assert.Equal("#FFFFCC00", roundTripped.SearchBarBorderColor);
        Assert.Equal(4.5, roundTripped.SearchBarBorderThickness);
        Assert.Equal(0.44, roundTripped.DividerOpacity);
        Assert.Equal(1.8, roundTripped.SurfaceShadowScale);
        Assert.Equal(0.7, roundTripped.ControlShadowScale);
        Assert.Equal("Aptos", roundTripped.FontFamily);
        Assert.Equal(1.2, roundTripped.HeadingFontScale);
        Assert.False(roundTripped.EnableSurfaceShadows);
        Assert.True(roundTripped.EnableControlShadows);
    }
}

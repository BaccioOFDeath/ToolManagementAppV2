using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class SettingsPageXamlTests
    {
        [Fact]
        public void SettingsPage_UsesAdminWorkbenchHeaderAndRemainingTabPolish()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml");

            Assert.Contains("Admin Settings Workbench", xaml, StringComparison.Ordinal);
            Assert.Contains("Workstation Defaults", xaml, StringComparison.Ordinal);
            Assert.Contains("Item Detail Visibility", xaml, StringComparison.Ordinal);
            Assert.Contains("Email Reminder Channel", xaml, StringComparison.Ordinal);
            Assert.Contains("SMS Reminder Channel", xaml, StringComparison.Ordinal);
            Assert.Contains("General handoff", xaml, StringComparison.Ordinal);
            Assert.Contains("Display contract", xaml, StringComparison.Ordinal);
            Assert.Contains("Sender directory", xaml, StringComparison.Ordinal);
            Assert.Contains("Messaging handoff", xaml, StringComparison.Ordinal);
            Assert.Contains("Settings desk ready", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void SettingsPage_PreservesSettingsBindingsCommandsAndPasswordHandlers()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml");

            Assert.Contains("TestDbCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("TestEmailCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("SendSelectedEmailPreviewCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("Preview Selected", xaml, StringComparison.Ordinal);
            Assert.Contains("EmailSignature", xaml, StringComparison.Ordinal);
            Assert.Contains("EmailTemplates", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedEmailTemplate", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedEmailTemplateSubject", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedEmailTemplateBody", xaml, StringComparison.Ordinal);
            Assert.Contains("EmailTemplateThemes", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedEmailTemplateTheme", xaml, StringComparison.Ordinal);
            Assert.Contains("ApplySelectedEmailTemplateThemeCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("{CustomerName}, {ItemNumber}, {DueDate}, {DaysOverdue}, {ContactInfo}", xaml, StringComparison.Ordinal);
            Assert.Contains("SaveEmailSettingsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("SaveMessagingSettingsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("SaveBackupSettingsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("BrowseCompanyLogoCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("SaveCompanyLogoCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("BrowseBackupDirectoryCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectAllItemDisplayCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectNoneItemDisplayCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("AddFromEmailCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("RemoveFromEmailCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("RefreshOutlookAccountsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OutlookAccountOptions", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedOutlookAccount", xaml, StringComparison.Ordinal);
            Assert.Contains("OutlookAccountStatus", xaml, StringComparison.Ordinal);
            Assert.Contains("EmailConfigurationStatus", xaml, StringComparison.Ordinal);
            Assert.Contains("SmtpPasswordBox", xaml, StringComparison.Ordinal);
            Assert.Contains("SmtpPasswordBox_PasswordChanged", xaml, StringComparison.Ordinal);
            Assert.Contains("SmsApiKeyBox", xaml, StringComparison.Ordinal);
            Assert.Contains("SmsApiKeyBox_PasswordChanged", xaml, StringComparison.Ordinal);
            Assert.Contains("PasswordIterationsBox_PreviewTextInput", xaml, StringComparison.Ordinal);
            Assert.Contains("AutoLogoutMinutesBox_PreviewTextInput", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeDesignerControl_ExposesFullAppThemeCustomizationControls()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ThemeDesignerControl.xaml");

            Assert.Contains("ItemsSource=\"{Binding ThemeOptions}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("BackgroundOverlayColor", xaml, StringComparison.Ordinal);
            Assert.Contains("SuccessColor", xaml, StringComparison.Ordinal);
            Assert.Contains("WarningColor", xaml, StringComparison.Ordinal);
            Assert.Contains("ErrorColor", xaml, StringComparison.Ordinal);
            Assert.Contains("NavigationColor", xaml, StringComparison.Ordinal);
            Assert.Contains("InputColor", xaml, StringComparison.Ordinal);
            Assert.Contains("ButtonColor", xaml, StringComparison.Ordinal);
            Assert.Contains("BorderColor", xaml, StringComparison.Ordinal);
            Assert.Contains("ShadowColor", xaml, StringComparison.Ordinal);
            Assert.Contains("FontFamily", xaml, StringComparison.Ordinal);
            Assert.Contains("SurfaceAltOpacity", xaml, StringComparison.Ordinal);
            Assert.Contains("BackgroundOverlayOpacity", xaml, StringComparison.Ordinal);
            Assert.Contains("NavigationOpacity", xaml, StringComparison.Ordinal);
            Assert.Contains("MenuDropDownOpacity", xaml, StringComparison.Ordinal);
            Assert.Contains("PanelCornerRadius", xaml, StringComparison.Ordinal);
            Assert.Contains("BackgroundImagePath", xaml, StringComparison.Ordinal);
            Assert.Contains("BackgroundImageStretch", xaml, StringComparison.Ordinal);
            Assert.Contains("BackgroundStretchOptions", xaml, StringComparison.Ordinal);
            Assert.Contains("BorderOpacity", xaml, StringComparison.Ordinal);
            Assert.Contains("BorderThickness", xaml, StringComparison.Ordinal);
            Assert.Contains("ControlBorderThickness", xaml, StringComparison.Ordinal);
            Assert.Contains("DividerOpacity", xaml, StringComparison.Ordinal);
            Assert.Contains("ShadowBlurRadius", xaml, StringComparison.Ordinal);
            Assert.Contains("ShadowDepth", xaml, StringComparison.Ordinal);
            Assert.Contains("ShadowDirection", xaml, StringComparison.Ordinal);
            Assert.Contains("SurfaceShadowScale", xaml, StringComparison.Ordinal);
            Assert.Contains("ControlShadowScale", xaml, StringComparison.Ordinal);
            Assert.Contains("PagePadding", xaml, StringComparison.Ordinal);
            Assert.Contains("CardPadding", xaml, StringComparison.Ordinal);
            Assert.Contains("FontScale", xaml, StringComparison.Ordinal);
            Assert.Contains("HeadingFontScale", xaml, StringComparison.Ordinal);
            Assert.Contains("DisabledOpacity", xaml, StringComparison.Ordinal);
            Assert.Contains("ControlHeight", xaml, StringComparison.Ordinal);
            Assert.Contains("DataGridRowHeight", xaml, StringComparison.Ordinal);
            Assert.Contains("DataGridHeaderHeight", xaml, StringComparison.Ordinal);
            Assert.Contains("InteractionIntensity", xaml, StringComparison.Ordinal);
            Assert.Contains("FocusRingOpacity", xaml, StringComparison.Ordinal);
            Assert.Contains("GridLineOpacity", xaml, StringComparison.Ordinal);
            Assert.Contains("MotionIntensity", xaml, StringComparison.Ordinal);
            Assert.Contains("DashboardHeaderColor", xaml, StringComparison.Ordinal);
            Assert.Contains("RentalsHeaderColor", xaml, StringComparison.Ordinal);
            Assert.Contains("SettingsHeaderColor", xaml, StringComparison.Ordinal);
            Assert.Contains("Page header bands", xaml, StringComparison.Ordinal);
            Assert.Contains("Shape and borders", xaml, StringComparison.Ordinal);
            Assert.Contains("Depth, spacing, and density", xaml, StringComparison.Ordinal);
            Assert.Contains("04 Density and interaction", xaml, StringComparison.Ordinal);
            Assert.Contains("05 Page headers", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeDesignerControl_SplitsCustomizationSurfaceIntoFocusedTabs()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ThemeDesignerControl.xaml");

            Assert.Contains("x:Name=\"ThemeDesignerTabs\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"01 Colors\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"02 Backgrounds and transparency\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"03 Shape and depth\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"04 Density and interaction\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"05 Page headers\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Theme pages", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeDesignerControl_ExposesPreviewLabForFullAppThemeCoverage()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ThemeDesignerControl.xaml");

            Assert.Contains("Theme coverage preview lab", xaml, StringComparison.Ordinal);
            Assert.Contains("Shell and card depth", xaml, StringComparison.Ordinal);
            Assert.Contains("Themed control matrix", xaml, StringComparison.Ordinal);
            Assert.Contains("Table density and selection", xaml, StringComparison.Ordinal);
            Assert.Contains("Transparent background lane", xaml, StringComparison.Ordinal);
            Assert.Contains("Print and document preview", xaml, StringComparison.Ordinal);
            Assert.Contains("Coverage board for controls, tables, transparent backgrounds, semantic states, and document preview surfaces.", xaml, StringComparison.Ordinal);
            Assert.Contains("<ComboBoxItem Content=\"Dropdown preview\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<DataGrid AutoGenerateColumns=\"False\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<CheckBox Content=\"Checked\" IsChecked=\"True\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeControlCustomizationOverrides_AreLoadedAfterFullCustomizationLayer()
        {
            var appXaml = ReadRepositoryFile("InventoryManagementApp", "App.xaml");
            var controlOverrides = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.ControlCustomizationOverrides.xaml");

            var fullLayerIndex = appXaml.IndexOf("Resources/Theme.FullCustomizationOverrides.xaml", StringComparison.Ordinal);
            var controlLayerIndex = appXaml.IndexOf("Resources/Theme.ControlCustomizationOverrides.xaml", StringComparison.Ordinal);

            Assert.True(fullLayerIndex >= 0, "Expected the app to load the full admin theme customization layer.");
            Assert.True(controlLayerIndex > fullLayerIndex, "Expected control overrides to load after full customization resources.");
            Assert.Contains("TargetType=\"TabControl\"", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"TabItem\"", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"CheckBox\"", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Slider\"", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ProgressBar\"", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("ThemeControlBorderThickness", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("ThemeControlMinHeight", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("x:Null", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("ThemeDisabledOpacity", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("GlassSurfaceBrush", controlOverrides, StringComparison.Ordinal);
        }

        static string ReadRepositoryFile(params string[] relativePathParts)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "InventoryManagementApp.sln")))
                directory = directory.Parent;

            Assert.NotNull(directory);
            var path = Path.Combine(directory!.FullName, Path.Combine(relativePathParts));
            Assert.True(File.Exists(path), $"Expected repository file at {path}");
            return File.ReadAllText(path);
        }
    }
}

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
            Assert.Contains("Shape and borders", xaml, StringComparison.Ordinal);
            Assert.Contains("Depth, spacing, and density", xaml, StringComparison.Ordinal);
            Assert.Contains("Interaction feel", xaml, StringComparison.Ordinal);
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

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
            Assert.Contains("OutlookAccountOptions", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedOutlookAccount", xaml, StringComparison.Ordinal);
            Assert.Contains("OutlookAccountStatus", xaml, StringComparison.Ordinal);
            Assert.Contains("SmtpPasswordBox", xaml, StringComparison.Ordinal);
            Assert.Contains("SmtpPasswordBox_PasswordChanged", xaml, StringComparison.Ordinal);
            Assert.Contains("SmsApiKeyBox", xaml, StringComparison.Ordinal);
            Assert.Contains("SmsApiKeyBox_PasswordChanged", xaml, StringComparison.Ordinal);
            Assert.Contains("PasswordIterationsBox_PreviewTextInput", xaml, StringComparison.Ordinal);
            Assert.Contains("AutoLogoutMinutesBox_PreviewTextInput", xaml, StringComparison.Ordinal);
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

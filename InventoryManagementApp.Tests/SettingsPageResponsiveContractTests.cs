using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class SettingsPageResponsiveContractTests
    {
        [Fact]
        public void SettingsPage_KeepsHeaderMetricsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml");

            Assert.Contains("<ColumnDefinition Width=\"1.15*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.85*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Column=\"2\" HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"160\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"260\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Column=\"2\" Columns=\"4\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2*\" MinWidth=\"360\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"3*\" MinWidth=\"520\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void SettingsPage_WrapsActionStripsSoPrimaryCommandsStayReachable()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml");

            Assert.Contains("SettingsPrimaryActionButton", xaml, StringComparison.Ordinal);
            Assert.Contains("SettingsActionButton", xaml, StringComparison.Ordinal);
            Assert.True(CountOccurrences(xaml, "<WrapPanel DockPanel.Dock=\"Right\" VerticalAlignment=\"Center\">") >= 5);
            Assert.Contains("<WrapPanel DockPanel.Dock=\"Left\" VerticalAlignment=\"Center\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Orientation=\"Horizontal\" DockPanel.Dock=\"Left\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Orientation=\"Horizontal\" VerticalAlignment=\"Center\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void SettingsPage_UsesScrollableContentAndLowerSplitPressureAcrossTabs()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml");

            Assert.True(CountOccurrences(xaml, "VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Auto\"") >= 7);
            Assert.True(CountOccurrences(xaml, "MinWidth=\"0\"") >= 20);
            Assert.Contains("<ColumnDefinition Width=\"2*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.45*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.65*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"500\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"460\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"440\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"420\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void SettingsPage_BoundsFormControlsAndItemDisplayTiles()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml");

            Assert.True(CountOccurrences(xaml, "<ColumnDefinition Width=\"155\"/>") >= 6);
            Assert.Contains("<Border Style=\"{StaticResource DesktopNoteCard}\" MinWidth=\"190\" MaxWidth=\"245\" MinHeight=\"46\" Margin=\"0,0,8,8\" Padding=\"10,8\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<TextBox MinWidth=\"190\" MaxWidth=\"260\" Text=\"{Binding NewFromEmail}\" Margin=\"0,0,6,6\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Button Style=\"{StaticResource SettingsPrimaryActionButton}\" Content=\"Save Backup Settings\" Command=\"{Binding SaveBackupSettingsCommand}\" Margin=\"8,0,0,0\" HorizontalAlignment=\"Left\" MinWidth=\"170\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"210\" Text=\"{Binding NewFromEmail}\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Style=\"{StaticResource DesktopNoteCard}\" Width=\"210\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void SettingsPage_PreservesSettingsWorkflowBindingsAndHandlers()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml");
            var requiredContracts = new[]
            {
                "TestDbCommand",
                "TestEmailCommand",
                "SaveEmailSettingsCommand",
                "SaveMessagingSettingsCommand",
                "SaveBackupSettingsCommand",
                "SelectAllItemDisplayCommand",
                "SelectNoneItemDisplayCommand",
                "RefreshOutlookAccountsCommand",
                "AddFromEmailCommand",
                "RemoveFromEmailCommand",
                "ApplySelectedEmailTemplateThemeCommand",
                "SendSelectedEmailPreviewCommand",
                "BrowseCompanyLogoCommand",
                "SaveCompanyLogoCommand",
                "BrowseBackupDirectoryCommand",
                "PasswordIterationsBox_PreviewTextInput",
                "AutoLogoutMinutesBox_PreviewTextInput",
                "SmtpPasswordBox_PasswordChanged",
                "SmsApiKeyBox_PasswordChanged"
            };

            foreach (var contract in requiredContracts)
                Assert.Contains(contract, xaml, StringComparison.Ordinal);
        }

        private static int CountOccurrences(string text, string value)
        {
            var count = 0;
            var index = 0;

            while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }
    }
}

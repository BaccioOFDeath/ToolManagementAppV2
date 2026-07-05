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
            var xaml = NormalizeNewlines(ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml"));

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
            var xaml = NormalizeNewlines(ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml"));

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
            var xaml = NormalizeNewlines(ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml"));

            Assert.True(CountOccurrences(xaml, "<ColumnDefinition Width=\"155\"/>") >= 6);
            Assert.Contains("<Border Style=\"{StaticResource DesktopNoteCard}\" MinWidth=\"190\" MaxWidth=\"245\" MinHeight=\"46\" Margin=\"0,0,8,8\" Padding=\"10,8\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<TextBox MinWidth=\"190\" MaxWidth=\"260\" Text=\"{Binding NewFromEmail}\" Margin=\"0,0,6,6\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource SettingsPrimaryActionButton}\" Content=\"Save Backup Settings\" Command=\"{Binding SaveBackupSettingsCommand}\" Margin=\"8,0,0,0\" HorizontalAlignment=\"Left\" MinWidth=\"170\"/>", xaml, StringComparison.Ordinal);
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

        [Fact]
        public void SettingsPage_CodeBehindQueuesSensitiveFieldSyncWithoutBlockingCallers()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml.cs");

            Assert.Contains("private bool _sensitiveFieldSyncQueued;", source, StringComparison.Ordinal);
            Assert.Contains("private void QueueSensitiveFieldSync(SettingsViewModel? sourceViewModel)", source, StringComparison.Ordinal);
            Assert.Contains("sourceViewModel == null || _sensitiveFieldSyncQueued || !ReferenceEquals(_settingsViewModel, sourceViewModel)", source, StringComparison.Ordinal);
            Assert.Contains("_sensitiveFieldSyncQueued = true;", source, StringComparison.Ordinal);
            Assert.Contains("Dispatcher.BeginInvoke(() => SyncSensitiveFieldsFromViewModel(sourceViewModel), DispatcherPriority.Background);", source, StringComparison.Ordinal);
            Assert.Contains("_sensitiveFieldSyncQueued = false;", source, StringComparison.Ordinal);
            Assert.Contains("private void SyncSensitiveFieldsFromViewModel(SettingsViewModel sourceViewModel)", source, StringComparison.Ordinal);
            Assert.Contains("if (!ReferenceEquals(_settingsViewModel, sourceViewModel))", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Dispatcher.Invoke(SyncSensitiveFieldsFromViewModel)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void SettingsPage_CodeBehindInitializesSettingsAfterFirstPaintWithDuplicateGuard()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml.cs");

            Assert.Contains("private Task? _initializeSettingsTask;", source, StringComparison.Ordinal);
            Assert.Contains("StartSettingsInitialization();", source, StringComparison.Ordinal);
            Assert.Contains("private void StartSettingsInitialization()", source, StringComparison.Ordinal);
            Assert.Contains("if (_settingsViewModel == null || _initializeSettingsTask != null)", source, StringComparison.Ordinal);
            Assert.Contains("_initializeSettingsCts = new CancellationTokenSource();", source, StringComparison.Ordinal);
            Assert.Contains("var version = ++_initializeSettingsVersion;", source, StringComparison.Ordinal);
            Assert.Contains("_initializeSettingsTask = InitializeSettingsAsync(_settingsViewModel, _initializeSettingsCts.Token, version);", source, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", source, StringComparison.Ordinal);
            Assert.Contains("await viewModel.InitializeAsync().ConfigureAwait(true);", source, StringComparison.Ordinal);
            Assert.Contains("QueueSensitiveFieldSync(viewModel);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void SettingsPage_CodeBehindCancelsStaleInitializationOnUnloadAndDataContextSwap()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml.cs");

            Assert.Contains("using System.Threading;", source, StringComparison.Ordinal);
            Assert.Contains("private CancellationTokenSource? _initializeSettingsCts;", source, StringComparison.Ordinal);
            Assert.Contains("private int _initializeSettingsVersion;", source, StringComparison.Ordinal);
            Assert.Contains("CancelSettingsInitialization();", source, StringComparison.Ordinal);
            Assert.Contains("private void CancelSettingsInitialization()", source, StringComparison.Ordinal);
            Assert.Contains("_initializeSettingsVersion++;", source, StringComparison.Ordinal);
            Assert.Contains("_initializeSettingsCts?.Cancel();", source, StringComparison.Ordinal);
            Assert.Contains("_initializeSettingsCts?.Dispose();", source, StringComparison.Ordinal);
            Assert.Contains("_initializeSettingsCts = null;", source, StringComparison.Ordinal);
            Assert.Contains("_initializeSettingsTask = null;", source, StringComparison.Ordinal);
            Assert.Contains("catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void SettingsPage_CodeBehindAvoidsStaleInitializationSuccessAndErrors()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml.cs");

            Assert.True(CountOccurrences(source, "version != _initializeSettingsVersion") >= 3);
            Assert.True(CountOccurrences(source, "!ReferenceEquals(_settingsViewModel, viewModel)") >= 3);
            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", source, StringComparison.Ordinal);
            Assert.Contains("MessageBox.Show(", source, StringComparison.Ordinal);
            Assert.Contains("$\"Failed to load settings: {ex.Message}\"", source, StringComparison.Ordinal);
            Assert.Contains("MessageBoxImage.Error", source, StringComparison.Ordinal);
            Assert.Contains("_settingsViewModel.PropertyChanged -= SettingsViewModel_PropertyChanged;", source, StringComparison.Ordinal);
            Assert.Contains("_settingsViewModel.PropertyChanged += SettingsViewModel_PropertyChanged;", source, StringComparison.Ordinal);
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

        private static string NormalizeNewlines(string source) => source.Replace("\r\n", "\n", StringComparison.Ordinal);

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

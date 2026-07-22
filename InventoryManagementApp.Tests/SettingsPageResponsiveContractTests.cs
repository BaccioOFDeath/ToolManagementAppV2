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
            Assert.Contains("<WrapPanel Grid.Column=\"2\" Style=\"{StaticResource PageHeaderStatsPanel}\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Style x:Key=\"SettingsStatCard\" TargetType=\"Border\" BasedOn=\"{StaticResource PageHeaderStatCard}\"/>", xaml, StringComparison.Ordinal);
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
            Assert.True(CountOccurrences(xaml, "<WrapPanel DockPanel.Dock=\"Right\" VerticalAlignment=\"Center\">") >= 4);
            Assert.DoesNotContain("<WrapPanel DockPanel.Dock=\"Left\" VerticalAlignment=\"Center\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Text=\"Admin Actions\"", xaml, StringComparison.Ordinal);
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
            Assert.Contains("!_isLoaded || sourceViewModel == null || _sensitiveFieldSyncQueued || !ReferenceEquals(_settingsViewModel, sourceViewModel)", source, StringComparison.Ordinal);
            Assert.Contains("_sensitiveFieldSyncQueued = true;", source, StringComparison.Ordinal);
            Assert.Contains("Dispatcher.BeginInvoke(new Action(() => SyncSensitiveFieldsFromViewModel(sourceViewModel)), DispatcherPriority.Background);", source, StringComparison.Ordinal);
            Assert.Contains("_sensitiveFieldSyncQueued = false;", source, StringComparison.Ordinal);
            Assert.Contains("private void SyncSensitiveFieldsFromViewModel(SettingsViewModel sourceViewModel)", source, StringComparison.Ordinal);
            Assert.Contains("if (!_isLoaded || !ReferenceEquals(_settingsViewModel, sourceViewModel))", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Dispatcher.Invoke(SyncSensitiveFieldsFromViewModel)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void SettingsPage_CodeBehindInitializesSettingsAfterFirstPaintWithDuplicateGuard()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml.cs");

            Assert.Contains("private Task? _initializeSettingsTask;", source, StringComparison.Ordinal);
            Assert.Contains("StartSettingsInitialization();", source, StringComparison.Ordinal);
            Assert.Contains("private void StartSettingsInitialization()", source, StringComparison.Ordinal);
            Assert.Contains("if (!_isLoaded || _settingsViewModel == null || _initializeSettingsTask != null)", source, StringComparison.Ordinal);
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

            Assert.Contains("private bool IsCurrentSettingsInitialization(SettingsViewModel viewModel, int version)", source, StringComparison.Ordinal);
            Assert.Contains("&& ReferenceEquals(_settingsViewModel, viewModel)", source, StringComparison.Ordinal);
            Assert.Contains("&& version == _initializeSettingsVersion;", source, StringComparison.Ordinal);
            Assert.True(CountOccurrences(source, "IsCurrentSettingsInitialization(viewModel, version)") >= 3);
            Assert.Contains("private void CompleteSettingsInitialization(SettingsViewModel viewModel, int version)", source, StringComparison.Ordinal);
            Assert.Contains("_initializeSettingsCts?.Dispose();", source, StringComparison.Ordinal);
            Assert.Contains("_initializeSettingsTask = null;", source, StringComparison.Ordinal);
            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", source, StringComparison.Ordinal);
            Assert.Contains("MessageBox.Show(", source, StringComparison.Ordinal);
            Assert.Contains("$\"Failed to load settings: {ex.Message}\"", source, StringComparison.Ordinal);
            Assert.Contains("MessageBoxImage.Error", source, StringComparison.Ordinal);
            Assert.Contains("_settingsViewModel.PropertyChanged -= SettingsViewModel_PropertyChanged;", source, StringComparison.Ordinal);
            Assert.Contains("_settingsViewModel.PropertyChanged += SettingsViewModel_PropertyChanged;", source, StringComparison.Ordinal);
        }

        [Fact]
        public void SettingsPage_CodeBehindCancelsQueuedThemeTabWorkAfterUnload()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml.cs");

            Assert.Contains("private bool _isLoaded;", source, StringComparison.Ordinal);
            Assert.Contains("private int _themeDesignerTabVersion;", source, StringComparison.Ordinal);
            Assert.Contains("_isLoaded = true;", source, StringComparison.Ordinal);
            Assert.Contains("_isLoaded = false;", source, StringComparison.Ordinal);
            Assert.Contains("_themeDesignerTabVersion++;", source, StringComparison.Ordinal);
            Assert.Contains("_themeDesignerTabRetryQueued = false;", source, StringComparison.Ordinal);
            Assert.Contains("if (_themeDesignerTabRetryQueued || !_isLoaded)", source, StringComparison.Ordinal);
            Assert.Contains("var version = ++_themeDesignerTabVersion;", source, StringComparison.Ordinal);
            Assert.Contains("if (!_isLoaded || version != _themeDesignerTabVersion)", source, StringComparison.Ordinal);
            Assert.Contains("AddThemeDesignerTab();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void SettingsPage_CodeBehindUsesIterativeVisualTraversalForThemeTabLookup()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml.cs");

            Assert.Contains("using System.Collections.Generic;", source, StringComparison.Ordinal);
            Assert.Contains("var pending = new Stack<DependencyObject>();", source, StringComparison.Ordinal);
            Assert.Contains("pending.Push(parent);", source, StringComparison.Ordinal);
            Assert.Contains("while (pending.Count > 0)", source, StringComparison.Ordinal);
            Assert.Contains("var current = pending.Pop();", source, StringComparison.Ordinal);
            Assert.Contains("var childCount = GetVisualChildrenCount(current);", source, StringComparison.Ordinal);
            Assert.Contains("for (var i = childCount - 1; i >= 0; i--)", source, StringComparison.Ordinal);
            Assert.Contains("pending.Push(child);", source, StringComparison.Ordinal);
            Assert.Contains("private static int GetVisualChildrenCount(DependencyObject parent)", source, StringComparison.Ordinal);
            Assert.Contains("catch (InvalidOperationException)", source, StringComparison.Ordinal);
            Assert.DoesNotContain("var nested = FindVisualChild<T>(child);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void SettingsPage_CodeBehindAddsCtrlFFocusForActiveTabEditableFields()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml.cs");

            Assert.Contains("PreviewKeyDown += SettingsPage_PreviewKeyDown;", source, StringComparison.Ordinal);
            Assert.Contains("private void SettingsPage_PreviewKeyDown(object sender, KeyEventArgs e)", source, StringComparison.Ordinal);
            Assert.Contains("e.Key != Key.F || (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control", source, StringComparison.Ordinal);
            Assert.Contains("var target = FindFirstEditableTextBoxInActiveTab() ?? FindFirstEditableTextBox(this);", source, StringComparison.Ordinal);
            Assert.Contains("target.Focus();", source, StringComparison.Ordinal);
            Assert.Contains("target.SelectAll();", source, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", source, StringComparison.Ordinal);
            Assert.Contains("private TextBox? FindFirstEditableTextBoxInActiveTab()", source, StringComparison.Ordinal);
            Assert.Contains("tabControl?.SelectedContent is DependencyObject selectedContent", source, StringComparison.Ordinal);
            Assert.Contains("tabControl?.SelectedItem is TabItem { Content: DependencyObject selectedTabContent }", source, StringComparison.Ordinal);
        }

        [Fact]
        public void SettingsPage_CodeBehindFiltersFocusTargetsWithoutRecursiveTraversal()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml.cs");

            Assert.Contains("private static TextBox? FindFirstEditableTextBox(DependencyObject parent)", source, StringComparison.Ordinal);
            Assert.Contains("var pending = new Stack<DependencyObject>();", source, StringComparison.Ordinal);
            Assert.Contains("if (current is TextBox textBox && IsUsableFocusTarget(textBox))", source, StringComparison.Ordinal);
            Assert.Contains("private static bool IsUsableFocusTarget(TextBox textBox)", source, StringComparison.Ordinal);
            Assert.Contains("textBox.IsVisible", source, StringComparison.Ordinal);
            Assert.Contains("textBox.IsEnabled", source, StringComparison.Ordinal);
            Assert.Contains("!textBox.IsReadOnly", source, StringComparison.Ordinal);
            Assert.Contains("textBox.Focusable", source, StringComparison.Ordinal);
            Assert.Contains("pending.Push(VisualTreeHelper.GetChild(current, i));", source, StringComparison.Ordinal);
            Assert.DoesNotContain("FindFirstEditableTextBox(child)", source, StringComparison.Ordinal);
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

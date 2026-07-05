using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class MainWindowResponsiveContractTests
    {
        [Fact]
        public void MainWindow_UsesScaledDesktopSafeShellDimensions()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "MainWindow.xaml");

            Assert.Contains("Width=\"1180\" Height=\"760\" MinWidth=\"880\" MinHeight=\"520\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"1280\" Height=\"800\" MinWidth=\"1040\" MinHeight=\"540\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"920\" MinHeight=\"520\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MainWindow_HeaderColumnsCanShrinkWithoutForcingHorizontalOverflow()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "MainWindow.xaml");

            Assert.Contains("<Grid VerticalAlignment=\"Center\" ClipToBounds=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"Auto\" MinWidth=\"0\" MaxWidth=\"260\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"Auto\" MinWidth=\"0\" MaxWidth=\"210\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"250\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"0\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"250\"\n                        MinWidth=\"210\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MainWindow_BoundsSearchAndUserSwitcherForScaledWidths()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "MainWindow.xaml");

            Assert.Contains("<pages:SearchBar x:Name=\"ShellSearchBar\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"180\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"720\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Button x:Name=\"ShellUserButton\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"196\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<StackPanel MaxWidth=\"126\" MinWidth=\"0\" VerticalAlignment=\"Center\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel Width=\"132\" VerticalAlignment=\"Center\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MainWindow_MenuScrollsInsteadOfClippingNavigationAtScaledWidths()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "MainWindow.xaml");

            Assert.Contains("<ScrollViewer HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VerticalScrollBarVisibility=\"Disabled\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Focusable=\"False\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Menu x:Name=\"ShellSectionMenu\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Overview\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Operations\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Insights\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Data\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Admin\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MainWindow_PageHeaderWrapsWorkflowActionsInBoundedArea()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "MainWindow.xaml");

            Assert.Contains("MinHeight=\"44\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"Auto\" MinWidth=\"0\" MaxWidth=\"420\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<StackPanel VerticalAlignment=\"Center\" MinWidth=\"0\" Margin=\"0,0,12,0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Column=\"1\" Orientation=\"Horizontal\" HorizontalAlignment=\"Right\" VerticalAlignment=\"Center\" MaxWidth=\"420\">", xaml, StringComparison.Ordinal);
            Assert.Equal(2, CountOccurrences(xaml, "MinWidth=\"96\"\n                            MaxWidth=\"180\""));
            Assert.Contains("Margin=\"0,0,4,4\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Margin=\"0,0,0,4\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MainWindow_FrameKeepsNavigationChromeOutOfPageKeyboardFlow()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "MainWindow.xaml");

            Assert.Contains("Name=\"MainFrame\"", xaml, StringComparison.Ordinal);
            Assert.Contains("NavigationUIVisibility=\"Hidden\"", xaml, StringComparison.Ordinal);
            Assert.Contains("JournalOwnership=\"OwnsJournal\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Focusable=\"False\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ClipToBounds=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("KeyboardNavigation.TabNavigation=\"Cycle\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MainWindow_FooterUsesShrinkableColumnsAndWrappingStatusActions()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "MainWindow.xaml");

            Assert.Contains("<ColumnDefinition Width=\"1.1*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"Auto\" MinWidth=\"0\" MaxWidth=\"145\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.7*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"Auto\" MinWidth=\"0\" MaxWidth=\"380\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Grid Grid.Column=\"0\" ClipToBounds=\"True\" MinWidth=\"0\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<TextBlock Text=\"Workflow status\" Style=\"{StaticResource LabelTextBlock}\" TextTrimming=\"CharacterEllipsis\" MaxWidth=\"120\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Column=\"3\" Orientation=\"Horizontal\" HorizontalAlignment=\"Right\" VerticalAlignment=\"Center\" MaxWidth=\"380\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel Grid.Column=\"3\" Orientation=\"Horizontal\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MainWindow_CodeBehindCompactsByWidthAndAvoidsRedundantResourceScaling()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "MainWindow.xaml.cs");

            Assert.Contains("const double CompactShellWidthThreshold = 1120;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("double? _lastAdaptiveResourceScale;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("availableWidth < CompactShellWidthThreshold", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SystemParameters.WorkArea.Width < CompactShellWidthThreshold", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (_lastAdaptiveResourceScale.HasValue && Math.Abs(_lastAdaptiveResourceScale.Value - scale) < 0.001)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_lastAdaptiveResourceScale = scale;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ShellUserButton.MaxWidth = compact ? 176 : 196;", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("ShellTitleButton.Width = compact ? 190 : 250;", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void MainWindow_CoalescesHighFrequencyAutoLogoutInputResets()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "MainWindow.xaml.cs");

            Assert.Contains("static readonly TimeSpan AutoLogoutInputResetInterval = TimeSpan.FromSeconds(1);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("DateTime _lastAutoLogoutResetUtc = DateTime.MinValue;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("InputManager.Current.PreProcessInput += InputManager_PreProcessInput;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("InputManager.Current.PreProcessInput -= InputManager_PreProcessInput;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ResetAutoLogoutTimerForInput(force: false);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ResetAutoLogoutTimerForInput(force: true);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (e.StagingItem.Input is KeyboardEventArgs or MouseButtonEventArgs)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (e.StagingItem.Input is MouseEventArgs)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("void ResetAutoLogoutTimerForInput(bool force)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("var now = DateTime.UtcNow;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (!force && now - _lastAutoLogoutResetUtc < AutoLogoutInputResetInterval)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_lastAutoLogoutResetUtc = now;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_mainViewModel.ResetAutoLogoutTimer();", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("MouseMove += (_, __) => vm.ResetAutoLogoutTimer();", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("KeyDown += (_, __) => vm.ResetAutoLogoutTimer();", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("MouseDown += (_, __) => vm.ResetAutoLogoutTimer();", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void MainWindow_PreservesCoreNavigationSearchUserAndWorkflowBindings()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "MainWindow.xaml");

            Assert.Contains("OpenDashboardCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenManageItemsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenRentalsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenCustomersCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenReportsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("GlobalSearchText", xaml, StringComparison.Ordinal);
            Assert.Contains("GlobalSearchCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("SwitchUserCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CurrentPage", xaml, StringComparison.Ordinal);
            Assert.Contains("CurrentWorkflowPrimaryCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CurrentWorkflowSecondaryCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CurrentWorkflowGuide", xaml, StringComparison.Ordinal);
            Assert.Contains("CurrentUserRole", xaml, StringComparison.Ordinal);
        }

        private static int CountOccurrences(string source, string value)
        {
            var count = 0;
            var index = 0;

            while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
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
                    return NormalizeLineEndings(File.ReadAllText(candidate));

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }

        private static string NormalizeLineEndings(string text) =>
            text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
    }
}

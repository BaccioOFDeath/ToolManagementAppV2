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

            Assert.Contains("Width=\"1180\" Height=\"760\" MinWidth=\"920\" MinHeight=\"520\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"1280\" Height=\"800\" MinWidth=\"1040\" MinHeight=\"540\"", xaml, StringComparison.Ordinal);
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
        public void MainWindow_PageHeaderWrapsWorkflowActionsInBoundedArea()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "MainWindow.xaml");

            Assert.Contains("<ColumnDefinition Width=\"Auto\" MinWidth=\"0\" MaxWidth=\"380\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<StackPanel VerticalAlignment=\"Center\" MinWidth=\"0\" Margin=\"0,0,12,0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Column=\"1\" Orientation=\"Horizontal\" HorizontalAlignment=\"Right\" VerticalAlignment=\"Center\" MaxWidth=\"380\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Margin=\"0,0,4,4\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Margin=\"0,0,0,4\"", xaml, StringComparison.Ordinal);
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
        public void MainWindow_PreservesCoreNavigationSearchUserAndWorkflowBindings()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "MainWindow.xaml");

            Assert.Contains("OpenDashboardCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("GlobalSearchText", xaml, StringComparison.Ordinal);
            Assert.Contains("GlobalSearchCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("SwitchUserCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CurrentPage", xaml, StringComparison.Ordinal);
            Assert.Contains("CurrentWorkflowPrimaryCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CurrentWorkflowSecondaryCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CurrentWorkflowGuide", xaml, StringComparison.Ordinal);
            Assert.Contains("CurrentUserRole", xaml, StringComparison.Ordinal);
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

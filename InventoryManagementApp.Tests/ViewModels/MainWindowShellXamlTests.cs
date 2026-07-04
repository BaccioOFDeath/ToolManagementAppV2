using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class MainWindowShellXamlTests
    {
        [Fact]
        public void MainWindow_UsesPersistentWorkflowStatusFooter()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "MainWindow.xaml");

            Assert.Contains("DesktopStatusFooter", xaml, StringComparison.Ordinal);
            Assert.Contains("Workflow status", xaml, StringComparison.Ordinal);
            Assert.Contains("ShellWorkflowTicker", xaml, StringComparison.Ordinal);
            Assert.Contains("CurrentWorkflowGuide", xaml, StringComparison.Ordinal);
            Assert.Contains("TextTrimming=\"CharacterEllipsis\"", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalAlignment=\"Stretch\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("RepeatBehavior=\"Forever\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Storyboard.TargetProperty=\"(TextBlock.RenderTransform).(TranslateTransform.X)\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MainWindow_StartsMaximizedForWorkbenchPages()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "MainWindow.xaml");

            Assert.Contains("WindowState=\"Maximized\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MainWindow_UsesCompactShellLayoutForShortLaptopScreens()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "MainWindow.xaml");
            var codeBehind = ReadRepositoryFile("InventoryManagementApp", "MainWindow.xaml.cs");

            Assert.Contains("MinHeight=\"520\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"ShellHeader\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"ShellMenu\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"PageHeaderBand\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"WorkflowGuideText\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"ShellStatusFooter\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"ShellFooterRow\"", xaml, StringComparison.Ordinal);

            Assert.Contains("CompactShellHeightThreshold = 820", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SystemParameters.WorkArea.Height < CompactShellHeightThreshold", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ShellStatusFooter.Visibility = compact ? Visibility.Collapsed : Visibility.Visible", codeBehind, StringComparison.Ordinal);
            Assert.Contains("WorkflowGuideText.Visibility = compact ? Visibility.Collapsed : Visibility.Visible", codeBehind, StringComparison.Ordinal);
            Assert.Contains("MainFrame.Margin = new Thickness(4)", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void MainWindow_UsesAdaptiveThemeScaleForWideScreens()
        {
            var codeBehind = ReadRepositoryFile("InventoryManagementApp", "MainWindow.xaml.cs");

            Assert.Contains("ApplyAdaptiveResourceScale();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("width >= 3400", codeBehind, StringComparison.Ordinal);
            Assert.Contains("width >= 2560", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SetScaledDoubleResource(\"ThemeBodyFontSize\", scale)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SetScaledDoubleResource(\"ThemeDataGridRowHeight\", scale)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SetScaledThicknessResource(\"CardPadding\", scale)", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void MainWindow_RendersUploadedBackgroundOnlyOnRootShell()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "MainWindow.xaml");

            Assert.Contains("Background=\"{DynamicResource BackgroundBrush}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Background=\"{DynamicResource MainContentBackgroundBrush}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Name=\"MainFrame\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MainWindowFooter_ExplainsCurrentLocationAndNextButtonDestinations()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "MainWindow.xaml");

            Assert.Contains("CurrentNavSectionTitle", xaml, StringComparison.Ordinal);
            Assert.Contains("CurrentPageTitle", xaml, StringComparison.Ordinal);
            Assert.Contains("Primary: ", xaml, StringComparison.Ordinal);
            Assert.Contains("CurrentWorkflowPrimaryActionText", xaml, StringComparison.Ordinal);
            Assert.Contains("Next: ", xaml, StringComparison.Ordinal);
            Assert.Contains("CurrentWorkflowSecondaryActionText", xaml, StringComparison.Ordinal);
            Assert.Contains("CurrentUserRole", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MainWindow_UsesPageSpecificHeaderBandBrushes()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "MainWindow.xaml");

            Assert.Contains("CurrentPageHeaderKey", xaml, StringComparison.Ordinal);
            Assert.Contains("PageHeaderDashboardBrush", xaml, StringComparison.Ordinal);
            Assert.Contains("PageHeaderRentalsBrush", xaml, StringComparison.Ordinal);
            Assert.Contains("PageHeaderSettingsBrush", xaml, StringComparison.Ordinal);
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

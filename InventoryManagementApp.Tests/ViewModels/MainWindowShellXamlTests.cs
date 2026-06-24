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
            Assert.Contains("HorizontalAlignment=\"Center\"", xaml, StringComparison.Ordinal);
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

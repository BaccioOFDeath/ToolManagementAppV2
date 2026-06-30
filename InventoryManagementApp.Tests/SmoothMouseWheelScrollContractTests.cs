using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class SmoothMouseWheelScrollContractTests
    {
        [Fact]
        public void MainWindowRoutesMouseWheelThroughSmoothScrollHandler()
        {
            var source = ReadRepoFile("InventoryManagementApp", "MainWindow.xaml.cs");

            Assert.Contains("using InventoryManagementApp.Utilities;", source, StringComparison.Ordinal);
            Assert.Contains("e.StagingItem.Input is MouseWheelEventArgs wheelArgs", source, StringComparison.Ordinal);
            Assert.Contains("SmoothMouseWheelScroll.TryHandle(wheelArgs)", source, StringComparison.Ordinal);
            Assert.Contains("_mainViewModel.ResetAutoLogoutTimer();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void SmoothWheelHandlerUsesReducedDeltasAndNearestScrollableParent()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Utilities", "SmoothMouseWheelScroll.cs");

            Assert.Contains("const double PixelsPerWheelStep = 24.0;", source, StringComparison.Ordinal);
            Assert.Contains("const double LogicalUnitsPerWheelStep = 0.35;", source, StringComparison.Ordinal);
            Assert.Contains("const double AnimationMilliseconds = 220.0;", source, StringComparison.Ordinal);
            Assert.Contains("FindScrollableViewer(source, e.Delta)", source, StringComparison.Ordinal);
            Assert.Contains("AnimateTo(scrollViewer, targetOffset);", source, StringComparison.Ordinal);
            Assert.Contains("DispatcherTimer(DispatcherPriority.Render", source, StringComparison.Ordinal);
            Assert.Contains("EaseOutCubic", source, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", source, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers.HasFlag(ModifierKeys.Control)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void StandardScrollViewersUsePixelScrolling()
        {
            var rangeScroll = ReadRepoFile("InventoryManagementApp", "Resources", "Theme.RangeScrollChromeOverrides.xaml");
            var adminCoverage = ReadRepoFile("InventoryManagementApp", "Resources", "Theme.AdminDesignerCoverageOverrides.xaml");

            Assert.Contains("<Setter Property=\"CanContentScroll\" Value=\"False\"/>", rangeScroll, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"CanContentScroll\" Value=\"False\"/>", adminCoverage, StringComparison.Ordinal);
        }

        static string ReadRepoFile(params string[] parts)
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

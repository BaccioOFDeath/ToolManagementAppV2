using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Views
{
    public class ThemeDesignerReadinessPanelTests
    {
        [Fact]
        public void ThemeDesignerControl_AddsDesignReadinessPanelFromCodeBehind()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ThemeDesignerControl.xaml.cs");

            Assert.Contains("AddDesignReadinessPanel();", source, StringComparison.Ordinal);
            Assert.Contains("private void AddDesignReadinessPanel()", source, StringComparison.Ordinal);
            Assert.Contains("Design readiness", source, StringComparison.Ordinal);
            Assert.Contains("Before saving a full-app redesign", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeDesignerReadinessPanel_UsesSharedStylesAndLiveStatusBinding()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ThemeDesignerControl.xaml.cs");

            Assert.Contains("DesktopInsetCard", source, StringComparison.Ordinal);
            Assert.Contains("PageTitleTextBlock", source, StringComparison.Ordinal);
            Assert.Contains("CaptionTextBlock", source, StringComparison.Ordinal);
            Assert.Contains("LabelTextBlock", source, StringComparison.Ordinal);
            Assert.Contains("nameof(ThemeDesignerViewModel.Status)", source, StringComparison.Ordinal);
            Assert.Contains("FallbackValue = \"Theme designer ready.\"", source, StringComparison.Ordinal);
            Assert.Contains("DockPanel.SetDock(panel, Dock.Bottom)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeDesignerReadinessPanel_CallsOutFullCustomizationRisks()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ThemeDesignerControl.xaml.cs");

            Assert.Contains("text contrast", source, StringComparison.Ordinal);
            Assert.Contains("transparent surface readability", source, StringComparison.Ordinal);
            Assert.Contains("focus-ring visibility", source, StringComparison.Ordinal);
            Assert.Contains("disabled-control clarity", source, StringComparison.Ordinal);
            Assert.Contains("table density", source, StringComparison.Ordinal);
            Assert.Contains("borderless affordances", source, StringComparison.Ordinal);
            Assert.Contains("shadow depth", source, StringComparison.Ordinal);
        }

        private static string ReadRepositoryFile(params string[] relativePathParts)
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

using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Resources
{
    public class ThemeFullCustomizationOverrideTests
    {
        [Fact]
        public void App_LoadsFullCustomizationOverridesAfterPolishedChrome()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "App.xaml");

            var polishedIndex = xaml.IndexOf("Resources/PolishedVisualHierarchy.xaml", StringComparison.Ordinal);
            var chromeIndex = xaml.IndexOf("Resources/Theme.WindowChrome.xaml", StringComparison.Ordinal);
            var overridesIndex = xaml.IndexOf("Resources/Theme.FullCustomizationOverrides.xaml", StringComparison.Ordinal);
            var convertersIndex = xaml.IndexOf("Resources/Converters.xaml", StringComparison.Ordinal);

            Assert.True(polishedIndex >= 0, "Polished theme hierarchy should still be loaded.");
            Assert.True(chromeIndex > polishedIndex, "Window chrome should load after polished hierarchy.");
            Assert.True(overridesIndex > chromeIndex, "Full customization overrides must load after polished chrome resources.");
            Assert.True(convertersIndex > overridesIndex, "Converters should remain after visual resources.");
        }

        [Fact]
        public void FullCustomizationOverrides_GiveAdminTokensFinalSayOverPolishedSurfaces()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.FullCustomizationOverrides.xaml");

            Assert.Contains("x:Key=\"ToolbarCard\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"DataGridColumnHeader\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"DesktopSummaryCard\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"DesktopInsetCard\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"DesktopPaneHeader\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"DesktopPaneSubheader\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"DesktopSectionActionStrip\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"DesktopStatusFooter\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"AdminHandoffCard\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"DesktopNoteCard\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"DataRunLogCard\"", xaml, StringComparison.Ordinal);

            Assert.Contains("{DynamicResource ThemeBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeSubtleBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeCardCornerRadius}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemePanelCornerRadius}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeFooterCornerRadius}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource CardPadding}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ControlPadding}", xaml, StringComparison.Ordinal);
            Assert.Contains("{x:Null}", xaml, StringComparison.Ordinal);
            Assert.Contains("{x:Null}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeGridLineBrush}", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void FullCustomizationOverrides_RemoveHardCodedAccentBordersFromFinalSharedChrome()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.FullCustomizationOverrides.xaml");

            Assert.DoesNotContain("BorderThickness\" Value=\"0,1,0,2\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("BorderThickness\" Value=\"1,1,1,2\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("BorderThickness\" Value=\"0,0,0,2\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("BorderBrush\" Value=\"{DynamicResource AccentBrush}\"", xaml, StringComparison.Ordinal);
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

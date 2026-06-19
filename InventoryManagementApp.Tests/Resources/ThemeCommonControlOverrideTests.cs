using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Resources
{
    public class ThemeCommonControlOverrideTests
    {
        [Fact]
        public void FullCustomizationOverrides_ThemeRemainingCommonControlChrome()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.FullCustomizationOverrides.xaml");

            Assert.Contains("x:Key=\"ThemeToolTipTemplate\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ToolTip\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"GroupBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Expander\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"RadioButton\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"DatePicker\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Calendar\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"TreeView\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"TreeViewItem\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ToolBar\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"StatusBar\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void FullCustomizationOverrides_CommonControlsUseAdminThemeTokens()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.FullCustomizationOverrides.xaml");

            Assert.Contains("{DynamicResource ThemeDialogSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeShellMenuBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeShellFooterBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource GlassSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource TextBoxBackgroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemePanelCornerRadius}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeSubtleBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeRaisedShadow}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlShadow}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDisabledOpacity}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlMinHeight}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeFontFamily}", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void FullCustomizationOverrides_CommonControlChromeLoadsAfterExistingSurfaceOverrides()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.FullCustomizationOverrides.xaml");

            var surfaceOverrideIndex = xaml.IndexOf("x:Key=\"DataRunLogCard\"", StringComparison.Ordinal);
            var tooltipTemplateIndex = xaml.IndexOf("x:Key=\"ThemeToolTipTemplate\"", StringComparison.Ordinal);
            var statusBarIndex = xaml.IndexOf("TargetType=\"StatusBar\"", StringComparison.Ordinal);

            Assert.True(surfaceOverrideIndex >= 0, "Existing shared surface overrides should remain in the final layer.");
            Assert.True(tooltipTemplateIndex > surfaceOverrideIndex, "Common control chrome should extend the final override layer after shared surfaces.");
            Assert.True(statusBarIndex > tooltipTemplateIndex, "Status bar chrome should be part of the common control override block.");
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

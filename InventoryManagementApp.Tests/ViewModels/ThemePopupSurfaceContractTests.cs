using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class ThemePopupSurfaceContractTests
    {
        [Fact]
        public void ThemeService_HonorsAdminTransparencyForPopupSurfaces()
        {
            var themeService = ReadRepositoryFile("InventoryManagementApp", "Services", "ThemeService.cs");

            Assert.Contains("ThemePopupSurfaceBrush", themeService, StringComparison.Ordinal);
            Assert.Contains("CreateBrush(settings.SurfaceAltColor, Math.Min(surfaceAltOpacity, settings.MenuOpacity))", themeService, StringComparison.Ordinal);
            Assert.Contains("ComboBoxPopupBackgroundBrush", themeService, StringComparison.Ordinal);
            Assert.DoesNotContain("\"ComboBoxPopupBackgroundBrush\", CreateBrush(settings.InputColor, settings.InputOpacity)", themeService, StringComparison.Ordinal);
            Assert.DoesNotContain("Math.Max(settings.InputOpacity, 0.9)", themeService, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeResources_RouteComboBoxDropdownRowsThroughPopupBackgroundBrush()
        {
            var styles = ReadRepositoryFile("InventoryManagementApp", "Resources", "Styles.xaml");
            var adminOverrides = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.AdminDesignerCoverageOverrides.xaml");

            Assert.Contains("x:Key=\"DropdownItemStyle\"", styles, StringComparison.Ordinal);
            Assert.Contains("Background\" Value=\"{DynamicResource ComboBoxPopupBackgroundBrush}\"", styles, StringComparison.Ordinal);
            Assert.Contains("<Style TargetType=\"ComboBoxItem\"", adminOverrides, StringComparison.Ordinal);
            Assert.Contains("Background\" Value=\"{DynamicResource ComboBoxPopupBackgroundBrush}\"", adminOverrides, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeResources_RouteClosedComboBoxChromeThroughThemeTemplate()
        {
            var styles = ReadRepositoryFile("InventoryManagementApp", "Resources", "Styles.xaml");

            Assert.Contains("<ToggleButton.Template>", styles, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"ToggleChrome\"", styles, StringComparison.Ordinal);
            Assert.Contains("Background=\"{TemplateBinding Background}\"", styles, StringComparison.Ordinal);
            Assert.Contains("Property=\"IsChecked\" Value=\"True\"", styles, StringComparison.Ordinal);
            Assert.Contains("Value=\"{DynamicResource ComboBoxPopupBackgroundBrush}\"", styles, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeResources_RouteSelectedTabsThroughSelectedThemeBrushes()
        {
            var controlOverrides = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.ControlCustomizationOverrides.xaml");

            Assert.Contains("<ControlTemplate TargetType=\"TabItem\">", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"TabChrome\"", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("Property=\"IsSelected\" Value=\"True\"", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("TargetName=\"TabChrome\" Property=\"Background\" Value=\"{DynamicResource ItemSelectedBrush}\"", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("Property=\"Foreground\" Value=\"{DynamicResource ItemSelectedForegroundBrush}\"", controlOverrides, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeResources_RouteContextMenusThroughPopupSurfaceBrush()
        {
            var tokens = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.Customization.xaml");
            var controlOverrides = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.ControlCustomizationOverrides.xaml");

            Assert.Contains("x:Key=\"ThemePopupSurfaceBrush\"", tokens, StringComparison.Ordinal);
            Assert.Contains("<Style TargetType=\"ContextMenu\">", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("Background\" Value=\"{DynamicResource ThemePopupSurfaceBrush}\"", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("BorderThickness\" Value=\"{DynamicResource ThemeControlBorderThickness}\"", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("Effect\" Value=\"{DynamicResource ThemeRaisedShadow}\"", controlOverrides, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeResources_RoutePopupItemsAndTooltipsThroughAdminThemeTokens()
        {
            var controlOverrides = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.ControlCustomizationOverrides.xaml");

            Assert.Contains("<Style TargetType=\"MenuItem\">", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("<Style TargetType=\"ToolTip\">", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("Background\" Value=\"{DynamicResource ThemePopupSurfaceBrush}\"", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("BorderThickness\" Value=\"{DynamicResource ThemeControlBorderThickness}\"", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("Opacity\" Value=\"{DynamicResource ThemeDisabledOpacity}\"", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("HasDropShadow\" Value=\"False\"", controlOverrides, StringComparison.Ordinal);
            Assert.Contains("Effect\" Value=\"{DynamicResource ThemeRaisedShadow}\"", controlOverrides, StringComparison.Ordinal);
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

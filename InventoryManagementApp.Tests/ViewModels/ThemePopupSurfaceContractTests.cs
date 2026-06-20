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
            Assert.Contains("CreateBrush(settings.InputColor, settings.InputOpacity)", themeService, StringComparison.Ordinal);
            Assert.DoesNotContain("Math.Max(settings.InputOpacity, 0.9)", themeService, StringComparison.Ordinal);
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

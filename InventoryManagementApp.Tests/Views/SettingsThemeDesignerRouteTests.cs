using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Views
{
    public class SettingsThemeDesignerRouteTests
    {
        [Fact]
        public void SettingsPageCodeBehind_RoutesAdminSettingsToThemeDesignerReliably()
        {
            var code = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml.cs");

            Assert.Contains("AddThemeDesignerTab();", code, StringComparison.Ordinal);
            Assert.Contains("Header = \"06 Themes\"", code, StringComparison.Ordinal);
            Assert.Contains("Content = new ThemeDesignerControl()", code, StringComparison.Ordinal);
            Assert.Contains("QueueThemeDesignerTabRetry", code, StringComparison.Ordinal);
            Assert.Contains("Dispatcher.BeginInvoke(AddThemeDesignerTab, DispatcherPriority.Loaded)", code, StringComparison.Ordinal);
            Assert.Contains("RenumberTabs(tabControl)", code, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeDesignerControl_ExposesFullAppCustomizationSurface()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ThemeDesignerControl.xaml");

            Assert.Contains("App Theme Designer", xaml, StringComparison.Ordinal);
            Assert.Contains("ImportThemeProfileCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ExportThemeProfileCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("GlassPresetCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("TransparentCanvasPresetCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("BorderlessPresetCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("DeepShadowPresetCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("BackgroundImagePath", xaml, StringComparison.Ordinal);
            Assert.Contains("BackgroundOverlayOpacity", xaml, StringComparison.Ordinal);
            Assert.Contains("SurfaceOpacity", xaml, StringComparison.Ordinal);
            Assert.Contains("ButtonOpacity", xaml, StringComparison.Ordinal);
            Assert.Contains("BordersVisible", xaml, StringComparison.Ordinal);
            Assert.Contains("BorderThickness", xaml, StringComparison.Ordinal);
            Assert.Contains("ControlBorderThickness", xaml, StringComparison.Ordinal);
            Assert.Contains("ButtonCornerRadius", xaml, StringComparison.Ordinal);
            Assert.Contains("ShadowDepth", xaml, StringComparison.Ordinal);
            Assert.Contains("SurfaceShadowScale", xaml, StringComparison.Ordinal);
            Assert.Contains("ControlShadowScale", xaml, StringComparison.Ordinal);
            Assert.Contains("Theme coverage preview lab", xaml, StringComparison.Ordinal);
        }

        private static string ReadRepositoryFile(params string[] relativePath)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, Path.Combine(relativePath));
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                directory = directory.Parent;
            }

            throw new FileNotFoundException("Could not locate repository file.", Path.Combine(relativePath));
        }
    }
}
